using System.Drawing;

namespace DatasetStudio.WinForms;

internal static class UiTheme
{
    public static readonly Color WindowBackground = Color.FromArgb(244, 245, 246);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceSoft = Color.FromArgb(249, 249, 249);
    public static readonly Color TextPrimary = Color.FromArgb(32, 32, 32);
    public static readonly Color TextSecondary = Color.FromArgb(92, 92, 92);
    public static readonly Color TextMuted = Color.FromArgb(138, 138, 138);
    public static readonly Color Border = Color.FromArgb(218, 220, 222);
    public static readonly Color BorderStrong = Color.FromArgb(194, 196, 198);
    public static readonly Color NavigationActive = Color.FromArgb(238, 239, 240);
    public static readonly Color NavigationHover = Color.FromArgb(246, 246, 246);
    public static readonly Color NavigationPressed = Color.FromArgb(229, 230, 231);
    public static readonly Color Viewer = Color.FromArgb(38, 38, 38);
    public static readonly Color ViewerText = Color.FromArgb(178, 178, 178);
    public static readonly Color PrimaryButton = Color.FromArgb(45, 45, 45);
    public static readonly Color PrimaryButtonHover = Color.FromArgb(62, 62, 62);
    public static readonly Color Danger = Color.FromArgb(176, 55, 55);
    public static readonly Color Warning = Color.FromArgb(181, 124, 35);
    public static readonly Color Success = Color.FromArgb(52, 120, 72);

    public static Button CreateButton(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Height = 34,
            AutoSize = false,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? PrimaryButton : Surface,
            ForeColor = primary ? Color.White : TextPrimary,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        button.FlatAppearance.BorderColor = primary ? PrimaryButton : BorderStrong;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary ? PrimaryButtonHover : NavigationHover;
        return button;
    }

    public static Label CreateSectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold),
        ForeColor = TextPrimary
    };
}
