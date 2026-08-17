using DatasetStudio.Core;
using DatasetStudio.WinForms.Controls;

namespace DatasetStudio.WinForms.Pages;

public sealed partial class RoiCalibrationPage : UserControl
{
    private readonly Dictionary<Button, (RoiKind? Kind, Color ActiveColor, Color BorderColor)> _toolButtons = new();
    private AppSession? _session;
    private List<RoiDefinition> _rois = new();
    private bool _syncingSelection;
    private RoiKind? _activeToolMode;

    public RoiCalibrationPage()
    {
        InitializeComponent();
        ConfigureRuntimeStyles();
        RegisterToolButtons();
        WireCanvasAndGridEvents();
        RefreshToolButtons();
        ApplyResponsiveLayout();
    }

    public void BindSession(AppSession session)
    {
        _session = session;
        LoadReferenceAndRois();
    }

    internal void ApplyResponsiveLayout()
    {
        if (ClientSize.Width <= 0 || _workArea.ColumnStyles.Count < 2) return;
        var dpi = Math.Max(1F, DeviceDpi / 96F);
        var width = ClientSize.Width;
        var logicalWidth = width / dpi;
        var compact = logicalWidth < 1120F;
        var gridWidth = Math.Clamp(width * 0.31F, 380F * dpi, 520F * dpi);
        var minimumViewer = 420F * dpi;
        if (width - gridWidth < minimumViewer)
            gridWidth = Math.Max(320F * dpi, width - minimumViewer);
        _workArea.ColumnStyles[1].Width = gridWidth;

        _modeLabel.Visible = !compact;
        _referenceLabel.Visible = !compact;
        var toolWidth = (compact ? 88 : 100) * dpi;
        foreach (var button in _toolButtons.Keys)
            button.Width = (int)toolWidth;
    }

    private void ConfigureRuntimeStyles()
    {
        UiTheme.StyleDataGridView(_grid);
    }

    private void RegisterToolButtons()
    {
        _toolButtons.Clear();
        _toolButtons[_selectToolButton] = (null, UiTheme.Accent, UiTheme.Border);
        _toolButtons[_screwToolButton] = (RoiKind.ScrewSlot, UiTheme.Success, Color.FromArgb(187, 247, 208));
        _toolButtons[_emptyToolButton] = (RoiKind.EmptySlot, Color.FromArgb(217, 119, 6), Color.FromArgb(253, 230, 138));
        _toolButtons[_springToolButton] = (RoiKind.SpringRegion, Color.FromArgb(126, 34, 206), Color.FromArgb(233, 213, 255));
        _toolButtons[_anomalyToolButton] = (RoiKind.AnomalyRegion, UiTheme.Danger, Color.FromArgb(254, 202, 202));
    }

    private void WireCanvasAndGridEvents()
    {
        _canvas.RoiCreated += Canvas_RoiCreated;
        _canvas.RoiChanged += Canvas_RoiChanged;
        _canvas.SelectionChanged += Canvas_SelectionChanged;
        _grid.SelectionChanged += Grid_SelectionChanged;
        _grid.CellEndEdit += Grid_CellEndEdit;
    }

    private void RoiCalibrationPage_Resize(object? sender, EventArgs e) => ApplyResponsiveLayout();
    private void SelectToolButton_Click(object? sender, EventArgs e) => SetSelectionMode();
    private void ScrewToolButton_Click(object? sender, EventArgs e) => SetCreateMode(RoiKind.ScrewSlot);
    private void EmptyToolButton_Click(object? sender, EventArgs e) => SetCreateMode(RoiKind.EmptySlot);
    private void SpringToolButton_Click(object? sender, EventArgs e) => SetCreateMode(RoiKind.SpringRegion);
    private void AnomalyToolButton_Click(object? sender, EventArgs e) => SetCreateMode(RoiKind.AnomalyRegion);
    private void ReferenceButton_Click(object? sender, EventArgs e) => SelectReferenceImage();
    private void DuplicateButton_Click(object? sender, EventArgs e) => DuplicateSelected();
    private void DeleteButton_Click(object? sender, EventArgs e) => DeleteSelected();
    private void FitButton_Click(object? sender, EventArgs e) => _canvas.FitToView();
    private void Canvas_RoiCreated(object? sender, RoiEventArgs e) => CreateRoi(e.Roi);
    private void Canvas_RoiChanged(object? sender, RoiEventArgs e) => SaveRoi(e.Roi);

