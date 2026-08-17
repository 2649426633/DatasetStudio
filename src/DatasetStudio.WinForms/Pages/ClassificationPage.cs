using DatasetStudio.Core;
using DatasetStudio.WinForms.Controls;

namespace DatasetStudio.WinForms.Pages;

public sealed class ClassificationPage : UserControl
{
    private readonly ListView _imagesList = new();
    private readonly ImageCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly Label _fileName = new();
    private readonly Label _pathLabel = new();
    private readonly RadioButton _trainGood = new() { Text = "Train GOOD" };
    private readonly RadioButton _testGood = new() { Text = "Test GOOD" };
    private readonly RadioButton _testNg = new() { Text = "Test NG" };
    private readonly RadioButton _ignore = new() { Text = "Ignore" };
    private readonly CheckedListBox _roiList = new();
    private readonly ComboBox _defectType = new();
    private readonly TextBox _note = new();
    private readonly Label _stats = new();
    private readonly CheckBox _onlyUnclassified = new() { Text = "仅未分类" };
    private readonly Button _saveNext = UiTheme.CreateButton("保存 + 下一张", true);
    private AppSession? _session;
    private List<ImageRecord> _images = new();
    private bool _loading;

    public ClassificationPage()
    {
        BackColor = UiTheme.WindowBackground;
        BuildLayout();
        WireEvents();
    }

    public void BindSession(AppSession session)
    {
        _session = session;
        session.Repository.ScanSourceDirectory(session.Project.SourceDirectory);
        ReloadRois();
        ReloadImages();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (!Visible || _session is null || _note.Focused)
            return base.ProcessCmdKey(ref msg, keyData);

        var key = keyData & Keys.KeyCode;
        if (key == Keys.T) { _trainGood.Checked = true; return true; }
        if (key == Keys.G) { _testGood.Checked = true; return true; }
        if (key == Keys.N) { _testNg.Checked = true; return true; }
        if (key == Keys.I) { _ignore.Checked = true; return true; }
        if (key == Keys.Enter) { TrySaveCurrent(true); return true; }
        if (key == Keys.Space) { MoveNext(true); return true; }
        if (key == Keys.Left) { MoveRelative(-1); return true; }
        if (key == Keys.Right) { MoveRelative(1); return true; }
        if (key is >= Keys.D1 and <= Keys.D9)
        {
            var index = key - Keys.D1;
            if (index < _roiList.Items.Count)
                _roiList.SetItemChecked(index, !_roiList.GetItemChecked(index));
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = UiTheme.WindowBackground,
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        root.Controls.Add(BuildImageListPanel(), 0, 0);
        root.Controls.Add(BuildViewerPanel(), 1, 0);
        root.Controls.Add(BuildClassificationPanel(), 2, 0);

        _stats.Dock = DockStyle.Fill;
        _stats.TextAlign = ContentAlignment.MiddleLeft;
        _stats.Padding = new Padding(12, 0, 0, 0);
        _stats.BackColor = UiTheme.Surface;
        _stats.ForeColor = UiTheme.TextSecondary;
        root.Controls.Add(_stats, 0, 1);
        root.SetColumnSpan(_stats, 3);
        Controls.Add(root);
    }

    private Control BuildImageListPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(12), Margin = new Padding(0, 0, 10, 0) };
        var title = UiTheme.CreateSectionTitle("数据 / 图片列表");
        title.Location = new Point(12, 12);
        var rescan = UiTheme.CreateButton("重新扫描");
        rescan.Size = new Size(88, 30);
        rescan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        rescan.Location = new Point(173, 8);
        rescan.Click += (_, _) => Rescan();
        _onlyUnclassified.AutoSize = true;
        _onlyUnclassified.Location = new Point(12, 45);
        _onlyUnclassified.CheckedChanged += (_, _) => ReloadList();

