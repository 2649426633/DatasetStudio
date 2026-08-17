namespace DatasetStudio.WinForms.Pages;

partial class ValidationPage
{
    private System.ComponentModel.IContainer? components = null;
    private TableLayoutPanel _rootLayout = null!;
    private CardPanel _summaryCard = null!;
    private TableLayoutPanel _summaryLayout = null!;
    private Label _summaryIcon = null!;
    private Label _summary = null!;
    private Label _summaryDetail = null!;
    private Button _runButton = null!;
    private CardPanel _gridCard = null!;
    private TableLayoutPanel _gridLayout = null!;
    private Label _gridHeader = null!;
    private DataGridView _grid = null!;
    private DataGridViewTextBoxColumn _stateColumn = null!;
    private DataGridViewTextBoxColumn _checkColumn = null!;
    private DataGridViewTextBoxColumn _valueColumn = null!;
    private DataGridViewTextBoxColumn _messageColumn = null!;

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
        _summaryCard = new CardPanel();
        _summaryLayout = new TableLayoutPanel();
        _summaryIcon = new Label();
        _summary = new Label();
        _summaryDetail = new Label();
        _runButton = new Button();
        _gridCard = new CardPanel();
        _gridLayout = new TableLayoutPanel();
        _gridHeader = new Label();
        _grid = new DataGridView();
        _stateColumn = new DataGridViewTextBoxColumn();
        _checkColumn = new DataGridViewTextBoxColumn();
        _valueColumn = new DataGridViewTextBoxColumn();
        _messageColumn = new DataGridViewTextBoxColumn();
        _rootLayout.SuspendLayout();
        _summaryCard.SuspendLayout();
        _summaryLayout.SuspendLayout();
        _gridCard.SuspendLayout();
        _gridLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        SuspendLayout();
        // 
        // rootLayout
        // 
        _rootLayout.BackColor = Color.FromArgb(244, 245, 246);
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_summaryCard, 0, 0);
        _rootLayout.Controls.Add(_gridCard, 0, 1);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Margin = new Padding(0);
        _rootLayout.Name = "rootLayout";
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // summaryCard
        // 
        _summaryCard.BackColor = Color.FromArgb(250, 250, 250);
        _summaryCard.BorderColor = Color.FromArgb(218, 220, 222);
        _summaryCard.Controls.Add(_summaryLayout);
        _summaryCard.Dock = DockStyle.Fill;
        _summaryCard.Margin = new Padding(0, 0, 0, 12);
        _summaryCard.Name = "summaryCard";
        _summaryCard.Padding = new Padding(16);
        // 
        // summaryLayout
        // 
        _summaryLayout.BackColor = Color.Transparent;
        _summaryLayout.ColumnCount = 3;
        _summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
        _summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        _summaryLayout.Controls.Add(_summaryIcon, 0, 0);
        _summaryLayout.Controls.Add(_summary, 1, 0);
        _summaryLayout.Controls.Add(_summaryDetail, 1, 1);
        _summaryLayout.Controls.Add(_runButton, 2, 0);
        _summaryLayout.Dock = DockStyle.Fill;
        _summaryLayout.Margin = new Padding(0);
        _summaryLayout.Name = "summaryLayout";
        _summaryLayout.RowCount = 2;
        _summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 54F));
        _summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));
        _summaryLayout.SetRowSpan(_summaryIcon, 2);
        _summaryLayout.SetRowSpan(_runButton, 2);
        // 
        // summaryIcon
        // 
        _summaryIcon.Dock = DockStyle.Fill;
        _summaryIcon.Font = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold);
        _summaryIcon.ForeColor = Color.FromArgb(64, 64, 64);
        _summaryIcon.Margin = new Padding(0, 0, 12, 0);
        _summaryIcon.Name = "summaryIcon";
        _summaryIcon.Text = "—";
        _summaryIcon.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // summary
        // 
        _summary.Dock = DockStyle.Fill;
        _summary.Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);
        _summary.ForeColor = Color.FromArgb(64, 64, 64);
        _summary.Name = "summary";
        _summary.Text = "尚未打开项目";
        _summary.TextAlign = ContentAlignment.BottomLeft;
        // 
        // summaryDetail
        // 
        _summaryDetail.Dock = DockStyle.Fill;
        _summaryDetail.Font = new Font("Microsoft YaHei UI", 9F);
        _summaryDetail.ForeColor = Color.FromArgb(64, 64, 64);
        _summaryDetail.Name = "summaryDetail";
        _summaryDetail.Text = "新建或打开项目后即可执行完整性校验。";
        _summaryDetail.TextAlign = ContentAlignment.TopLeft;
        // 
        // runButton
        // 
        _runButton.Anchor = AnchorStyles.Right;
        _runButton.BackColor = Color.FromArgb(32, 32, 32);
        _runButton.Cursor = Cursors.Hand;
        _runButton.FlatAppearance.BorderColor = Color.FromArgb(32, 32, 32);
        _runButton.FlatStyle = FlatStyle.Flat;
        _runButton.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        _runButton.ForeColor = Color.White;
        _runButton.Name = "runButton";
        _runButton.Size = new Size(120, 36);
        _runButton.Text = "重新校验";
        _runButton.UseVisualStyleBackColor = false;
        _runButton.Click += RunButton_Click;
        // 
        // gridCard
        // 
        _gridCard.BackColor = Color.White;
        _gridCard.BorderColor = Color.FromArgb(218, 220, 222);
        _gridCard.Controls.Add(_gridLayout);
        _gridCard.Dock = DockStyle.Fill;
        _gridCard.Margin = new Padding(0);
        _gridCard.Name = "gridCard";
        _gridCard.Padding = new Padding(1);
        // 
        // gridLayout
        // 
        _gridLayout.BackColor = Color.White;
        _gridLayout.ColumnCount = 1;
        _gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _gridLayout.Controls.Add(_gridHeader, 0, 0);
        _gridLayout.Controls.Add(_grid, 0, 1);
        _gridLayout.Dock = DockStyle.Fill;
        _gridLayout.Margin = new Padding(0);
        _gridLayout.Name = "gridLayout";
        _gridLayout.RowCount = 2;
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        _gridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // gridHeader
        // 
        _gridHeader.BackColor = Color.FromArgb(250, 250, 250);
        _gridHeader.Dock = DockStyle.Fill;
        _gridHeader.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
        _gridHeader.ForeColor = Color.FromArgb(32, 32, 32);
        _gridHeader.Name = "gridHeader";
        _gridHeader.Padding = new Padding(16, 0, 0, 0);
        _gridHeader.Text = "数据完整性与规则校验项";
        _gridHeader.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // grid
        // 
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.ColumnHeadersHeight = 38;
        _grid.Columns.AddRange(new DataGridViewColumn[] { _stateColumn, _checkColumn, _valueColumn, _messageColumn });
        _grid.Dock = DockStyle.Fill;
        _grid.GridColor = Color.FromArgb(218, 220, 222);
        _grid.Name = "grid";
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        // 
        // stateColumn
        // 
        _stateColumn.HeaderText = "状态";
        _stateColumn.Name = "State";
        _stateColumn.ReadOnly = true;
        _stateColumn.Width = 70;
        // 
        // checkColumn
        // 
        _checkColumn.HeaderText = "检查项";
        _checkColumn.Name = "Check";
        _checkColumn.ReadOnly = true;
        _checkColumn.Width = 220;
        // 
        // valueColumn
        // 
        _valueColumn.HeaderText = "数量";
        _valueColumn.Name = "Value";
        _valueColumn.ReadOnly = true;
        _valueColumn.Width = 90;
        // 
        // messageColumn
        // 
        _messageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _messageColumn.HeaderText = "说明";
        _messageColumn.Name = "Message";
        _messageColumn.ReadOnly = true;
        // 
        // ValidationPage
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(244, 245, 246);
        Controls.Add(_rootLayout);
        Name = "ValidationPage";
        Size = new Size(1200, 760);
        _rootLayout.ResumeLayout(false);
        _summaryCard.ResumeLayout(false);
        _summaryLayout.ResumeLayout(false);
        _gridCard.ResumeLayout(false);
        _gridLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        ResumeLayout(false);
    }
}
