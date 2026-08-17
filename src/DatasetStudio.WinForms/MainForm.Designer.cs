using DatasetStudio.WinForms.Pages;

namespace DatasetStudio.WinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private TableLayoutPanel _rootLayout = null!;
    private TableLayoutPanel _headerLayout = null!;
    private TableLayoutPanel _brand = null!;
    private Label _brandMark = null!;
    private TableLayoutPanel _brandText = null!;
    private Label _brandTitle = null!;
    private Label _brandSubtitle = null!;
    private FlowLayoutPanel _navigation = null!;
    private NavigationButton _btnClassification = null!;
    private NavigationButton _btnRoi = null!;
    private NavigationButton _btnValidation = null!;
    private NavigationButton _btnExport = null!;
    private Label _currentProject = null!;
    private FlowLayoutPanel _projectButtons = null!;
    private Button _btnNewProject = null!;
    private Button _btnOpenProject = null!;
    private Panel _separator = null!;
    private Panel _content = null!;
    private ClassificationPage _classificationPage = null!;
    private RoiCalibrationPage _roiPage = null!;
    private ValidationPage _validationPage = null!;
    private ExportPage _exportPage = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _rootLayout = new TableLayoutPanel();
        _headerLayout = new TableLayoutPanel();
        _brand = new TableLayoutPanel();
        _brandMark = new Label();
        _brandText = new TableLayoutPanel();
        _brandTitle = new Label();
        _brandSubtitle = new Label();
        _navigation = new FlowLayoutPanel();
        _btnClassification = new NavigationButton();
        _btnRoi = new NavigationButton();
        _btnValidation = new NavigationButton();
        _btnExport = new NavigationButton();
        _currentProject = new Label();
        _projectButtons = new FlowLayoutPanel();
        _btnNewProject = new Button();
        _btnOpenProject = new Button();
        _separator = new Panel();
        _content = new Panel();
        _classificationPage = new ClassificationPage();
        _roiPage = new RoiCalibrationPage();
        _validationPage = new ValidationPage();
        _exportPage = new ExportPage();
        _rootLayout.SuspendLayout();
        _headerLayout.SuspendLayout();
        _brand.SuspendLayout();
        _brandText.SuspendLayout();
        _navigation.SuspendLayout();
        _projectButtons.SuspendLayout();
        _content.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        _rootLayout.BackColor = Color.FromArgb(244, 245, 246);
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_headerLayout, 0, 0);
        _rootLayout.Controls.Add(_content, 0, 1);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Location = new Point(0, 0);
        _rootLayout.Margin = new Padding(0);
        _rootLayout.Name = "rootLayout";
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.Size = new Size(1480, 900);
        // 
        // headerLayout
        // 
        _headerLayout.BackColor = Color.White;
        _headerLayout.ColumnCount = 4;
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
        _headerLayout.Controls.Add(_brand, 0, 0);
        _headerLayout.Controls.Add(_navigation, 1, 0);
        _headerLayout.Controls.Add(_currentProject, 2, 0);
        _headerLayout.Controls.Add(_projectButtons, 3, 0);
        _headerLayout.Controls.Add(_separator, 0, 1);
        _headerLayout.Dock = DockStyle.Fill;
        _headerLayout.Location = new Point(0, 0);
        _headerLayout.Margin = new Padding(0);
        _headerLayout.Name = "headerLayout";
        _headerLayout.RowCount = 2;
        _headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        _headerLayout.SetColumnSpan(_separator, 4);
        _headerLayout.Size = new Size(1480, 62);
        // 
        // brand
        // 
        _brand.BackColor = Color.White;
        _brand.ColumnCount = 2;
        _brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F));
        _brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _brand.Controls.Add(_brandMark, 0, 0);
        _brand.Controls.Add(_brandText, 1, 0);
        _brand.Dock = DockStyle.Fill;
        _brand.Location = new Point(0, 0);
        _brand.Margin = new Padding(0);
        _brand.Name = "brand";
        _brand.Padding = new Padding(20, 0, 0, 0);
        _brand.RowCount = 1;
        _brand.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _brand.Size = new Size(240, 61);
        // 
        // brandMark
        // 
        _brandMark.Anchor = AnchorStyles.Left;
        _brandMark.BackColor = Color.FromArgb(32, 32, 32);
        _brandMark.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
        _brandMark.ForeColor = Color.White;
        _brandMark.Location = new Point(20, 14);
        _brandMark.Margin = new Padding(0);
        _brandMark.Name = "brandMark";
        _brandMark.Size = new Size(32, 32);
        _brandMark.Text = "KZ";
        _brandMark.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // brandText
        // 
        _brandText.BackColor = Color.White;
        _brandText.ColumnCount = 1;
        _brandText.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _brandText.Controls.Add(_brandTitle, 0, 0);
        _brandText.Controls.Add(_brandSubtitle, 0, 1);
        _brandText.Dock = DockStyle.Fill;
        _brandText.Location = new Point(62, 0);
        _brandText.Margin = new Padding(0);
        _brandText.Name = "brandText";
        _brandText.RowCount = 2;
        _brandText.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        _brandText.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        _brandText.Size = new Size(178, 61);
        // 
        // brandTitle
        // 
        _brandTitle.Dock = DockStyle.Fill;
        _brandTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        _brandTitle.ForeColor = Color.FromArgb(32, 32, 32);
        _brandTitle.Location = new Point(3, 0);
        _brandTitle.Name = "brandTitle";
        _brandTitle.Size = new Size(172, 31);
        _brandTitle.Text = "科准 DATASET STUDIO";
        _brandTitle.TextAlign = ContentAlignment.BottomLeft;
        // 
        // brandSubtitle
        // 
        _brandSubtitle.Dock = DockStyle.Fill;
        _brandSubtitle.Font = new Font("Microsoft YaHei UI", 7.5F);
        _brandSubtitle.ForeColor = Color.FromArgb(92, 92, 92);
        _brandSubtitle.Location = new Point(3, 31);
        _brandSubtitle.Name = "brandSubtitle";
        _brandSubtitle.Size = new Size(172, 18);
        _brandSubtitle.Text = "INDUSTRIAL VISION TOOL";
        _brandSubtitle.TextAlign = ContentAlignment.TopLeft;
        // 
        // navigation
        // 
        _navigation.BackColor = Color.White;
        _navigation.Controls.Add(_btnClassification);
        _navigation.Controls.Add(_btnRoi);
        _navigation.Controls.Add(_btnValidation);
        _navigation.Controls.Add(_btnExport);
        _navigation.Dock = DockStyle.Fill;
        _navigation.Location = new Point(240, 0);
        _navigation.Margin = new Padding(0);
        _navigation.Name = "navigation";
        _navigation.Size = new Size(420, 61);
        _navigation.WrapContents = false;
        // 
        // btnClassification
        // 
        _btnClassification.Active = true;
        _btnClassification.BackColor = Color.FromArgb(238, 239, 240);
        _btnClassification.Cursor = Cursors.Hand;
        _btnClassification.FlatAppearance.BorderSize = 0;
        _btnClassification.FlatAppearance.MouseDownBackColor = Color.FromArgb(228, 229, 230);
        _btnClassification.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 239, 240);
        _btnClassification.FlatStyle = FlatStyle.Flat;
        _btnClassification.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _btnClassification.ForeColor = Color.FromArgb(32, 32, 32);
        _btnClassification.Location = new Point(0, 0);
        _btnClassification.Margin = new Padding(0);
        _btnClassification.Name = "btnClassification";
        _btnClassification.Size = new Size(105, 62);
        _btnClassification.Text = "数据集分类";
        _btnClassification.UseVisualStyleBackColor = false;
        _btnClassification.Click += BtnClassification_Click;
        // 
        // btnRoi
        // 
        _btnRoi.Active = false;
        _btnRoi.BackColor = Color.White;
        _btnRoi.Cursor = Cursors.Hand;
        _btnRoi.FlatAppearance.BorderSize = 0;
        _btnRoi.FlatAppearance.MouseDownBackColor = Color.FromArgb(228, 229, 230);
        _btnRoi.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 246, 246);
        _btnRoi.FlatStyle = FlatStyle.Flat;
        _btnRoi.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnRoi.ForeColor = Color.FromArgb(64, 64, 64);
        _btnRoi.Location = new Point(105, 0);
        _btnRoi.Margin = new Padding(0);
        _btnRoi.Name = "btnRoi";
        _btnRoi.Size = new Size(105, 62);
        _btnRoi.Text = "ROI 标定";
        _btnRoi.UseVisualStyleBackColor = false;
        _btnRoi.Click += BtnRoi_Click;
        // 
        // btnValidation
        // 
        _btnValidation.Active = false;
        _btnValidation.BackColor = Color.White;
        _btnValidation.Cursor = Cursors.Hand;
        _btnValidation.FlatAppearance.BorderSize = 0;
        _btnValidation.FlatAppearance.MouseDownBackColor = Color.FromArgb(228, 229, 230);
        _btnValidation.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 246, 246);
        _btnValidation.FlatStyle = FlatStyle.Flat;
        _btnValidation.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnValidation.ForeColor = Color.FromArgb(64, 64, 64);
        _btnValidation.Location = new Point(210, 0);
        _btnValidation.Margin = new Padding(0);
        _btnValidation.Name = "btnValidation";
        _btnValidation.Size = new Size(105, 62);
        _btnValidation.Text = "数据校验";
        _btnValidation.UseVisualStyleBackColor = false;
        _btnValidation.Click += BtnValidation_Click;
        // 
        // btnExport
        // 
        _btnExport.Active = false;
        _btnExport.BackColor = Color.White;
        _btnExport.Cursor = Cursors.Hand;
        _btnExport.FlatAppearance.BorderSize = 0;
        _btnExport.FlatAppearance.MouseDownBackColor = Color.FromArgb(228, 229, 230);
        _btnExport.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 246, 246);
        _btnExport.FlatStyle = FlatStyle.Flat;
        _btnExport.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnExport.ForeColor = Color.FromArgb(64, 64, 64);
        _btnExport.Location = new Point(315, 0);
        _btnExport.Margin = new Padding(0);
        _btnExport.Name = "btnExport";
        _btnExport.Size = new Size(105, 62);
        _btnExport.Text = "导出 / 发布";
        _btnExport.UseVisualStyleBackColor = false;
        _btnExport.Click += BtnExport_Click;
        // 
        // currentProject
        // 
        _currentProject.AutoEllipsis = true;
        _currentProject.Dock = DockStyle.Fill;
        _currentProject.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
        _currentProject.ForeColor = Color.FromArgb(64, 64, 64);
        _currentProject.Location = new Point(663, 0);
        _currentProject.Name = "currentProject";
        _currentProject.Padding = new Padding(0, 0, 12, 0);
        _currentProject.Size = new Size(604, 61);
        _currentProject.Text = "当前项目：未打开\r\n就绪";
        _currentProject.TextAlign = ContentAlignment.MiddleRight;
        // 
        // projectButtons
        // 
        _projectButtons.BackColor = Color.White;
        _projectButtons.Controls.Add(_btnNewProject);
        _projectButtons.Controls.Add(_btnOpenProject);
        _projectButtons.Dock = DockStyle.Fill;
        _projectButtons.FlowDirection = FlowDirection.LeftToRight;
        _projectButtons.Location = new Point(1270, 0);
        _projectButtons.Margin = new Padding(0);
        _projectButtons.Name = "projectButtons";
        _projectButtons.Padding = new Padding(0, 13, 10, 0);
        _projectButtons.Size = new Size(210, 61);
        _projectButtons.WrapContents = false;
        // 
        // btnNewProject
        // 
        _btnNewProject.BackColor = Color.FromArgb(32, 32, 32);
        _btnNewProject.Cursor = Cursors.Hand;
        _btnNewProject.FlatAppearance.BorderColor = Color.FromArgb(32, 32, 32);
        _btnNewProject.FlatAppearance.MouseDownBackColor = Color.FromArgb(51, 51, 51);
        _btnNewProject.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 51, 51);
        _btnNewProject.FlatStyle = FlatStyle.Flat;
        _btnNewProject.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _btnNewProject.ForeColor = Color.White;
        _btnNewProject.Location = new Point(3, 16);
        _btnNewProject.Name = "btnNewProject";
        _btnNewProject.Size = new Size(96, 36);
        _btnNewProject.Text = "新建项目";
        _btnNewProject.UseVisualStyleBackColor = false;
        _btnNewProject.Click += BtnNewProject_Click;
        // 
        // btnOpenProject
        // 
        _btnOpenProject.BackColor = Color.White;
        _btnOpenProject.Cursor = Cursors.Hand;
        _btnOpenProject.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _btnOpenProject.FlatAppearance.MouseDownBackColor = Color.FromArgb(228, 229, 230);
        _btnOpenProject.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 246, 246);
        _btnOpenProject.FlatStyle = FlatStyle.Flat;
        _btnOpenProject.Font = new Font("Microsoft YaHei UI", 9.5F);
        _btnOpenProject.ForeColor = Color.FromArgb(32, 32, 32);
        _btnOpenProject.Location = new Point(105, 16);
        _btnOpenProject.Name = "btnOpenProject";
        _btnOpenProject.Size = new Size(96, 36);
        _btnOpenProject.Text = "打开项目";
        _btnOpenProject.UseVisualStyleBackColor = false;
        _btnOpenProject.Click += BtnOpenProject_Click;
        // 
        // separator
        // 
        _separator.BackColor = Color.FromArgb(218, 220, 222);
        _separator.Dock = DockStyle.Fill;
        _separator.Location = new Point(0, 61);
        _separator.Margin = new Padding(0);
        _separator.Name = "separator";
        _separator.Size = new Size(1480, 1);
        // 
        // content
        // 
        _content.BackColor = Color.FromArgb(244, 245, 246);
        _content.Controls.Add(_classificationPage);
        _content.Controls.Add(_roiPage);
        _content.Controls.Add(_validationPage);
        _content.Controls.Add(_exportPage);
        _content.Dock = DockStyle.Fill;
        _content.Location = new Point(0, 62);
        _content.Margin = new Padding(0);
        _content.Name = "content";
        _content.Padding = new Padding(12);
        _content.Size = new Size(1480, 838);
        // 
        // classificationPage
        // 
        _classificationPage.BackColor = Color.FromArgb(244, 245, 246);
        _classificationPage.Dock = DockStyle.Fill;
        _classificationPage.Location = new Point(12, 12);
        _classificationPage.Name = "classificationPage";
        _classificationPage.Size = new Size(1456, 814);
        _classificationPage.TabIndex = 0;
        _classificationPage.Visible = true;
        // 
        // roiPage
        // 
        _roiPage.BackColor = Color.FromArgb(244, 245, 246);
        _roiPage.Dock = DockStyle.Fill;
        _roiPage.Location = new Point(12, 12);
        _roiPage.Name = "roiPage";
        _roiPage.Size = new Size(1456, 814);
        _roiPage.TabIndex = 1;
        _roiPage.Visible = false;
        // 
        // validationPage
        // 
        _validationPage.BackColor = Color.FromArgb(244, 245, 246);
        _validationPage.Dock = DockStyle.Fill;
        _validationPage.Location = new Point(12, 12);
        _validationPage.Name = "validationPage";
        _validationPage.Size = new Size(1456, 814);
        _validationPage.TabIndex = 2;
        _validationPage.Visible = false;
        // 
        // exportPage
        // 
        _exportPage.BackColor = Color.FromArgb(244, 245, 246);
        _exportPage.Dock = DockStyle.Fill;
        _exportPage.Location = new Point(12, 12);
        _exportPage.Name = "exportPage";
        _exportPage.Size = new Size(1456, 814);
        _exportPage.TabIndex = 3;
        _exportPage.Visible = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(244, 245, 246);
        ClientSize = new Size(1480, 900);
        Controls.Add(_rootLayout);
        Font = new Font("Microsoft YaHei UI", 10F);
        KeyPreview = true;
        MinimumSize = new Size(860, 520);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "科准 Dataset Studio 数据集工具";
        WindowState = FormWindowState.Maximized;
        Resize += MainForm_Resize;
        _rootLayout.ResumeLayout(false);
        _headerLayout.ResumeLayout(false);
        _brand.ResumeLayout(false);
        _brandText.ResumeLayout(false);
        _navigation.ResumeLayout(false);
        _projectButtons.ResumeLayout(false);
        _content.ResumeLayout(false);
        ResumeLayout(false);
    }
}
