using DatasetStudio.Core;
using DatasetStudio.WinForms.Services;

namespace DatasetStudio.WinForms.Pages;

public sealed partial class ClassificationPage : UserControl
{
    private readonly SemaphoreSlim _alignmentGate = new(1, 1);
    private AppSession? _session;
    private List<ImageRecord> _images = new();
    private List<RoiDefinition> _rois = new();
    private AlignmentPreviewResult? _currentAlignment;
    private CancellationTokenSource? _scanCts;
    private CancellationTokenSource? _alignmentCts;
    private long _previewRequestId;
    private string _scanStatus = string.Empty;
    private bool _showRaw;
    private bool _loading;

    public ClassificationPage()
    {
        InitializeComponent();
        ConfigureRuntimeStyles();
        UpdateNgControls();
        ApplyResponsiveLayout();
        Disposed += (_, _) => CancelBackgroundWork();
    }

    public void BindSession(AppSession session)
    {
        var sessionChanged = !ReferenceEquals(_session, session);
        if (sessionChanged)
            CancelBackgroundWork();

        _session = session;
        _currentAlignment = null;
        _showRaw = false;
        ReloadRois();
        ReloadImages();

        // Opening a project must never wait for a full source-directory SHA scan.
        // Existing catalog content is shown immediately; source reconciliation runs behind it.
        if (sessionChanged)
            StartBackgroundScan(session);
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
        if (key == Keys.V)
        {
            _showRaw = !_showRaw;
            if (_imagesList.SelectedItems.Count > 0 &&
                _imagesList.SelectedItems[0].Tag is ImageRecord selected)
            {
                RefreshImagePreview(selected);
            }
            return true;
        }
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

    internal void ApplyResponsiveLayout()
    {
        if (ClientSize.Width <= 0 || _rootLayout.ColumnStyles.Count < 3) return;
        var dpi = Math.Max(1F, DeviceDpi / 96F);
        var width = ClientSize.Width;
        var left = Math.Clamp(width * 0.22F, 270F * dpi, 400F * dpi);
        var right = Math.Clamp(width * 0.25F, 320F * dpi, 420F * dpi);
        var minimumViewer = 300F * dpi;
        if (left + right + minimumViewer > width)
        {
            var availableForSides = Math.Max(0F, width - minimumViewer);
            left = availableForSides * 0.46F;
            right = availableForSides * 0.54F;
        }

        _rootLayout.ColumnStyles[0].Width = left;
        _rootLayout.ColumnStyles[2].Width = right;

        var listWidth = Math.Max(0, _imagesList.ClientSize.Width);
        if (listWidth > 160 && _imagesList.Columns.Count >= 2)
        {
            var statusWidth = (int)Math.Round(96F * dpi);
            _imagesList.Columns[1].Width = statusWidth;
            _imagesList.Columns[0].Width = Math.Max((int)(120F * dpi), listWidth - statusWidth - 6);
        }
    }

    private void ConfigureRuntimeStyles()
    {
        UiTheme.StyleListView(_imagesList, darkSelection: true);
        UiTheme.StyleTextBox(_searchBox);
        UiTheme.StyleOptionButton(_onlyUnclassified);
        UiTheme.StyleCheckedListBox(_roiList);
        UiTheme.StyleComboBox(_defectType);
        UiTheme.StyleTextBox(_note);

        if (_defectType.Items.Count == 0)
        {
            _defectType.Items.AddRange(Enum.GetNames<DefectType>()
                .Where(x => x != nameof(DefectType.None))
                .Cast<object>()
                .ToArray());
            if (_defectType.Items.Count > 0) _defectType.SelectedIndex = 0;
        }

        RefreshCategoryButtonStyles();
    }

    private void ClassificationPage_Resize(object? sender, EventArgs e) => ApplyResponsiveLayout();
    private async void RescanButton_Click(object? sender, EventArgs e) => await ScanSourceDirectoryAsync(showCompletionMessage: true);
    private void SearchBox_TextChanged(object? sender, EventArgs e) => ReloadList();
    private void OnlyUnclassified_CheckedChanged(object? sender, EventArgs e) => ReloadList();
    private async void ImagesList_SelectedIndexChanged(object? sender, EventArgs e) => await ShowSelectedImageAsync();
    private void SaveNext_Click(object? sender, EventArgs e) => TrySaveCurrent(true);

    private void Category_CheckedChanged(object? sender, EventArgs e)
    {
        RefreshCategoryButtonStyles();
        UpdateNgControls();

        if (_loading || sender is not RadioButton option || !option.Checked)
            return;

        // NG still needs one or more ROI selections before it can be saved reliably.
        if (ReferenceEquals(option, _testNg))
            return;

        BeginInvoke(new Action(() =>
        {
            if (_loading || !option.Checked || _testNg.Checked)
                return;
            TrySaveCurrent(true);
        }));
    }

    private void RefreshCategoryButtonStyles()
    {
        ApplyCategoryStyle(_trainGood, Color.FromArgb(21, 128, 61));
        ApplyCategoryStyle(_testGood, Color.FromArgb(3, 105, 161));
        ApplyCategoryStyle(_testNg, UiTheme.Danger);
        ApplyCategoryStyle(_ignore, Color.FromArgb(75, 85, 99));
    }

    private static void ApplyCategoryStyle(RadioButton radio, Color activeColor)
    {
        radio.BackColor = radio.Checked ? activeColor : UiTheme.Surface;
        radio.ForeColor = radio.Checked ? Color.White : UiTheme.TextPrimary;
        radio.FlatAppearance.BorderColor = radio.Checked ? activeColor : UiTheme.Border;
        radio.FlatAppearance.MouseOverBackColor = radio.Checked ? activeColor : UiTheme.SurfaceHover;
    }

    private void StartBackgroundScan(AppSession session)
    {
        _ = ScanSourceDirectoryAsync(showCompletionMessage: false, expectedSession: session);
    }

    private async Task ScanSourceDirectoryAsync(
        bool showCompletionMessage,
        AppSession? expectedSession = null)
    {
        var session = expectedSession ?? _session;
        if (session is null) return;

        _scanCts?.Cancel();
        _scanCts?.Dispose();
        var cts = new CancellationTokenSource();
        _scanCts = cts;
        _scanStatus = "后台扫描源目录…";
        UpdateStats();

        var selectedId = GetSelectedImageId();
        try
        {
            var added = await session.Repository.ScanSourceDirectoryAsync(
                session.Project.SourceDirectory,
                cts.Token);

            if (cts.IsCancellationRequested || !ReferenceEquals(session, _session))
                return;

            _scanStatus = added > 0
                ? $"扫描完成，新加入 {added} 张"
                : "扫描完成";
            ReloadImages(selectedId);

            if (showCompletionMessage)
            {
                MessageBox.Show(
                    this,
                    $"扫描完成，新加入 {added} 张图片。",
                    "Dataset Studio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (OperationCanceledException)
        {
            // Switching project or restarting a scan intentionally cancels the old scan.
        }
        catch (Exception ex)
        {
            if (!ReferenceEquals(session, _session)) return;
            _scanStatus = $"后台扫描失败：{ex.Message}";
            UpdateStats();
            if (showCompletionMessage)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "扫描失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        finally
        {
            if (ReferenceEquals(_scanCts, cts))
            {
                _scanCts.Dispose();
                _scanCts = null;
                UpdateStats();
            }
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
        try
        {
            _imagesList.Items.Clear();
            var query = _searchBox.Text.Trim();
            foreach (var image in _images.Where(x =>
                         (!_onlyUnclassified.Checked || !x.IsClassified) &&
                         (query.Length == 0 || x.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))))
            {
                var item = new ListViewItem(image.FileName) { Tag = image };
                item.SubItems.Add(image.StatusText);
                _imagesList.Items.Add(item);
                if (selectId.HasValue && image.Id == selectId.Value)
                    item.Selected = true;
            }
        }
        finally
        {
            _imagesList.EndUpdate();
        }

        if (_imagesList.SelectedItems.Count == 0 && _imagesList.Items.Count > 0)
            _imagesList.Items[0].Selected = true;
    }

    private void ReloadRois()
    {
        if (_session is null) return;
        _rois = _session.Repository.LoadRois();
        _roiList.Items.Clear();
        foreach (var roi in _rois.Where(x => x.Enabled))
            _roiList.Items.Add(roi.Id);

        ApplyRoisForCurrentPreview();
    }

    private async Task ShowSelectedImageAsync()
    {
        if (_imagesList.SelectedItems.Count == 0) return;
        if (_imagesList.SelectedItems[0].Tag is not ImageRecord image) return;
        var session = _session;
        if (session is null) return;

        _alignmentCts?.Cancel();
        _alignmentCts?.Dispose();
        var cts = new CancellationTokenSource();
        _alignmentCts = cts;
        var requestId = Interlocked.Increment(ref _previewRequestId);

        _loading = true;
        try
        {
            _fileName.Text = image.FileName;
            _currentAlignment = null;

            // Show the raw image immediately. Expensive SIFT/ECC work happens later in the background.
            _canvas.LoadImage(image.SourcePath);
            _canvas.SetRois(Array.Empty<RoiDefinition>());

            if (image.Width == 0 && _canvas.ImageSize.Width > 0)
            {
                session.Repository.UpdateImageDimensions(
                    image.Id,
                    _canvas.ImageSize.Width,
                    _canvas.ImageSize.Height);
                image.Width = _canvas.ImageSize.Width;
                image.Height = _canvas.ImageSize.Height;
            }

            _trainGood.Checked = image.Split == DatasetSplit.Train && image.Truth == ImageTruth.Good;
            _testGood.Checked = image.Split == DatasetSplit.Test && image.Truth == ImageTruth.Good;
            _testNg.Checked = image.Split == DatasetSplit.Test && image.Truth == ImageTruth.Ng;
            _ignore.Checked = image.Split == DatasetSplit.Ignore || image.Truth == ImageTruth.Ignore;
            for (var i = 0; i < _roiList.Items.Count; i++)
            {
                _roiList.SetItemChecked(
                    i,
                    image.GetDefectRoiIds().Contains(
                        _roiList.Items[i]?.ToString() ?? string.Empty,
                        StringComparer.OrdinalIgnoreCase));
            }

            if (image.DefectType != DefectType.None)
                _defectType.SelectedItem = image.DefectType.ToString();
            _note.Text = image.Note;

            _pathLabel.Text = File.Exists(session.ReferenceImagePath)
                ? $"{image.SourcePath}   |   正在后台配准…（界面可继续操作）"
                : $"{image.SourcePath}   |   尚未设置 reference_aligned.png，当前仅显示原图";
        }
        catch (Exception ex)
        {
            _currentAlignment = new AlignmentPreviewResult
            {
                Success = false,
                Method = "failed",
                Error = ex.Message
            };
            _pathLabel.Text = $"{image.SourcePath}   |   图片加载失败：{ex.Message}";
        }
        finally
        {
            _loading = false;
            UpdateNgControls();
            RefreshCategoryButtonStyles();
        }

        if (!File.Exists(session.ReferenceImagePath) || cts.IsCancellationRequested)
            return;

        try
        {
            var result = await RunAlignmentAsync(session, image, cts.Token);
            if (cts.IsCancellationRequested ||
                requestId != _previewRequestId ||
                !ReferenceEquals(session, _session) ||
                !IsCurrentSelection(image.Id))
            {
                return;
            }

            _currentAlignment = result;
            RefreshImagePreview(image);
        }
        catch (OperationCanceledException)
        {
            // Rapid navigation intentionally abandons stale preview requests.
        }
        catch (Exception ex)
        {
            if (requestId != _previewRequestId ||
                !ReferenceEquals(session, _session) ||
                !IsCurrentSelection(image.Id))
            {
                return;
            }

            _currentAlignment = new AlignmentPreviewResult
            {
                Success = false,
                Method = "failed",
                Error = ex.Message
            };
            RefreshImagePreview(image);
        }
        finally
        {
            if (ReferenceEquals(_alignmentCts, cts))
            {
                _alignmentCts.Dispose();
                _alignmentCts = null;
            }
        }
    }

    private async Task<AlignmentPreviewResult> RunAlignmentAsync(
        AppSession session,
        ImageRecord image,
        CancellationToken cancellationToken)
    {
        await _alignmentGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () => session.GetAlignmentPreview(image.SourcePath, image.Sha256))
                .ConfigureAwait(false);
        }
        finally
        {
            _alignmentGate.Release();
        }
    }

    private void RefreshImagePreview(ImageRecord image)
    {
        var alignedOk =
            _currentAlignment?.Success == true &&
            !string.IsNullOrWhiteSpace(_currentAlignment.AlignedPath) &&
            File.Exists(_currentAlignment.AlignedPath);

        if (!_showRaw && alignedOk)
        {
            _canvas.LoadImage(_currentAlignment!.AlignedPath);
            _canvas.SetRois(_rois);
            _pathLabel.Text =
                $"{image.SourcePath}   |   对齐预览（V 切换原图）   |   {_currentAlignment.Summary}";
        }
        else
        {
            _canvas.LoadImage(image.SourcePath);
            _canvas.SetRois(Array.Empty<RoiDefinition>());

            if (_showRaw && alignedOk)
            {
                _pathLabel.Text =
                    $"{image.SourcePath}   |   原图（V 切换对齐图）   |   {_currentAlignment!.Summary}";
            }
            else if (_currentAlignment is null)
            {
                var hasReference = _session is not null && File.Exists(_session.ReferenceImagePath);
                _pathLabel.Text = hasReference
                    ? $"{image.SourcePath}   |   正在后台配准…（界面可继续操作）"
                    : $"{image.SourcePath}   |   尚未设置 reference_aligned.png，当前仅显示原图";
            }
            else
            {
                _pathLabel.Text =
                    $"{image.SourcePath}   |   配准失败：{_currentAlignment.Error}";
            }
        }

        UpdateNgControls();
    }

    private void ApplyRoisForCurrentPreview()
    {
        if (_showRaw || _currentAlignment?.Success != true)
            _canvas.SetRois(Array.Empty<RoiDefinition>());
        else
            _canvas.SetRois(_rois);
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
            if (_currentAlignment?.Success != true)
            {
                MessageBox.Show(
                    this,
                    "Test NG 需要先成功配准到 reference_aligned.png，才能可靠选择标准坐标 ROI。\n\n配准正在后台运行时界面不会卡住；请等当前图片出现 ROI 后再标 NG，或将无法使用的图片设为 Ignore。",
                    "配准未通过",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            split = DatasetSplit.Test;
            truth = ImageTruth.Ng;
            rois = _roiList.CheckedItems
                .Cast<object>()
                .Select(x => x.ToString()!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
            if (rois.Length == 0)
            {
                MessageBox.Show(this, "Test NG 必须至少选择一个异常 ROI。", "标签不完整", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            defectType = Enum.TryParse<DefectType>(_defectType.SelectedItem?.ToString(), out var parsed)
                ? parsed
                : DefectType.Other;
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
            if (!onlyUnclassified ||
                _imagesList.Items[index].Tag is ImageRecord record && !record.IsClassified)
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

    private bool IsCurrentSelection(long imageId) =>
        _imagesList.SelectedItems.Count > 0 &&
        _imagesList.SelectedItems[0].Tag is ImageRecord current &&
        current.Id == imageId;

    private long? GetSelectedImageId() =>
        _imagesList.SelectedItems.Count > 0 &&
        _imagesList.SelectedItems[0].Tag is ImageRecord current
            ? current.Id
            : null;

    private void CancelBackgroundWork()
    {
        _scanCts?.Cancel();
        _scanCts?.Dispose();
        _scanCts = null;

        _alignmentCts?.Cancel();
        _alignmentCts?.Dispose();
        _alignmentCts = null;
        Interlocked.Increment(ref _previewRequestId);
    }

    private void UpdateNgControls()
    {
        var enabled = _testNg.Checked && _currentAlignment?.Success == true;
        _roiList.Enabled = enabled;
        _defectType.Enabled = enabled;
    }

    private void UpdateStats()
    {
        if (_session is null)
        {
            _stats.Text = "尚未打开项目";
            return;
        }

        var c = _session.Repository.GetCounts();
        var prefix = string.IsNullOrWhiteSpace(_scanStatus) ? string.Empty : _scanStatus + "   |   ";
        _stats.Text = prefix +
            $"已分类 {c.Classified}/{c.Total}   |   Train GOOD {c.TrainGood}   |   Test GOOD {c.TestGood}   |   NG {c.TestNg}   |   Ignore {c.Ignored}   |   未分类 {c.Unclassified}";
    }
}
