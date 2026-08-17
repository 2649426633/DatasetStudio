using System.Drawing.Drawing2D;
using DatasetStudio.Core;

namespace DatasetStudio.WinForms.Controls;

public sealed class RoiEventArgs(RoiDefinition roi) : EventArgs
{
    public RoiDefinition Roi { get; } = roi;
}

public sealed class RoiSelectionEventArgs(RoiDefinition? roi) : EventArgs
{
    public RoiDefinition? Roi { get; } = roi;
}

public sealed class ImageCanvas : Control
{
    private Image? _image;
    private string _imagePath = string.Empty;
    private float _zoom = 1f;
    private PointF _offset;
    private bool _panning;
    private Point _lastMouse;
    private bool _creating;
    private PointF _createStart;
    private RectangleF _createPreview;
    private bool _moving;
    private PointF _moveStart;
    private Point _roiStart;
    private bool _resizing;
    private int _resizeHandle = -1;
    private Rectangle _resizeStart;
    private readonly List<RoiDefinition> _rois = new();

    public bool AllowRoiEditing { get; set; }
    public bool ShowRois { get; set; } = true;
    public RoiKind? PendingCreateKind { get; set; }
    public RoiDefinition? SelectedRoi { get; private set; }
    public Size ImageSize => _image?.Size ?? Size.Empty;
    public string ImagePath => _imagePath;

    public event EventHandler<RoiEventArgs>? RoiCreated;
    public event EventHandler<RoiEventArgs>? RoiChanged;
    public event EventHandler<RoiSelectionEventArgs>? SelectionChanged;

    public ImageCanvas()
    {
        DoubleBuffered = true;
        BackColor = UiTheme.Viewer;
        ForeColor = UiTheme.ViewerText;
        TabStop = true;
        SetStyle(ControlStyles.Selectable, true);
    }

