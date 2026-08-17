using DatasetStudio.Core;
using DatasetStudio.WinForms.Controls;

namespace DatasetStudio.WinForms.Pages;

public sealed class RoiCalibrationPage : UserControl
{
    private readonly ImageCanvas _canvas = new() { Dock = DockStyle.Fill, AllowRoiEditing = true };
    private readonly DataGridView _grid = new();
    private readonly Label _referenceLabel = new();
    private readonly Label _modeLabel = new();
    private readonly Dictionary<Button, (RoiKind? Kind, Color ActiveColor, Color BorderColor)> _toolButtons = new();
    private AppSession? _session;
    private List<RoiDefinition> _rois = new();
    private bool _syncingSelection;
    private RoiKind? _activeToolMode;
    private TableLayoutPanel? _workArea;

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
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.WindowBackground,
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.Controls.Add(BuildToolbar(), 0, 0);

        _workArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 12, 0, 0),
            Margin = Padding.Empty,
            BackColor = UiTheme.WindowBackground
        };
        _workArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _workArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
        var viewerPanel = UiTheme.CreateCard(new Padding(1));
        viewerPanel.Margin = new Padding(0, 0, 12, 0);
        viewerPanel.Controls.Add(_canvas);
        _workArea.Controls.Add(viewerPanel, 0, 0);
        _workArea.Controls.Add(BuildGridPanel(), 1, 0);
        root.Controls.Add(_workArea, 0, 1);
        Controls.Add(root);
        Resize += (_, _) => ApplyResponsiveLayout();
        ApplyResponsiveLayout();
    }

    internal void ApplyResponsiveLayout()
    {
        if (_workArea is null || ClientSize.Width <= 0) return;
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

    private Control BuildToolbar()
    {
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(12, 0, 8, 0),
            BackColor = UiTheme.Surface
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var tools = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        var toolsLabel = new Label
        {
            Text = "标定工具：",
            AutoSize = false,
            Size = new Size(76, 49),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextMuted,
            Font = UiTheme.CreateFont(9F, FontStyle.Bold),
            Margin = Padding.Empty
        };
        tools.Controls.Add(toolsLabel);
        tools.Controls.Add(CreateToolButton("选择 / 移动", null, UiTheme.Accent, UiTheme.Border));
        tools.Controls.Add(CreateToolButton("螺丝孔 (S)", RoiKind.ScrewSlot, UiTheme.Success, Color.FromArgb(187, 247, 208)));
        tools.Controls.Add(CreateToolButton("空位 (E)", RoiKind.EmptySlot, Color.FromArgb(217, 119, 6), Color.FromArgb(253, 230, 138)));
        tools.Controls.Add(CreateToolButton("弹簧区 (P)", RoiKind.SpringRegion, Color.FromArgb(126, 34, 206), Color.FromArgb(233, 213, 255)));
        tools.Controls.Add(CreateToolButton("异常区 (A)", RoiKind.AnomalyRegion, UiTheme.Danger, Color.FromArgb(254, 202, 202)));

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        _modeLabel.AutoSize = false;
        _modeLabel.Size = new Size(126, 49);
        _modeLabel.Text = "模式：选择 / 移动";
        _modeLabel.ForeColor = UiTheme.TextSecondary;
        _modeLabel.Font = UiTheme.CreateFont(8.5F, FontStyle.Bold);
        _modeLabel.TextAlign = ContentAlignment.MiddleRight;
        _modeLabel.Margin = Padding.Empty;

        _referenceLabel.AutoSize = false;
        _referenceLabel.Size = new Size(160, 49);
        _referenceLabel.ForeColor = UiTheme.TextMuted;
        _referenceLabel.Font = UiTheme.CreateFont(8.5F);
        _referenceLabel.TextAlign = ContentAlignment.MiddleRight;
        _referenceLabel.AutoEllipsis = true;
        _referenceLabel.Margin = new Padding(8, 0, 8, 0);

        var reference = UiTheme.CreateButton("更换参考图");
        reference.Size = new Size(118, 32);
        reference.Margin = new Padding(0, 8, 0, 0);
        reference.Click += (_, _) => SelectReferenceImage();
        actions.Controls.Add(_modeLabel);
        actions.Controls.Add(_referenceLabel);
        actions.Controls.Add(reference);

        toolbar.Controls.Add(tools, 0, 0);
        toolbar.Controls.Add(actions, 1, 0);
        return toolbar;
    }

    private Button CreateToolButton(string text, RoiKind? kind, Color activeColor, Color borderColor)
    {
        var button = UiTheme.CreateButton(text);
        button.Size = new Size(kind is RoiKind.SpringRegion or RoiKind.AnomalyRegion ? 104 : 100, 32);
        button.Margin = new Padding(0, 8, 6, 0);
        _toolButtons[button] = (kind, activeColor, borderColor);
        button.Click += (_, _) =>
        {
            if (kind.HasValue)
            {
                SetCreateMode(kind.Value);
            }
            else
            {
                _canvas.PendingCreateKind = null;
                _activeToolMode = null;
                _modeLabel.Text = "模式：选择 / 移动";
                RefreshToolButtons();
            }
        };
        RefreshToolButtons();
        return button;
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

    private Control BuildGridPanel()
    {
        var panel = UiTheme.CreateCard(new Padding(15));

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
        _grid.Columns.Add(CreateTextColumn("Id", "ID", 13F, true));
        _grid.Columns.Add(CreateTextColumn("Kind", "类别", 18F, true));
        _grid.Columns.Add(CreateTextColumn("Expected", "期望", 18F, true));
        _grid.Columns.Add(CreateTextColumn("X", "X", 9F));
        _grid.Columns.Add(CreateTextColumn("Y", "Y", 9F));
        _grid.Columns.Add(CreateTextColumn("W", "W", 9F));
        _grid.Columns.Add(CreateTextColumn("H", "H", 9F));
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Enabled",
            HeaderText = "启用",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 15F
        });
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

    private static DataGridViewTextBoxColumn CreateTextColumn(string name, string header, float fillWeight, bool readOnly = false) =>
        new()
        {
            Name = name,
            HeaderText = header,
            ReadOnly = readOnly,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = fillWeight
        };

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
        _canvas.PendingCreateKind = null;
        _activeToolMode = null;
        _modeLabel.Text = "模式：选择 / 移动";
        RefreshToolButtons();
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
