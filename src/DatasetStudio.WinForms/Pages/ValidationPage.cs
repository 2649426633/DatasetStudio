using DatasetStudio.Core;
using DatasetStudio.Infrastructure;

namespace DatasetStudio.WinForms.Pages;

public sealed partial class ValidationPage : UserControl
{
    private AppSession? _session;

    public ValidationPage()
    {
        InitializeComponent();
        UiTheme.StyleDataGridView(_grid);
        ApplySummaryStyle(UiTheme.TextSecondary, UiTheme.SurfaceSoft, UiTheme.Border, "—");
    }

    public void BindSession(AppSession session)
    {
        _session = session;

        // Opening a project should stay lightweight. Full validation can touch every
        // source record/file, so MainForm runs it only when the user enters this page.
        _grid.Rows.Clear();
        _summary.Text = "项目已打开";
        _summaryDetail.Text = "进入“数据校验”页时会执行完整校验。";
        ApplySummaryStyle(UiTheme.TextSecondary, UiTheme.SurfaceSoft, UiTheme.Border, "—");
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

    private void RunButton_Click(object? sender, EventArgs e) => RunValidation();

    private void ApplySummaryStyle(Color foreground, Color background, Color border, string icon)
    {
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
