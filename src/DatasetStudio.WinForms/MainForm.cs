using DatasetStudio.WinForms.Pages;

namespace DatasetStudio.WinForms;

public sealed class MainForm : Form
{
    private readonly Panel _content = new();
    private readonly Label _currentProject = new();
    private readonly Button _btnClassification;
    private readonly Button _btnRoi;
    private readonly Button _btnValidation;
    private readonly Button _btnExport;
    private readonly ClassificationPage _classificationPage = new();
    private readonly RoiCalibrationPage _roiPage = new();
    private readonly ValidationPage _validationPage = new();
    private readonly ExportPage _exportPage = new();
    private TableLayoutPanel? _headerLayout;
    private FlowLayoutPanel? _navigation;
    private readonly Dictionary<Control, FontSpec> _responsiveFonts = new();
    private readonly Dictionary<Control, Font> _generatedFonts = new();
    private readonly Dictionary<DataGridView, (FontSpec Header, FontSpec Cells, FontSpec Alternating)> _gridFonts = new();
    private readonly Dictionary<DataGridView, (Font Header, Font Cells, Font Alternating)> _generatedGridFonts = new();
    private Button? _btnNewProject;
    private Button? _btnOpenProject;
    private float _appliedFontScale = -1F;
    private AppSession? _session;

