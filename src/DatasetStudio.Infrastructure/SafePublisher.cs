namespace DatasetStudio.Infrastructure;

public sealed class SafePublisher
{
    private static readonly string[] ManagedEntries =
    {
        "configs",
        Path.Combine("artifacts", "reference"),
        "dataset_roi_dino",
        "dataset_manifest.csv",
        "dataset_report.json"
    };

    public string Publish(string packageDirectory, string targetDirectory)
    {
        if (!Directory.Exists(packageDirectory))
            throw new DirectoryNotFoundException(packageDirectory);

        Directory.CreateDirectory(targetDirectory);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupRoot = Path.Combine(targetDirectory, "_datasetstudio_backups", stamp);
        Directory.CreateDirectory(backupRoot);

        foreach (var relative in ManagedEntries)
        {
            var existing = Path.Combine(targetDirectory, relative);
            if (Directory.Exists(existing))
                CopyDirectory(existing, Path.Combine(backupRoot, relative), true);
            else if (File.Exists(existing))
            {
                var backupFile = Path.Combine(backupRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
                File.Copy(existing, backupFile, true);
            }
        }

        foreach (var relative in ManagedEntries)
        {
            var source = Path.Combine(packageDirectory, relative);
            var target = Path.Combine(targetDirectory, relative);
            if (Directory.Exists(source))
                CopyDirectory(source, target, true);
            else if (File.Exists(source))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target, true);
            }
        }

        return backupRoot;
    }

    private static void CopyDirectory(string source, string target, bool overwrite)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            var destination = Path.Combine(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite);
        }
    }
}
