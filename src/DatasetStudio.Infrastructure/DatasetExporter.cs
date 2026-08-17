using System.Text;
using System.Text.Json;
using DatasetStudio.Core;

namespace DatasetStudio.Infrastructure;

public sealed class DatasetExporter
{
    public ExportResult Export(
        string projectDirectory,
        DatasetProject project,
        CatalogRepository repository)
    {
        var referenceSource = Resolve(projectDirectory, project.ReferenceImage);
        var configSource = Resolve(projectDirectory, project.ProductConfig);
        if (!File.Exists(referenceSource))
            throw new FileNotFoundException("缺少 reference_aligned.png，不能导出。", referenceSource);
        if (!File.Exists(configSource))
            throw new FileNotFoundException($"缺少 {Path.GetFileName(configSource)}，请先保存 ROI 配置。", configSource);

        var (referenceWidth, referenceHeight) = ReadReferenceSize(configSource);
        var validator = new DatasetValidator();
        if (!validator.CanExport(repository, out var validation, referenceWidth, referenceHeight))
        {
            var errors = string.Join(Environment.NewLine,
                validation.Where(x => x.Severity == ValidationSeverity.Error)
                    .Select(x => $"- {x.Name}: {x.Message}"));
            throw new InvalidOperationException("数据校验未通过：" + Environment.NewLine + errors);
        }

        var exportsRoot = Path.Combine(projectDirectory, "exports");
        Directory.CreateDirectory(exportsRoot);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var staging = Path.Combine(exportsRoot, stamp + ".staging");
        var final = Path.Combine(exportsRoot, stamp);
        if (Directory.Exists(staging)) Directory.Delete(staging, true);
        Directory.CreateDirectory(staging);

        try
        {
            var categories = project.Categories ?? new DatasetCategoryOptions();
            var datasetRoot = Path.Combine(staging, "dataset_roi_dino");
            var trainGoodDir = ResolveCategoryDirectory(datasetRoot, categories.TrainGoodDirectory, categories.TrainGoodLabel);
            var testGoodDir = ResolveCategoryDirectory(datasetRoot, categories.TestGoodDirectory, categories.TestGoodLabel);
            var testNgDir = ResolveCategoryDirectory(datasetRoot, categories.TestNgDirectory, categories.TestNgLabel);

            var distinctDirectories = new[] { trainGoodDir, testGoodDir, testNgDir }
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (distinctDirectories != 3)
                throw new InvalidOperationException("Train GOOD / Test GOOD / Test NG 的导出目录不能相同。");

            Directory.CreateDirectory(trainGoodDir);
            Directory.CreateDirectory(testGoodDir);
            Directory.CreateDirectory(testNgDir);

            CopyAndVerify(configSource, Path.Combine(staging, "configs", Path.GetFileName(configSource)));
            CopyAndVerify(referenceSource, Path.Combine(staging, "artifacts", "reference", "reference_aligned.png"));

            var manifest = new StringBuilder();
            manifest.AppendLine("file,split,truth,defect_type,defect_rois,sha256,source_file");
            var trainIndex = 0;
            var goodIndex = 0;
            var ngIndexByFolder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var trainGood = 0;
            var testGood = 0;
            var testNg = 0;

            foreach (var image in repository.LoadImages())
            {
                if (image.Split == DatasetSplit.Ignore || image.Truth == ImageTruth.Ignore || !image.IsClassified)
                    continue;

                string targetDirectory;
                string targetName;
                if (image.Split == DatasetSplit.Train && image.Truth == ImageTruth.Good)
                {
                    targetDirectory = trainGoodDir;
                    targetName = $"good_{++trainIndex:D4}{Path.GetExtension(image.FileName).ToLowerInvariant()}";
                    trainGood++;
                }
                else if (image.Split == DatasetSplit.Test && image.Truth == ImageTruth.Good)
                {
                    targetDirectory = testGoodDir;
                    targetName = $"good_test_{++goodIndex:D4}{Path.GetExtension(image.FileName).ToLowerInvariant()}";
                    testGood++;
                }
                else if (image.Split == DatasetSplit.Test && image.Truth == ImageTruth.Ng)
                {
                    var scenario = BuildNgScenario(image);
                    targetDirectory = Path.Combine(testNgDir, scenario);
                    Directory.CreateDirectory(targetDirectory);
                    ngIndexByFolder.TryGetValue(scenario, out var index);
                    index++;
                    ngIndexByFolder[scenario] = index;
                    targetName = $"{scenario}_{index:D4}{Path.GetExtension(image.FileName).ToLowerInvariant()}";
                    testNg++;
                }
                else
                {
                    continue;
                }

                var target = Path.Combine(targetDirectory, targetName);
                CopyAndVerify(image.SourcePath, target, image.Sha256);
                manifest.AppendLine(string.Join(",", new[]
                {
                    Csv(Path.GetRelativePath(staging, target)),
                    Csv(image.Split.ToString().ToLowerInvariant()),
                    Csv(image.Truth == ImageTruth.Good ? "GOOD" : "NG"),
                    Csv(image.DefectType == DefectType.None ? string.Empty : image.DefectType.ToString()),
                    Csv(image.DefectRois),
                    Csv(image.Sha256),
                    Csv(image.FileName)
                }));
            }

            var manifestPath = Path.Combine(staging, "dataset_manifest.csv");
            File.WriteAllText(manifestPath, manifest.ToString(), new UTF8Encoding(true));
            var report = new
            {
                schema_version = 1,
                generated_at = DateTimeOffset.Now,
                project = project.Name,
                train_good = trainGood,
                test_good = testGood,
                test_ng = testNg,
                category_directories = new
                {
                    train_good = NormalizeForReport(categories.TrainGoodDirectory),
                    test_good = NormalizeForReport(categories.TestGoodDirectory),
                    test_ng = NormalizeForReport(categories.TestNgDirectory)
                },
                source_files_are_read_only = true,
                source_target_sha256_verified = true
            };
            File.WriteAllText(
                Path.Combine(staging, "dataset_report.json"),
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

            if (Directory.Exists(final))
                throw new IOException($"导出目录已存在：{final}");
            Directory.Move(staging, final);
            return new ExportResult
            {
                PackageDirectory = final,
                ManifestPath = Path.Combine(final, "dataset_manifest.csv"),
                TrainGood = trainGood,
                TestGood = testGood,
                TestNg = testNg
            };
        }
        catch
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            throw;
        }
    }

