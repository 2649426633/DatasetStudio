namespace DatasetStudio.WinForms.Pages;

partial class ExportPage
{
    private System.ComponentModel.IContainer? components = null;
    private CardPanel _rootCard = null!;
    private TableLayoutPanel _mainLayout = null!;
    private Label _titleLabel = null!;
    private Label _projectPath = null!;
    private TableLayoutPanel _generatedCountsTable = null!;
    private TableLayoutPanel _generatedPanel = null!;
    private Label _generatedTitle = null!;
    private Label _generatedList = null!;
    private TableLayoutPanel _countsPanel = null!;
    private TableLayoutPanel _trainGoodPanel = null!;
    private Label _trainGoodTitle = null!;
    private Label _trainGoodCount = null!;
    private TableLayoutPanel _testGoodPanel = null!;
    private Label _testGoodTitle = null!;
    private Label _testGoodCount = null!;
    private TableLayoutPanel _testNgPanel = null!;
    private Label _testNgTitle = null!;
    private Label _testNgCount = null!;
    private TableLayoutPanel _ignoredPanel = null!;
    private Label _ignoredTitle = null!;
    private Label _ignoredCount = null!;
    private TableLayoutPanel _actionPanel = null!;
    private Button _validateButton = null!;
    private Button _generateButton = null!;
    private Label _lastPackage = null!;
    private Label _publishTitle = null!;
    private TableLayoutPanel _publishPanel = null!;
    private TextBox _publishTarget = null!;
    private Button _browseButton = null!;
    private Button _publishButton = null!;
    private Label _safetyLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _rootCard = new CardPanel();
        _mainLayout = new TableLayoutPanel();
        _titleLabel = new Label();
        _projectPath = new Label();
        _generatedCountsTable = new TableLayoutPanel();
        _generatedPanel = new TableLayoutPanel();
        _generatedTitle = new Label();
        _generatedList = new Label();
        _countsPanel = new TableLayoutPanel();
        _trainGoodPanel = new TableLayoutPanel();
        _trainGoodTitle = new Label();
        _trainGoodCount = new Label();
        _testGoodPanel = new TableLayoutPanel();
        _testGoodTitle = new Label();
        _testGoodCount = new Label();
        _testNgPanel = new TableLayoutPanel();
        _testNgTitle = new Label();
        _testNgCount = new Label();
        _ignoredPanel = new TableLayoutPanel();
        _ignoredTitle = new Label();
        _ignoredCount = new Label();
        _actionPanel = new TableLayoutPanel();
        _validateButton = new Button();
        _generateButton = new Button();
        _lastPackage = new Label();
        _publishTitle = new Label();
        _publishPanel = new TableLayoutPanel();
        _publishTarget = new TextBox();
        _browseButton = new Button();
        _publishButton = new Button();
        _safetyLabel = new Label();
        _rootCard.SuspendLayout();
        _mainLayout.SuspendLayout();
        _generatedCountsTable.SuspendLayout();
        _generatedPanel.SuspendLayout();
        _countsPanel.SuspendLayout();
        _trainGoodPanel.SuspendLayout();
        _testGoodPanel.SuspendLayout();
        _testNgPanel.SuspendLayout();
        _ignoredPanel.SuspendLayout();
        _actionPanel.SuspendLayout();
        _publishPanel.SuspendLayout();
        SuspendLayout();
        //
        // rootCard
        //
        _rootCard.BackColor = Color.White;
        _rootCard.BorderColor = Color.FromArgb(218, 220, 222);
        _rootCard.Controls.Add(_mainLayout);
        _rootCard.Dock = DockStyle.Fill;
        _rootCard.Margin = Padding.Empty;
        _rootCard.Name = "rootCard";
        _rootCard.Padding = new Padding(24);
        //
        // mainLayout
        //
        _mainLayout.BackColor = Color.White;
        _mainLayout.ColumnCount = 1;
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _mainLayout.Controls.Add(_titleLabel, 0, 0);
        _mainLayout.Controls.Add(_projectPath, 0, 1);
        _mainLayout.Controls.Add(_generatedCountsTable, 0, 2);
        _mainLayout.Controls.Add(_actionPanel, 0, 3);
        _mainLayout.Controls.Add(_lastPackage, 0, 4);
        _mainLayout.Controls.Add(_publishTitle, 0, 5);
        _mainLayout.Controls.Add(_publishPanel, 0, 6);
        _mainLayout.Controls.Add(_safetyLabel, 0, 7);
        _mainLayout.Dock = DockStyle.Fill;
        _mainLayout.Margin = Padding.Empty;
        _mainLayout.Name = "mainLayout";
        _mainLayout.RowCount = 8;
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        //
        // titleLabel
        //
        _titleLabel.Dock = DockStyle.Fill;
        _titleLabel.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        _titleLabel.ForeColor = Color.FromArgb(32, 32, 32);
        _titleLabel.Name = "titleLabel";
        _titleLabel.Text = "导出 / 发布 ProductAlignPackage";
        _titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // projectPath
        //
        _projectPath.AutoEllipsis = true;
        _projectPath.Dock = DockStyle.Fill;
        _projectPath.Font = new Font("Microsoft YaHei UI", 9F);
        _projectPath.ForeColor = Color.FromArgb(64, 64, 64);
        _projectPath.Margin = new Padding(0, 4, 0, 10);
        _projectPath.Name = "projectPath";
        _projectPath.Text = "项目：未打开";
        _projectPath.TextAlign = ContentAlignment.MiddleLeft;
        //
        // generatedCountsTable
        //
        _generatedCountsTable.BackColor = Color.White;
        _generatedCountsTable.ColumnCount = 2;
        _generatedCountsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        _generatedCountsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        _generatedCountsTable.Controls.Add(_generatedPanel, 0, 0);
        _generatedCountsTable.Controls.Add(_countsPanel, 1, 0);
        _generatedCountsTable.Dock = DockStyle.Fill;
        _generatedCountsTable.Margin = Padding.Empty;
        _generatedCountsTable.Name = "generatedCountsTable";
        _generatedCountsTable.RowCount = 1;
        _generatedCountsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // generatedPanel
        //
        _generatedPanel.BackColor = Color.FromArgb(250, 250, 250);
        _generatedPanel.ColumnCount = 1;
        _generatedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _generatedPanel.Controls.Add(_generatedTitle, 0, 0);
        _generatedPanel.Controls.Add(_generatedList, 0, 1);
        _generatedPanel.Dock = DockStyle.Fill;
        _generatedPanel.Margin = new Padding(0, 0, 8, 0);
        _generatedPanel.Name = "generatedPanel";
        _generatedPanel.Padding = new Padding(14);
        _generatedPanel.RowCount = 2;
        _generatedPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        _generatedPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // generatedTitle
        //
        _generatedTitle.Dock = DockStyle.Fill;
        _generatedTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _generatedTitle.ForeColor = Color.FromArgb(64, 64, 64);
        _generatedTitle.Name = "generatedTitle";
        _generatedTitle.Text = "即将生成";
        _generatedTitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // generatedList
        //
        _generatedList.Dock = DockStyle.Fill;
        _generatedList.Font = new Font("Consolas", 9.5F);
        _generatedList.ForeColor = Color.FromArgb(32, 32, 32);
        _generatedList.Name = "generatedList";
        _generatedList.Text = "✅ configs\\<product>.json\r\n✅ artifacts\\reference\\reference_aligned.png\r\n✅ dataset_roi_dino\\train\\good\r\n✅ dataset_roi_dino\\test\\good\r\n✅ dataset_roi_dino\\test\\ng\r\n✅ dataset_manifest.csv\r\n✅ dataset_report.json";
        _generatedList.TextAlign = ContentAlignment.TopLeft;
        //
        // countsPanel
        //
        _countsPanel.BackColor = Color.FromArgb(250, 250, 250);
        _countsPanel.ColumnCount = 2;
        _countsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _countsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _countsPanel.Controls.Add(_trainGoodPanel, 0, 0);
        _countsPanel.Controls.Add(_testGoodPanel, 1, 0);
        _countsPanel.Controls.Add(_testNgPanel, 0, 1);
        _countsPanel.Controls.Add(_ignoredPanel, 1, 1);
        _countsPanel.Dock = DockStyle.Fill;
        _countsPanel.Margin = new Padding(8, 0, 0, 0);
        _countsPanel.Name = "countsPanel";
        _countsPanel.Padding = new Padding(8);
        _countsPanel.RowCount = 2;
        _countsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        _countsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        //
        // trainGoodPanel
        //
        _trainGoodPanel.BackColor = Color.White;
        _trainGoodPanel.ColumnCount = 1;
        _trainGoodPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _trainGoodPanel.Controls.Add(_trainGoodTitle, 0, 0);
        _trainGoodPanel.Controls.Add(_trainGoodCount, 0, 1);
        _trainGoodPanel.Dock = DockStyle.Fill;
        _trainGoodPanel.Margin = new Padding(6);
        _trainGoodPanel.Name = "trainGoodPanel";
        _trainGoodPanel.Padding = new Padding(8, 5, 8, 5);
        _trainGoodPanel.RowCount = 2;
        _trainGoodPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        _trainGoodPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        //
        // trainGoodTitle
        //
        _trainGoodTitle.Dock = DockStyle.Fill;
        _trainGoodTitle.Font = new Font("Microsoft YaHei UI", 8F);
        _trainGoodTitle.ForeColor = Color.FromArgb(92, 92, 92);
        _trainGoodTitle.Name = "trainGoodTitle";
        _trainGoodTitle.Text = "Train GOOD";
        _trainGoodTitle.TextAlign = ContentAlignment.BottomLeft;
        //
        // trainGoodCount
        //
        _trainGoodCount.Dock = DockStyle.Fill;
        _trainGoodCount.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        _trainGoodCount.ForeColor = Color.FromArgb(21, 128, 61);
        _trainGoodCount.Name = "trainGoodCount";
        _trainGoodCount.Text = "0 张";
        _trainGoodCount.TextAlign = ContentAlignment.TopLeft;
        //
        // testGoodPanel
        //
        _testGoodPanel.BackColor = Color.White;
        _testGoodPanel.ColumnCount = 1;
        _testGoodPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _testGoodPanel.Controls.Add(_testGoodTitle, 0, 0);
        _testGoodPanel.Controls.Add(_testGoodCount, 0, 1);
        _testGoodPanel.Dock = DockStyle.Fill;
        _testGoodPanel.Margin = new Padding(6);
        _testGoodPanel.Name = "testGoodPanel";
        _testGoodPanel.Padding = new Padding(8, 5, 8, 5);
        _testGoodPanel.RowCount = 2;
        _testGoodPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        _testGoodPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        _testGoodTitle.Dock = DockStyle.Fill;
        _testGoodTitle.Font = new Font("Microsoft YaHei UI", 8F);
        _testGoodTitle.ForeColor = Color.FromArgb(92, 92, 92);
        _testGoodTitle.Name = "testGoodTitle";
        _testGoodTitle.Text = "Test GOOD";
        _testGoodTitle.TextAlign = ContentAlignment.BottomLeft;
        _testGoodCount.Dock = DockStyle.Fill;
        _testGoodCount.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        _testGoodCount.ForeColor = Color.FromArgb(3, 105, 161);
        _testGoodCount.Name = "testGoodCount";
        _testGoodCount.Text = "0 张";
        _testGoodCount.TextAlign = ContentAlignment.TopLeft;
        //
        // testNgPanel
        //
        _testNgPanel.BackColor = Color.White;
        _testNgPanel.ColumnCount = 1;
        _testNgPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _testNgPanel.Controls.Add(_testNgTitle, 0, 0);
        _testNgPanel.Controls.Add(_testNgCount, 0, 1);
        _testNgPanel.Dock = DockStyle.Fill;
        _testNgPanel.Margin = new Padding(6);
        _testNgPanel.Name = "testNgPanel";
        _testNgPanel.Padding = new Padding(8, 5, 8, 5);
        _testNgPanel.RowCount = 2;
        _testNgPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        _testNgPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        _testNgTitle.Dock = DockStyle.Fill;
        _testNgTitle.Font = new Font("Microsoft YaHei UI", 8F);
        _testNgTitle.ForeColor = Color.FromArgb(92, 92, 92);
        _testNgTitle.Name = "testNgTitle";
        _testNgTitle.Text = "Test NG";
        _testNgTitle.TextAlign = ContentAlignment.BottomLeft;
        _testNgCount.Dock = DockStyle.Fill;
        _testNgCount.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        _testNgCount.ForeColor = Color.FromArgb(185, 28, 28);
        _testNgCount.Name = "testNgCount";
        _testNgCount.Text = "0 张";
        _testNgCount.TextAlign = ContentAlignment.TopLeft;
        //
        // ignoredPanel
        //
        _ignoredPanel.BackColor = Color.White;
        _ignoredPanel.ColumnCount = 1;
        _ignoredPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _ignoredPanel.Controls.Add(_ignoredTitle, 0, 0);
        _ignoredPanel.Controls.Add(_ignoredCount, 0, 1);
        _ignoredPanel.Dock = DockStyle.Fill;
        _ignoredPanel.Margin = new Padding(6);
        _ignoredPanel.Name = "ignoredPanel";
        _ignoredPanel.Padding = new Padding(8, 5, 8, 5);
        _ignoredPanel.RowCount = 2;
        _ignoredPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        _ignoredPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        _ignoredTitle.Dock = DockStyle.Fill;
        _ignoredTitle.Font = new Font("Microsoft YaHei UI", 8F);
        _ignoredTitle.ForeColor = Color.FromArgb(92, 92, 92);
        _ignoredTitle.Name = "ignoredTitle";
        _ignoredTitle.Text = "Ignore";
        _ignoredTitle.TextAlign = ContentAlignment.BottomLeft;
        _ignoredCount.Dock = DockStyle.Fill;
        _ignoredCount.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        _ignoredCount.ForeColor = Color.FromArgb(92, 92, 92);
        _ignoredCount.Name = "ignoredCount";
        _ignoredCount.Text = "0 张";
        _ignoredCount.TextAlign = ContentAlignment.TopLeft;
        //
        // actionPanel
        //
        _actionPanel.BackColor = Color.White;
        _actionPanel.ColumnCount = 3;
        _actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
        _actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 162F));
        _actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _actionPanel.Controls.Add(_validateButton, 0, 0);
        _actionPanel.Controls.Add(_generateButton, 1, 0);
        _actionPanel.Dock = DockStyle.Fill;
        _actionPanel.Margin = new Padding(0, 12, 0, 12);
        _actionPanel.Name = "actionPanel";
        _actionPanel.RowCount = 1;
        _actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // validateButton
        //
        _validateButton.BackColor = Color.White;
        _validateButton.Cursor = Cursors.Hand;
        _validateButton.Dock = DockStyle.Fill;
        _validateButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _validateButton.FlatStyle = FlatStyle.Flat;
        _validateButton.Font = new Font("Microsoft YaHei UI", 9.5F);
        _validateButton.ForeColor = Color.FromArgb(32, 32, 32);
        _validateButton.Margin = new Padding(0, 0, 12, 0);
        _validateButton.Name = "validateButton";
        _validateButton.Text = "校验数据";
        _validateButton.UseVisualStyleBackColor = false;
        _validateButton.Click += ValidateButton_Click;
        //
        // generateButton
        //
        _generateButton.BackColor = Color.FromArgb(32, 32, 32);
        _generateButton.Cursor = Cursors.Hand;
        _generateButton.Dock = DockStyle.Fill;
        _generateButton.FlatAppearance.BorderColor = Color.FromArgb(32, 32, 32);
        _generateButton.FlatStyle = FlatStyle.Flat;
        _generateButton.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _generateButton.ForeColor = Color.White;
        _generateButton.Margin = Padding.Empty;
        _generateButton.Name = "generateButton";
        _generateButton.Text = "生成数据包";
        _generateButton.UseVisualStyleBackColor = false;
        _generateButton.Click += GenerateButton_Click;
        //
        // lastPackage
        //
        _lastPackage.AutoEllipsis = true;
        _lastPackage.Dock = DockStyle.Fill;
        _lastPackage.Font = new Font("Microsoft YaHei UI", 9F);
        _lastPackage.ForeColor = Color.FromArgb(64, 64, 64);
        _lastPackage.Margin = new Padding(0, 0, 0, 14);
        _lastPackage.Name = "lastPackage";
        _lastPackage.Text = "尚未生成本次数据包";
        _lastPackage.TextAlign = ContentAlignment.MiddleLeft;
        //
        // publishTitle
        //
        _publishTitle.Dock = DockStyle.Fill;
        _publishTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _publishTitle.ForeColor = Color.FromArgb(64, 64, 64);
        _publishTitle.Name = "publishTitle";
        _publishTitle.Text = "发布到 ProductAlignInspector 目标目录";
        _publishTitle.TextAlign = ContentAlignment.BottomLeft;
        //
        // publishPanel
        //
        _publishPanel.BackColor = Color.White;
        _publishPanel.ColumnCount = 3;
        _publishPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _publishPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _publishPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
        _publishPanel.Controls.Add(_publishTarget, 0, 0);
        _publishPanel.Controls.Add(_browseButton, 1, 0);
        _publishPanel.Controls.Add(_publishButton, 2, 0);
        _publishPanel.Dock = DockStyle.Fill;
        _publishPanel.Margin = Padding.Empty;
        _publishPanel.Name = "publishPanel";
        _publishPanel.RowCount = 1;
        _publishPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // publishTarget
        //
        _publishTarget.BackColor = Color.White;
        _publishTarget.BorderStyle = BorderStyle.FixedSingle;
        _publishTarget.Dock = DockStyle.Fill;
        _publishTarget.Font = new Font("Microsoft YaHei UI", 9.5F);
        _publishTarget.ForeColor = Color.FromArgb(32, 32, 32);
        _publishTarget.Margin = new Padding(0, 0, 8, 0);
        _publishTarget.Name = "publishTarget";
        _publishTarget.PlaceholderText = "例如 D:\\Brunei";
        //
        // browseButton
        //
        _browseButton.BackColor = Color.White;
        _browseButton.Cursor = Cursors.Hand;
        _browseButton.Dock = DockStyle.Fill;
        _browseButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _browseButton.FlatStyle = FlatStyle.Flat;
        _browseButton.Font = new Font("Microsoft YaHei UI", 9F);
        _browseButton.ForeColor = Color.FromArgb(32, 32, 32);
        _browseButton.Margin = new Padding(0, 0, 8, 0);
        _browseButton.Name = "browseButton";
        _browseButton.Text = "浏览";
        _browseButton.UseVisualStyleBackColor = false;
        _browseButton.Click += BrowseButton_Click;
        //
        // publishButton
        //
        _publishButton.BackColor = Color.White;
        _publishButton.Cursor = Cursors.Hand;
        _publishButton.Dock = DockStyle.Fill;
        _publishButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _publishButton.FlatStyle = FlatStyle.Flat;
        _publishButton.Font = new Font("Microsoft YaHei UI", 9F);
        _publishButton.ForeColor = Color.FromArgb(32, 32, 32);
        _publishButton.Margin = Padding.Empty;
        _publishButton.Name = "publishButton";
        _publishButton.Text = "安全发布";
        _publishButton.UseVisualStyleBackColor = false;
        _publishButton.Click += PublishButton_Click;
        //
        // safetyLabel
        //
        _safetyLabel.Dock = DockStyle.Fill;
        _safetyLabel.Font = new Font("Microsoft YaHei UI", 9F);
        _safetyLabel.ForeColor = Color.FromArgb(92, 92, 92);
        _safetyLabel.Margin = new Padding(0, 14, 0, 0);
        _safetyLabel.Name = "safetyLabel";
        _safetyLabel.Text = "安全策略：源图片永不删除/移动/重命名；生成与发布都先进入 staging；复制文件逐个做 SHA-256 校验。发布前备份 DatasetStudio 管理的目标项，失败会尝试自动回滚。";
        _safetyLabel.TextAlign = ContentAlignment.TopLeft;
        //
        // ExportPage
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(244, 245, 246);
        Controls.Add(_rootCard);
        Name = "ExportPage";
        Size = new Size(1200, 760);
        _rootCard.ResumeLayout(false);
        _mainLayout.ResumeLayout(false);
        _generatedCountsTable.ResumeLayout(false);
        _generatedPanel.ResumeLayout(false);
        _countsPanel.ResumeLayout(false);
        _trainGoodPanel.ResumeLayout(false);
        _testGoodPanel.ResumeLayout(false);
        _testNgPanel.ResumeLayout(false);
        _ignoredPanel.ResumeLayout(false);
        _actionPanel.ResumeLayout(false);
        _publishPanel.ResumeLayout(false);
        _publishPanel.PerformLayout();
        ResumeLayout(false);
    }
}
