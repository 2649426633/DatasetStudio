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
            var trainGoodDir = Path.Combine(staging, "dataset_roi_dino", "train", "good");
            var testGoodDir = Path.Combine(staging, "dataset_roi_dino", "test", "good");
            var testNgDir = Path.Combine(staging, "dataset_roi_dino", "test", "ng");
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

            ValidateProductAlignCompatibility(staging, configSource);

            var report = new
            {
                schema_version = 1,
                generated_at = DateTimeOffset.Now,
                project = project.Name,
                train_good = trainGood,
                test_good = testGood,
                test_ng = testNg,
                source_files_are_read_only = true,
                source_target_sha256_verified = true,
                product_align_inspector_compatible = true,
                dataset_layout = "dataset_roi_dino.v1"
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

        // ProductAlignInspector exact-label parser understands missing_ and defect_.
        // The precise defect type (Wrong/Excess/Surface/Other) remains in dataset_manifest.csv.
        return image.DefectType == DefectType.Missing
            ? string.Join("+", ids.Select(id => $"missing_{id}"))
            : string.Join("+", ids.Select(id => $"defect_{id}"));
    }

    private static void ValidateProductAlignCompatibility(string packageRoot, string sourceConfigPath)
    {
        var reference = Path.Combine(packageRoot, "artifacts", "reference", "reference_aligned.png");
        var trainGood = Path.Combine(packageRoot, "dataset_roi_dino", "train", "good");
        var ngRoot = Path.Combine(packageRoot, "dataset_roi_dino", "test", "ng");

        if (!File.Exists(reference))
            throw new InvalidOperationException("ProductAlignInspector 兼容性检查失败：缺少 artifacts/reference/reference_aligned.png。");

        var trainGoodCount = Directory.Exists(trainGood)
            ? Directory.EnumerateFiles(trainGood).Count(IsSupportedImage)
            : 0;
        if (trainGoodCount < 2)
            throw new InvalidOperationException("ProductAlignInspector 兼容性检查失败：train/good 至少需要 2 张 GOOD 原图。");

        var knownRois = ReadEnabledRoiIds(sourceConfigPath);
        if (knownRois.Count == 0)
            throw new InvalidOperationException("ProductAlignInspector 兼容性检查失败：当前没有启用的 ROI。");

        if (!Directory.Exists(ngRoot)) return;

        foreach (var scenarioDirectory in Directory.EnumerateDirectories(ngRoot))
        {
            var scenario = Path.GetFileName(scenarioDirectory);
            foreach (var token in scenario.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string roiId;
                if (token.StartsWith("missing_", StringComparison.OrdinalIgnoreCase))
                    roiId = token["missing_".Length..];
                else if (token.StartsWith("defect_", StringComparison.OrdinalIgnoreCase))
                    roiId = token["defect_".Length..];
                else
                    throw new InvalidOperationException(
                        $"ProductAlignInspector 兼容性检查失败：NG 目录“{scenario}”包含不支持的前缀。");

                if (!knownRois.Contains(roiId))
                    throw new InvalidOperationException(
                        $"ProductAlignInspector 兼容性检查失败：NG 目录“{scenario}”引用不存在或未启用的 ROI “{roiId}”。");
            }
        }
    }

    private static HashSet<string> ReadEnabledRoiIds(string configPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(configPath));
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var propertyName in new[] { "screw_slots", "spring_regions", "anomaly_regions" })
        {
            if (!document.RootElement.TryGetProperty(propertyName, out var array) ||
                array.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var item in array.EnumerateArray())
            {
                var enabled = !item.TryGetProperty("enabled", out var enabledElement) ||
                              enabledElement.ValueKind != JsonValueKind.False;
                if (!enabled || !item.TryGetProperty("id", out var idElement))
                    continue;

                var id = idElement.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                    result.Add(id);
            }
        }
        return result;
    }

    private static bool IsSupportedImage(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tif" or ".tiff";
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