    public void LoadImage(string? path)
    {
        _image?.Dispose();
        _image = null;
        _imagePath = string.Empty;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            using var source = Image.FromFile(path);
            _image = new Bitmap(source);
            _imagePath = path;
        }
        FitToView();
        Invalidate();
    }

    public void SetRois(IEnumerable<RoiDefinition> rois)
    {
        var selectedId = SelectedRoi?.Id;
        _rois.Clear();
        _rois.AddRange(rois);
        SelectedRoi = selectedId is null
            ? null
            : _rois.FirstOrDefault(x => string.Equals(x.Id, selectedId, StringComparison.OrdinalIgnoreCase));
        Invalidate();
    }

    public void SelectRoi(string? id)
    {
        SelectedRoi = string.IsNullOrWhiteSpace(id)
            ? null
            : _rois.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        SelectionChanged?.Invoke(this, new RoiSelectionEventArgs(SelectedRoi));
        Invalidate();
    }

    public void FitToView()
    {
        if (_image is null || ClientSize.Width <= 20 || ClientSize.Height <= 20)
        {
            _zoom = 1f;
            _offset = PointF.Empty;
            return;
        }
        var zx = (ClientSize.Width - 24f) / _image.Width;
        var zy = (ClientSize.Height - 24f) / _image.Height;
        _zoom = Math.Clamp(Math.Min(zx, zy), 0.02f, 20f);
        _offset = new PointF(
            (ClientSize.Width - _image.Width * _zoom) / 2f,
            (ClientSize.Height - _image.Height * _zoom) / 2f);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (_image is null)
        {
            using var brush = new SolidBrush(ForeColor);
            const string text = "选择图片后在此显示\n滚轮缩放 · 中键/右键拖动";
            var size = e.Graphics.MeasureString(text, Font);
            e.Graphics.DrawString(text, Font, brush,
                (ClientSize.Width - size.Width) / 2f,
                (ClientSize.Height - size.Height) / 2f);
            return;
        }

        var destination = new RectangleF(_offset.X, _offset.Y, _image.Width * _zoom, _image.Height * _zoom);
        e.Graphics.DrawImage(_image, destination);

        if (ShowRois)
        {
            foreach (var roi in _rois.Where(x => x.Enabled))
                DrawRoi(e.Graphics, roi, roi.Id == SelectedRoi?.Id);
        }

        if (_creating && _createPreview.Width > 0 && _createPreview.Height > 0)
        {
            using var pen = new Pen(Color.White, 2f) { DashStyle = DashStyle.Dash };
            e.Graphics.DrawRectangle(pen, Rectangle.Round(ToScreen(_createPreview)));
        }

        DrawCanvasBadge(e.Graphics);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_image is null) return;
        var before = ScreenToImage(e.Location);
        var factor = e.Delta > 0 ? 1.12f : 0.89f;
        _zoom = Math.Clamp(_zoom * factor, 0.02f, 30f);
        _offset = new PointF(e.X - before.X * _zoom, e.Y - before.Y * _zoom);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        _lastMouse = e.Location;

        if (e.Button is MouseButtons.Middle or MouseButtons.Right)
        {
            _panning = true;
            Cursor = Cursors.SizeAll;
            return;
        }

        if (!AllowRoiEditing || e.Button != MouseButtons.Left || _image is null)
            return;

        if (PendingCreateKind.HasValue)
        {
            _creating = true;
            _createStart = ClampToImage(ScreenToImage(e.Location));
            _createPreview = new RectangleF(_createStart, SizeF.Empty);
            return;
        }

        if (SelectedRoi is not null)
        {
            var handle = HitTestHandle(e.Location, SelectedRoi);
            if (handle >= 0)
            {
                _resizing = true;
                _resizeHandle = handle;
                _resizeStart = new Rectangle(SelectedRoi.X, SelectedRoi.Y, SelectedRoi.Width, SelectedRoi.Height);
                return;
            }
        }

        var hit = HitTest(e.Location);
        SelectedRoi = hit;
        SelectionChanged?.Invoke(this, new RoiSelectionEventArgs(hit));
        if (hit is not null)
        {
            _moving = true;
            _moveStart = ScreenToImage(e.Location);
            _roiStart = new Point(hit.X, hit.Y);
        }
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_panning)
        {
            _offset = new PointF(_offset.X + e.X - _lastMouse.X, _offset.Y + e.Y - _lastMouse.Y);
            _lastMouse = e.Location;
            Invalidate();
            return;
        }

        if (_creating)
        {
            var current = ClampToImage(ScreenToImage(e.Location));
            _createPreview = Normalize(_createStart, current);
            Invalidate();
            return;
        }

        if (_resizing && SelectedRoi is not null)
        {
            ResizeSelected(ClampToImage(ScreenToImage(e.Location)));
            Invalidate();
            return;
        }

        if (_moving && SelectedRoi is not null)
        {
            var current = ScreenToImage(e.Location);
            var dx = (int)Math.Round(current.X - _moveStart.X);
            var dy = (int)Math.Round(current.Y - _moveStart.Y);
            SelectedRoi.X = Math.Max(0, _roiStart.X + dx);
            SelectedRoi.Y = Math.Max(0, _roiStart.Y + dy);
            ClampRoi(SelectedRoi);
            Invalidate();
            return;
        }

        if (AllowRoiEditing && SelectedRoi is not null)
        {
            Cursor = HitTestHandle(e.Location, SelectedRoi) switch
            {
                0 or 3 => Cursors.SizeNWSE,
                1 or 2 => Cursors.SizeNESW,
                _ => Cursors.Default
            };
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_panning)
        {
            _panning = false;
            Cursor = Cursors.Default;
        }

        if (_creating)
        {
            _creating = false;
            if (_createPreview.Width >= 4 && _createPreview.Height >= 4 && PendingCreateKind.HasValue)
            {
                var roi = new RoiDefinition
                {
                    Kind = PendingCreateKind.Value,
                    X = (int)Math.Round(_createPreview.X),
                    Y = (int)Math.Round(_createPreview.Y),
                    Width = Math.Max(1, (int)Math.Round(_createPreview.Width)),
                    Height = Math.Max(1, (int)Math.Round(_createPreview.Height)),
                    Enabled = true
                };
                RoiCreated?.Invoke(this, new RoiEventArgs(roi));
            }
            _createPreview = RectangleF.Empty;
            Invalidate();
        }

        if (_moving && SelectedRoi is not null)
        {
            _moving = false;
            RoiChanged?.Invoke(this, new RoiEventArgs(SelectedRoi));
        }

        if (_resizing && SelectedRoi is not null)
        {
            _resizing = false;
            _resizeHandle = -1;
            Cursor = Cursors.Default;
            RoiChanged?.Invoke(this, new RoiEventArgs(SelectedRoi));
        }
    }

    protected override bool IsInputKey(Keys keyData)
    {
        if ((keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
            return true;
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!AllowRoiEditing || SelectedRoi is null) return;
        var step = e.Shift ? 10 : 1;
        switch (e.KeyCode)
        {
            case Keys.Left: SelectedRoi.X -= step; break;
            case Keys.Right: SelectedRoi.X += step; break;
            case Keys.Up: SelectedRoi.Y -= step; break;
            case Keys.Down: SelectedRoi.Y += step; break;
            default: return;
        }
        SelectedRoi.X = Math.Max(0, SelectedRoi.X);
        SelectedRoi.Y = Math.Max(0, SelectedRoi.Y);
        ClampRoi(SelectedRoi);
        RoiChanged?.Invoke(this, new RoiEventArgs(SelectedRoi));
        e.Handled = true;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _image?.Dispose();
        base.Dispose(disposing);
    }

    private void DrawRoi(Graphics graphics, RoiDefinition roi, bool selected)
    {
        var color = selected ? Color.FromArgb(245, 88, 72) : roi.Kind switch
        {
            RoiKind.ScrewSlot => Color.FromArgb(86, 190, 120),
            RoiKind.EmptySlot => Color.FromArgb(230, 180, 70),
            RoiKind.SpringRegion => Color.FromArgb(210, 110, 190),
            _ => Color.FromArgb(220, 220, 220)
        };
        using var pen = new Pen(color, selected ? 3f : 2f);
        var rect = Rectangle.Round(ToScreen(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height)));
        graphics.DrawRectangle(pen, rect);
        using var brush = new SolidBrush(Color.FromArgb(210, 22, 22, 22));
        var textSize = graphics.MeasureString(roi.Id, Font);
        var labelRect = new RectangleF(rect.Left, Math.Max(0, rect.Top - textSize.Height - 2), textSize.Width + 8, textSize.Height + 2);
        graphics.FillRectangle(brush, labelRect);
        using var textBrush = new SolidBrush(color);
        graphics.DrawString(roi.Id, Font, textBrush, labelRect.Left + 4, labelRect.Top + 1);

        if (selected && AllowRoiEditing)
            DrawResizeHandles(graphics, rect);
    }

    private void DrawCanvasBadge(Graphics graphics)
    {
        if (_image is null) return;

        var name = Path.GetFileName(_imagePath);
        var text = $"{name}   ·   {Math.Round(_zoom * 100)}%";
        using var font = UiTheme.CreateFont(9F, FontStyle.Bold);
        var textSize = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, 28), TextFormatFlags.NoPadding);
        var badge = new Rectangle(12, 12, textSize.Width + 20, 28);

        using var path = new GraphicsPath();
        const int radius = 6;
        path.AddArc(badge.Left, badge.Top, radius * 2, radius * 2, 180, 90);
        path.AddArc(badge.Right - radius * 2, badge.Top, radius * 2, radius * 2, 270, 90);
        path.AddArc(badge.Right - radius * 2, badge.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(badge.Left, badge.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        using var fill = new SolidBrush(Color.FromArgb(225, 27, 27, 27));
        using var border = new Pen(Color.FromArgb(64, 64, 64));
        graphics.FillPath(fill, path);
        graphics.DrawPath(border, path);
        TextRenderer.DrawText(
            graphics,
            text,
            font,
            Rectangle.Inflate(badge, -10, 0),
            Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
    }

    private static void DrawResizeHandles(Graphics graphics, Rectangle rect)
    {
        foreach (var point in GetHandlePoints(rect))
        {
            var handle = new Rectangle(point.X - 5, point.Y - 5, 10, 10);
            graphics.FillRectangle(Brushes.White, handle);
            graphics.DrawRectangle(Pens.Black, handle);
        }
    }

    private RoiDefinition? HitTest(Point screen)
    {
        for (var i = _rois.Count - 1; i >= 0; i--)
        {
            var roi = _rois[i];
            if (ToScreen(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height)).Contains(screen))
                return roi;
        }
        return null;
    }

    private int HitTestHandle(Point screen, RoiDefinition roi)
    {
        var rect = Rectangle.Round(ToScreen(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height)));
        var points = GetHandlePoints(rect);
        for (var i = 0; i < points.Length; i++)
        {
            if (Math.Abs(screen.X - points[i].X) <= 8 && Math.Abs(screen.Y - points[i].Y) <= 8)
                return i;
        }
        return -1;
    }

    private void ResizeSelected(PointF imagePoint)
    {
        if (SelectedRoi is null || _image is null) return;
        const int minSize = 4;
        var left = _resizeStart.Left;
        var top = _resizeStart.Top;
        var right = _resizeStart.Right;
        var bottom = _resizeStart.Bottom;
        var x = (int)Math.Round(imagePoint.X);
        var y = (int)Math.Round(imagePoint.Y);

        switch (_resizeHandle)
        {
            case 0:
                left = Math.Min(x, right - minSize);
                top = Math.Min(y, bottom - minSize);
                break;
            case 1:
                right = Math.Max(x, left + minSize);
                top = Math.Min(y, bottom - minSize);
                break;
            case 2:
                left = Math.Min(x, right - minSize);
                bottom = Math.Max(y, top + minSize);
                break;
            case 3:
                right = Math.Max(x, left + minSize);
                bottom = Math.Max(y, top + minSize);
                break;
        }

        left = Math.Clamp(left, 0, _image.Width - 1);
        top = Math.Clamp(top, 0, _image.Height - 1);
        right = Math.Clamp(right, left + 1, _image.Width);
        bottom = Math.Clamp(bottom, top + 1, _image.Height);
        SelectedRoi.X = left;
        SelectedRoi.Y = top;
        SelectedRoi.Width = right - left;
        SelectedRoi.Height = bottom - top;
    }

    private PointF ScreenToImage(Point point) => new(
        (point.X - _offset.X) / _zoom,
        (point.Y - _offset.Y) / _zoom);

    private RectangleF ToScreen(RectangleF imageRect) => new(
        _offset.X + imageRect.X * _zoom,
        _offset.Y + imageRect.Y * _zoom,
        imageRect.Width * _zoom,
        imageRect.Height * _zoom);

    private PointF ClampToImage(PointF point)
    {
        if (_image is null) return point;
        return new PointF(Math.Clamp(point.X, 0, _image.Width), Math.Clamp(point.Y, 0, _image.Height));
    }

    private void ClampRoi(RoiDefinition roi)
    {
        if (_image is null) return;
        roi.Width = Math.Clamp(roi.Width, 1, _image.Width);
        roi.Height = Math.Clamp(roi.Height, 1, _image.Height);
        roi.X = Math.Clamp(roi.X, 0, Math.Max(0, _image.Width - roi.Width));
        roi.Y = Math.Clamp(roi.Y, 0, Math.Max(0, _image.Height - roi.Height));
    }

    private static Point[] GetHandlePoints(Rectangle rect) =>
    [
        new Point(rect.Left, rect.Top),
        new Point(rect.Right, rect.Top),
        new Point(rect.Left, rect.Bottom),
        new Point(rect.Right, rect.Bottom)
    ];

    private static RectangleF Normalize(PointF a, PointF b) => new(
        Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
}
