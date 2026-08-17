namespace DatasetStudio.WinForms;

/// <summary>Header navigation button with the reference design's active underline.</summary>
internal sealed class NavigationButton : Button
{
    private bool _active;

    public bool Active
    {
        get => _active;
        set
        {
            if (_active == value) return;
            _active = value;
            Invalidate();
        }
    }

    public NavigationButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);
        if (!Active) return;

        using var pen = new Pen(UiTheme.TextPrimary, 2F);
        pevent.Graphics.DrawLine(pen, 0, Height - 2, Width, Height - 2);
    }
}
