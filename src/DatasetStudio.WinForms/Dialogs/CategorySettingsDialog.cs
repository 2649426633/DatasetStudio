using DatasetStudio.Core;

namespace DatasetStudio.WinForms.Dialogs;

public sealed class CategorySettingsDialog : Form
{
    private readonly TextBox _trainGoodLabel = new();
    private readonly TextBox _testGoodLabel = new();
    private readonly TextBox _testNgLabel = new();
    private readonly TextBox _ignoreLabel = new();
    private readonly TextBox _trainGoodDirectory = new();
    private readonly TextBox _testGoodDirectory = new();
    private readonly TextBox _testNgDirectory = new();

    public DatasetCategoryOptions Result { get; private set; }

    public CategorySettingsDialog(DatasetCategoryOptions current)
    {
        Result = Clone(current);
        Text = "类别 / 目录设置";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(660, 500);
        MinimumSize = new Size(660, 500);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = UiTheme.WindowBackground;
        Font = new Font("Microsoft YaHei UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildLayout();
        LoadValues();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20),
            BackColor = UiTheme.WindowBackground
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

        var intro = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(600, 0),
            Text = "分类内部状态保持不变，只修改界面显示名称和导出目录。目录均相对于 dataset_roi_dino，不能使用绝对路径或 ..。",
            ForeColor = UiTheme.TextSecondary,
            Margin = new Padding(0, 0, 0, 14)
        };
        root.Controls.Add(intro, 0, 0);

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            BackColor = UiTheme.Surface,
            Padding = new Padding(18),
            Margin = Padding.Empty
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        AddField(fields, 0, "Train GOOD 显示名", _trainGoodLabel);
        AddField(fields, 1, "Train GOOD 目录", _trainGoodDirectory);
        AddField(fields, 2, "Test GOOD 显示名", _testGoodLabel);
        AddField(fields, 3, "Test GOOD 目录", _testGoodDirectory);
        AddField(fields, 4, "Test NG 显示名", _testNgLabel);
        AddField(fields, 5, "Test NG 目录", _testNgDirectory);
        AddField(fields, 6, "Ignore 显示名", _ignoreLabel);

        var tip = new Label
        {
            AutoSize = true,
            Text = @"示例：train\good、test\good、test\ng，也可以改成 train\ok、verify\normal、verify\defect。",
            ForeColor = UiTheme.TextMuted,
            Margin = new Padding(3, 8, 3, 0)
        };
        fields.Controls.Add(tip, 0, 7);
        fields.SetColumnSpan(tip, 2);
        root.Controls.Add(fields, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0),
            WrapContents = false
        };
        var save = UiTheme.CreateButton("保存", true);
        save.Size = new Size(100, 34);
        save.Click += (_, _) => SaveAndClose();
        var cancel = UiTheme.CreateButton("取消");
        cancel.Size = new Size(100, 34);
        cancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 2);

        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel table, int row, string labelText, TextBox editor)
    {
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = UiTheme.TextSecondary
        };
        editor.Dock = DockStyle.Fill;
        editor.Margin = new Padding(3, 6, 3, 6);
        editor.Font = new Font("Microsoft YaHei UI", 10.5F);
        table.Controls.Add(label, 0, row);
        table.Controls.Add(editor, 1, row);
    }

    private void LoadValues()
    {
        _trainGoodLabel.Text = Result.TrainGoodLabel;
        _testGoodLabel.Text = Result.TestGoodLabel;
        _testNgLabel.Text = Result.TestNgLabel;
        _ignoreLabel.Text = Result.IgnoreLabel;
        _trainGoodDirectory.Text = Result.TrainGoodDirectory;
        _testGoodDirectory.Text = Result.TestGoodDirectory;
        _testNgDirectory.Text = Result.TestNgDirectory;
    }

    private void SaveAndClose()
    {
        try
        {
            var trainLabel = RequireLabel(_trainGoodLabel.Text, "Train GOOD");
            var testGoodLabel = RequireLabel(_testGoodLabel.Text, "Test GOOD");
            var testNgLabel = RequireLabel(_testNgLabel.Text, "Test NG");
            var ignoreLabel = RequireLabel(_ignoreLabel.Text, "Ignore");
            var trainDirectory = NormalizeDirectory(_trainGoodDirectory.Text, trainLabel);
            var testGoodDirectory = NormalizeDirectory(_testGoodDirectory.Text, testGoodLabel);
            var testNgDirectory = NormalizeDirectory(_testNgDirectory.Text, testNgLabel);

            var directories = new[] { trainDirectory, testGoodDirectory, testNgDirectory };
            if (directories.Distinct(StringComparer.OrdinalIgnoreCase).Count() != directories.Length)
                throw new InvalidOperationException("三个类别的导出目录不能相同。");

            Result = new DatasetCategoryOptions
            {
                TrainGoodLabel = trainLabel,
                TestGoodLabel = testGoodLabel,
                TestNgLabel = testNgLabel,
                IgnoreLabel = ignoreLabel,
                TrainGoodDirectory = trainDirectory,
                TestGoodDirectory = testGoodDirectory,
                TestNgDirectory = testNgDirectory
            };
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "类别设置无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static string RequireLabel(string value, string name)
    {
        var text = value.Trim();
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"{name} 的显示名称不能为空。");
        return text;
    }

    private static string NormalizeDirectory(string value, string label)
    {
        var segments = value
            .Trim()
            .Replace('/', '\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
            throw new InvalidOperationException($"{label} 的导出目录不能为空。");

        foreach (var segment in segments)
        {
            if (segment is "." or ".." || segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException($"{label} 的目录包含无效路径段：{segment}");
        }
        return string.Join("\\", segments);
    }

    private static DatasetCategoryOptions Clone(DatasetCategoryOptions source) => new()
    {
        TrainGoodLabel = source.TrainGoodLabel,
        TestGoodLabel = source.TestGoodLabel,
        TestNgLabel = source.TestNgLabel,
        IgnoreLabel = source.IgnoreLabel,
        TrainGoodDirectory = source.TrainGoodDirectory,
        TestGoodDirectory = source.TestGoodDirectory,
        TestNgDirectory = source.TestNgDirectory
    };
}
