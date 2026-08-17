using DatasetStudio.Core;
using DatasetStudio.Infrastructure;

namespace DatasetStudio.WinForms.Pages;

public sealed class ValidationPage : UserControl
{
    private readonly DataGridView _grid = new();
    private readonly Label _summary = new();
    private AppSession? _session;

    public ValidationPage()
    {
        BackColor = UiTheme.WindowBackground;
        BuildLayout();
    }

    public void BindSession(AppSession session)
    {
        _session = session;
        RunValidation();
    }

    public void RunValidation()
    {
        _grid.Rows.Clear();
        if (_session is null)
        {
            _summary.Text = "尚未打开项目";
            return;
        }

        var size = _session.ReferenceImageSize;
        var items = new DatasetValidator().Validate(_session.Repository, size.Width, size.Height);
        foreach (var item in items)
        {
            var icon = item.Severity switch
            {
                ValidationSeverity.Ok => "✅",
                ValidationSeverity.Warning => "⚠",
                _ => "❌"
            };
            var rowIndex = _grid.Rows.Add(icon, item.Name, item.Value, item.Message);
            var row = _grid.Rows[rowIndex];
            row.DefaultCellStyle.ForeColor = item.Severity switch
            {
                ValidationSeverity.Error => UiTheme.Danger,
                ValidationSeverity.Warning => UiTheme.Warning,
                _ => UiTheme.TextPrimary
            };
        }
        var errors = items.Count(x => x.Severity == ValidationSeverity.Error);
        var warnings = items.Count(x => x.Severity == ValidationSeverity.Warning);
        _summary.Text = errors == 0
            ? $"校验完成：无阻断错误，警告 {warnings} 项。"
            : $"校验失败：发现 {errors} 个阻断错误，必须修复后才能导出。";
        _summary.ForeColor = errors == 0 ? UiTheme.Success : UiTheme.Danger;
    }

    private void BuildLayout()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(22) };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var title = UiTheme.CreateSectionTitle("Dataset Validation / 数据完整性校验");
        UiTheme.AddRow(layout, title, SizeType.AutoSize, 0, new Padding(0, 0, 0, 8));

        _summary.AutoSize = false;
        _summary.Dock = DockStyle.Fill;
        _summary.ForeColor = UiTheme.TextSecondary;
        UiTheme.AddRow(layout, _summary, SizeType.Absolute, 28, new Padding(0, 0, 0, 8));

        var run = UiTheme.CreateButton("重新校验", true);
        run.Size = new Size(120, 36);
        run.Anchor = AnchorStyles.Left;
        run.Click += (_, _) => RunValidation();
        UiTheme.AddRow(layout, run, SizeType.Absolute, 44);

        UiTheme.StyleDataGridView(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "State", HeaderText = "状态", Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Check", HeaderText = "检查项", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "数量", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "说明", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        UiTheme.AddRow(layout, _grid, SizeType.Percent, 100F, new Padding(0, 12, 0, 0));

        panel.Controls.Add(layout);
        Controls.Add(panel);
    }
}