    private void Canvas_SelectionChanged(object? sender, RoiSelectionEventArgs e)
    {
        if (_syncingSelection) return;
        _syncingSelection = true;
        try { SelectGridRow(e.Roi?.Id); }
        finally { _syncingSelection = false; }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_syncingSelection || _grid.SelectedRows.Count == 0) return;
        _syncingSelection = true;
        try { _canvas.SelectRoi(_grid.SelectedRows[0].Cells["Id"].Value?.ToString()); }
        finally { _syncingSelection = false; }
    }

    private void Grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e) => ApplyGridEdit(e.RowIndex);

    private void SetSelectionMode()
    {
        _canvas.PendingCreateKind = null;
        _activeToolMode = null;
        _modeLabel.Text = "模式：选择 / 移动";
        RefreshToolButtons();
    }

    private void RefreshToolButtons()
    {
        foreach (var (button, state) in _toolButtons)
        {
            var active = state.Kind == _activeToolMode;
            button.BackColor = active ? state.ActiveColor : UiTheme.Surface;
            button.ForeColor = active ? Color.White : state.ActiveColor;
            button.Font = UiTheme.CreateFont(8.5F, active ? FontStyle.Bold : FontStyle.Regular);
            button.FlatAppearance.BorderColor = active ? state.ActiveColor : state.BorderColor;
            button.FlatAppearance.MouseOverBackColor = active ? state.ActiveColor : UiTheme.SurfaceHover;
        }
    }

    private void SelectReferenceImage()
    {
        if (_session is null)
        {
            MessageBox.Show(this, "请先新建或打开项目。", "Dataset Studio");
            return;
        }

        if (_rois.Count > 0)
        {
            var replace = MessageBox.Show(
                this,
                "当前项目已经存在 ROI。更换参考图会改变标准坐标系，现有 ROI 不会自动转换。\r\n\r\n确定继续吗？",
                "更换参考图",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (replace != DialogResult.Yes) return;
        }

        var choice = MessageBox.Show(
            this,
            "请选择参考图来源：\r\n\r\n" +
            "【是】从一张正常 GOOD 原图自动定位、旋转矫正并生成 reference_aligned.png\r\n" +
            "【否】导入已经生成好的 reference_aligned.png\r\n" +
            "【取消】返回",
            "参考图",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (choice == DialogResult.Yes)
            CreateReferenceFromGood();
        else if (choice == DialogResult.No)
            ImportExistingReference();
    }

    private void CreateReferenceFromGood()
    {
        if (_session is null) return;
        using var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*",
            Title = "选择一张正常 GOOD 原图创建标准参考图"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            UseWaitCursor = true;
            var result = _session.CreateReferenceFromGood(dialog.FileName);
            LoadReferenceAndRois();
            MessageBox.Show(
                this,
                $"reference_aligned.png 已生成。\r\n" +
                $"尺寸：{result.Width} × {result.Height}\r\n" +
                $"检测角度：{result.DetectedAngleDeg:F2}°",
                "参考图创建完成",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "参考图创建失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void ImportExistingReference()
    {
        if (_session is null) return;
        using var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*",
            Title = "选择已有 reference_aligned.png"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            _session.ImportReferenceImage(dialog.FileName);
            LoadReferenceAndRois();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "参考图导入失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetCreateMode(RoiKind kind)
    {
        if (_session is null || !File.Exists(_session.ReferenceImagePath))
        {
            MessageBox.Show(this, "请先选择 reference_aligned.png 参考图。", "ROI 标定", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _canvas.PendingCreateKind = kind;
        _activeToolMode = kind;
        _modeLabel.Text = $"模式：绘制 {kind}";
        RefreshToolButtons();
    }

    private void CreateRoi(RoiDefinition roi)
    {
        if (_session is null) return;
        roi.Id = NextId(roi.Kind);
        roi.Expected = roi.Kind switch
        {
            RoiKind.ScrewSlot => "screw",
            RoiKind.EmptySlot => "empty",
            _ => string.Empty
        };
        roi.ExpectedCount = roi.Kind == RoiKind.SpringRegion ? 4 : null;
        _session.Repository.SaveRoi(roi);
        _session.WriteProductConfig();
        SetSelectionMode();
        ReloadRois(roi.Id);
    }

    private void SaveRoi(RoiDefinition roi)
    {
        if (_session is null) return;
        ClampRoiToReference(roi);
        _session.Repository.SaveRoi(roi);
        _session.WriteProductConfig();
        ReloadRois(roi.Id);
    }

    private void ApplyGridEdit(int rowIndex)
    {
        if (_session is null || rowIndex < 0 || rowIndex >= _grid.Rows.Count) return;
        var id = _grid.Rows[rowIndex].Cells["Id"].Value?.ToString();
        var roi = _rois.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        if (roi is null) return;

        var row = _grid.Rows[rowIndex];
        roi.X = ParseInt(row.Cells["X"].Value, roi.X);
        roi.Y = ParseInt(row.Cells["Y"].Value, roi.Y);
        roi.Width = Math.Max(1, ParseInt(row.Cells["W"].Value, roi.Width));
        roi.Height = Math.Max(1, ParseInt(row.Cells["H"].Value, roi.Height));
        roi.Enabled = Convert.ToBoolean(row.Cells["Enabled"].Value ?? true);
        ClampRoiToReference(roi);
        SaveRoi(roi);
    }

    private void DuplicateSelected()
    {
        if (_session is null || _grid.SelectedRows.Count == 0) return;
        var id = _grid.SelectedRows[0].Cells["Id"].Value?.ToString();
        var source = _rois.FirstOrDefault(x => x.Id == id);
        if (source is null) return;
        var copy = new RoiDefinition
        {
            Id = NextId(source.Kind),
            Kind = source.Kind,
            X = source.X + 12,
            Y = source.Y + 12,
            Width = source.Width,
            Height = source.Height,
            Expected = source.Expected,
            ExpectedCount = source.ExpectedCount,
            Enabled = source.Enabled
        };
        ClampRoiToReference(copy);
        _session.Repository.SaveRoi(copy);
        _session.WriteProductConfig();
        ReloadRois(copy.Id);
    }

    private void DeleteSelected()
    {
        if (_session is null || _grid.SelectedRows.Count == 0) return;
        var id = _grid.SelectedRows[0].Cells["Id"].Value?.ToString();
        if (string.IsNullOrWhiteSpace(id)) return;
        if (MessageBox.Show(this, $"删除 ROI {id}？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        _session.Repository.DeleteRoi(id);
        _session.WriteProductConfig();
        ReloadRois();
    }

    private void LoadReferenceAndRois()
    {
        if (_session is null) return;
        _referenceLabel.Text = File.Exists(_session.ReferenceImagePath)
            ? $"参考图：{Path.GetFileName(_session.ReferenceImagePath)}"
            : "参考图：未设置";
        _canvas.LoadImage(File.Exists(_session.ReferenceImagePath) ? _session.ReferenceImagePath : null);
        ReloadRois();
    }

    private void ReloadRois(string? selectId = null)
    {
        if (_session is null) return;
        _rois = _session.Repository.LoadRois();
        _canvas.SetRois(_rois);
        _grid.Rows.Clear();
        foreach (var roi in _rois)
        {
            var row = _grid.Rows.Add(roi.Id, roi.Kind, roi.Expected, roi.X, roi.Y, roi.Width, roi.Height, roi.Enabled);
            if (roi.Id == selectId) _grid.Rows[row].Selected = true;
        }
        if (!string.IsNullOrWhiteSpace(selectId)) _canvas.SelectRoi(selectId);
    }

    private void SelectGridRow(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (string.Equals(row.Cells["Id"].Value?.ToString(), id, StringComparison.OrdinalIgnoreCase))
            {
                row.Selected = true;
                _grid.CurrentCell = row.Cells["Id"];
                return;
            }
        }
    }

    private void ClampRoiToReference(RoiDefinition roi)
    {
        var size = _canvas.ImageSize;
        if (size.Width <= 0 || size.Height <= 0) return;
        roi.Width = Math.Clamp(roi.Width, 1, size.Width);
        roi.Height = Math.Clamp(roi.Height, 1, size.Height);
        roi.X = Math.Clamp(roi.X, 0, Math.Max(0, size.Width - roi.Width));
        roi.Y = Math.Clamp(roi.Y, 0, Math.Max(0, size.Height - roi.Height));
    }

    private string NextId(RoiKind kind)
    {
        var prefix = kind switch
        {
            RoiKind.ScrewSlot => "S",
            RoiKind.EmptySlot => "E",
            RoiKind.SpringRegion => "SPRING",
            _ => "SURFACE"
        };
        var numbers = _rois
            .Where(x => x.Kind == kind && x.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id[prefix.Length..])
            .Select(text => int.TryParse(text, out var number) ? number : 0);
        return $"{prefix}{numbers.DefaultIfEmpty(0).Max() + 1:00}";
    }

    private static int ParseInt(object? value, int fallback) =>
        int.TryParse(Convert.ToString(value), out var parsed) ? parsed : fallback;
}
