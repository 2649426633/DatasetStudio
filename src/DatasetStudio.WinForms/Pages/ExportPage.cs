using DatasetStudio.Infrastructure;

namespace DatasetStudio.WinForms.Pages;

public sealed class ExportPage : UserControl
{
    private readonly Label _projectPath = new();
    private readonly Label _counts = new();
    private readonly Label _lastPackage = new();
    private readonly TextBox _publishTarget = new();
    private AppSession? _session;
    private string? _lastPackageDirectory;

    public ExportPage()
    {
        BackColor = UiTheme.WindowBackground;
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
            _counts.Text = string.Empty;
            return;
        }
        var counts = _session.Repository.GetCounts();
        _projectPath.Text = $"项目目录：{_session.ProjectDirectory}\n导出目录：{Path.Combine(_session.ProjectDirectory, "exports")}";
        _counts.Text = $"Train GOOD    {counts.TrainGood}\nTest GOOD     {counts.TestGood}\nTest NG       {counts.TestNg}\n未分类         {counts.Unclassified}";
    }

    private void BuildLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(24) };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var title = UiTheme.CreateSectionTitle("导出 / 发布 ProductAlignPackage");
        UiTheme.AddRow(layout, title, SizeType.AutoSize, 0, new Padding(0, 0, 0, 10));

        _projectPath.AutoSize = false;
        _projectPath.Dock = DockStyle.Fill;
        _projectPath.ForeColor = UiTheme.TextSecondary;
        UiTheme.AddRow(layout, _projectPath, SizeType.Absolute, 52, new Padding(0, 0, 0, 14));

        UiTheme.AddRow(layout, BuildGeneratedCountsPanel(), SizeType.Percent, 100F, Padding.Empty);

        UiTheme.AddRow(layout, BuildActionButtonsPanel(), SizeType.Absolute, 44, new Padding(0, 12, 0, 12));

        _lastPackage.AutoSize = false;
        _lastPackage.Dock = DockStyle.Fill;
        _lastPackage.ForeColor = UiTheme.TextSecondary;
        _lastPackage.Text = "尚未生成本次数据包";
        UiTheme.AddRow(layout, _lastPackage, SizeType.Absolute, 52, new Padding(0, 0, 0, 14));

        var publishTitle = UiTheme.CreateFieldLabel("发布到 ProductAlignInspector 目标目录");
        UiTheme.AddRow(layout, publishTitle, SizeType.AutoSize, 0, new Padding(0, 0, 0, 6));

        UiTheme.AddRow(layout, BuildPublishPanel(), SizeType.Absolute, 36);

        var safety = UiTheme.CreateMutedText("安全策略：源图片永不删除/移动/重命名；生成与发布都先进入 staging；复制文件逐个做 SHA-256 校验。发布前备份 DatasetStudio 管理的目标项，失败会尝试自动回滚。");
        safety.Dock = DockStyle.Fill;
        safety.TextAlign = ContentAlignment.TopLeft;
        UiTheme.AddRow(layout, safety, SizeType.Absolute, 60, new Padding(0, 14, 0, 0));

        panel.Controls.Add(layout);
        Controls.Add(panel);
    }

    private Control BuildGeneratedCountsPanel()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

        var generatedTitle = UiTheme.CreateFieldLabel("即将生成");
        var generated = new Label
        {
            Text = "✅ configs\\<product>.json\n✅ artifacts\\reference\\reference_aligned.png\n✅ dataset_roi_dino\\train\\good\n✅ dataset_roi_dino\\test\\good\n✅ dataset_roi_dino\\test\\ng\n✅ dataset_manifest.csv\n✅ dataset_report.json",
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10.5F),
            ForeColor = UiTheme.TextPrimary,
            TextAlign = ContentAlignment.TopLeft
        };

        var countsTitle = UiTheme.CreateFieldLabel("数据统计");
        _counts.Dock = DockStyle.Fill;
        _counts.Font = new Font("Consolas", 10.5F);
        _counts.ForeColor = UiTheme.TextPrimary;
        _counts.TextAlign = ContentAlignment.TopLeft;

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty, BackColor = UiTheme.Surface };
        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        left.Controls.Add(generatedTitle, 0, 0);
        left.Controls.Add(generated, 0, 1);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(32, 0, 0, 0), BackColor = UiTheme.Surface };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        right.Controls.Add(countsTitle, 0, 0);
        right.Controls.Add(_counts, 0, 1);

        table.Controls.Add(left, 0, 0);
        table.Controls.Add(right, 1, 0);
        return table;
    }

    private Control BuildActionButtonsPanel()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var validate = UiTheme.CreateButton("校验数据");
        validate.Size = new Size(130, 36);
        validate.Anchor = AnchorStyles.Left;
        validate.Click += (_, _) => ValidateOnly();
        var generate = UiTheme.CreateButton("生成数据包", true);
        generate.Size = new Size(150, 36);
        generate.Anchor = AnchorStyles.Left;
        generate.Margin = new Padding(12, 0, 0, 0);
        generate.Click += (_, _) => GeneratePackage();

        table.Controls.Add(validate, 0, 0);
        table.Controls.Add(generate, 1, 0);
        return table;
    }

    private Control BuildPublishPanel()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));

        _publishTarget.PlaceholderText = @"例如 D:\Brunei";
        _publishTarget.BorderStyle = BorderStyle.FixedSingle;
        _publishTarget.BackColor = UiTheme.Surface;
        _publishTarget.ForeColor = UiTheme.TextPrimary;
        _publishTarget.Font = new Font("Microsoft YaHei UI", 10F);
        _publishTarget.AutoSize = false;
        _publishTarget.Dock = DockStyle.Fill;
        _publishTarget.Margin = new Padding(0, 0, 8, 0);

        var browse = UiTheme.CreateButton("浏览");
        browse.Dock = DockStyle.Fill;
        browse.Margin = new Padding(0, 0, 8, 0);
        browse.Click += (_, _) => BrowsePublishTarget();
        var publish = UiTheme.CreateButton("安全发布", false);
        publish.Dock = DockStyle.Fill;
        publish.Click += (_, _) => PublishPackage();

        table.Controls.Add(_publishTarget, 0, 0);
        table.Controls.Add(browse, 1, 0);
        table.Controls.Add(publish, 2, 0);
        return table;
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
}
