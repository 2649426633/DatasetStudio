using System.Drawing.Imaging;
using System.Text.Json;
using DatasetStudio.Core;
using DatasetStudio.Infrastructure;
using DatasetStudio.WinForms.Services;

namespace DatasetStudio.WinForms;

public sealed class AppSession
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string ProjectDirectory { get; }
    public DatasetProject Project { get; private set; }
    public CatalogRepository Repository { get; }

    public string ProjectFilePath => Path.Combine(ProjectDirectory, "project.json");
    public string ReferenceImagePath => Resolve(Project.ReferenceImage);
    public string ProductConfigPath => Resolve(Project.ProductConfig);

    public Size ReferenceImageSize
    {
        get
        {
            if (!File.Exists(ReferenceImagePath)) return Size.Empty;
            using var image = Image.FromFile(ReferenceImagePath);
            return image.Size;
        }
    }

    private AppSession(string projectDirectory, DatasetProject project)
    {
        ProjectDirectory = Path.GetFullPath(projectDirectory);
        Project = project;
        Repository = new CatalogRepository(ProjectDirectory);
        Repository.Initialize();
    }

    public static AppSession Create(string projectDirectory, string sourceDirectory)
    {
        Directory.CreateDirectory(projectDirectory);
        var name = new DirectoryInfo(projectDirectory).Name;
        if (string.IsNullOrWhiteSpace(name)) name = new DirectoryInfo(sourceDirectory).Name;
        var safeName = string.Concat(name.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_'));
        var project = new DatasetProject
        {
            Name = name,
            SourceDirectory = Path.GetFullPath(sourceDirectory),
            ReferenceImage = "reference\\reference_aligned.png",
            ProductConfig = $"configs\\{safeName.ToLowerInvariant()}.json"
        };
        var session = new AppSession(projectDirectory, project);
        session.SaveProject();

        // Do not scan/hash source images on the UI thread here. ClassificationPage
        // starts the initial scan in the background after the project is visible.
        session.WriteProductConfig();
        return session;
    }

    public static AppSession Open(string projectDirectory)
    {
        var projectFile = Path.Combine(projectDirectory, "project.json");
        if (!File.Exists(projectFile))
            throw new FileNotFoundException("所选目录中没有 project.json。", projectFile);

        var json = File.ReadAllText(projectFile);
        var project = JsonSerializer.Deserialize<DatasetProject>(json, JsonOptions)
            ?? throw new InvalidDataException("project.json 内容无效。");
        return new AppSession(projectDirectory, project);
    }

    public void SaveProject()
    {
        Directory.CreateDirectory(ProjectDirectory);
        File.WriteAllText(ProjectFilePath, JsonSerializer.Serialize(Project, JsonOptions));
    }

    public ReferenceBuildResult CreateReferenceFromGood(string sourceFile)
    {
        var directory = Path.Combine(ProjectDirectory, "reference");
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, "reference_aligned.png");
        var result = ProductAlignmentService.CreateReferenceFromGood(sourceFile, target);
        Project.ReferenceImage = "reference\\reference_aligned.png";
        SaveProject();
        ClearAlignmentCache();
        WriteProductConfig();
        return result;
    }

    public void ImportReferenceImage(string sourceFile)
    {
        var directory = Path.Combine(ProjectDirectory, "reference");
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, "reference_aligned.png");
        using (var image = Image.FromFile(sourceFile))
        using (var copy = new Bitmap(image))
            copy.Save(target, ImageFormat.Png);
        Project.ReferenceImage = "reference\\reference_aligned.png";
        SaveProject();
        ClearAlignmentCache();
        WriteProductConfig();
    }

    public AlignmentPreviewResult GetAlignmentPreview(string sourcePath, string sourceSha256)
    {
        if (!File.Exists(ReferenceImagePath))
        {
            return new AlignmentPreviewResult
            {
                Success = false,
                Error = "尚未设置 reference_aligned.png。"
            };
        }

        var referenceInfo = new FileInfo(ReferenceImagePath);
        var sourceKey = string.IsNullOrWhiteSpace(sourceSha256)
            ? CatalogRepository.ComputeSha256(sourcePath)
            : sourceSha256;
        var shortSha = sourceKey.Length > 20 ? sourceKey[..20] : sourceKey;
        var cacheKey = $"{shortSha}_{referenceInfo.Length}_{referenceInfo.LastWriteTimeUtc.Ticks}";
        var cacheDirectory = Path.Combine(ProjectDirectory, "cache", "aligned");
        var alignedPath = Path.Combine(cacheDirectory, cacheKey + ".png");
        var metadataPath = Path.Combine(cacheDirectory, cacheKey + ".json");

        var cached = ProductAlignmentService.ReadMetadata(metadataPath);
        if (cached?.Success == true && File.Exists(alignedPath))
        {
            cached.AlignedPath = alignedPath;
            return cached;
        }

        var result = ProductAlignmentService.AlignToReference(sourcePath, ReferenceImagePath, alignedPath);
        if (result.Success)
            ProductAlignmentService.WriteMetadata(metadataPath, result);
        return result;
    }

    public void ClearAlignmentCache()
    {
        var cacheDirectory = Path.Combine(ProjectDirectory, "cache", "aligned");
        try
        {
            if (Directory.Exists(cacheDirectory))
                Directory.Delete(cacheDirectory, true);
        }
        catch
        {
            // Cache is expendable. A locked preview file should not block changing the reference.
        }
    }

    public void WriteProductConfig()
    {
        var rois = Repository.LoadRois();
        var size = ReferenceImageSize;

        var screwSlots = rois
            .Where(x => x.Kind is RoiKind.ScrewSlot or RoiKind.EmptySlot)
            .OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Select(x => new
            {
                id = x.Id,
                roi = new[] { x.X, x.Y, x.Width, x.Height },
                expected = x.Kind == RoiKind.EmptySlot ? "empty" : "screw",
                enabled = x.Enabled
            });
        var springRegions = rois
            .Where(x => x.Kind == RoiKind.SpringRegion)
            .OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Select(x => new
            {
                id = x.Id,
                roi = new[] { x.X, x.Y, x.Width, x.Height },
                expected_count = x.ExpectedCount ?? 4,
                enabled = x.Enabled
            });
        var anomalyRegions = rois
            .Where(x => x.Kind is RoiKind.AnomalyRegion or RoiKind.CustomRegion)
            .OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Select(x => new
            {
                id = x.Id,
                roi = new[] { x.X, x.Y, x.Width, x.Height },
                enabled = x.Enabled,
                custom_type = x.Kind == RoiKind.CustomRegion ? x.Expected : null
            });

        var config = new
        {
            schema_version = 1,
            product = Project.Name,
            coordinate_system = new
            {
                reference_image = "artifacts/reference/reference_aligned.png",
                image_width = size.Width,
                image_height = size.Height
            },
            screw_slots = screwSlots,
            spring_regions = springRegions,
            anomaly_regions = anomalyRegions
        };

        Directory.CreateDirectory(Path.GetDirectoryName(ProductConfigPath)!);
        File.WriteAllText(ProductConfigPath, JsonSerializer.Serialize(config, JsonOptions));
    }

    private string Resolve(string path) => Path.IsPathRooted(path)
        ? path
        : Path.GetFullPath(Path.Combine(ProjectDirectory, path));
}
