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
    private AppSession? _session;

    public MainForm()
    {
        Text = "科准 Dataset Studio 数据集工具";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1180, 720);
        BackColor = UiTheme.WindowBackground;
        Font = new Font("Microsoft YaHei UI", 10F);
        KeyPreview = true;

        _btnClassification = CreateNavButton("数据集分类", true);
        _btnRoi = CreateNavButton("ROI 标定", false);
        _btnValidation = CreateNavButton("数据校验", false);
        _btnExport = CreateNavButton("导出 / 发布", false);

        BuildLayout();
        WireEvents();
        ShowPage(_classificationPage, _btnClassification);
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
                    _currentProject.Text = $"当前项目：{_session.Project.Name}  |  已撤销上一笔分类";
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 218F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 234F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 224F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));

        // 品牌区：中文名强调色 + 英文名次级色，形成双色标识。
        var brand = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(20, 0, 0, 0),
            Margin = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        var brandZh = new Label
        {
            Text = "科准",
            AutoSize = false,
            Size = new Size(46, 64),
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.Accent,
            Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold)
        };
        var brandEn = new Label
        {
            Text = "Dataset Studio",
            AutoSize = false,
            Size = new Size(128, 64),
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextSecondary,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        brand.Controls.Add(brandZh);
        brand.Controls.Add(brandEn);

        var navigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = UiTheme.Surface
        };
        navigation.Controls.AddRange(new Control[] { _btnClassification, _btnRoi, _btnValidation, _btnExport });

        _currentProject.Text = "当前项目：未打开";
        _currentProject.Dock = DockStyle.Fill;
        _currentProject.TextAlign = ContentAlignment.MiddleRight;
        _currentProject.Padding = new Padding(0, 0, 14, 0);
        _currentProject.ForeColor = UiTheme.TextSecondary;

        var projectButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(4, 14, 12, 0),
            BackColor = UiTheme.Surface
        };
        var btnNew = UiTheme.CreateButton("新建项目", true);
        btnNew.Width = 96;
        var btnOpen = UiTheme.CreateButton("打开项目");
        btnOpen.Width = 96;
        btnNew.Click += (_, _) => CreateProject();
        btnOpen.Click += (_, _) => OpenProject();
        projectButtons.Controls.Add(btnNew);
        projectButtons.Controls.Add(btnOpen);

        var separator = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Border, Margin = Padding.Empty };
        header.Controls.Add(brand, 0, 0);
        header.Controls.Add(navigation, 1, 0);
        header.Controls.Add(_currentProject, 2, 0);
        header.Controls.Add(projectButtons, 3, 0);
        header.Controls.Add(separator, 0, 1);
        header.SetColumnSpan(separator, 4);

        _content.Dock = DockStyle.Fill;
        _content.BackColor = UiTheme.WindowBackground;
        _content.Padding = new Padding(12);
        foreach (var page in new Control[] { _classificationPage, _roiPage, _validationPage, _exportPage })
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _content.Controls.Add(page);
        }

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(_content, 0, 1);
        Controls.Add(root);
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
        _currentProject.Text = $"当前项目：{session.Project.Name}";
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
            button.BackColor = isActive ? UiTheme.AccentSoft : UiTheme.Surface;
            button.ForeColor = isActive ? UiTheme.Accent : UiTheme.TextSecondary;
            button.Font = new Font("Microsoft YaHei UI", 10F, isActive ? FontStyle.Bold : FontStyle.Regular);
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
        var button = new Button
        {
            Text = text,
            Size = new Size(116, 64),
            Margin = Padding.Empty,
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? UiTheme.AccentSoft : UiTheme.Surface,
            ForeColor = active ? UiTheme.Accent : UiTheme.TextSecondary,
            Font = new Font("Microsoft YaHei UI", 10F, active ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = active ? UiTheme.AccentSoft : UiTheme.NavigationHover;
        button.FlatAppearance.MouseDownBackColor = UiTheme.NavigationPressed;
        return button;
    }
}
