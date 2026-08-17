using System.Drawing;

namespace DatasetStudio.WinForms;

/// <summary>
/// Shared visual language for the desktop client. The palette mirrors the
/// reference DatasetStudio design: neutral surfaces, dark readable type and
/// one restrained charcoal action colour.
/// </summary>
internal static class UiTheme
{
    public const string FontFamily = "Microsoft YaHei UI";

    // Base surfaces
    public static readonly Color WindowBackground = Color.FromArgb(244, 245, 246); // #f4f5f6
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceSoft = Color.FromArgb(250, 250, 250);      // #fafafa
    public static readonly Color SurfaceHover = Color.FromArgb(246, 246, 246);     // #f6f6f6

    // Text. These values deliberately avoid low-contrast decorative greys.
    public static readonly Color TextPrimary = Color.FromArgb(32, 32, 32);         // #202020
    public static readonly Color TextSecondary = Color.FromArgb(64, 64, 64);       // #404040
    public static readonly Color TextMuted = Color.FromArgb(92, 92, 92);           // #5c5c5c

    // Borders and navigation
    public static readonly Color Border = Color.FromArgb(218, 220, 222);           // #dadcde
    public static readonly Color BorderStrong = Color.FromArgb(194, 196, 198);     // #c2c4c6
    public static readonly Color NavigationActive = Color.FromArgb(238, 239, 240); // #eeeff0
    public static readonly Color NavigationHover = Color.FromArgb(246, 246, 246);
    public static readonly Color NavigationPressed = Color.FromArgb(228, 229, 230);

    // Charcoal is the primary action colour used by the reference design.
    public static readonly Color Accent = TextPrimary;
    public static readonly Color AccentHover = Color.FromArgb(51, 51, 51);
    public static readonly Color AccentSoft = NavigationActive;

    // Image viewer
    public static readonly Color Viewer = Color.FromArgb(30, 30, 30);
    public static readonly Color ViewerText = Color.FromArgb(212, 212, 212);

    // Semantic colours keep their contrast on light panels.
    public static readonly Color Danger = Color.FromArgb(185, 28, 28);
    public static readonly Color Warning = Color.FromArgb(180, 83, 9);
    public static readonly Color Success = Color.FromArgb(21, 128, 61);

    // Point fonts participate in WinForms DPI autoscaling together with the
    // code-built layout, so moving the app between display scales keeps text
    // and controls in the same proportion.
    public static Font CreateFont(float size = 10F, FontStyle style = FontStyle.Regular) =>
        new(FontFamily, size, style, GraphicsUnit.Point);

    public static Font CreateMonoFont(float size = 9.5F, FontStyle style = FontStyle.Regular) =>
        new("Consolas", size, style, GraphicsUnit.Point);

    public static CardPanel CreateCard(Padding padding) => new()
    {
        Dock = DockStyle.Fill,
        BackColor = Surface,
        Padding = padding,
        Margin = Padding.Empty
    };

    public static Button CreateButton(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Height = 36,
            AutoSize = false,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = primary ? Accent : Surface,
            ForeColor = primary ? Color.White : TextPrimary,
            Cursor = Cursors.Hand,
            Font = CreateFont(9.5F, primary ? FontStyle.Bold : FontStyle.Regular),
            TextImageRelation = TextImageRelation.ImageBeforeText
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
        Font = CreateFont(11F, FontStyle.Bold),
        ForeColor = TextPrimary
    };

    public static Label CreateFieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = CreateFont(9.5F, FontStyle.Bold),
        ForeColor = TextSecondary
    };

    public static Label CreateMutedText(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Font = CreateFont(9F),
        ForeColor = TextMuted
    };

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Surface;
        textBox.ForeColor = TextPrimary;
        textBox.Font = CreateFont(9.5F);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Surface;
        comboBox.ForeColor = TextPrimary;
        comboBox.Font = CreateFont(9.5F);
    }

    public static void StyleCheckedListBox(CheckedListBox list)
    {
        list.BorderStyle = BorderStyle.FixedSingle;
        list.BackColor = Surface;
        list.ForeColor = TextPrimary;
        list.Font = CreateFont(9.5F);
    }

    public static void StyleOptionButton(ButtonBase option)
    {
        option.Font = CreateFont(9.5F);
        option.ForeColor = TextPrimary;
    }

    /// <summary>Append a new row to a single-column adaptive layout.</summary>
    public static void AddRow(TableLayoutPanel layout, Control control, SizeType type, float height = 0f, Padding? margin = null)
    {
        var row = layout.RowCount;
        layout.RowCount = row + 1;
        layout.RowStyles.Add(new RowStyle(type, height));
        control.Margin = margin ?? Padding.Empty;
        layout.Controls.Add(control, 0, row);
    }

    /// <summary>Apply a legible grid treatment with a distinct header and selection state.</summary>
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
            Padding = new Padding(10, 0, 8, 0),
            Font = CreateFont(9.5F, FontStyle.Bold)
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = TextPrimary,
            SelectionBackColor = AccentSoft,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(10, 0, 8, 0),
            Font = CreateFont(9.5F)
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(252, 252, 252),
            ForeColor = TextPrimary,
            SelectionBackColor = AccentSoft,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(10, 0, 8, 0),
            Font = CreateFont(9.5F)
        };

        grid.RowTemplate.Height = 38;
        grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
    }

    /// <summary>Draw a ListView with the same header, zebra row and selection language as grids.</summary>
    public static void StyleListView(ListView list, bool darkSelection = false)
    {
        list.BackColor = Surface;
        list.ForeColor = TextPrimary;
        list.Font = CreateFont(9.5F);
        list.BorderStyle = BorderStyle.FixedSingle;
        list.FullRowSelect = true;
        list.HideSelection = false;
        list.MultiSelect = false;
        list.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        list.OwnerDraw = true;

        list.DrawColumnHeader += (_, e) =>
        {
            using var background = new SolidBrush(SurfaceSoft);
            using var borderPen = new Pen(Border);
            using var font = CreateFont(9F, FontStyle.Bold);
            e.Graphics.FillRectangle(background, e.Bounds);
            e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            TextRenderer.DrawText(
                e.Graphics,
                e.Header?.Text ?? string.Empty,
                font,
                Rectangle.FromLTRB(e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Right - 4, e.Bounds.Bottom),
                TextSecondary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        };
        list.DrawItem += (_, _) => { };
        list.DrawSubItem += (_, e) =>
        {
            if (e.Item is null || e.SubItem is null) return;
            var isSelected = e.Item.Selected;
            var background = isSelected ? (darkSelection ? Accent : AccentSoft) : e.ItemIndex % 2 == 0 ? Surface : SurfaceSoft;
            using var brush = new SolidBrush(background);
            e.Graphics.FillRectangle(brush, e.Bounds);
            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem.Text,
                list.Font,
                Rectangle.FromLTRB(e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Right - 4, e.Bounds.Bottom),
                isSelected && darkSelection ? Color.White : TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        };
    }
}
