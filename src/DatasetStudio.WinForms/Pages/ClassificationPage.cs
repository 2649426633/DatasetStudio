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
    private readonly TextBox _searchBox = new() { PlaceholderText = "按文件名搜索..." };
    private readonly Button _saveNext = UiTheme.CreateButton("保存 + 下一张", true);
    private TableLayoutPanel? _rootLayout;
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
        _rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = UiTheme.WindowBackground,
            Margin = Padding.Empty
        };
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

        _rootLayout.Controls.Add(BuildImageListPanel(), 0, 0);
        _rootLayout.Controls.Add(BuildViewerPanel(), 1, 0);
        _rootLayout.Controls.Add(BuildClassificationPanel(), 2, 0);

        _stats.Dock = DockStyle.Fill;
        _stats.TextAlign = ContentAlignment.MiddleLeft;
        _stats.Padding = new Padding(14, 0, 0, 0);
        _stats.BackColor = UiTheme.Surface;
        _stats.ForeColor = UiTheme.TextSecondary;
        _stats.Font = UiTheme.CreateFont(9F, FontStyle.Bold);
        var statsCard = UiTheme.CreateCard(new Padding(1));
        statsCard.Margin = new Padding(0, 10, 0, 0);
        statsCard.Controls.Add(_stats);
        _rootLayout.Controls.Add(statsCard, 0, 1);
        _rootLayout.SetColumnSpan(statsCard, 3);
        Controls.Add(_rootLayout);
        Resize += (_, _) => ApplyResponsiveLayout();
        ApplyResponsiveLayout();
    }

    internal void ApplyResponsiveLayout()
    {
        if (_rootLayout is null || ClientSize.Width <= 0) return;
        var dpi = Math.Max(1F, DeviceDpi / 96F);
        var width = ClientSize.Width;
        var left = Math.Clamp(width * 0.22F, 250F * dpi, 340F * dpi);
        var right = Math.Clamp(width * 0.25F, 320F * dpi, 410F * dpi);
        var minimumViewer = 300F * dpi;
        if (left + right + minimumViewer > width)
        {
            var availableForSides = Math.Max(0F, width - minimumViewer);
            left = availableForSides * 0.44F;
            right = availableForSides * 0.56F;
        }

        _rootLayout.ColumnStyles[0].Width = left;
        _rootLayout.ColumnStyles[2].Width = right;
    }

    private Control BuildImageListPanel()
    {
        var card = UiTheme.CreateCard(new Padding(1));
        card.Margin = new Padding(0, 0, 10, 0);
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.Surface,
            Padding = new Padding(14),
            Margin = Padding.Empty
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = UiTheme.CreateSectionTitle("数据 / 图片列表");
        title.Dock = DockStyle.Fill;
        title.TextAlign = ContentAlignment.MiddleLeft;
        var rescan = UiTheme.CreateButton("重新扫描");
        rescan.Size = new Size(96, 30);
        rescan.Margin = new Padding(8, 0, 0, 0);
        rescan.Click += (_, _) => Rescan();
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(rescan, 1, 0);

        UiTheme.StyleTextBox(_searchBox);
        _searchBox.Dock = DockStyle.Fill;
        _searchBox.Margin = new Padding(0, 4, 0, 4);
        _searchBox.TextChanged += (_, _) => ReloadList();

        _onlyUnclassified.AutoSize = true;
        _onlyUnclassified.Dock = DockStyle.Fill;
        _onlyUnclassified.TextAlign = ContentAlignment.MiddleLeft;
        UiTheme.StyleOptionButton(_onlyUnclassified);
        _onlyUnclassified.CheckedChanged += (_, _) => ReloadList();

        _imagesList.View = View.Details;
        UiTheme.StyleListView(_imagesList, darkSelection: true);
        _imagesList.Columns.Add("文件", 150);
        _imagesList.Columns.Add("状态", 90);
        _imagesList.Dock = DockStyle.Fill;
        _imagesList.Margin = new Padding(0, 6, 0, 0);

        panel.Controls.Add(header, 0, 0);
        panel.Controls.Add(_searchBox, 0, 1);
        panel.Controls.Add(_onlyUnclassified, 0, 2);
        panel.Controls.Add(_imagesList, 0, 3);
        card.Controls.Add(panel);
        return card;
    }

    private Control BuildViewerPanel()
    {
        var panel = UiTheme.CreateCard(new Padding(1));
        panel.Margin = new Padding(0, 0, 10, 0);
        _canvas.Dock = DockStyle.Fill;
        _canvas.AllowRoiEditing = false;
        _canvas.ShowRois = true;
        panel.Controls.Add(_canvas);
        return panel;
    }

    private Control BuildClassificationPanel()
    {
        var panel = UiTheme.CreateCard(new Padding(17));

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoScroll = true,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var title = UiTheme.CreateSectionTitle("当前图片信息");
        UiTheme.AddRow(layout, title, SizeType.AutoSize, 0, new Padding(0, 0, 0, 8));

        var meta = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10, 6, 10, 6),
            Margin = Padding.Empty,
            BackColor = UiTheme.SurfaceSoft
        };
        meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        meta.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _fileName.AutoSize = false;
        _fileName.Dock = DockStyle.Fill;
        _fileName.Font = UiTheme.CreateFont(10F, FontStyle.Bold);
        _fileName.ForeColor = UiTheme.TextPrimary;
        _fileName.Text = "未选择图片";

        _pathLabel.AutoSize = false;
        _pathLabel.Dock = DockStyle.Fill;
        _pathLabel.ForeColor = UiTheme.TextMuted;
        _pathLabel.Font = UiTheme.CreateFont(8.5F);
        _pathLabel.Text = "请从左侧列表选择图片开始分类";
        meta.Controls.Add(_fileName, 0, 0);
        meta.Controls.Add(_pathLabel, 0, 1);
        UiTheme.AddRow(layout, meta, SizeType.Absolute, 64, new Padding(0, 0, 0, 10));

        var category = UiTheme.CreateFieldLabel("分类");
        UiTheme.AddRow(layout, category, SizeType.AutoSize, 0, new Padding(0, 0, 0, 4));
        UiTheme.AddRow(layout, BuildCategorySelector(), SizeType.Absolute, 82);

        var roiTitle = UiTheme.CreateFieldLabel("NG 异常 ROI");
        UiTheme.AddRow(layout, roiTitle, SizeType.AutoSize, 0, new Padding(0, 10, 0, 4));

        _roiList.CheckOnClick = true;
        UiTheme.StyleCheckedListBox(_roiList);
        _roiList.Dock = DockStyle.Fill;
        UiTheme.AddRow(layout, _roiList, SizeType.Absolute, 108, new Padding(0, 0, 0, 8));

        var defectTitle = UiTheme.CreateFieldLabel("缺陷类型");
        UiTheme.AddRow(layout, defectTitle, SizeType.AutoSize, 0, new Padding(0, 0, 0, 4));

        _defectType.DropDownStyle = ComboBoxStyle.DropDownList;
        _defectType.Dock = DockStyle.Fill;
        UiTheme.StyleComboBox(_defectType);
        _defectType.Items.AddRange(Enum.GetNames<DefectType>().Where(x => x != nameof(DefectType.None)).Cast<object>().ToArray());
        if (_defectType.Items.Count > 0) _defectType.SelectedIndex = 0;
        UiTheme.AddRow(layout, _defectType, SizeType.Absolute, 34, new Padding(0, 0, 0, 8));

        var noteTitle = UiTheme.CreateFieldLabel("备注");
        UiTheme.AddRow(layout, noteTitle, SizeType.AutoSize, 0, new Padding(0, 0, 0, 4));

        _note.Multiline = true;
        _note.ScrollBars = ScrollBars.Vertical;
        UiTheme.StyleTextBox(_note);
        _note.Dock = DockStyle.Fill;
        UiTheme.AddRow(layout, _note, SizeType.Absolute, 72, new Padding(0, 0, 0, 10));

        _saveNext.Dock = DockStyle.Fill;
        _saveNext.Margin = Padding.Empty;
        UiTheme.AddRow(layout, _saveNext, SizeType.Absolute, 40);

        var help = UiTheme.CreateMutedText("快捷键：T/G/N/I 分类 · 1-9 ROI · Enter 保存 · Space 下一张未分类 · ←/→ 切换");
        help.Dock = DockStyle.Fill;
        help.TextAlign = ContentAlignment.TopLeft;
        UiTheme.AddRow(layout, help, SizeType.Absolute, 54, new Padding(0, 10, 0, 0));

        panel.Controls.Add(layout);
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
        var query = _searchBox.Text.Trim();
        foreach (var image in _images.Where(x =>
                     (!_onlyUnclassified.Checked || !x.IsClassified) &&
                     (query.Length == 0 || x.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))))
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

    private Control BuildCategorySelector()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        ConfigureCategoryButton(_trainGood, "Train GOOD     T", Color.FromArgb(21, 128, 61));
        ConfigureCategoryButton(_testGood, "Test GOOD      G", Color.FromArgb(3, 105, 161));
        ConfigureCategoryButton(_testNg, "Test NG          N", UiTheme.Danger);
        ConfigureCategoryButton(_ignore, "Ignore             I", Color.FromArgb(75, 85, 99));

        table.Controls.Add(_trainGood, 0, 0);
        table.Controls.Add(_testGood, 1, 0);
        table.Controls.Add(_testNg, 0, 1);
        table.Controls.Add(_ignore, 1, 1);
        return table;
    }

    private static void ConfigureCategoryButton(RadioButton radio, string text, Color activeColor)
    {
        radio.Text = text;
        radio.Appearance = Appearance.Button;
        radio.AutoSize = false;
        radio.Dock = DockStyle.Fill;
        radio.Margin = new Padding(0, 0, 6, 6);
        radio.FlatStyle = FlatStyle.Flat;
        radio.TextAlign = ContentAlignment.MiddleCenter;
        radio.Font = UiTheme.CreateFont(9F, FontStyle.Bold);
        radio.Cursor = Cursors.Hand;
        radio.UseVisualStyleBackColor = false;

        void RefreshStyle()
        {
            radio.BackColor = radio.Checked ? activeColor : UiTheme.Surface;
            radio.ForeColor = radio.Checked ? Color.White : UiTheme.TextPrimary;
            radio.FlatAppearance.BorderColor = radio.Checked ? activeColor : UiTheme.Border;
            radio.FlatAppearance.MouseOverBackColor = radio.Checked ? activeColor : UiTheme.SurfaceHover;
        }

        radio.CheckedChanged += (_, _) => RefreshStyle();
        RefreshStyle();
    }

    private void UpdateStats()
    {
        if (_session is null) { _stats.Text = "尚未打开项目"; return; }
        var c = _session.Repository.GetCounts();
        _stats.Text = $"已分类 {c.Classified}/{c.Total}   |   Train GOOD {c.TrainGood}   |   Test GOOD {c.TestGood}   |   NG {c.TestNg}   |   Ignore {c.Ignored}   |   未分类 {c.Unclassified}";
    }
}
