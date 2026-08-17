using System.Security.Cryptography;
using System.Text.Json;
using DatasetStudio.Core;
using Microsoft.Data.Sqlite;

namespace DatasetStudio.Infrastructure;

public sealed class CatalogRepository
{
    private readonly string _connectionString;

    public CatalogRepository(string projectDirectory)
    {
        Directory.CreateDirectory(projectDirectory);
        var dbPath = Path.Combine(projectDirectory, "catalog.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    public void Initialize()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA foreign_keys=ON;
            PRAGMA journal_mode=WAL;

            CREATE TABLE IF NOT EXISTS Images (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SourcePath TEXT NOT NULL UNIQUE,
                FileName TEXT NOT NULL,
                Sha256 TEXT NOT NULL,
                Width INTEGER NOT NULL DEFAULT 0,
                Height INTEGER NOT NULL DEFAULT 0,
                Split INTEGER NOT NULL DEFAULT 0,
                Truth INTEGER NOT NULL DEFAULT 0,
                DefectType INTEGER NOT NULL DEFAULT 0,
                DefectRois TEXT NOT NULL DEFAULT '',
                Note TEXT NOT NULL DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS Rois (
                RowId INTEGER PRIMARY KEY AUTOINCREMENT,
                Id TEXT NOT NULL UNIQUE,
                Kind INTEGER NOT NULL,
                X INTEGER NOT NULL,
                Y INTEGER NOT NULL,
                Width INTEGER NOT NULL,
                Height INTEGER NOT NULL,
                Expected TEXT NOT NULL DEFAULT '',
                ExpectedCount INTEGER NULL,
                Enabled INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Operations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Time TEXT NOT NULL,
                Operation TEXT NOT NULL,
                ImageId INTEGER NULL,
                OldValue TEXT NOT NULL DEFAULT '',
                NewValue TEXT NOT NULL DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS IX_Images_Sha256 ON Images(Sha256);
            CREATE INDEX IF NOT EXISTS IX_Images_SplitTruth ON Images(Split, Truth);
            """;
        command.ExecuteNonQuery();
    }

    public int ScanSourceDirectory(string sourceDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException(sourceDirectory);

        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".bmp", ".png", ".jpg", ".jpeg", ".tif", ".tiff"
        };
        var files = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(path => extensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        using var connection = Open();
        var existing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using (var read = connection.CreateCommand())
        {
            read.CommandText = "SELECT SourcePath, Sha256 FROM Images;";
            using var reader = read.ExecuteReader();
            while (reader.Read())
                existing[reader.GetString(0)] = reader.GetString(1);
        }

        using var transaction = connection.BeginTransaction();
        var inserted = 0;
        foreach (var file in files)
        {
            var fullPath = Path.GetFullPath(file);
            var sha = ComputeSha256(fullPath);
            existing.TryGetValue(fullPath, out var oldSha);

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            if (oldSha is null)
            {
                command.CommandText = """
                    INSERT INTO Images (SourcePath, FileName, Sha256)
                    VALUES ($path, $name, $sha);
                    """;
                inserted++;
            }
            else if (!string.Equals(oldSha, sha, StringComparison.OrdinalIgnoreCase))
            {
                // 同一路径文件内容发生变化时，旧标签不能继续沿用。
                command.CommandText = """
                    UPDATE Images
                    SET FileName=$name, Sha256=$sha, Width=0, Height=0,
                        Split=0, Truth=0, DefectType=0, DefectRois=''
                    WHERE SourcePath=$path;
                    """;
            }
            else
            {
                command.CommandText = "UPDATE Images SET FileName=$name WHERE SourcePath=$path;";
            }

            command.Parameters.AddWithValue("$path", fullPath);
            command.Parameters.AddWithValue("$name", Path.GetFileName(fullPath));
            command.Parameters.AddWithValue("$sha", sha);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
        return inserted;
    }

    public List<ImageRecord> LoadImages()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, SourcePath, FileName, Sha256, Width, Height,
                   Split, Truth, DefectType, DefectRois, Note
            FROM Images
            ORDER BY FileName COLLATE NOCASE;
            """;
        using var reader = command.ExecuteReader();
        var result = new List<ImageRecord>();
        while (reader.Read())
        {
            result.Add(new ImageRecord
            {
                Id = reader.GetInt64(0),
                SourcePath = reader.GetString(1),
                FileName = reader.GetString(2),
                Sha256 = reader.GetString(3),
                Width = reader.GetInt32(4),
                Height = reader.GetInt32(5),
                Split = (DatasetSplit)reader.GetInt32(6),
                Truth = (ImageTruth)reader.GetInt32(7),
                DefectType = (DefectType)reader.GetInt32(8),
                DefectRois = reader.GetString(9),
                Note = reader.GetString(10)
            });
        }
        return result;
    }

    public void UpdateImageDimensions(long imageId, int width, int height)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Images SET Width=$w, Height=$h WHERE Id=$id;";
        command.Parameters.AddWithValue("$w", width);
        command.Parameters.AddWithValue("$h", height);
        command.Parameters.AddWithValue("$id", imageId);
        command.ExecuteNonQuery();
    }

    public void UpdateClassification(
        long imageId,
        DatasetSplit split,
        ImageTruth truth,
        DefectType defectType,
        IEnumerable<string> defectRois,
        string note)
    {
        var oldImage = LoadImage(imageId) ?? throw new InvalidOperationException($"找不到图片记录: {imageId}");
        var roiText = string.Join("|",
            defectRois
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

        var oldSnapshot = ClassificationSnapshot.From(oldImage);
        var newSnapshot = new ClassificationSnapshot
        {
            Split = split,
            Truth = truth,
            DefectType = defectType,
            DefectRois = roiText,
            Note = note ?? string.Empty
        };

        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        ApplyClassification(connection, transaction, imageId, newSnapshot);

        using var log = connection.CreateCommand();
        log.Transaction = transaction;
        log.CommandText = """
            INSERT INTO Operations(Time, Operation, ImageId, OldValue, NewValue)
            VALUES($time, 'Classify', $id, $old, $new);
            """;
        log.Parameters.AddWithValue("$time", DateTimeOffset.Now.ToString("O"));
        log.Parameters.AddWithValue("$id", imageId);
        log.Parameters.AddWithValue("$old", JsonSerializer.Serialize(oldSnapshot));
        log.Parameters.AddWithValue("$new", JsonSerializer.Serialize(newSnapshot));
        log.ExecuteNonQuery();
        transaction.Commit();
    }

    public long? UndoLastClassification()
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();

        long operationId;
        long imageId;
        string oldJson;
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT Id, ImageId, OldValue
                FROM Operations
                WHERE Operation='Classify' AND ImageId IS NOT NULL
                ORDER BY Id DESC
                LIMIT 1;
                """;
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            operationId = reader.GetInt64(0);
            imageId = reader.GetInt64(1);
            oldJson = reader.GetString(2);
        }

        ClassificationSnapshot? snapshot;
        try
        {
            snapshot = JsonSerializer.Deserialize<ClassificationSnapshot>(oldJson);
        }
        catch (JsonException)
        {
            // 兼容最早版本仅记录状态文字的 Operations；这类历史记录无法无损回滚。
            return null;
        }
        if (snapshot is null) return null;

        ApplyClassification(connection, transaction, imageId, snapshot);
        using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM Operations WHERE Id=$id;";
            delete.Parameters.AddWithValue("$id", operationId);
            delete.ExecuteNonQuery();
        }
        transaction.Commit();
        return imageId;
    }

    public List<RoiDefinition> LoadRois()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT RowId, Id, Kind, X, Y, Width, Height, Expected, ExpectedCount, Enabled
            FROM Rois ORDER BY Id COLLATE NOCASE;
            """;
        using var reader = command.ExecuteReader();
        var result = new List<RoiDefinition>();
        while (reader.Read())
        {
            result.Add(new RoiDefinition
            {
                RowId = reader.GetInt64(0),
                Id = reader.GetString(1),
                Kind = (RoiKind)reader.GetInt32(2),
                X = reader.GetInt32(3),
                Y = reader.GetInt32(4),
                Width = reader.GetInt32(5),
                Height = reader.GetInt32(6),
                Expected = reader.GetString(7),
                ExpectedCount = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                Enabled = reader.GetInt32(9) != 0
            });
        }
        return result;
    }

    public void SaveRoi(RoiDefinition roi)
    {
        if (string.IsNullOrWhiteSpace(roi.Id))
            throw new ArgumentException("ROI ID 不能为空。", nameof(roi));
        if (roi.Width <= 0 || roi.Height <= 0)
            throw new ArgumentException("ROI 宽高必须大于 0。", nameof(roi));

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Rois(Id, Kind, X, Y, Width, Height, Expected, ExpectedCount, Enabled)
            VALUES($id,$kind,$x,$y,$w,$h,$expected,$count,$enabled)
            ON CONFLICT(Id) DO UPDATE SET
                Kind=excluded.Kind, X=excluded.X, Y=excluded.Y,
                Width=excluded.Width, Height=excluded.Height,
                Expected=excluded.Expected, ExpectedCount=excluded.ExpectedCount,
                Enabled=excluded.Enabled;
            """;
        command.Parameters.AddWithValue("$id", roi.Id.Trim());
        command.Parameters.AddWithValue("$kind", (int)roi.Kind);
        command.Parameters.AddWithValue("$x", roi.X);
        command.Parameters.AddWithValue("$y", roi.Y);
        command.Parameters.AddWithValue("$w", roi.Width);
        command.Parameters.AddWithValue("$h", roi.Height);
        command.Parameters.AddWithValue("$expected", roi.Expected ?? string.Empty);
        command.Parameters.AddWithValue("$count", (object?)roi.ExpectedCount ?? DBNull.Value);
        command.Parameters.AddWithValue("$enabled", roi.Enabled ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void DeleteRoi(string id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Rois WHERE Id=$id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public DatasetCounts GetCounts()
    {
        var images = LoadImages();
        return new DatasetCounts(
            images.Count,
            images.Count(x => x.IsClassified),
            images.Count(x => x.Split == DatasetSplit.Train && x.Truth == ImageTruth.Good),
            images.Count(x => x.Split == DatasetSplit.Test && x.Truth == ImageTruth.Good),
            images.Count(x => x.Split == DatasetSplit.Test && x.Truth == ImageTruth.Ng),
            images.Count(x => x.Split == DatasetSplit.Ignore || x.Truth == ImageTruth.Ignore),
            images.Count(x => !x.IsClassified));
    }

    public List<IGrouping<string, ImageRecord>> GetDuplicateHashGroups() =>
        LoadImages()
            .Where(x => !string.IsNullOrWhiteSpace(x.Sha256))
            .GroupBy(x => x.Sha256, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .ToList();

    public static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private ImageRecord? LoadImage(long imageId) => LoadImages().FirstOrDefault(x => x.Id == imageId);

    private static void ApplyClassification(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long imageId,
        ClassificationSnapshot snapshot)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Images
            SET Split=$split, Truth=$truth, DefectType=$type, DefectRois=$rois, Note=$note
            WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$split", (int)snapshot.Split);
        command.Parameters.AddWithValue("$truth", (int)snapshot.Truth);
        command.Parameters.AddWithValue("$type", (int)snapshot.DefectType);
        command.Parameters.AddWithValue("$rois", snapshot.DefectRois ?? string.Empty);
        command.Parameters.AddWithValue("$note", snapshot.Note ?? string.Empty);
        command.Parameters.AddWithValue("$id", imageId);
        command.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private sealed class ClassificationSnapshot
    {
        public DatasetSplit Split { get; set; }
        public ImageTruth Truth { get; set; }
        public DefectType DefectType { get; set; }
        public string DefectRois { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        public static ClassificationSnapshot From(ImageRecord image) => new()
        {
            Split = image.Split,
            Truth = image.Truth,
            DefectType = image.DefectType,
            DefectRois = image.DefectRois,
            Note = image.Note
        };
    }
}
