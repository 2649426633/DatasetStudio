using System.Text.Json.Serialization;

namespace DatasetStudio.Core;

public enum DatasetSplit
{
    Unclassified = 0,
    Train = 1,
    Test = 2,
    Ignore = 3
}

public enum ImageTruth
{
    Unclassified = 0,
    Good = 1,
    Ng = 2,
    Ignore = 3
}

public enum DefectType
{
    None = 0,
    Missing = 1,
    Excess = 2,
    Wrong = 3,
    Surface = 4,
    Other = 5
}

public enum RoiKind
{
    ScrewSlot = 1,
    EmptySlot = 2,
    SpringRegion = 3,
    AnomalyRegion = 4,
    CustomRegion = 5
}

public enum ValidationSeverity
{
    Ok,
    Warning,
    Error
}

public sealed class DatasetProject
{
    [JsonPropertyName("schema_version")]
    public int SchemaVersion { get; set; } = 1;

    [JsonPropertyName("project")]
    public string Name { get; set; } = "NewProject";

    [JsonPropertyName("source_directory")]
    public string SourceDirectory { get; set; } = string.Empty;

    [JsonPropertyName("reference_image")]
    public string ReferenceImage { get; set; } = "reference\\reference_aligned.png";

    [JsonPropertyName("product_config")]
    public string ProductConfig { get; set; } = "configs\\product.json";
}

public sealed class ImageRecord
{
    public long Id { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DatasetSplit Split { get; set; }
    public ImageTruth Truth { get; set; }
    public DefectType DefectType { get; set; }
    public string DefectRois { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public bool IsClassified => Split != DatasetSplit.Unclassified && Truth != ImageTruth.Unclassified;

    public string StatusText => (Split, Truth) switch
    {
        (DatasetSplit.Train, ImageTruth.Good) => "Train GOOD",
        (DatasetSplit.Test, ImageTruth.Good) => "Test GOOD",
        (DatasetSplit.Test, ImageTruth.Ng) => "Test NG",
        (DatasetSplit.Ignore, _) => "Ignore",
        _ => "未分类"
    };

    public IReadOnlyList<string> GetDefectRoiIds() =>
        string.IsNullOrWhiteSpace(DefectRois)
            ? Array.Empty<string>()
            : DefectRois.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public sealed class RoiDefinition
{
    public long RowId { get; set; }
    public string Id { get; set; } = string.Empty;
    public RoiKind Kind { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Expected { get; set; } = string.Empty;
    public int? ExpectedCount { get; set; }
    public bool Enabled { get; set; } = true;

    public override string ToString() => Id;
}

public sealed record ValidationItem(
    string Name,
    int Value,
    ValidationSeverity Severity,
    string Message);

public sealed record DatasetCounts(
    int Total,
    int Classified,
    int TrainGood,
    int TestGood,
    int TestNg,
    int Ignored,
    int Unclassified);

public sealed class ExportResult
{
    public required string PackageDirectory { get; init; }
    public required string ManifestPath { get; init; }
    public int TrainGood { get; init; }
    public int TestGood { get; init; }
    public int TestNg { get; init; }
}
