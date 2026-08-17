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
        var stagingRoot = Path.Combine(targetDirectory, "_datasetstudio_staging", stamp);
        Directory.CreateDirectory(backupRoot);
        Directory.CreateDirectory(stagingRoot);
        var replacementStarted = false;

        try
        {
            // 1) 先把待发布数据复制到目标盘 staging，并逐文件校验 SHA256。
            foreach (var relative in ManagedEntries)
            {
                var source = Path.Combine(packageDirectory, relative);
                var staged = Path.Combine(stagingRoot, relative);
                if (Directory.Exists(source))
                    CopyDirectoryVerified(source, staged);
                else if (File.Exists(source))
                    CopyFileVerified(source, staged);
                else
                    throw new FileNotFoundException($"数据包缺少发布项：{relative}", source);
            }

            // 2) 完整备份 DatasetStudio 管理范围内的现有目标数据。
            foreach (var relative in ManagedEntries)
            {
                var existing = Path.Combine(targetDirectory, relative);
                var backup = Path.Combine(backupRoot, relative);
                if (Directory.Exists(existing))
                    CopyDirectoryVerified(existing, backup);
                else if (File.Exists(existing))
                    CopyFileVerified(existing, backup);
            }

            // 只有 staging + backup 都完整成功后，才允许开始修改目标。
            replacementStarted = true;

            // 3) 替换目标。任何一步失败都会进入 rollback。
            foreach (var relative in ManagedEntries)
            {
                DeleteEntry(Path.Combine(targetDirectory, relative));
                var staged = Path.Combine(stagingRoot, relative);
                var target = Path.Combine(targetDirectory, relative);
                MoveEntry(staged, target);
            }

            // 4) 发布完成后再次用原数据包校验目标。
            foreach (var relative in ManagedEntries)
                VerifyEntry(Path.Combine(packageDirectory, relative), Path.Combine(targetDirectory, relative));

            if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, true);
            return backupRoot;
        }
        catch
        {
            if (replacementStarted)
            {
                foreach (var relative in ManagedEntries)
                {
                    var target = Path.Combine(targetDirectory, relative);
                    DeleteEntry(target);
                    var backup = Path.Combine(backupRoot, relative);
                    if (Directory.Exists(backup))
                        CopyDirectoryVerified(backup, target);
                    else if (File.Exists(backup))
                        CopyFileVerified(backup, target);
                }
            }

            if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, true);
            throw;
        }
    }

    private static void MoveEntry(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        if (Directory.Exists(source))
            Directory.Move(source, target);
        else if (File.Exists(source))
            File.Move(source, target);
        else
            throw new FileNotFoundException($"staging 发布项不存在：{source}", source);
    }

    private static void DeleteEntry(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
        else if (File.Exists(path)) File.Delete(path);
    }

    private static void CopyDirectoryVerified(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            CopyFileVerified(file, Path.Combine(target, relative));
        }
    }

    private static void CopyFileVerified(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, true);
        var sourceSha = CatalogRepository.ComputeSha256(source);
        var targetSha = CatalogRepository.ComputeSha256(target);
        if (!string.Equals(sourceSha, targetSha, StringComparison.OrdinalIgnoreCase))
            throw new IOException($"发布 SHA256 校验失败：{source}");
    }

    private static void VerifyEntry(string source, string target)
    {
        if (File.Exists(source))
        {
            if (!File.Exists(target)) throw new FileNotFoundException("发布目标文件缺失。", target);
            var sourceSha = CatalogRepository.ComputeSha256(source);
            var targetSha = CatalogRepository.ComputeSha256(target);
            if (!string.Equals(sourceSha, targetSha, StringComparison.OrdinalIgnoreCase))
                throw new IOException($"发布后 SHA256 不一致：{target}");
            return;
        }

        if (!Directory.Exists(source) || !Directory.Exists(target))
            throw new DirectoryNotFoundException($"发布目标目录缺失：{target}");

        var sourceFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(source, file))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var targetFiles = Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(target, file))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (!sourceFiles.SequenceEqual(targetFiles, StringComparer.OrdinalIgnoreCase))
            throw new IOException($"发布后目录文件清单不一致：{target}");

        foreach (var relative in sourceFiles)
        {
            var sourceSha = CatalogRepository.ComputeSha256(Path.Combine(source, relative));
            var targetSha = CatalogRepository.ComputeSha256(Path.Combine(target, relative));
            if (!string.Equals(sourceSha, targetSha, StringComparison.OrdinalIgnoreCase))
                throw new IOException($"发布后 SHA256 不一致：{Path.Combine(target, relative)}");
        }
    }
}
