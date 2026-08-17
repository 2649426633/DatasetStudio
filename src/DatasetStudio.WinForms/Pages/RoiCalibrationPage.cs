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
        var viewerPanel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(1), Margin = new Padding(10, 0) };
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
            Text = "滚轮：缩放\n中键/右键：平移\n方向键：1px\nShift+方向键：10px",
            Location = new Point(14, 526),
            Size = new Size(148, 90),
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
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.BackgroundColor = UiTheme.Surface;
        _grid.BorderStyle = BorderStyle.FixedSingle;
        _grid.Columns.Add("Id", "ID");
        _grid.Columns.Add("Kind", "Type");
        _grid.Columns.Add("Expected", "Expected");
        _grid.Columns.Add("X", "X");
        _grid.Columns.Add("Y", "Y");
        _grid.Columns.Add("W", "W");
        _grid.Columns.Add("H", "H");
        _grid.Columns.Add("Enabled", "Enabled");

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
        _canvas.SelectionChanged += (_, e) => SelectGridRow(e.Roi?.Id);
        _grid.SelectionChanged += (_, _) =>
        {
            if (_grid.SelectedRows.Count > 0)
                _canvas.SelectRoi(_grid.SelectedRows[0].Cells[0].Value?.ToString());
        };
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
        _session.Repository.SaveRoi(roi);
        _session.WriteProductConfig();
        ReloadRois(roi.Id);
    }

    private void DuplicateSelected()
    {
        if (_session is null || _grid.SelectedRows.Count == 0) return;
        var id = _grid.SelectedRows[0].Cells[0].Value?.ToString();
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
        _session.Repository.SaveRoi(copy);
        _session.WriteProductConfig();
        ReloadRois(copy.Id);
    }

    private void DeleteSelected()
    {
        if (_session is null || _grid.SelectedRows.Count == 0) return;
        var id = _grid.SelectedRows[0].Cells[0].Value?.ToString();
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
            var row = _grid.Rows.Add(roi.Id, roi.Kind, roi.Expected, roi.X, roi.Y, roi.Width, roi.Height, roi.Enabled ? "✓" : "");
            if (roi.Id == selectId) _grid.Rows[row].Selected = true;
        }
        if (!string.IsNullOrWhiteSpace(selectId)) _canvas.SelectRoi(selectId);
    }

    private void SelectGridRow(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (string.Equals(row.Cells[0].Value?.ToString(), id, StringComparison.OrdinalIgnoreCase))
            {
                row.Selected = true;
                _grid.CurrentCell = row.Cells[0];
                return;
            }
        }
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
            .Where(x => x.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id[prefix.Length..])
            .Select(text => int.TryParse(text, out var number) ? number : 0);
        return $"{prefix}{numbers.DefaultIfEmpty(0).Max() + 1:00}";
    }
}
