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

        if (_moving && SelectedRoi is not null)
        {
            var current = ScreenToImage(e.Location);
            var dx = (int)Math.Round(current.X - _moveStart.X);
            var dy = (int)Math.Round(current.Y - _moveStart.Y);
            SelectedRoi.X = Math.Max(0, _roiStart.X + dx);
            SelectedRoi.Y = Math.Max(0, _roiStart.Y + dy);
            ClampRoi(SelectedRoi);
            Invalidate();
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
        roi.X = Math.Clamp(roi.X, 0, Math.Max(0, _image.Width - roi.Width));
        roi.Y = Math.Clamp(roi.Y, 0, Math.Max(0, _image.Height - roi.Height));
    }

    private static RectangleF Normalize(PointF a, PointF b) => new(
        Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
}