    public MainForm()
    {
        Text = "科准 Dataset Studio 数据集工具";
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(860, 520);
        BackColor = UiTheme.WindowBackground;
        Font = UiTheme.CreateFont(10F);
        KeyPreview = true;

        _btnClassification = CreateNavButton("数据集分类", true);
        _btnRoi = CreateNavButton("ROI 标定", false);
        _btnValidation = CreateNavButton("数据校验", false);
        _btnExport = CreateNavButton("导出 / 发布", false);

        BuildLayout();
        CaptureResponsiveFonts(this);
        WireEvents();
        ShowPage(_classificationPage, _btnClassification);
        Resize += (_, _) => ApplyResponsiveLayout();
        ApplyResponsiveLayout();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Z) && _session is not null)
        {
            // 备注文本框聚焦时保留 WinForms 自己的文本撤销；其他位置才撤销分类。
            if (FindFocusedControl(this) is TextBoxBase)
                return base.ProcessCmdKey(ref msg, keyData);

            try
            {
                var imageId = _session.Repository.UndoLastClassification();
                if (imageId is null)
                {
                    System.Media.SystemSounds.Beep.Play();
                }
                else
                {
                    _classificationPage.BindSession(_session);
                    _currentProject.Text = $"当前项目：{_session.Project.Name}\n已撤销上一笔分类";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "撤销失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = UiTheme.WindowBackground
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty
        };
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
        _headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));

        // Compact lock-up matching the reference interface: dark mark, clear
        // product name and a deliberately quieter subtitle.
        var brand = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 0, 0, 0),
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface,
            ColumnCount = 2,
            RowCount = 1
        };
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F));
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var brandMark = new Label
        {
            Text = "KZ",
            AutoSize = false,
            Size = new Size(32, 32),
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = UiTheme.Accent,
            ForeColor = Color.White,
            Font = UiTheme.CreateFont(8.5F, FontStyle.Bold)
        };
        var brandText = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        brandText.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        brandText.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        var brandTitle = new Label
        {
            Text = "科准 DATASET STUDIO",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = UiTheme.TextPrimary,
            Font = UiTheme.CreateFont(10F, FontStyle.Bold)
        };
        var brandSubtitle = new Label
        {
            Text = "INDUSTRIAL VISION TOOL",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = UiTheme.TextMuted,
            Font = UiTheme.CreateFont(7.5F)
        };
        brandText.Controls.Add(brandTitle, 0, 0);
        brandText.Controls.Add(brandSubtitle, 0, 1);
        brand.Controls.Add(brandMark, 0, 0);
        brand.Controls.Add(brandText, 1, 0);

        _navigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        _navigation.Controls.AddRange(new Control[] { _btnClassification, _btnRoi, _btnValidation, _btnExport });

        _currentProject.Text = "当前项目：未打开\n就绪";
        _currentProject.Dock = DockStyle.Fill;
        _currentProject.TextAlign = ContentAlignment.MiddleRight;
        _currentProject.Padding = new Padding(0, 0, 12, 0);
        _currentProject.ForeColor = UiTheme.TextSecondary;
        _currentProject.Font = UiTheme.CreateFont(8.5F, FontStyle.Bold);
        _currentProject.AutoEllipsis = true;

        var projectButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 13, 10, 0),
            BackColor = UiTheme.Surface
        };
        _btnNewProject = UiTheme.CreateButton("新建项目", true);
        _btnNewProject.Width = 96;
        _btnOpenProject = UiTheme.CreateButton("打开项目");
        _btnOpenProject.Width = 96;
        _btnNewProject.Click += (_, _) => CreateProject();
        _btnOpenProject.Click += (_, _) => OpenProject();
        projectButtons.Controls.Add(_btnNewProject);
        projectButtons.Controls.Add(_btnOpenProject);

        var separator = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Border, Margin = Padding.Empty };
        _headerLayout.Controls.Add(brand, 0, 0);
        _headerLayout.Controls.Add(_navigation, 1, 0);
        _headerLayout.Controls.Add(_currentProject, 2, 0);
        _headerLayout.Controls.Add(projectButtons, 3, 0);
        _headerLayout.Controls.Add(separator, 0, 1);
        _headerLayout.SetColumnSpan(separator, 4);

        _content.Dock = DockStyle.Fill;
        _content.BackColor = UiTheme.WindowBackground;
        _content.Padding = new Padding(12);
        foreach (var page in new Control[] { _classificationPage, _roiPage, _validationPage, _exportPage })
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _content.Controls.Add(page);
        }

        root.Controls.Add(_headerLayout, 0, 0);
        root.Controls.Add(_content, 0, 1);
        Controls.Add(root);
        ApplyResponsiveLayout();
    }

    private void WireEvents()
    {
        _btnClassification.Click += (_, _) => ShowPage(_classificationPage, _btnClassification);
        _btnRoi.Click += (_, _) => ShowPage(_roiPage, _btnRoi);
        _btnValidation.Click += (_, _) =>
        {
            _validationPage.RunValidation();
            ShowPage(_validationPage, _btnValidation);
        };
        _btnExport.Click += (_, _) =>
        {
            _exportPage.RefreshSummary();
            ShowPage(_exportPage, _btnExport);
        };
    }

    private void CreateProject()
    {
        using var sourceDialog = new FolderBrowserDialog
        {
            Description = "选择原始图片目录（源目录只读，不会移动或删除文件）",
            UseDescriptionForTitle = true
        };
        if (sourceDialog.ShowDialog(this) != DialogResult.OK) return;

        using var projectDialog = new FolderBrowserDialog
        {
            Description = "选择 DatasetStudio 项目目录，例如 D:\\DatasetProjects\\Brunei",
            UseDescriptionForTitle = true
        };
        if (projectDialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            BindSession(AppSession.Create(projectDialog.SelectedPath, sourceDialog.SelectedPath));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "创建项目失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenProject()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择包含 project.json 的 DatasetStudio 项目目录",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            BindSession(AppSession.Open(dialog.SelectedPath));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "打开项目失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BindSession(AppSession session)
    {
        _session = session;
        _currentProject.Text = $"当前项目：{session.Project.Name}\n{session.Project.SourceDirectory}";
        _classificationPage.BindSession(session);
        _roiPage.BindSession(session);
        _validationPage.BindSession(session);
        _exportPage.BindSession(session);
        ShowPage(_classificationPage, _btnClassification);
    }

    private void ShowPage(Control page, Button active)
    {
        foreach (Control child in _content.Controls) child.Visible = false;
        page.Visible = true;
        page.BringToFront();
        foreach (var button in new[] { _btnClassification, _btnRoi, _btnValidation, _btnExport })
        {
            var isActive = button == active;
            if (button is NavigationButton navigationButton) navigationButton.Active = isActive;
            button.BackColor = isActive ? UiTheme.AccentSoft : UiTheme.Surface;
            button.ForeColor = isActive ? UiTheme.Accent : UiTheme.TextSecondary;
            button.Font = UiTheme.CreateFont(9.5F, isActive ? FontStyle.Bold : FontStyle.Regular);
            button.FlatAppearance.MouseOverBackColor = isActive ? UiTheme.AccentSoft : UiTheme.NavigationHover;
        }
    }

    private static Control? FindFocusedControl(Control root)
    {
        if (root.Focused) return root;
        foreach (Control child in root.Controls)
        {
            if (!child.ContainsFocus) continue;
            return FindFocusedControl(child) ?? child;
        }
        return null;
    }

    private static Button CreateNavButton(string text, bool active)
    {
        var button = new NavigationButton
        {
            Text = text,
            Size = new Size(105, 62),
            Margin = Padding.Empty,
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? UiTheme.AccentSoft : UiTheme.Surface,
            ForeColor = active ? UiTheme.Accent : UiTheme.TextSecondary,
            Font = UiTheme.CreateFont(9.5F, active ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        button.Active = active;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = active ? UiTheme.AccentSoft : UiTheme.NavigationHover;
        button.FlatAppearance.MouseDownBackColor = UiTheme.NavigationPressed;
        return button;
    }

    private void ApplyResponsiveLayout()
    {
        if (_headerLayout is null || _navigation is null || ClientSize.Width <= 0) return;

        var dpi = Math.Max(1F, DeviceDpi / 96F);
        var logicalWidth = ClientSize.Width / dpi;
        var compact = logicalWidth < 1120F;
        var hideProjectStatus = logicalWidth < 930F;

        var brandWidth = (hideProjectStatus ? 230F : compact ? 200F : 240F) * dpi;
        var navigationWidth = (hideProjectStatus ? 420F : compact ? 360F : 420F) * dpi;
        var actionsWidth = (compact ? 190F : 210F) * dpi;
        _headerLayout.ColumnStyles[0].SizeType = SizeType.Absolute;
        _headerLayout.ColumnStyles[0].Width = brandWidth;
        _headerLayout.ColumnStyles[1].SizeType = SizeType.Absolute;
        _headerLayout.ColumnStyles[1].Width = navigationWidth;
        _headerLayout.ColumnStyles[3].SizeType = SizeType.Absolute;
        _headerLayout.ColumnStyles[3].Width = actionsWidth;

        _currentProject.Visible = !hideProjectStatus;
        _headerLayout.ColumnStyles[2].SizeType = hideProjectStatus ? SizeType.Absolute : SizeType.Percent;
        _headerLayout.ColumnStyles[2].Width = hideProjectStatus ? 0F : 100F;

        var navWidth = Math.Max(72, (int)(navigationWidth / 4F));
        foreach (var button in new[] { _btnClassification, _btnRoi, _btnValidation, _btnExport })
            button.Width = navWidth;
        var projectButtonWidth = (int)((compact ? 84F : 96F) * dpi);
        if (_btnNewProject is not null) _btnNewProject.Width = projectButtonWidth;
        if (_btnOpenProject is not null) _btnOpenProject.Width = projectButtonWidth;

        var fontScale = logicalWidth switch
        {
            < 980F => 0.88F,
            < 1120F => 0.94F,
            > 1650F => 1.08F,
            > 1350F => 1.03F,
            _ => 1F
        };
        ApplyResponsiveFonts(fontScale);
        _classificationPage.ApplyResponsiveLayout();
        _roiPage.ApplyResponsiveLayout();
    }

    private void CaptureResponsiveFonts(Control root)
    {
        _responsiveFonts[root] = FontSpec.From(root.Font);
        if (root is DataGridView grid)
        {
            _gridFonts[grid] = (
                FontSpec.From(grid.ColumnHeadersDefaultCellStyle.Font ?? grid.Font),
                FontSpec.From(grid.DefaultCellStyle.Font ?? grid.Font),
                FontSpec.From(grid.AlternatingRowsDefaultCellStyle.Font ?? grid.Font));
        }
        foreach (Control child in root.Controls) CaptureResponsiveFonts(child);
    }

    private void ApplyResponsiveFonts(float scale)
    {
        if (Math.Abs(_appliedFontScale - scale) < 0.001F || _responsiveFonts.Count == 0) return;
        _appliedFontScale = scale;
        SuspendLayout();
        try
        {
            foreach (var (control, spec) in _responsiveFonts)
            {
                if (control.IsDisposed) continue;
                var next = spec.Create(scale);
                control.Font = next;
                if (_generatedFonts.Remove(control, out var previous)) previous.Dispose();
                _generatedFonts[control] = next;
            }

            foreach (var (grid, fonts) in _gridFonts)
            {
                if (grid.IsDisposed) continue;
                var next = (fonts.Header.Create(scale), fonts.Cells.Create(scale), fonts.Alternating.Create(scale));
                grid.ColumnHeadersDefaultCellStyle.Font = next.Item1;
                grid.DefaultCellStyle.Font = next.Item2;
                grid.AlternatingRowsDefaultCellStyle.Font = next.Item3;
                if (_generatedGridFonts.Remove(grid, out var previous))
                {
                    previous.Header.Dispose();
                    previous.Cells.Dispose();
                    previous.Alternating.Dispose();
                }
                _generatedGridFonts[grid] = next;
            }
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var font in _generatedFonts.Values.Distinct()) font.Dispose();
            _generatedFonts.Clear();
            foreach (var fonts in _generatedGridFonts.Values)
            {
                fonts.Header.Dispose();
                fonts.Cells.Dispose();
                fonts.Alternating.Dispose();
            }
            _generatedGridFonts.Clear();
        }
        base.Dispose(disposing);
    }

    private readonly record struct FontSpec(string Family, float Size, FontStyle Style, GraphicsUnit Unit)
    {
        public static FontSpec From(Font font) => new(font.FontFamily.Name, font.Size, font.Style, font.Unit);
        public Font Create(float scale) => new(Family, Math.Max(6F, Size * scale), Style, Unit);
    }
}
