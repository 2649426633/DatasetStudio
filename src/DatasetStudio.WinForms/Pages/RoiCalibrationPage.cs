using DatasetStudio.Core;
using DatasetStudio.WinForms.Controls;

namespace DatasetStudio.WinForms.Pages;

public sealed class RoiCalibrationPage : UserControl
{
    private readonly ImageCanvas _canvas = new() { Dock = DockStyle.Fill, AllowRoiEditing = true };
    private readonly DataGridView _grid = new();
    private readonly Label _referenceLabel = new();
    private readonly Label _modeLabel = new();
    private AppSession? _session;
    private List<RoiDefinition> _rois = new();
    private bool _syncingSelection;

    public RoiCalibrationPage()
    {
        BackColor = UiTheme.WindowBackground;
        BuildLayout();
        WireEvents();
    }

    public void BindSession(AppSession session)
    {
        _session = session;
        LoadReferenceAndRois();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.WindowBackground,
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460F));

        root.Controls.Add(BuildToolPanel(), 0, 0);
        var viewerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            Padding = new Padding(1),
            Margin = new Padding(10, 0, 10, 0)
        };
        viewerPanel.Controls.Add(_canvas);
        root.Controls.Add(viewerPanel, 1, 0);
        root.Controls.Add(BuildGridPanel(), 2, 0);
        Controls.Add(root);
    }

    private Control BuildToolPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(14) };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var title = UiTheme.CreateSectionTitle("ROI 工具");
        UiTheme.AddRow(layout, title, SizeType.AutoSize, 0, new Padding(0, 0, 0, 10));

        var reference = UiTheme.CreateButton("选择参考图", true);
        reference.Dock = DockStyle.Fill;
        reference.Click += (_, _) => SelectReferenceImage();
        UiTheme.AddRow(layout, reference, SizeType.Absolute, 36);

        _referenceLabel.AutoSize = false;
        _referenceLabel.Dock = DockStyle.Fill;
        _referenceLabel.ForeColor = UiTheme.TextMuted;
        _referenceLabel.TextAlign = ContentAlignment.TopLeft;
        UiTheme.AddRow(layout, _referenceLabel, SizeType.Absolute, 52, new Padding(0, 4, 0, 4));

        var hint = UiTheme.CreateMutedText("ROI 只能画在\nreference_aligned.png\n标准坐标系上");
        hint.AutoSize = true;
        hint.TextAlign = ContentAlignment.TopLeft;
        UiTheme.AddRow(layout, hint, SizeType.AutoSize, 0, new Padding(0, 0, 0, 10));

        var buttons = new[]
        {
            ("S  应有螺丝", RoiKind.ScrewSlot),
            ("E  应为空位", RoiKind.EmptySlot),
            ("P  弹簧区域", RoiKind.SpringRegion),
            ("A  通用异常", RoiKind.AnomalyRegion)
        };
        foreach (var (text, kind) in buttons)
        {
            var button = UiTheme.CreateButton(text);
            button.Dock = DockStyle.Fill;
            button.Click += (_, _) => SetCreateMode(kind);
            UiTheme.AddRow(layout, button, SizeType.Absolute, 38, new Padding(0, 0, 0, 6));
        }

        var selectMode = UiTheme.CreateButton("选择 / 移动");
        selectMode.Dock = DockStyle.Fill;
        selectMode.Click += (_, _) =>
        {
            _canvas.PendingCreateKind = null;
            _modeLabel.Text = "模式：选择 / 移动";
        };
        UiTheme.AddRow(layout, selectMode, SizeType.Absolute, 38);

        _modeLabel.AutoSize = false;
        _modeLabel.Dock = DockStyle.Fill;
        _modeLabel.Text = "模式：选择 / 移动";
        _modeLabel.ForeColor = UiTheme.TextSecondary;
        _modeLabel.TextAlign = ContentAlignment.TopLeft;
        UiTheme.AddRow(layout, _modeLabel, SizeType.Absolute, 34, new Padding(0, 4, 0, 10));

        var help = UiTheme.CreateMutedText("滚轮：缩放\n中键/右键：平移\n拖四角：缩放 ROI\n方向键：1px\nShift+方向键：10px");
        help.Dock = DockStyle.Fill;
        help.TextAlign = ContentAlignment.TopLeft;
        UiTheme.AddRow(layout, help, SizeType.Percent, 100F, new Padding(0, 0, 0, 0));

        panel.Controls.Add(layout);
        return panel;
    }

    private Control BuildGridPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(14) };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var title = UiTheme.CreateSectionTitle("ROI 列表");
        UiTheme.AddRow(layout, title, SizeType.AutoSize, 0, new Padding(0, 0, 0, 8));

        UiTheme.StyleDataGridView(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = false;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kind", HeaderText = "Type", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expected", HeaderText = "Expected", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "X", HeaderText = "X" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Y", HeaderText = "Y" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "W", HeaderText = "W" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "H", HeaderText = "H" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "Enabled" });
        foreach (var name in new[] { "Id", "Kind", "Expected", "Enabled" })
            _grid.Columns[name].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        foreach (var name in new[] { "X", "Y", "W", "H" })
            _grid.Columns[name].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        UiTheme.AddRow(layout, _grid, SizeType.Percent, 100F, new Padding(0, 0, 0, 10));

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var duplicate = UiTheme.CreateButton("复制 ROI");
        duplicate.Size = new Size(120, 36);
        duplicate.Anchor = AnchorStyles.Left;
        duplicate.Click += (_, _) => DuplicateSelected();
        var delete = UiTheme.CreateButton("删除", false);
        delete.Size = new Size(100, 36);
        delete.Anchor = AnchorStyles.Left;
        delete.Margin = new Padding(10, 0, 0, 0);
        delete.ForeColor = UiTheme.Danger;
        delete.Click += (_, _) => DeleteSelected();
        var fit = UiTheme.CreateButton("适应窗口");
        fit.Size = new Size(120, 36);
        fit.Anchor = AnchorStyles.Right;
        fit.Click += (_, _) => _canvas.FitToView();

        buttons.Controls.Add(duplicate, 0, 0);
        buttons.Controls.Add(delete, 1, 0);
        buttons.Controls.Add(fit, 3, 0);
        UiTheme.AddRow(layout, buttons, SizeType.Absolute, 44);

        panel.Controls.Add(layout);
        return panel;
    }

    private void WireEvents()
    {
        _canvas.RoiCreated += (_, e) => CreateRoi(e.Roi);
        _canvas.RoiChanged += (_, e) => SaveRoi(e.Roi);
        _canvas.SelectionChanged += (_, e) =>
        {
            if (_syncingSelection) return;
            _syncingSelection = true;
            try { SelectGridRow(e.Roi?.Id); }
            finally { _syncingSelection = false; }
        };
        _grid.SelectionChanged += (_, _) =>
        {
            if (_syncingSelection || _grid.SelectedRows.Count == 0) return;
            _syncingSelection = true;
            try { _canvas.SelectRoi(_grid.SelectedRows[0].Cells["Id"].Value?.ToString()); }
            finally { _syncingSelection = false; }
        };
        _grid.CellEndEdit += (_, e) => ApplyGridEdit(e.RowIndex);
    }

    private void SelectReferenceImage()
    {
        if (_session is null)
        {
            MessageBox.Show(this, "请先新建或打开项目。", "Dataset Studio");
            return;
        }
        using var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*",
            Title = "选择用于对齐后标准坐标的参考图"
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
        _modeLabel.Text = $"模式：绘制 {kind}";
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
        _canvas.PendingCreateKind = null;
        _modeLabel.Text = "模式：选择 / 移动";
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
            ? $"参考图：\n{Path.GetFileName(_session.ReferenceImagePath)}"
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