        _imagesList.View = View.Details;
        _imagesList.FullRowSelect = true;
        _imagesList.HideSelection = false;
        _imagesList.MultiSelect = false;
        _imagesList.BorderStyle = BorderStyle.FixedSingle;
        _imagesList.Columns.Add("文件", 155);
        _imagesList.Columns.Add("状态", 96);
        _imagesList.Location = new Point(12, 72);
        _imagesList.Size = new Size(249, 500);
        _imagesList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        panel.Controls.Add(title);
        panel.Controls.Add(rescan);
        panel.Controls.Add(_onlyUnclassified);
        panel.Controls.Add(_imagesList);
        return panel;
    }

    private Control BuildViewerPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(1), Margin = new Padding(0, 0, 10, 0) };
        _canvas.Dock = DockStyle.Fill;
        _canvas.AllowRoiEditing = false;
        _canvas.ShowRois = true;
        panel.Controls.Add(_canvas);
        return panel;
    }

    private Control BuildClassificationPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(16) };
        var title = UiTheme.CreateSectionTitle("当前图片信息");
        title.Location = new Point(16, 16);
        _fileName.Location = new Point(16, 48);
        _fileName.Size = new Size(300, 22);
        _fileName.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _pathLabel.Location = new Point(16, 72);
        _pathLabel.Size = new Size(300, 42);
        _pathLabel.ForeColor = UiTheme.TextMuted;

        var category = new Label { Text = "分类", Location = new Point(16, 124), AutoSize = true, ForeColor = UiTheme.TextSecondary };
        var radios = new[] { _trainGood, _testGood, _testNg, _ignore };
        for (var i = 0; i < radios.Length; i++)
        {
            radios[i].Location = new Point(20, 150 + i * 30);
            radios[i].AutoSize = true;
            radios[i].Font = new Font("Microsoft YaHei UI", 9.5F);
        }

        var roiTitle = new Label { Text = "NG 异常 ROI", Location = new Point(16, 280), AutoSize = true, ForeColor = UiTheme.TextSecondary };
        _roiList.Location = new Point(16, 305);
        _roiList.Size = new Size(300, 120);
        _roiList.CheckOnClick = true;
        _roiList.BorderStyle = BorderStyle.FixedSingle;

        var defectTitle = new Label { Text = "缺陷类型", Location = new Point(16, 438), AutoSize = true, ForeColor = UiTheme.TextSecondary };
        _defectType.DropDownStyle = ComboBoxStyle.DropDownList;
        _defectType.Location = new Point(16, 463);
        _defectType.Size = new Size(300, 30);
        _defectType.Items.AddRange(Enum.GetNames<DefectType>().Where(x => x != nameof(DefectType.None)).Cast<object>().ToArray());
        if (_defectType.Items.Count > 0) _defectType.SelectedIndex = 0;

        var noteTitle = new Label { Text = "备注", Location = new Point(16, 506), AutoSize = true, ForeColor = UiTheme.TextSecondary };
        _note.Location = new Point(16, 530);
        _note.Size = new Size(300, 66);
        _note.Multiline = true;
        _note.ScrollBars = ScrollBars.Vertical;
        _saveNext.Location = new Point(16, 612);
        _saveNext.Size = new Size(300, 38);
        _saveNext.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        var help = new Label
        {
            Text = "快捷键：T/G/N/I 分类 · 1-9 ROI · Enter 保存 · Space 下一张未分类 · ←/→ 切换",
            Location = new Point(16, 660),
            Size = new Size(300, 58),
            ForeColor = UiTheme.TextMuted,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        panel.Controls.AddRange(new Control[]
        {
            title, _fileName, _pathLabel, category,
            _trainGood, _testGood, _testNg, _ignore,
            roiTitle, _roiList, defectTitle, _defectType,
            noteTitle, _note, _saveNext, help
        });
        return panel;
    }

    private void WireEvents()
    {
        _imagesList.SelectedIndexChanged += (_, _) => ShowSelectedImage();
        _saveNext.Click += (_, _) => TrySaveCurrent(true);
        _testNg.CheckedChanged += (_, _) => UpdateNgControls();
        _trainGood.CheckedChanged += (_, _) => UpdateNgControls();
        _testGood.CheckedChanged += (_, _) => UpdateNgControls();
        _ignore.CheckedChanged += (_, _) => UpdateNgControls();
    }

    private void Rescan()
    {
        if (_session is null) return;
        try
        {
            var added = _session.Repository.ScanSourceDirectory(_session.Project.SourceDirectory);
            ReloadImages();
            MessageBox.Show(this, $"扫描完成，新加入 {added} 张图片。", "Dataset Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "扫描失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ReloadImages(long? selectId = null)
    {
        if (_session is null) return;
        _images = _session.Repository.LoadImages();
        ReloadList(selectId);
        UpdateStats();
    }

    private void ReloadList(long? selectId = null)
    {
        _imagesList.BeginUpdate();
        _imagesList.Items.Clear();
        foreach (var image in _images.Where(x => !_onlyUnclassified.Checked || !x.IsClassified))
        {
            var item = new ListViewItem(image.FileName) { Tag = image };
            item.SubItems.Add(image.StatusText);
            _imagesList.Items.Add(item);
            if (selectId.HasValue && image.Id == selectId.Value) item.Selected = true;
        }
        _imagesList.EndUpdate();
        if (_imagesList.SelectedItems.Count == 0 && _imagesList.Items.Count > 0)
            _imagesList.Items[0].Selected = true;
    }

    private void ReloadRois()
    {
        if (_session is null) return;
        var rois = _session.Repository.LoadRois();
        _roiList.Items.Clear();
        foreach (var roi in rois.Where(x => x.Enabled)) _roiList.Items.Add(roi.Id);
        _canvas.SetRois(rois);
    }

    private void ShowSelectedImage()
    {
        if (_loading || _imagesList.SelectedItems.Count == 0) return;
        if (_imagesList.SelectedItems[0].Tag is not ImageRecord image) return;
        _loading = true;
        try
        {
            _fileName.Text = image.FileName;
            _pathLabel.Text = image.SourcePath;
            _canvas.LoadImage(image.SourcePath);
            if (_session is not null && image.Width == 0 && _canvas.ImageSize.Width > 0)
            {
                _session.Repository.UpdateImageDimensions(image.Id, _canvas.ImageSize.Width, _canvas.ImageSize.Height);
                image.Width = _canvas.ImageSize.Width;
                image.Height = _canvas.ImageSize.Height;
            }

            _trainGood.Checked = image.Split == DatasetSplit.Train && image.Truth == ImageTruth.Good;
            _testGood.Checked = image.Split == DatasetSplit.Test && image.Truth == ImageTruth.Good;
            _testNg.Checked = image.Split == DatasetSplit.Test && image.Truth == ImageTruth.Ng;
            _ignore.Checked = image.Split == DatasetSplit.Ignore || image.Truth == ImageTruth.Ignore;
            for (var i = 0; i < _roiList.Items.Count; i++)
                _roiList.SetItemChecked(i, image.GetDefectRoiIds().Contains(_roiList.Items[i]?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase));
            if (image.DefectType != DefectType.None)
                _defectType.SelectedItem = image.DefectType.ToString();
            _note.Text = image.Note;
        }
        finally
        {
            _loading = false;
            UpdateNgControls();
        }
    }

    private void TrySaveCurrent(bool moveNext)
    {
        if (_session is null || _imagesList.SelectedItems.Count == 0) return;
        if (_imagesList.SelectedItems[0].Tag is not ImageRecord image) return;

        DatasetSplit split;
        ImageTruth truth;
        DefectType defectType = DefectType.None;
        var rois = Array.Empty<string>();

        if (_trainGood.Checked) { split = DatasetSplit.Train; truth = ImageTruth.Good; }
        else if (_testGood.Checked) { split = DatasetSplit.Test; truth = ImageTruth.Good; }
        else if (_testNg.Checked)
        {
            split = DatasetSplit.Test;
            truth = ImageTruth.Ng;
            rois = _roiList.CheckedItems.Cast<object>().Select(x => x.ToString()!).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (rois.Length == 0)
            {
                MessageBox.Show(this, "Test NG 必须至少选择一个异常 ROI。", "标签不完整", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            defectType = Enum.TryParse<DefectType>(_defectType.SelectedItem?.ToString(), out var parsed) ? parsed : DefectType.Other;
        }
        else if (_ignore.Checked) { split = DatasetSplit.Ignore; truth = ImageTruth.Ignore; }
        else
        {
            MessageBox.Show(this, "请先选择 Train GOOD / Test GOOD / Test NG / Ignore。", "未选择分类", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var currentIndex = _images.FindIndex(x => x.Id == image.Id);
        _session.Repository.UpdateClassification(image.Id, split, truth, defectType, rois, _note.Text.Trim());
        ReloadImages();
        ReloadRois();
        if (moveNext) SelectBestNext(currentIndex);
    }

    private void SelectBestNext(int previousIndex)
    {
        if (_imagesList.Items.Count == 0) return;
        var nextUnclassified = _imagesList.Items.Cast<ListViewItem>()
            .FirstOrDefault(item => item.Tag is ImageRecord record && !record.IsClassified);
        if (nextUnclassified is not null)
        {
            nextUnclassified.Selected = true;
            nextUnclassified.EnsureVisible();
            return;
        }
        var index = Math.Clamp(previousIndex, 0, _imagesList.Items.Count - 1);
        _imagesList.Items[index].Selected = true;
        _imagesList.Items[index].EnsureVisible();
    }

    private void MoveNext(bool onlyUnclassified)
    {
        if (_imagesList.Items.Count == 0) return;
        var start = _imagesList.SelectedIndices.Count > 0 ? _imagesList.SelectedIndices[0] : -1;
        for (var offset = 1; offset <= _imagesList.Items.Count; offset++)
        {
            var index = (start + offset) % _imagesList.Items.Count;
            if (!onlyUnclassified || _imagesList.Items[index].Tag is ImageRecord record && !record.IsClassified)
            {
                _imagesList.Items[index].Selected = true;
                _imagesList.Items[index].EnsureVisible();
                return;
            }
        }
    }

    private void MoveRelative(int delta)
    {
        if (_imagesList.Items.Count == 0) return;
        var current = _imagesList.SelectedIndices.Count > 0 ? _imagesList.SelectedIndices[0] : 0;
        var next = Math.Clamp(current + delta, 0, _imagesList.Items.Count - 1);
        _imagesList.Items[next].Selected = true;
        _imagesList.Items[next].EnsureVisible();
    }

    private void UpdateNgControls()
    {
        var enabled = _testNg.Checked;
        _roiList.Enabled = enabled;
        _defectType.Enabled = enabled;
    }

    private void UpdateStats()
    {
        if (_session is null) { _stats.Text = "尚未打开项目"; return; }
        var c = _session.Repository.GetCounts();
        _stats.Text = $"已分类 {c.Classified}/{c.Total}   |   Train GOOD {c.TrainGood}   |   Test GOOD {c.TestGood}   |   NG {c.TestNg}   |   Ignore {c.Ignored}   |   未分类 {c.Unclassified}";
    }
}