    private static string ResolveCategoryDirectory(string datasetRoot, string relativeDirectory, string label)
    {
        if (string.IsNullOrWhiteSpace(relativeDirectory))
            throw new InvalidOperationException($"类别“{label}”的导出目录不能为空。");

        var segments = relativeDirectory
            .Replace('/', '\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
            throw new InvalidOperationException($"类别“{label}”的导出目录不能为空。");

        foreach (var segment in segments)
        {
            if (segment is "." or ".." || segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException($"类别“{label}”的目录包含无效路径段：{segment}");
        }

        var path = datasetRoot;
        foreach (var segment in segments)
            path = Path.Combine(path, segment);
        return path;
    }

    private static string NormalizeForReport(string path) =>
        string.Join("/", path.Replace('/', '\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static (int Width, int Height) ReadReferenceSize(string configPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(configPath));
        if (!document.RootElement.TryGetProperty("coordinate_system", out var coordinate))
            return (0, 0);
        var width = coordinate.TryGetProperty("image_width", out var widthElement) ? widthElement.GetInt32() : 0;
        var height = coordinate.TryGetProperty("image_height", out var heightElement) ? heightElement.GetInt32() : 0;
        return (width, height);
    }

    private static string BuildNgScenario(ImageRecord image)
    {
        var ids = image.GetDefectRoiIds();
        if (ids.Count == 0) return "defect_UNKNOWN";
        return image.DefectType switch
        {
            DefectType.Missing => string.Join("+", ids.Select(id => $"missing_{id}")),
            DefectType.Wrong => string.Join("+", ids.Select(id => $"wrong_{id}")),
            _ => string.Join("+", ids.Select(id => $"defect_{id}"))
        };
    }

    private static void CopyAndVerify(string source, string target, string? expectedSha = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, false);
        var sourceSha = string.IsNullOrWhiteSpace(expectedSha)
            ? CatalogRepository.ComputeSha256(source)
            : expectedSha;
        var targetSha = CatalogRepository.ComputeSha256(target);
        if (!string.Equals(sourceSha, targetSha, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(target);
            throw new IOException($"文件哈希校验失败：{Path.GetFileName(source)}");
        }
    }

    private static string Resolve(string root, string relativeOrAbsolute) =>
        Path.IsPathRooted(relativeOrAbsolute)
            ? relativeOrAbsolute
            : Path.GetFullPath(Path.Combine(root, relativeOrAbsolute));

    private static string Csv(string value) =>
        '"' + (value ?? string.Empty).Replace("\"", "\"\"") + '"';
}
