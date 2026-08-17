using System.Drawing.Drawing2D;

namespace DatasetStudio.WinForms;

/// <summary>
/// A quiet, bordered surface used to group one task on the light workspace.
/// WinForms panels do not expose a modern card border, so it is drawn here
/// instead of relying on the operating-system theme.
/// </summary>
public sealed class CardPanel : Panel
{
    public int CornerRadius { get; set; } = 8;
    public Color BorderColor { get; set; } = UiTheme.Border;

    public CardPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = UiTheme.Surface;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var bounds = ClientRectangle;
        bounds.Width--;
        bounds.Height--;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = CreateRoundedPath(bounds, CornerRadius);
        using var pen = new Pen(BorderColor);
        e.Graphics.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var diameter = Math.Min(Math.Min(bounds.Width, bounds.Height), Math.Max(2, radius * 2));
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
