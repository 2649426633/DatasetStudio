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
        {
            components?.Dispose();
        }
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
        _rootLayout.Margin = new Padding(0);
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
        _toolbar.Margin = new Padding(0);
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
        _toolsPanel.Margin = new Padding(0);
        _toolsPanel.Name = "toolsPanel";
        _toolsPanel.WrapContents = false;
        // 
        // toolsLabel
        // 
        _toolsLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        _toolsLabel.ForeColor = Color.FromArgb(92, 92, 92);
        _toolsLabel.Margin = new Padding(0);
        _toolsLabel.Name = "toolsLabel";
        _toolsLabel.Size = new Size(76, 49);
        _toolsLabel.Text = "标定工具：";
        _toolsLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // selectToolButton
        // 
        _selectToolButton.BackColor = Color.White;
        _selectToolButton.Cursor = Cursors.Hand;
        _selectToolButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _selectToolButton.FlatStyle = FlatStyle.Flat;
        _selectToolButton.Font = new Font("Microsoft YaHei UI", 8.5F);
        _selectToolButton.ForeColor = Color.FromArgb(32, 32, 32);
        _selectToolButton.Margin = new Padding(0, 8, 6, 0);
        _selectToolButton.Name = "selectToolButton";
        _selectToolButton.Size = new Size(100, 32);
        _selectToolButton.Text = "选择 / 移动";
        _selectToolButton.UseVisualStyleBackColor = false;
        _selectToolButton.Click += SelectToolButton_Click;
        // 
        // screwToolButton
        // 
        _screwToolButton.BackColor = Color.White;
        _screwToolButton.Cursor = Cursors.Hand;
        _screwToolButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _screwToolButton.FlatStyle = FlatStyle.Flat;
        _screwToolButton.Font = new Font("Microsoft YaHei UI", 8.5F);
        _screwToolButton.ForeColor = Color.FromArgb(32, 32, 32);
        _screwToolButton.Margin = new Padding(0, 8, 6, 0);
        _screwToolButton.Name = "screwToolButton";
        _screwToolButton.Size = new Size(100, 32);
        _screwToolButton.Text = "螺丝孔 (S)";
        _screwToolButton.UseVisualStyleBackColor = false;
        _screwToolButton.Click += ScrewToolButton_Click;
        // 
        // emptyToolButton
        // 
        _emptyToolButton.BackColor = Color.White;
        _emptyToolButton.Cursor = Cursors.Hand;
        _emptyToolButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _emptyToolButton.FlatStyle = FlatStyle.Flat;
        _emptyToolButton.Font = new Font("Microsoft YaHei UI", 8.5F);
        _emptyToolButton.ForeColor = Color.FromArgb(32, 32, 32);
        _emptyToolButton.Margin = new Padding(0, 8, 6, 0);
        _emptyToolButton.Name = "emptyToolButton";
        _emptyToolButton.Size = new Size(100, 32);
        _emptyToolButton.Text = "空位 (E)";
        _emptyToolButton.UseVisualStyleBackColor = false;
        _emptyToolButton.Click += EmptyToolButton_Click;
        // 
        // springToolButton
        // 
        _springToolButton.BackColor = Color.White;
        _springToolButton.Cursor = Cursors.Hand;
        _springToolButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _springToolButton.FlatStyle = FlatStyle.Flat;
        _springToolButton.Font = new Font("Microsoft YaHei UI", 8.5F);
        _springToolButton.ForeColor = Color.FromArgb(32, 32, 32);
        _springToolButton.Margin = new Padding(0, 8, 6, 0);
        _springToolButton.Name = "springToolButton";
        _springToolButton.Size = new Size(104, 32);
        _springToolButton.Text = "弹簧区 (P)";
        _springToolButton.UseVisualStyleBackColor = false;
        _springToolButton.Click += SpringToolButton_Click;
        // 
        // anomalyToolButton
        // 
        _anomalyToolButton.BackColor = Color.White;
        _anomalyToolButton.Cursor = Cursors.Hand;
        _anomalyToolButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _anomalyToolButton.FlatStyle = FlatStyle.Flat;
        _anomalyToolButton.Font = new Font("Microsoft YaHei UI", 8.5F);
        _anomalyToolButton.ForeColor = Color.FromArgb(32, 32, 32);
        _anomalyToolButton.Margin = new Padding(0, 8, 6, 0);
        _anomalyToolButton.Name = "anomalyToolButton";
        _anomalyToolButton.Size = new Size(104, 32);
        _anomalyToolButton.Text = "异常区 (A)";
        _anomalyToolButton.UseVisualStyleBackColor = false;
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
        _actionsPanel.Margin = new Padding(0);
        _actionsPanel.Name = "actionsPanel";
        _actionsPanel.WrapContents = false;
        // 
        // modeLabel
        // 
        _modeLabel.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
        _modeLabel.ForeColor = Color.FromArgb(64, 64, 64);
        _modeLabel.Margin = new Padding(0);
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
        _workArea.Margin = new Padding(0);
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
        _gridCard.Margin = new Padding(0);
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
        _gridLayout.Margin = new Padding(0);
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
        // idColumn
        // 
        _idColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _idColumn.FillWeight = 13F;
        _idColumn.HeaderText = "ID";
        _idColumn.Name = "Id";
        _idColumn.ReadOnly = true;
        // 
        // kindColumn
        // 
        _kindColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _kindColumn.FillWeight = 18F;
        _kindColumn.HeaderText = "类别";
        _kindColumn.Name = "Kind";
        _kindColumn.ReadOnly = true;
        // 
        // expectedColumn
        // 
        _expectedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _expectedColumn.FillWeight = 18F;
        _expectedColumn.HeaderText = "期望";
        _expectedColumn.Name = "Expected";
        _expectedColumn.ReadOnly = true;
        // 
        // xColumn
        // 
        _xColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _xColumn.FillWeight = 9F;
        _xColumn.HeaderText = "X";
        _xColumn.Name = "X";
        // 
        // yColumn
        // 
        _yColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _yColumn.FillWeight = 9F;
        _yColumn.HeaderText = "Y";
        _yColumn.Name = "Y";
        // 
        // wColumn
        // 
        _wColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _wColumn.FillWeight = 9F;
        _wColumn.HeaderText = "W";
        _wColumn.Name = "W";
        // 
        // hColumn
        // 
        _hColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _hColumn.FillWeight = 9F;
        _hColumn.HeaderText = "H";
        _hColumn.Name = "H";
        // 
        // enabledColumn
        // 
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
        _gridButtons.Margin = new Padding(0);
        _gridButtons.Name = "gridButtons";
        _gridButtons.RowCount = 1;
        _gridButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // duplicateButton
        // 
        _duplicateButton.BackColor = Color.White;
        _duplicateButton.Cursor = Cursors.Hand;
        _duplicateButton.Dock = DockStyle.Fill;
        _duplicateButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _duplicateButton.FlatStyle = FlatStyle.Flat;
        _duplicateButton.Font = new Font("Microsoft YaHei UI", 9F);
        _duplicateButton.ForeColor = Color.FromArgb(32, 32, 32);
        _duplicateButton.Margin = new Padding(0, 6, 6, 6);
        _duplicateButton.Name = "duplicateButton";
        _duplicateButton.Text = "复制 ROI";
        _duplicateButton.UseVisualStyleBackColor = false;
        _duplicateButton.Click += DuplicateButton_Click;
        // 
        // deleteButton
        // 
        _deleteButton.BackColor = Color.White;
        _deleteButton.Cursor = Cursors.Hand;
        _deleteButton.Dock = DockStyle.Fill;
        _deleteButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _deleteButton.FlatStyle = FlatStyle.Flat;
        _deleteButton.Font = new Font("Microsoft YaHei UI", 9F);
        _deleteButton.ForeColor = Color.FromArgb(185, 28, 28);
        _deleteButton.Margin = new Padding(4, 6, 6, 6);
        _deleteButton.Name = "deleteButton";
        _deleteButton.Text = "删除";
        _deleteButton.UseVisualStyleBackColor = false;
        _deleteButton.Click += DeleteButton_Click;
        // 
        // fitButton
        // 
        _fitButton.BackColor = Color.White;
        _fitButton.Cursor = Cursors.Hand;
        _fitButton.Dock = DockStyle.Fill;
        _fitButton.FlatAppearance.BorderColor = Color.FromArgb(194, 196, 198);
        _fitButton.FlatStyle = FlatStyle.Flat;
        _fitButton.Font = new Font("Microsoft YaHei UI", 9F);
        _fitButton.ForeColor = Color.FromArgb(32, 32, 32);
        _fitButton.Margin = new Padding(6);
        _fitButton.Name = "fitButton";
        _fitButton.Text = "适应窗口";
        _fitButton.UseVisualStyleBackColor = false;
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
}
