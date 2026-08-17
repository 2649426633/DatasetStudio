using DatasetStudio.Core;
using DatasetStudio.Infrastructure;

namespace DatasetStudio.WinForms.Pages;

public sealed class ValidationPage : UserControl
{
    private readonly DataGridView _grid = new();
    private readonly Label _summary = new();
    private readonly Label _summaryDetail = new();
    private readonly Label _summaryIcon = new();
    private CardPanel? _summaryCard;
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
            _summaryDetail.Text = "新建或打开项目后即可执行完整性校验。";
            ApplySummaryStyle(UiTheme.TextSecondary, UiTheme.SurfaceSoft, UiTheme.Border, "—");
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
            row.DefaultCellStyle.BackColor = item.Severity switch
            {
                ValidationSeverity.Error => Color.FromArgb(255, 245, 245),
                ValidationSeverity.Warning => Color.FromArgb(255, 253, 245),
                _ => UiTheme.Surface
            };
        }
        var errors = items.Count(x => x.Severity == ValidationSeverity.Error);
        var warnings = items.Count(x => x.Severity == ValidationSeverity.Warning);
        if (errors > 0)
        {
            _summary.Text = $"校验未通过（{errors} 项阻断错误）";
            _summaryDetail.Text = "存在数据完整性或标签冲突问题，请根据下方检查列表修复后再导出。";
            ApplySummaryStyle(UiTheme.Danger, Color.FromArgb(254, 242, 242), Color.FromArgb(254, 202, 202), "×");
        }
        else if (warnings > 0)
        {
            _summary.Text = $"校验通过，但有 {warnings} 项警告建议";
            _summaryDetail.Text = "数据符合基础导出规则，可继续导出，但建议关注提示项。";
            ApplySummaryStyle(UiTheme.Warning, Color.FromArgb(255, 251, 235), Color.FromArgb(253, 230, 138), "!");
        }
        else
        {
            _summary.Text = "校验全部通过（Ready for Export）";
            _summaryDetail.Text = "数据满足工业视觉训练与评测规范，可安全导出或发布标准包。";
            ApplySummaryStyle(UiTheme.Success, Color.FromArgb(240, 253, 244), Color.FromArgb(187, 247, 208), "✓");
        }
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = UiTheme.WindowBackground
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _summaryCard = UiTheme.CreateCard(new Padding(16));
        _summaryCard.Margin = new Padding(0, 0, 0, 12);
        var summaryLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = Color.Transparent
        };
        summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
        summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 54F));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));

        _summaryIcon.Dock = DockStyle.Fill;
        _summaryIcon.TextAlign = ContentAlignment.MiddleCenter;
        _summaryIcon.Font = UiTheme.CreateFont(22F, FontStyle.Bold);
        _summaryIcon.Margin = new Padding(0, 0, 12, 0);
        summaryLayout.Controls.Add(_summaryIcon, 0, 0);
        summaryLayout.SetRowSpan(_summaryIcon, 2);

        _summary.AutoSize = false;
        _summary.Dock = DockStyle.Fill;
        _summary.TextAlign = ContentAlignment.BottomLeft;
        _summary.Font = UiTheme.CreateFont(13F, FontStyle.Bold);
        _summaryDetail.AutoSize = false;
        _summaryDetail.Dock = DockStyle.Fill;
        _summaryDetail.TextAlign = ContentAlignment.TopLeft;
        _summaryDetail.Font = UiTheme.CreateFont(9F);
        summaryLayout.Controls.Add(_summary, 1, 0);
        summaryLayout.Controls.Add(_summaryDetail, 1, 1);

        var run = UiTheme.CreateButton("重新校验", true);
        run.Size = new Size(120, 36);
        run.Anchor = AnchorStyles.Right;
        run.Click += (_, _) => RunValidation();
        summaryLayout.Controls.Add(run, 2, 0);
        summaryLayout.SetRowSpan(run, 2);
        _summaryCard.Controls.Add(summaryLayout);

        var gridCard = UiTheme.CreateCard(new Padding(1));
        var gridLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        gridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        gridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var gridHeader = new Label
        {
            Text = "数据完整性与规则校验项",
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.SurfaceSoft,
            ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.CreateFont(10.5F, FontStyle.Bold)
        };

        UiTheme.StyleDataGridView(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "State", HeaderText = "状态", Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Check", HeaderText = "检查项", Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "数量", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "说明", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        gridLayout.Controls.Add(gridHeader, 0, 0);
        gridLayout.Controls.Add(_grid, 0, 1);
        gridCard.Controls.Add(gridLayout);

        root.Controls.Add(_summaryCard, 0, 0);
        root.Controls.Add(gridCard, 0, 1);
        Controls.Add(root);
        ApplySummaryStyle(UiTheme.TextSecondary, UiTheme.SurfaceSoft, UiTheme.Border, "—");
    }

    private void ApplySummaryStyle(Color foreground, Color background, Color border, string icon)
    {
        if (_summaryCard is null) return;
        _summaryCard.BackColor = background;
        _summaryCard.BorderColor = border;
        _summaryCard.Invalidate();
        _summary.ForeColor = foreground;
        _summaryDetail.ForeColor = foreground;
        _summaryIcon.ForeColor = foreground;
        _summaryIcon.Text = icon;
        foreach (Control child in _summaryCard.Controls) child.BackColor = background;
    }
}
