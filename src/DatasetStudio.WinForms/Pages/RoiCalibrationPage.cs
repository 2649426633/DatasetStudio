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
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 178F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430F));

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
        var title = UiTheme.CreateSectionTitle("ROI 工具");
        title.Location = new Point(14, 14);
        var reference = UiTheme.CreateButton("选择参考图", true);
        reference.Location = new Point(14, 48);
        reference.Size = new Size(148, 34);
        reference.Click += (_, _) => SelectReferenceImage();

        _referenceLabel.Location = new Point(14, 90);
        _referenceLabel.Size = new Size(148, 60);
        _referenceLabel.ForeColor = UiTheme.TextMuted;

        var hint = new Label
        {
            Text = "ROI 只能画在\nreference_aligned.png\n标准坐标系上",
            Location = new Point(14, 150),
            Size = new Size(148, 62),
            ForeColor = UiTheme.TextSecondary
        };

        var buttons = new[]
        {
            ("S  应有螺丝", RoiKind.ScrewSlot),
            ("E  应为空位", RoiKind.EmptySlot),
            ("P  弹簧区域", RoiKind.SpringRegion),
            ("A  通用异常", RoiKind.AnomalyRegion)
        };
        for (var i = 0; i < buttons.Length; i++)
        {
            var button = UiTheme.CreateButton(buttons[i].Item1);
            button.Location = new Point(14, 230 + i * 44);
            button.Size = new Size(148, 34);
            var kind = buttons[i].Item2;
            button.Click += (_, _) => SetCreateMode(kind);
            panel.Controls.Add(button);
        }

        var selectMode = UiTheme.CreateButton("选择 / 移动");
        selectMode.Location = new Point(14, 416);
        selectMode.Size = new Size(148, 34);
        selectMode.Click += (_, _) =>
        {
            _canvas.PendingCreateKind = null;
            _modeLabel.Text = "模式：选择 / 移动";
        };
        _modeLabel.Location = new Point(14, 462);
        _modeLabel.Size = new Size(148, 42);
        _modeLabel.Text = "模式：选择 / 移动";
        _modeLabel.ForeColor = UiTheme.TextSecondary;

        var help = new Label
        {
            Text = "滚轮：缩放\n中键/右键：平移\n拖四角：缩放 ROI\n方向键：1px\nShift+方向键：10px",
            Location = new Point(14, 526),
            Size = new Size(148, 110),
            ForeColor = UiTheme.TextMuted
        };

        panel.Controls.AddRange(new Control[] { title, reference, _referenceLabel, hint, selectMode, _modeLabel, help });
        return panel;
    }

    private Control BuildGridPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(14) };
        var title = UiTheme.CreateSectionTitle("ROI 列表");
        title.Location = new Point(14, 14);

        _grid.Location = new Point(14, 48);
        _grid.Size = new Size(402, 560);
        _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.ReadOnly = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.BackgroundColor = UiTheme.Surface;
        _grid.BorderStyle = BorderStyle.FixedSingle;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kind", HeaderText = "Type", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expected", HeaderText = "Expected", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "X", HeaderText = "X" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Y", HeaderText = "Y" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "W", HeaderText = "W" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "H", HeaderText = "H" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "Enabled" });

        var duplicate = UiTheme.CreateButton("复制 ROI");
        duplicate.Location = new Point(14, 624);
        duplicate.Size = new Size(120, 34);
        duplicate.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        duplicate.Click += (_, _) => DuplicateSelected();
        var delete = UiTheme.CreateButton("删除", false);
        delete.Location = new Point(144, 624);
        delete.Size = new Size(100, 34);
        delete.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        delete.ForeColor = UiTheme.Danger;
        delete.Click += (_, _) => DeleteSelected();
        var fit = UiTheme.CreateButton("适应窗口");
        fit.Location = new Point(254, 624);
        fit.Size = new Size(120, 34);
        fit.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        fit.Click += (_, _) => _canvas.FitToView();

        panel.Controls.AddRange(new Control[] { title, _grid, duplicate, delete, fit });
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
