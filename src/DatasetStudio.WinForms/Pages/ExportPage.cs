using DatasetStudio.Infrastructure;

namespace DatasetStudio.WinForms.Pages;

public sealed class ExportPage : UserControl
{
    private readonly Label _projectPath = new();
    private readonly Label _generated = new();
    private readonly Label _counts = new();
    private readonly Label _lastPackage = new();
    private readonly TextBox _publishTarget = new();
    private AppSession? _session;
    private string? _lastPackageDirectory;

    public ExportPage()
    {
        BackColor = UiTheme.WindowBackground;
        Font = new Font("Microsoft YaHei UI", 10F);
        BuildLayout();
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
            _generated.Text = string.Empty;
            _counts.Text = string.Empty;
            return;
        }

        var categories = _session.Project.Categories;
        var counts = _session.Repository.GetCounts();
        _projectPath.Text = $"项目目录：{_session.ProjectDirectory}\n导出目录：{Path.Combine(_session.ProjectDirectory, "exports")}";
        _generated.Text =
            "✅ configs\\<product>.json\n" +
            "✅ artifacts\\reference\\reference_aligned.png\n" +
            $"✅ dataset_roi_dino\\{DisplayDirectory(categories.TrainGoodDirectory)}\n" +
            $"✅ dataset_roi_dino\\{DisplayDirectory(categories.TestGoodDirectory)}\n" +
            $"✅ dataset_roi_dino\\{DisplayDirectory(categories.TestNgDirectory)}\n" +
            "✅ dataset_manifest.csv\n" +
            "✅ dataset_report.json";
        _counts.Text =
            $"{categories.TrainGoodLabel}    {counts.TrainGood}\n" +
            $"{categories.TestGoodLabel}     {counts.TestGood}\n" +
            $"{categories.TestNgLabel}       {counts.TestNg}\n" +
            $"未分类         {counts.Unclassified}";
    }

    private void BuildLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(24) };
        var title = UiTheme.CreateSectionTitle("导出 / 发布 ProductAlignPackage");
        title.Location = new Point(24, 22);

        _projectPath.Location = new Point(24, 58);
        _projectPath.Size = new Size(980, 56);
        _projectPath.ForeColor = UiTheme.TextSecondary;
        _projectPath.Font = new Font("Microsoft YaHei UI", 10F);

        var generatedTitle = new Label
        {
            Text = "即将生成（类别目录可在“数据集分类 → 类别 / 目录设置”中修改）",
            Location = new Point(24, 128),
            AutoSize = true,
            ForeColor = UiTheme.TextSecondary,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        _generated.Location = new Point(24, 158);
        _generated.Size = new Size(590, 180);
        _generated.Font = new Font("Consolas", 10.5F);
        _generated.ForeColor = UiTheme.TextPrimary;

        _counts.Location = new Point(640, 158);
        _counts.Size = new Size(340, 150);
        _counts.Font = new Font("Microsoft YaHei UI", 10.5F);
        _counts.ForeColor = UiTheme.TextPrimary;

        var validate = UiTheme.CreateButton("校验数据");
        validate.Location = new Point(24, 354);
        validate.Size = new Size(130, 38);
        validate.Font = new Font("Microsoft YaHei UI", 10F);
        validate.Click += (_, _) => ValidateOnly();

        var generate = UiTheme.CreateButton("生成数据包", true);
        generate.Location = new Point(166, 354);
        generate.Size = new Size(150, 38);
        generate.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        generate.Click += (_, _) => GeneratePackage();

        _lastPackage.Location = new Point(24, 410);
        _lastPackage.Size = new Size(980, 58);
        _lastPackage.ForeColor = UiTheme.TextSecondary;
        _lastPackage.Font = new Font("Microsoft YaHei UI", 10F);
        _lastPackage.Text = "尚未生成本次数据包";

        var publishTitle = new Label
        {
            Text = "发布到 ProductAlignInspector 目标目录",
            Location = new Point(24, 488),
            AutoSize = true,
            ForeColor = UiTheme.TextSecondary,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        _publishTarget.Location = new Point(24, 516);
        _publishTarget.Size = new Size(650, 32);
        _publishTarget.Font = new Font("Microsoft YaHei UI", 10F);
        _publishTarget.PlaceholderText = @"例如 D:\Brunei";

        var browse = UiTheme.CreateButton("浏览");
        browse.Location = new Point(686, 514);
        browse.Size = new Size(88, 34);
        browse.Click += (_, _) => BrowsePublishTarget();

        var publish = UiTheme.CreateButton("安全发布", false);
        publish.Location = new Point(786, 514);
        publish.Size = new Size(110, 34);
        publish.Click += (_, _) => PublishPackage();

        var safety = new Label
        {
            Text = "安全策略：源图片永不删除/移动/重命名；生成与发布都先进入 staging；复制文件逐个做 SHA-256 校验。发布前备份 DatasetStudio 管理的目标项，失败会尝试自动回滚。",
            Location = new Point(24, 574),
            Size = new Size(980, 66),
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Microsoft YaHei UI", 9.5F)
        };

        panel.Controls.AddRange(new Control[]
        {
            title, _projectPath, generatedTitle, _generated, _counts,
            validate, generate, _lastPackage,
            publishTitle, _publishTarget, browse, publish, safety
        });
        Controls.Add(panel);
    }

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

    private static string DisplayDirectory(string path) =>
        path.Replace('/', '\\').Trim('\\');
}
