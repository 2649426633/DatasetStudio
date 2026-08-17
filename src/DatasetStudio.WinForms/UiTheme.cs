using System.Drawing;

namespace DatasetStudio.WinForms;

internal static class UiTheme
{
    // 基础表面
    public static readonly Color WindowBackground = Color.FromArgb(242, 245, 249);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceSoft = Color.FromArgb(246, 248, 251);
    public static readonly Color SurfaceHover = Color.FromArgb(236, 242, 249);

    // 文本
    // 所有正文颜色均满足白底下的清晰可读对比度，避免浅灰文字在普通显示器上发虚。
    public static readonly Color TextPrimary = Color.FromArgb(31, 41, 55);
    public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);
    public static readonly Color TextMuted = Color.FromArgb(90, 102, 120);

    // 边框
    public static readonly Color Border = Color.FromArgb(213, 222, 232);
    public static readonly Color BorderStrong = Color.FromArgb(181, 196, 212);

    // 品牌强调色
    public static readonly Color Accent = Color.FromArgb(0, 92, 173);
    public static readonly Color AccentHover = Color.FromArgb(0, 73, 140);
    public static readonly Color AccentSoft = Color.FromArgb(227, 240, 253);

    // 导航
    public static readonly Color NavigationActive = Color.FromArgb(227, 240, 253);
    public static readonly Color NavigationHover = Color.FromArgb(240, 245, 251);
    public static readonly Color NavigationPressed = Color.FromArgb(218, 233, 248);

    // 图片查看区
    public static readonly Color Viewer = Color.FromArgb(38, 38, 38);
    public static readonly Color ViewerText = Color.FromArgb(178, 178, 178);

    // 语义色
    public static readonly Color Danger = Color.FromArgb(180, 35, 24);
    public static readonly Color Warning = Color.FromArgb(145, 85, 0);
    public static readonly Color Success = Color.FromArgb(22, 117, 69);

    public static Button CreateButton(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Height = 40,
            AutoSize = false,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : Surface,
            ForeColor = primary ? Color.White : TextPrimary,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        button.FlatAppearance.BorderColor = primary ? Accent : BorderStrong;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary ? AccentHover : SurfaceHover;
        button.FlatAppearance.MouseDownBackColor = primary ? AccentHover : NavigationPressed;
        return button;
    }

    public static Label CreateSectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
        ForeColor = TextPrimary
    };

    public static Label CreateFieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
        ForeColor = TextSecondary
    };

    public static Label CreateMutedText(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Font = new Font("Microsoft YaHei UI", 9.5F),
        ForeColor = TextMuted
    };

    /// <summary>向单列 TableLayoutPanel 追加一行，自动覆盖默认行样式，用于构建响应式纵向布局。</summary>
    public static void AddRow(TableLayoutPanel layout, Control control, SizeType type, float height = 0f, Padding? margin = null)
    {
        var row = layout.RowCount;
        layout.RowCount = row + 1;
        layout.RowStyles.Add(new RowStyle(type, height));
        control.Margin = margin ?? Padding.Empty;
        layout.Controls.Add(control, 0, row);
    }

    /// <summary>统一 DataGridView 的现代浅色样式（标题栏、行高、选中态、斑马纹）。</summary>
    public static void StyleDataGridView(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Border;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SurfaceSoft,
            ForeColor = TextSecondary,
            SelectionBackColor = SurfaceSoft,
            SelectionForeColor = TextSecondary,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = TextPrimary,
            SelectionBackColor = AccentSoft,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(8, 0, 0, 0),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SurfaceSoft,
            ForeColor = TextPrimary,
            SelectionBackColor = AccentSoft,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(8, 0, 0, 0),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        grid.RowTemplate.Height = 36;
        grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
    }

    /// <summary>统一 ListView 的基础外观（保持默认表头，避免过度自绘引入复杂度）。</summary>
    public static void StyleListView(ListView list)
    {
        list.BackColor = Surface;
        list.ForeColor = TextPrimary;
        list.Font = new Font("Microsoft YaHei UI", 10F);
        list.BorderStyle = BorderStyle.None;
        list.FullRowSelect = true;
        list.HideSelection = false;
        list.MultiSelect = false;
    }
}
