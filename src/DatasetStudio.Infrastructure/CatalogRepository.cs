using System.Security.Cryptography;
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
            """;
        command.ExecuteNonQuery();
    }

    public int ScanSourceDirectory(string sourceDirectory)
    {
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".bmp", ".png", ".jpg", ".jpeg", ".tif", ".tiff"
        };

        var files = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(path => extensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var inserted = 0;

        foreach (var file in files)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT OR IGNORE INTO Images (SourcePath, FileName, Sha256)
                VALUES ($path, $name, $sha);
                """;
            command.Parameters.AddWithValue("$path", Path.GetFullPath(file));
            command.Parameters.AddWithValue("$name", Path.GetFileName(file));
            command.Parameters.AddWithValue("$sha", ComputeSha256(file));
            inserted += command.ExecuteNonQuery();
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
        var old = LoadImage(imageId)?.StatusText ?? string.Empty;
        var roiText = string.Join('|', defectRois.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());

        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE Images
                SET Split=$split, Truth=$truth, DefectType=$type, DefectRois=$rois, Note=$note
                WHERE Id=$id;
                """;
            command.Parameters.AddWithValue("$split", (int)split);
            command.Parameters.AddWithValue("$truth", (int)truth);
            command.Parameters.AddWithValue("$type", (int)defectType);
            command.Parameters.AddWithValue("$rois", roiText);
            command.Parameters.AddWithValue("$note", note ?? string.Empty);
            command.Parameters.AddWithValue("$id", imageId);
            command.ExecuteNonQuery();
        }

        using (var log = connection.CreateCommand())
        {
            log.Transaction = transaction;
            log.CommandText = """
                INSERT INTO Operations(Time, Operation, ImageId, OldValue, NewValue)
                VALUES($time, 'Classify', $id, $old, $new);
                """;
            log.Parameters.AddWithValue("$time", DateTimeOffset.Now.ToString("O"));
            log.Parameters.AddWithValue("$id", imageId);
            log.Parameters.AddWithValue("$old", old);
            log.Parameters.AddWithValue("$new", $"{split}/{truth}/{defectType}/{roiText}");
            log.ExecuteNonQuery();
        }
        transaction.Commit();
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
        command.Parameters.AddWithValue("$id", roi.Id);
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

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
