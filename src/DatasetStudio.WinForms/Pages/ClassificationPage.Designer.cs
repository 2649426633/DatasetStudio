using DatasetStudio.WinForms.Controls;

namespace DatasetStudio.WinForms.Pages;

partial class ClassificationPage
{
    private System.ComponentModel.IContainer? components = null;
    private TableLayoutPanel _rootLayout = null!;
    private CardPanel _imageListCard = null!;
    private TableLayoutPanel _imageListLayout = null!;
    private TableLayoutPanel _imageListHeader = null!;
    private Label _imageListTitle = null!;
    private Button _rescanButton = null!;
    private TextBox _searchBox = null!;
    private CheckBox _onlyUnclassified = null!;
    private ListView _imagesList = null!;
    private ColumnHeader _fileColumn = null!;
    private ColumnHeader _statusColumn = null!;
    private CardPanel _viewerCard = null!;
    private ImageCanvas _canvas = null!;
    private CardPanel _classificationCard = null!;
    private TableLayoutPanel _classificationLayout = null!;
    private Label _classificationTitle = null!;
    private TableLayoutPanel _metaLayout = null!;
    private Label _fileName = null!;
    private Label _pathLabel = null!;
    private Label _categoryLabel = null!;
    private TableLayoutPanel _categoryTable = null!;
    private RadioButton _trainGood = null!;
    private RadioButton _testGood = null!;
    private RadioButton _testNg = null!;
    private RadioButton _ignore = null!;
    private Label _roiTitle = null!;
    private CheckedListBox _roiList = null!;
    private Label _defectTitle = null!;
    private ComboBox _defectType = null!;
    private Label _noteTitle = null!;
    private TextBox _note = null!;
    private Button _saveNext = null!;
    private Label _helpLabel = null!;
    private CardPanel _statsCard = null!;
    private Label _stats = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _rootLayout = new TableLayoutPanel();
        _imageListCard = new CardPanel();
        _imageListLayout = new TableLayoutPanel();
        _imageListHeader = new TableLayoutPanel();
        _imageListTitle = new Label();
        _rescanButton = new Button();
        _searchBox = new TextBox();
        _onlyUnclassified = new CheckBox();
        _imagesList = new ListView();
        _fileColumn = new ColumnHeader();
        _statusColumn = new ColumnHeader();
        _viewerCard = new CardPanel();
        _canvas = new ImageCanvas();
        _classificationCard = new CardPanel();
        _classificationLayout = new TableLayoutPanel();
        _classificationTitle = new Label();
        _metaLayout = new TableLayoutPanel();
        _fileName = new Label();
        _pathLabel = new Label();
        _categoryLabel = new Label();
        _categoryTable = new TableLayoutPanel();
        _trainGood = new RadioButton();
        _testGood = new RadioButton();
        _testNg = new RadioButton();
        _ignore = new RadioButton();
        _roiTitle = new Label();
        _roiList = new CheckedListBox();
        _defectTitle = new Label();
        _defectType = new ComboBox();
        _noteTitle = new Label();
        _note = new TextBox();
        _saveNext = new Button();
        _helpLabel = new Label();
        _statsCard = new CardPanel();
        _stats = new Label();
        _rootLayout.SuspendLayout();
        _imageListCard.SuspendLayout();
        _imageListLayout.SuspendLayout();
        _imageListHeader.SuspendLayout();
        _viewerCard.SuspendLayout();
        _classificationCard.SuspendLayout();
        _classificationLayout.SuspendLayout();
        _metaLayout.SuspendLayout();
        _categoryTable.SuspendLayout();
        _statsCard.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        _rootLayout.BackColor = Color.FromArgb(244, 245, 246);
        _rootLayout.ColumnCount = 3;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        _rootLayout.Controls.Add(_imageListCard, 0, 0);
        _rootLayout.Controls.Add(_viewerCard, 1, 0);
        _rootLayout.Controls.Add(_classificationCard, 2, 0);
        _rootLayout.Controls.Add(_statsCard, 0, 1);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Margin = new Padding(0);
        _rootLayout.Name = "rootLayout";
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _rootLayout.SetColumnSpan(_statsCard, 3);
        // 
        // imageListCard
        // 
        _imageListCard.BackColor = Color.White;
        _imageListCard.BorderColor = Color.FromArgb(218, 220, 222);
        _imageListCard.Controls.Add(_imageListLayout);
        _imageListCard.Dock = DockStyle.Fill;
        _imageListCard.Margin = new Padding(0, 0, 10, 0);
        _imageListCard.Name = "imageListCard";
        _imageListCard.Padding = new Padding(1);
        // 
        // imageListLayout
        // 
        _imageListLayout.BackColor = Color.White;
        _imageListLayout.ColumnCount = 1;
        _imageListLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _imageListLayout.Controls.Add(_imageListHeader, 0, 0);
        _imageListLayout.Controls.Add(_searchBox, 0, 1);
        _imageListLayout.Controls.Add(_onlyUnclassified, 0, 2);
        _imageListLayout.Controls.Add(_imagesList, 0, 3);
        _imageListLayout.Dock = DockStyle.Fill;
        _imageListLayout.Margin = new Padding(0);
        _imageListLayout.Name = "imageListLayout";
        _imageListLayout.Padding = new Padding(14);
        _imageListLayout.RowCount = 4;
        _imageListLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _imageListLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _imageListLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _imageListLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // imageListHeader
        // 
        _imageListHeader.BackColor = Color.White;
        _imageListHeader.ColumnCount = 2;
        _imageListHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _imageListHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
        _imageListHeader.Controls.Add(_imageListTitle, 0, 0);
        _imageListHeader.Controls.Add(_rescanButton, 1, 0);
        _imageListHeader.Dock = DockStyle.Fill;
        _imageListHeader.Margin = new Padding(0);
        _imageListHeader.Name = "imageListHeader";
        _imageListHeader.RowCount = 1;
        _imageListHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // imageListTitle
        // 
        _imageListTitle.Dock = DockStyle.Fill;
        _imageListTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        _imageListTitle.ForeColor = Color.FromArgb(32, 32, 32);
        _imageListTitle.Name = "imageListTitle";
        _imageListTitle.Text = "数据 / 图片列表";
        _imageListTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // rescanButton
        // 
        _rescanButton.BackColor = Color.White;
        _rescanButton.Cursor = Cursors.Hand;
        _rescanButton.Dock = DockStyle.Fill;
        _rescanButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _rescanButton.FlatStyle = FlatStyle.Flat;
        _rescanButton.Font = new Font("Microsoft YaHei UI", 9F);
        _rescanButton.ForeColor = Color.FromArgb(32, 32, 32);
        _rescanButton.Margin = new Padding(8, 2, 0, 2);
        _rescanButton.Name = "rescanButton";
        _rescanButton.Text = "重新扫描";
        _rescanButton.UseVisualStyleBackColor = false;
        _rescanButton.Click += RescanButton_Click;
        // 
        // searchBox
        // 
        _searchBox.BackColor = Color.White;
        _searchBox.BorderStyle = BorderStyle.FixedSingle;
        _searchBox.Dock = DockStyle.Fill;
        _searchBox.Font = new Font("Microsoft YaHei UI", 9.5F);
        _searchBox.ForeColor = Color.FromArgb(32, 32, 32);
        _searchBox.Margin = new Padding(0, 4, 0, 4);
        _searchBox.Name = "searchBox";
        _searchBox.PlaceholderText = "按文件名搜索...";
        _searchBox.TextChanged += SearchBox_TextChanged;
        // 
        // onlyUnclassified
        // 
        _onlyUnclassified.AutoSize = true;
        _onlyUnclassified.Dock = DockStyle.Fill;
        _onlyUnclassified.Font = new Font("Microsoft YaHei UI", 9.5F);
        _onlyUnclassified.ForeColor = Color.FromArgb(32, 32, 32);
        _onlyUnclassified.Name = "onlyUnclassified";
        _onlyUnclassified.Text = "仅未分类";
        _onlyUnclassified.TextAlign = ContentAlignment.MiddleLeft;
        _onlyUnclassified.CheckedChanged += OnlyUnclassified_CheckedChanged;
        // 
        // imagesList
        // 
        _imagesList.BackColor = Color.White;
        _imagesList.BorderStyle = BorderStyle.FixedSingle;
        _imagesList.Columns.AddRange(new ColumnHeader[] { _fileColumn, _statusColumn });
        _imagesList.Dock = DockStyle.Fill;
        _imagesList.Font = new Font("Microsoft YaHei UI", 9.5F);
        _imagesList.ForeColor = Color.FromArgb(32, 32, 32);
        _imagesList.FullRowSelect = true;
        _imagesList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _imagesList.HideSelection = false;
        _imagesList.Margin = new Padding(0, 6, 0, 0);
        _imagesList.MultiSelect = false;
        _imagesList.Name = "imagesList";
        _imagesList.UseCompatibleStateImageBehavior = false;
        _imagesList.View = View.Details;
        _imagesList.SelectedIndexChanged += ImagesList_SelectedIndexChanged;
        _fileColumn.Text = "文件";
        _fileColumn.Width = 190;
        _statusColumn.Text = "状态";
        _statusColumn.Width = 90;
        // 
        // viewerCard
        // 
        _viewerCard.BackColor = Color.White;
        _viewerCard.BorderColor = Color.FromArgb(218, 220, 222);
        _viewerCard.Controls.Add(_canvas);
        _viewerCard.Dock = DockStyle.Fill;
        _viewerCard.Margin = new Padding(0, 0, 10, 0);
        _viewerCard.Name = "viewerCard";
        _viewerCard.Padding = new Padding(1);
        // 
        // canvas
        // 
        _canvas.AllowRoiEditing = false;
        _canvas.BackColor = Color.FromArgb(30, 30, 30);
        _canvas.Dock = DockStyle.Fill;
        _canvas.Name = "canvas";
        _canvas.ShowRois = true;
        // 
        // classificationCard
        // 
        _classificationCard.BackColor = Color.White;
        _classificationCard.BorderColor = Color.FromArgb(218, 220, 222);
        _classificationCard.Controls.Add(_classificationLayout);
        _classificationCard.Dock = DockStyle.Fill;
        _classificationCard.Margin = new Padding(0);
        _classificationCard.Name = "classificationCard";
        _classificationCard.Padding = new Padding(17);
        // 
        // classificationLayout
        // 
        _classificationLayout.AutoScroll = true;
        _classificationLayout.BackColor = Color.White;
        _classificationLayout.ColumnCount = 1;
        _classificationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _classificationLayout.Controls.Add(_classificationTitle, 0, 0);
        _classificationLayout.Controls.Add(_metaLayout, 0, 1);
        _classificationLayout.Controls.Add(_categoryLabel, 0, 2);
        _classificationLayout.Controls.Add(_categoryTable, 0, 3);
        _classificationLayout.Controls.Add(_roiTitle, 0, 4);
        _classificationLayout.Controls.Add(_roiList, 0, 5);
        _classificationLayout.Controls.Add(_defectTitle, 0, 6);
        _classificationLayout.Controls.Add(_defectType, 0, 7);
        _classificationLayout.Controls.Add(_noteTitle, 0, 8);
        _classificationLayout.Controls.Add(_note, 0, 9);
        _classificationLayout.Controls.Add(_saveNext, 0, 10);
        _classificationLayout.Controls.Add(_helpLabel, 0, 11);
        _classificationLayout.Dock = DockStyle.Fill;
        _classificationLayout.Margin = new Padding(0);
        _classificationLayout.Name = "classificationLayout";
        _classificationLayout.RowCount = 12;
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        _classificationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        // 
        // classificationTitle
        // 
        _classificationTitle.Dock = DockStyle.Fill;
        _classificationTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        _classificationTitle.ForeColor = Color.FromArgb(32, 32, 32);
        _classificationTitle.Name = "classificationTitle";
        _classificationTitle.Text = "当前图片信息";
        _classificationTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // metaLayout
        // 
        _metaLayout.BackColor = Color.FromArgb(250, 250, 250);
        _metaLayout.ColumnCount = 1;
        _metaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _metaLayout.Controls.Add(_fileName, 0, 0);
        _metaLayout.Controls.Add(_pathLabel, 0, 1);
        _metaLayout.Dock = DockStyle.Fill;
        _metaLayout.Margin = new Padding(0, 0, 0, 10);
        _metaLayout.Name = "metaLayout";
        _metaLayout.Padding = new Padding(10, 6, 10, 6);
        _metaLayout.RowCount = 2;
        _metaLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _metaLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _fileName.AutoEllipsis = true;
        _fileName.Dock = DockStyle.Fill;
        _fileName.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        _fileName.ForeColor = Color.FromArgb(32, 32, 32);
        _fileName.Name = "fileName";
        _fileName.Text = "未选择图片";
        _fileName.TextAlign = ContentAlignment.MiddleLeft;
        _pathLabel.AutoEllipsis = true;
        _pathLabel.Dock = DockStyle.Fill;
        _pathLabel.Font = new Font("Microsoft YaHei UI", 8.5F);
        _pathLabel.ForeColor = Color.FromArgb(92, 92, 92);
        _pathLabel.Name = "pathLabel";
        _pathLabel.Text = "请从左侧列表选择图片开始分类";
        _pathLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // categoryLabel
        // 
        _categoryLabel.Dock = DockStyle.Fill;
        _categoryLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _categoryLabel.ForeColor = Color.FromArgb(64, 64, 64);
        _categoryLabel.Name = "categoryLabel";
        _categoryLabel.Text = "分类";
        _categoryLabel.TextAlign = ContentAlignment.BottomLeft;
        // 
        // categoryTable
        // 
        _categoryTable.BackColor = Color.White;
        _categoryTable.ColumnCount = 2;
        _categoryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _categoryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _categoryTable.Controls.Add(_trainGood, 0, 0);
        _categoryTable.Controls.Add(_testGood, 1, 0);
        _categoryTable.Controls.Add(_testNg, 0, 1);
        _categoryTable.Controls.Add(_ignore, 1, 1);
        _categoryTable.Dock = DockStyle.Fill;
        _categoryTable.Margin = new Padding(0);
        _categoryTable.Name = "categoryTable";
        _categoryTable.RowCount = 2;
        _categoryTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        _categoryTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        // 
        // trainGood
        // 
        _trainGood.Appearance = Appearance.Button;
        _trainGood.BackColor = Color.White;
        _trainGood.Cursor = Cursors.Hand;
        _trainGood.Dock = DockStyle.Fill;
        _trainGood.FlatAppearance.BorderColor = Color.FromArgb(218, 220, 222);
        _trainGood.FlatStyle = FlatStyle.Flat;
        _trainGood.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _trainGood.ForeColor = Color.FromArgb(32, 32, 32);
        _trainGood.Margin = new Padding(0, 0, 6, 6);
        _trainGood.Name = "trainGood";
        _trainGood.Text = "Train GOOD     T";
        _trainGood.TextAlign = ContentAlignment.MiddleCenter;
        _trainGood.UseVisualStyleBackColor = false;
        _trainGood.CheckedChanged += Category_CheckedChanged;
        // 
        // testGood
        // 
        _testGood.Appearance = Appearance.Button;
        _testGood.BackColor = Color.White;
        _testGood.Cursor = Cursors.Hand;
        _testGood.Dock = DockStyle.Fill;
        _testGood.FlatAppearance.BorderColor = Color.FromArgb(218, 220, 222);
        _testGood.FlatStyle = FlatStyle.Flat;
        _testGood.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _testGood.ForeColor = Color.FromArgb(32, 32, 32);
        _testGood.Margin = new Padding(0, 0, 6, 6);
        _testGood.Name = "testGood";
        _testGood.Text = "Test GOOD      G";
        _testGood.TextAlign = ContentAlignment.MiddleCenter;
        _testGood.UseVisualStyleBackColor = false;
        _testGood.CheckedChanged += Category_CheckedChanged;
        // 
        // testNg
        // 
        _testNg.Appearance = Appearance.Button;
        _testNg.BackColor = Color.White;
        _testNg.Cursor = Cursors.Hand;
        _testNg.Dock = DockStyle.Fill;
        _testNg.FlatAppearance.BorderColor = Color.FromArgb(218, 220, 222);
        _testNg.FlatStyle = FlatStyle.Flat;
        _testNg.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _testNg.ForeColor = Color.FromArgb(32, 32, 32);
        _testNg.Margin = new Padding(0, 0, 6, 6);
        _testNg.Name = "testNg";
        _testNg.Text = "Test NG          N";
        _testNg.TextAlign = ContentAlignment.MiddleCenter;
        _testNg.UseVisualStyleBackColor = false;
        _testNg.CheckedChanged += Category_CheckedChanged;
        // 
        // ignore
        // 
        _ignore.Appearance = Appearance.Button;
        _ignore.BackColor = Color.White;
        _ignore.Cursor = Cursors.Hand;
        _ignore.Dock = DockStyle.Fill;
        _ignore.FlatAppearance.BorderColor = Color.FromArgb(218, 220, 222);
        _ignore.FlatStyle = FlatStyle.Flat;
        _ignore.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _ignore.ForeColor = Color.FromArgb(32, 32, 32);
        _ignore.Margin = new Padding(0, 0, 6, 6);
        _ignore.Name = "ignore";
        _ignore.Text = "Ignore             I";
        _ignore.TextAlign = ContentAlignment.MiddleCenter;
        _ignore.UseVisualStyleBackColor = false;
        _ignore.CheckedChanged += Category_CheckedChanged;
        // 
        // roiTitle
        // 
        _roiTitle.Dock = DockStyle.Fill;
        _roiTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _roiTitle.ForeColor = Color.FromArgb(64, 64, 64);
        _roiTitle.Name = "roiTitle";
        _roiTitle.Text = "NG 异常 ROI";
        _roiTitle.TextAlign = ContentAlignment.BottomLeft;
        // 
        // roiList
        // 
        _roiList.BackColor = Color.White;
        _roiList.BorderStyle = BorderStyle.FixedSingle;
        _roiList.CheckOnClick = true;
        _roiList.Dock = DockStyle.Fill;
        _roiList.Font = new Font("Microsoft YaHei UI", 9.5F);
        _roiList.ForeColor = Color.FromArgb(32, 32, 32);
        _roiList.Margin = new Padding(0, 0, 0, 8);
        _roiList.Name = "roiList";
        // 
        // defectTitle
        // 
        _defectTitle.Dock = DockStyle.Fill;
        _defectTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _defectTitle.ForeColor = Color.FromArgb(64, 64, 64);
        _defectTitle.Name = "defectTitle";
        _defectTitle.Text = "缺陷类型";
        _defectTitle.TextAlign = ContentAlignment.BottomLeft;
        // 
        // defectType
        // 
        _defectType.BackColor = Color.White;
        _defectType.Dock = DockStyle.Fill;
        _defectType.DropDownStyle = ComboBoxStyle.DropDownList;
        _defectType.FlatStyle = FlatStyle.Flat;
        _defectType.Font = new Font("Microsoft YaHei UI", 9.5F);
        _defectType.ForeColor = Color.FromArgb(32, 32, 32);
        _defectType.Margin = new Padding(0, 0, 0, 8);
        _defectType.Name = "defectType";
        // 
        // noteTitle
        // 
        _noteTitle.Dock = DockStyle.Fill;
        _noteTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _noteTitle.ForeColor = Color.FromArgb(64, 64, 64);
        _noteTitle.Name = "noteTitle";
        _noteTitle.Text = "备注";
        _noteTitle.TextAlign = ContentAlignment.BottomLeft;
        // 
        // note
        // 
        _note.BackColor = Color.White;
        _note.BorderStyle = BorderStyle.FixedSingle;
        _note.Dock = DockStyle.Fill;
        _note.Font = new Font("Microsoft YaHei UI", 9.5F);
        _note.ForeColor = Color.FromArgb(32, 32, 32);
        _note.Margin = new Padding(0, 0, 0, 10);
        _note.Multiline = true;
        _note.Name = "note";
        _note.ScrollBars = ScrollBars.Vertical;
        // 
        // saveNext
        // 
        _saveNext.BackColor = Color.FromArgb(32, 32, 32);
        _saveNext.Cursor = Cursors.Hand;
        _saveNext.Dock = DockStyle.Fill;
        _saveNext.FlatAppearance.BorderColor = Color.FromArgb(32, 32, 32);
        _saveNext.FlatStyle = FlatStyle.Flat;
        _saveNext.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _saveNext.ForeColor = Color.White;
        _saveNext.Margin = new Padding(0, 0, 0, 8);
        _saveNext.Name = "saveNext";
        _saveNext.Text = "保存 + 下一张";
        _saveNext.UseVisualStyleBackColor = false;
        _saveNext.Click += SaveNext_Click;
        // 
        // helpLabel
        // 
        _helpLabel.Dock = DockStyle.Fill;
        _helpLabel.Font = new Font("Microsoft YaHei UI", 9F);
        _helpLabel.ForeColor = Color.FromArgb(92, 92, 92);
        _helpLabel.Name = "helpLabel";
        _helpLabel.Text = "快捷键：T/G/N/I 分类 · 1-9 ROI · Enter 保存 · Space 下一张未分类 · ←/→ 切换";
        _helpLabel.TextAlign = ContentAlignment.TopLeft;
        // 
        // statsCard
        // 
        _statsCard.BackColor = Color.White;
        _statsCard.BorderColor = Color.FromArgb(218, 220, 222);
        _statsCard.Controls.Add(_stats);
        _statsCard.Dock = DockStyle.Fill;
        _statsCard.Margin = new Padding(0, 10, 0, 0);
        _statsCard.Name = "statsCard";
        _statsCard.Padding = new Padding(1);
        // 
        // stats
        // 
        _stats.BackColor = Color.White;
        _stats.Dock = DockStyle.Fill;
        _stats.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _stats.ForeColor = Color.FromArgb(64, 64, 64);
        _stats.Name = "stats";
        _stats.Padding = new Padding(14, 0, 0, 0);
        _stats.Text = "尚未打开项目";
        _stats.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ClassificationPage
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(244, 245, 246);
        Controls.Add(_rootLayout);
        Name = "ClassificationPage";
        Size = new Size(1480, 820);
        Resize += ClassificationPage_Resize;
        _rootLayout.ResumeLayout(false);
        _imageListCard.ResumeLayout(false);
        _imageListLayout.ResumeLayout(false);
        _imageListLayout.PerformLayout();
        _imageListHeader.ResumeLayout(false);
        _viewerCard.ResumeLayout(false);
        _classificationCard.ResumeLayout(false);
        _classificationLayout.ResumeLayout(false);
        _classificationLayout.PerformLayout();
        _metaLayout.ResumeLayout(false);
        _categoryTable.ResumeLayout(false);
        _statsCard.ResumeLayout(false);
        ResumeLayout(false);
    }
}
