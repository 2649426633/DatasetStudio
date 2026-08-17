using DatasetStudio.WinForms.Controls;

namespace DatasetStudio.WinForms.Pages;

partial class RoiCalibrationPage
{
    private System.ComponentModel.IContainer? components = null;
    private TableLayoutPanel _rootLayout = null!;
    private TableLayoutPanel _toolbar = null!;
    private FlowLayoutPanel _toolsPanel = null!;
    private Label _toolsLabel = null!;
    private Button _selectToolButton = null!;
    private Button _screwToolButton = null!;
    private Button _emptyToolButton = null!;
    private Button _springToolButton = null!;
    private Button _anomalyToolButton = null!;
    private FlowLayoutPanel _actionsPanel = null!;
    private Label _modeLabel = null!;
    private Label _referenceLabel = null!;
    private Button _referenceButton = null!;
    private TableLayoutPanel _workArea = null!;
    private CardPanel _viewerCard = null!;
    private ImageCanvas _canvas = null!;
    private CardPanel _gridCard = null!;
    private TableLayoutPanel _gridLayout = null!;
    private Label _gridTitle = null!;
    private DataGridView _grid = null!;
    private DataGridViewTextBoxColumn _idColumn = null!;
    private DataGridViewTextBoxColumn _kindColumn = null!;
    private DataGridViewTextBoxColumn _expectedColumn = null!;
    private DataGridViewTextBoxColumn _xColumn = null!;
    private DataGridViewTextBoxColumn _yColumn = null!;
    private DataGridViewTextBoxColumn _wColumn = null!;
    private DataGridViewTextBoxColumn _hColumn = null!;
    private DataGridViewCheckBoxColumn _enabledColumn = null!;
    private TableLayoutPanel _gridButtons = null!;
    private Button _duplicateButton = null!;
    private Button _deleteButton = null!;
    private Button _fitButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _rootLayout = new TableLayoutPanel();
        _toolbar = new TableLayoutPanel();
        _toolsPanel = new FlowLayoutPanel();
        _toolsLabel = new Label();
        _selectToolButton = new Button();
        _screwToolButton = new Button();
        _emptyToolButton = new Button();
        _springToolButton = new Button();
        _anomalyToolButton = new Button();
        _actionsPanel = new FlowLayoutPanel();
        _modeLabel = new Label();
        _referenceLabel = new Label();
        _referenceButton = new Button();
        _workArea = new TableLayoutPanel();
        _viewerCard = new CardPanel();
        _canvas = new ImageCanvas();
        _gridCard = new CardPanel();
        _gridLayout = new TableLayoutPanel();
        _gridTitle = new Label();
        _grid = new DataGridView();
        _idColumn = new DataGridViewTextBoxColumn();
        _kindColumn = new DataGridViewTextBoxColumn();
        _expectedColumn = new DataGridViewTextBoxColumn();
        _xColumn = new DataGridViewTextBoxColumn();
        _yColumn = new DataGridViewTextBoxColumn();
        _wColumn = new DataGridViewTextBoxColumn();
        _hColumn = new DataGridViewTextBoxColumn();
        _enabledColumn = new DataGridViewCheckBoxColumn();
        _gridButtons = new TableLayoutPanel();
        _duplicateButton = new Button();
        _deleteButton = new Button();
        _fitButton = new Button();
        _rootLayout.SuspendLayout();
        _toolbar.SuspendLayout();
        _toolsPanel.SuspendLayout();
        _actionsPanel.SuspendLayout();
        _workArea.SuspendLayout();
        _viewerCard.SuspendLayout();
        _gridCard.SuspendLayout();
        _gridLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        _gridButtons.SuspendLayout();
        SuspendLayout();
        //
        // rootLayout
        //
        _rootLayout.BackColor = Color.FromArgb(244, 245, 246);
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_toolbar, 0, 0);
        _rootLayout.Controls.Add(_workArea, 0, 1);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Margin = Padding.Empty;
        _rootLayout.Name = "rootLayout";
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // toolbar
        //
        _toolbar.BackColor = Color.White;
        _toolbar.ColumnCount = 2;
        _toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _toolbar.Controls.Add(_toolsPanel, 0, 0);
        _toolbar.Controls.Add(_actionsPanel, 1, 0);
        _toolbar.Dock = DockStyle.Fill;
        _toolbar.Margin = Padding.Empty;
        _toolbar.Name = "toolbar";
        _toolbar.Padding = new Padding(12, 0, 8, 0);
        _toolbar.RowCount = 1;
        _toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // toolsPanel
        //
        _toolsPanel.BackColor = Color.White;
        _toolsPanel.Controls.Add(_toolsLabel);
        _toolsPanel.Controls.Add(_selectToolButton);
        _toolsPanel.Controls.Add(_screwToolButton);
        _toolsPanel.Controls.Add(_emptyToolButton);
        _toolsPanel.Controls.Add(_springToolButton);
        _toolsPanel.Controls.Add(_anomalyToolButton);
        _toolsPanel.Dock = DockStyle.Fill;
        _toolsPanel.FlowDirection = FlowDirection.LeftToRight;
        _toolsPanel.Margin = Padding.Empty;
        _toolsPanel.Name = "toolsPanel";
        _toolsPanel.WrapContents = false;
        //
        // toolsLabel
        //
        _toolsLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _toolsLabel.ForeColor = Color.FromArgb(92, 92, 92);
        _toolsLabel.Margin = Padding.Empty;
        _toolsLabel.Name = "toolsLabel";
        _toolsLabel.Size = new Size(76, 49);
        _toolsLabel.Text = "标定工具：";
        _toolsLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // tool buttons
        //
        ConfigureDesignerToolButton(_selectToolButton, "选择 / 移动", 100);
        ConfigureDesignerToolButton(_screwToolButton, "螺丝孔 (S)", 100);
        ConfigureDesignerToolButton(_emptyToolButton, "空位 (E)", 100);
        ConfigureDesignerToolButton(_springToolButton, "弹簧区 (P)", 104);
        ConfigureDesignerToolButton(_anomalyToolButton, "异常区 (A)", 104);
        _selectToolButton.Click += SelectToolButton_Click;
        _screwToolButton.Click += ScrewToolButton_Click;
        _emptyToolButton.Click += EmptyToolButton_Click;
        _springToolButton.Click += SpringToolButton_Click;
        _anomalyToolButton.Click += AnomalyToolButton_Click;
        //
        // actionsPanel
        //
        _actionsPanel.AutoSize = true;
        _actionsPanel.BackColor = Color.White;
        _actionsPanel.Controls.Add(_modeLabel);
        _actionsPanel.Controls.Add(_referenceLabel);
        _actionsPanel.Controls.Add(_referenceButton);
        _actionsPanel.Dock = DockStyle.Fill;
        _actionsPanel.FlowDirection = FlowDirection.LeftToRight;
        _actionsPanel.Margin = Padding.Empty;
        _actionsPanel.Name = "actionsPanel";
        _actionsPanel.WrapContents = false;
        //
        // modeLabel
        //
        _modeLabel.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
        _modeLabel.ForeColor = Color.FromArgb(64, 64, 64);
        _modeLabel.Margin = Padding.Empty;
        _modeLabel.Name = "modeLabel";
        _modeLabel.Size = new Size(126, 49);
        _modeLabel.Text = "模式：选择 / 移动";
        _modeLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // referenceLabel
        //
        _referenceLabel.AutoEllipsis = true;
        _referenceLabel.Font = new Font("Microsoft YaHei UI", 8.5F);
        _referenceLabel.ForeColor = Color.FromArgb(92, 92, 92);
        _referenceLabel.Margin = new Padding(8, 0, 8, 0);
        _referenceLabel.Name = "referenceLabel";
        _referenceLabel.Size = new Size(160, 49);
        _referenceLabel.Text = "参考图：未设置";
        _referenceLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // referenceButton
        //
        _referenceButton.BackColor = Color.White;
        _referenceButton.Cursor = Cursors.Hand;
        _referenceButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _referenceButton.FlatStyle = FlatStyle.Flat;
        _referenceButton.Font = new Font("Microsoft YaHei UI", 9F);
        _referenceButton.ForeColor = Color.FromArgb(32, 32, 32);
        _referenceButton.Margin = new Padding(0, 8, 0, 0);
        _referenceButton.Name = "referenceButton";
        _referenceButton.Size = new Size(118, 32);
        _referenceButton.Text = "更换参考图";
        _referenceButton.UseVisualStyleBackColor = false;
        _referenceButton.Click += ReferenceButton_Click;
        //
        // workArea
        //
        _workArea.BackColor = Color.FromArgb(244, 245, 246);
        _workArea.ColumnCount = 2;
        _workArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _workArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
        _workArea.Controls.Add(_viewerCard, 0, 0);
        _workArea.Controls.Add(_gridCard, 1, 0);
        _workArea.Dock = DockStyle.Fill;
        _workArea.Margin = Padding.Empty;
        _workArea.Name = "workArea";
        _workArea.Padding = new Padding(0, 12, 0, 0);
        _workArea.RowCount = 1;
        _workArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // viewerCard
        //
        _viewerCard.BackColor = Color.White;
        _viewerCard.BorderColor = Color.FromArgb(218, 220, 222);
        _viewerCard.Controls.Add(_canvas);
        _viewerCard.Dock = DockStyle.Fill;
        _viewerCard.Margin = new Padding(0, 0, 12, 0);
        _viewerCard.Name = "viewerCard";
        _viewerCard.Padding = new Padding(1);
        //
        // canvas
        //
        _canvas.AllowRoiEditing = true;
        _canvas.BackColor = Color.FromArgb(30, 30, 30);
        _canvas.Dock = DockStyle.Fill;
        _canvas.Name = "canvas";
        _canvas.ShowRois = true;
        //
        // gridCard
        //
        _gridCard.BackColor = Color.White;
        _gridCard.BorderColor = Color.FromArgb(218, 220, 222);
        _gridCard.Controls.Add(_gridLayout);
        _gridCard.Dock = DockStyle.Fill;
        _gridCard.Margin = Padding.Empty;
        _gridCard.Name = "gridCard";
        _gridCard.Padding = new Padding(15);
        //
        // gridLayout
        //
        _gridLayout.BackColor = Color.White;
        _gridLayout.ColumnCount = 1;
        _gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _gridLayout.Controls.Add(_gridTitle, 0, 0);
        _gridLayout.Controls.Add(_grid, 0, 1);
        _gridLayout.Controls.Add(_gridButtons, 0, 2);
        _gridLayout.Dock = DockStyle.Fill;
        _gridLayout.Margin = Padding.Empty;
        _gridLayout.Name = "gridLayout";
        _gridLayout.RowCount = 3;
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        //
        // gridTitle
        //
        _gridTitle.Dock = DockStyle.Fill;
        _gridTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        _gridTitle.ForeColor = Color.FromArgb(32, 32, 32);
        _gridTitle.Name = "gridTitle";
        _gridTitle.Text = "ROI 列表";
        _gridTitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // grid
        //
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.ColumnHeadersHeight = 38;
        _grid.Columns.AddRange(new DataGridViewColumn[] { _idColumn, _kindColumn, _expectedColumn, _xColumn, _yColumn, _wColumn, _hColumn, _enabledColumn });
        _grid.Dock = DockStyle.Fill;
        _grid.GridColor = Color.FromArgb(218, 220, 222);
        _grid.Margin = new Padding(0, 0, 0, 10);
        _grid.MultiSelect = false;
        _grid.Name = "grid";
        _grid.ReadOnly = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        //
        // columns
        //
        ConfigureTextColumn(_idColumn, "Id", "ID", 13F, true);
        ConfigureTextColumn(_kindColumn, "Kind", "类别", 18F, true);
        ConfigureTextColumn(_expectedColumn, "Expected", "期望", 18F, true);
        ConfigureTextColumn(_xColumn, "X", "X", 9F, false);
        ConfigureTextColumn(_yColumn, "Y", "Y", 9F, false);
        ConfigureTextColumn(_wColumn, "W", "W", 9F, false);
        ConfigureTextColumn(_hColumn, "H", "H", 9F, false);
        _enabledColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _enabledColumn.FillWeight = 15F;
        _enabledColumn.HeaderText = "启用";
        _enabledColumn.Name = "Enabled";
        //
        // gridButtons
        //
        _gridButtons.BackColor = Color.White;
        _gridButtons.ColumnCount = 4;
        _gridButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
        _gridButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        _gridButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _gridButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
        _gridButtons.Controls.Add(_duplicateButton, 0, 0);
        _gridButtons.Controls.Add(_deleteButton, 1, 0);
        _gridButtons.Controls.Add(_fitButton, 3, 0);
        _gridButtons.Dock = DockStyle.Fill;
        _gridButtons.Margin = Padding.Empty;
        _gridButtons.Name = "gridButtons";
        _gridButtons.RowCount = 1;
        _gridButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        ConfigureActionButton(_duplicateButton, "复制 ROI", Color.FromArgb(32, 32, 32));
        ConfigureActionButton(_deleteButton, "删除", Color.FromArgb(185, 28, 28));
        ConfigureActionButton(_fitButton, "适应窗口", Color.FromArgb(32, 32, 32));
        _duplicateButton.Click += DuplicateButton_Click;
        _deleteButton.Click += DeleteButton_Click;
        _fitButton.Click += FitButton_Click;
        //
        // RoiCalibrationPage
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(244, 245, 246);
        Controls.Add(_rootLayout);
        Name = "RoiCalibrationPage";
        Size = new Size(1480, 820);
        Resize += RoiCalibrationPage_Resize;
        _rootLayout.ResumeLayout(false);
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        _toolsPanel.ResumeLayout(false);
        _actionsPanel.ResumeLayout(false);
        _workArea.ResumeLayout(false);
        _viewerCard.ResumeLayout(false);
        _gridCard.ResumeLayout(false);
        _gridLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        _gridButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private static void ConfigureDesignerToolButton(Button button, string text, int width)
    {
        button.BackColor = Color.White;
        button.Cursor = Cursors.Hand;
        button.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Microsoft YaHei UI", 8.5F);
        button.ForeColor = Color.FromArgb(32, 32, 32);
        button.Margin = new Padding(0, 8, 6, 0);
        button.Size = new Size(width, 32);
        button.Text = text;
        button.UseVisualStyleBackColor = false;
    }

    private static void ConfigureTextColumn(DataGridViewTextBoxColumn column, string name, string header, float fillWeight, bool readOnly)
    {
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        column.FillWeight = fillWeight;
        column.HeaderText = header;
        column.Name = name;
        column.ReadOnly = readOnly;
    }

    private static void ConfigureActionButton(Button button, string text, Color foreColor)
    {
        button.BackColor = Color.White;
        button.Cursor = Cursors.Hand;
        button.Dock = DockStyle.Fill;
        button.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Microsoft YaHei UI", 9F);
        button.ForeColor = foreColor;
        button.Margin = new Padding(0, 5, 8, 5);
        button.Text = text;
        button.UseVisualStyleBackColor = false;
    }
}
