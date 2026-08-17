using DatasetStudio.Infrastructure;

namespace DatasetStudio.WinForms.Pages;

public sealed partial class ExportPage : UserControl
{
    private AppSession? _session;
    private string? _lastPackageDirectory;

    public ExportPage()
    {
        InitializeComponent();
    }

    public void BindSession(AppSession session)
    {
        _session = session;
        _lastPackageDirectory = null;
        RefreshSummary();
    }

    public void RefreshSummary()
    {
        if (_session is null)
        {
            _projectPath.Text = "项目：未打开";
            foreach (var label in new[] { _trainGoodCount, _testGoodCount, _testNgCount, _ignoredCount }) label.Text = "0 张";
            return;
        }
        var counts = _session.Repository.GetCounts();
        _projectPath.Text = $"项目目录：{_session.ProjectDirectory}\n导出目录：{Path.Combine(_session.ProjectDirectory, "exports")}";
        _trainGoodCount.Text = $"{counts.TrainGood} 张";
        _testGoodCount.Text = $"{counts.TestGood} 张";
        _testNgCount.Text = $"{counts.TestNg} 张";
        _ignoredCount.Text = $"{counts.Ignored} 张";
    }

    private void ValidateButton_Click(object? sender, EventArgs e) => ValidateOnly();
    private void GenerateButton_Click(object? sender, EventArgs e) => GeneratePackage();
    private void BrowseButton_Click(object? sender, EventArgs e) => BrowsePublishTarget();
    private void PublishButton_Click(object? sender, EventArgs e) => PublishPackage();

    private void ValidateOnly()
    {
        if (_session is null) return;
        var size = _session.ReferenceImageSize;
        var validator = new DatasetValidator();
        var ok = validator.CanExport(_session.Repository, out var items, size.Width, size.Height);
        var message = string.Join(Environment.NewLine,
            items.Select(x => $"{(x.Severity == DatasetStudio.Core.ValidationSeverity.Ok ? "✅" : x.Severity == DatasetStudio.Core.ValidationSeverity.Warning ? "⚠" : "❌")} {x.Name}: {x.Value} - {x.Message}"));
        MessageBox.Show(this, message, ok ? "校验通过" : "校验失败", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void GeneratePackage()
    {
        if (_session is null) return;
        try
        {
            _session.WriteProductConfig();
            var result = new DatasetExporter().Export(_session.ProjectDirectory, _session.Project, _session.Repository);
            _lastPackageDirectory = result.PackageDirectory;
            _lastPackage.Text = $"已生成：{result.PackageDirectory}\nManifest：{result.ManifestPath}";
            RefreshSummary();
            MessageBox.Show(this, $"数据包生成完成。\n{result.PackageDirectory}", "Dataset Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BrowsePublishTarget()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择 ProductAlignInspector 项目目录，例如 D:\\Brunei",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            _publishTarget.Text = dialog.SelectedPath;
    }

    private void PublishPackage()
    {
        if (_session is null) return;
        if (string.IsNullOrWhiteSpace(_lastPackageDirectory) || !Directory.Exists(_lastPackageDirectory))
        {
            MessageBox.Show(this, "请先点击“生成数据包”。", "发布", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(_publishTarget.Text))
        {
            MessageBox.Show(this, "请选择发布目标目录。", "发布", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var target = _publishTarget.Text.Trim();
        if (MessageBox.Show(this,
                $"即将安全发布到：\n{target}\n\n现有 DatasetStudio 管理数据会先备份，再通过 staging + SHA256 校验替换。继续吗？",
                "确认发布", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            var backup = new SafePublisher().Publish(_lastPackageDirectory, target);
            MessageBox.Show(this, $"发布完成。\n备份目录：{backup}", "Dataset Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "发布失败 / 已尝试回滚", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
