using DatasetStudio.WinForms.Pages;

namespace DatasetStudio.WinForms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<Control, FontSpec> _responsiveFonts = new();
    private readonly Dictionary<Control, Font> _generatedFonts = new();
    private readonly Dictionary<DataGridView, (FontSpec Header, FontSpec Cells, FontSpec Alternating)> _gridFonts = new();
    private readonly Dictionary<DataGridView, (Font Header, Font Cells, Font Alternating)> _generatedGridFonts = new();
    private float _appliedFontScale = -1F;
    private AppSession? _session;

    public MainForm()
    {
        InitializeComponent();
        CaptureResponsiveFonts(this);
        ShowPage(_classificationPage, _btnClassification);
        ApplyResponsiveLayout();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Z) && _session is not null)
        {
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

    private void MainForm_Resize(object? sender, EventArgs e) => ApplyResponsiveLayout();

    private void BtnClassification_Click(object? sender, EventArgs e) =>
        ShowPage(_classificationPage, _btnClassification);

    private void BtnRoi_Click(object? sender, EventArgs e) =>
        ShowPage(_roiPage, _btnRoi);

    private void BtnValidation_Click(object? sender, EventArgs e)
    {
        _validationPage.RunValidation();
        ShowPage(_validationPage, _btnValidation);
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        _exportPage.RefreshSummary();
        ShowPage(_exportPage, _btnExport);
    }

    private void BtnNewProject_Click(object? sender, EventArgs e) => CreateProject();
    private void BtnOpenProject_Click(object? sender, EventArgs e) => OpenProject();

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
        foreach (Control child in _content.Controls)
            child.Visible = false;

        page.Visible = true;
        page.BringToFront();

        foreach (var button in new Button[] { _btnClassification, _btnRoi, _btnValidation, _btnExport })
        {
            var isActive = button == active;
            if (button is NavigationButton navigationButton)
                navigationButton.Active = isActive;

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

    private void ApplyResponsiveLayout()
    {
        if (ClientSize.Width <= 0 || _headerLayout.ColumnStyles.Count < 4)
            return;

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
        foreach (var button in new Button[] { _btnClassification, _btnRoi, _btnValidation, _btnExport })
            button.Width = navWidth;

        var projectButtonWidth = (int)((compact ? 84F : 96F) * dpi);
        _btnNewProject.Width = projectButtonWidth;
        _btnOpenProject.Width = projectButtonWidth;

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

        foreach (Control child in root.Controls)
            CaptureResponsiveFonts(child);
    }

    private void ApplyResponsiveFonts(float scale)
    {
        if (Math.Abs(_appliedFontScale - scale) < 0.001F || _responsiveFonts.Count == 0)
            return;

        _appliedFontScale = scale;
        SuspendLayout();
        try
        {
            foreach (var (control, spec) in _responsiveFonts)
            {
                if (control.IsDisposed) continue;
                var next = spec.Create(scale);
                control.Font = next;
                if (_generatedFonts.Remove(control, out var previous))
                    previous.Dispose();
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
            components?.Dispose();

            foreach (var font in _generatedFonts.Values.Distinct())
                font.Dispose();
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
