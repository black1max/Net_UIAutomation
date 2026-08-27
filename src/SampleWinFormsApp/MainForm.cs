using System;
using System.Drawing;
using System.Windows.Forms;

namespace SampleWinFormsApp
{
    public partial class MainForm : Form
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private GroupBox grpInputs;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblAge;
        private NumericUpDown numAge;
        private Label lblDepartment;
        private ComboBox cmbDepartment;
        private CheckBox chkIsAdmin;
        private CheckBox chkEnableNotifications;

        private GroupBox grpRadio;
        private RadioButton radThemeLight;
        private RadioButton radThemeDark;
        private RadioButton radThemeSystem;

        private GroupBox grpActions;
        private Button btnSubmit;
        private Button btnClear;
        private Button btnToggleStatus;
        private ProgressBar prgStatus;

        private GroupBox grpOutput;
        private TextBox txtLog;
        private DataGridView dgvUsers;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        public MainForm()
        {
            InitializeComponent();
            SetupSampleData();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new Label();
            this.grpInputs = new GroupBox();
            this.lblUsername = new Label();
            this.txtUsername = new TextBox();
            this.lblAge = new Label();
            this.numAge = new NumericUpDown();
            this.lblDepartment = new Label();
            this.cmbDepartment = new ComboBox();
            this.chkIsAdmin = new CheckBox();
            this.chkEnableNotifications = new CheckBox();

            this.grpRadio = new GroupBox();
            this.radThemeLight = new RadioButton();
            this.radThemeDark = new RadioButton();
            this.radThemeSystem = new RadioButton();

            this.grpActions = new GroupBox();
            this.btnSubmit = new Button();
            this.btnClear = new Button();
            this.btnToggleStatus = new Button();
            this.prgStatus = new ProgressBar();

            this.grpOutput = new GroupBox();
            this.txtLog = new TextBox();
            this.dgvUsers = new DataGridView();

            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel();

            ((System.ComponentModel.ISupportInitialize)(this.numAge)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();

            // MainForm
            this.Text = "Sample WinForms Target Application";
            this.Size = new Size(820, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            // lblTitle
            this.lblTitle.Text = "WinForms Automation Target Application (.NET Framework / .NET)";
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(15, 12);
            this.lblTitle.Size = new Size(750, 30);

            // grpInputs
            this.grpInputs.Text = "ユーザー情報入力 (User Inputs)";
            this.grpInputs.Location = new Point(15, 50);
            this.grpInputs.Size = new Size(380, 220);

            this.lblUsername.Text = "ユーザー名 (Username):";
            this.lblUsername.Location = new Point(15, 30);
            this.lblUsername.AutoSize = true;

            this.txtUsername.Name = "txtUsername";
            this.txtUsername.AccessibleName = "txtUsername";
            this.txtUsername.Location = new Point(160, 27);
            this.txtUsername.Size = new Size(200, 23);
            this.txtUsername.Text = "Yamada Taro";

            this.lblAge.Text = "年齢 (Age):";
            this.lblAge.Location = new Point(15, 65);
            this.lblAge.AutoSize = true;

            this.numAge.Name = "numAge";
            this.numAge.AccessibleName = "numAge";
            this.numAge.Location = new Point(160, 62);
            this.numAge.Size = new Size(100, 23);
            this.numAge.Value = 30;

            this.lblDepartment.Text = "部署 (Department):";
            this.lblDepartment.Location = new Point(15, 100);
            this.lblDepartment.AutoSize = true;

            this.cmbDepartment.Name = "cmbDepartment";
            this.cmbDepartment.AccessibleName = "cmbDepartment";
            this.cmbDepartment.Location = new Point(160, 97);
            this.cmbDepartment.Size = new Size(200, 23);
            this.cmbDepartment.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDepartment.Items.AddRange(new object[] { "開発部 (Development)", "営業部 (Sales)", "総務部 (General Affairs)", "情シス (IT Support)" });
            this.cmbDepartment.SelectedIndex = 0;

            this.chkIsAdmin.Name = "chkIsAdmin";
            this.chkIsAdmin.AccessibleName = "chkIsAdmin";
            this.chkIsAdmin.Text = "管理者権限 (Administrator)";
            this.chkIsAdmin.Location = new Point(15, 140);
            this.chkIsAdmin.Size = new Size(200, 25);
            this.chkIsAdmin.Checked = true;

            this.chkEnableNotifications.Name = "chkEnableNotifications";
            this.chkEnableNotifications.AccessibleName = "chkEnableNotifications";
            this.chkEnableNotifications.Text = "通知を有効化 (Notifications)";
            this.chkEnableNotifications.Location = new Point(15, 175);
            this.chkEnableNotifications.Size = new Size(200, 25);

            this.grpInputs.Controls.Add(this.lblUsername);
            this.grpInputs.Controls.Add(this.txtUsername);
            this.grpInputs.Controls.Add(this.lblAge);
            this.grpInputs.Controls.Add(this.numAge);
            this.grpInputs.Controls.Add(this.lblDepartment);
            this.grpInputs.Controls.Add(this.cmbDepartment);
            this.grpInputs.Controls.Add(this.chkIsAdmin);
            this.grpInputs.Controls.Add(this.chkEnableNotifications);

            // grpRadio
            this.grpRadio.Text = "テーマ選択 (Theme Selection)";
            this.grpRadio.Location = new Point(410, 50);
            this.grpRadio.Size = new Size(380, 100);

            this.radThemeLight.Name = "radThemeLight";
            this.radThemeLight.AccessibleName = "radThemeLight";
            this.radThemeLight.Text = "ライト (Light)";
            this.radThemeLight.Location = new Point(20, 35);
            this.radThemeLight.Size = new Size(100, 25);
            this.radThemeLight.Checked = true;

            this.radThemeDark.Name = "radThemeDark";
            this.radThemeDark.AccessibleName = "radThemeDark";
            this.radThemeDark.Text = "ダーク (Dark)";
            this.radThemeDark.Location = new Point(140, 35);
            this.radThemeDark.Size = new Size(100, 25);

            this.radThemeSystem.Name = "radThemeSystem";
            this.radThemeSystem.AccessibleName = "radThemeSystem";
            this.radThemeSystem.Text = "システム (System)";
            this.radThemeSystem.Location = new Point(260, 35);
            this.radThemeSystem.Size = new Size(110, 25);

            this.grpRadio.Controls.Add(this.radThemeLight);
            this.grpRadio.Controls.Add(this.radThemeDark);
            this.grpRadio.Controls.Add(this.radThemeSystem);

            // grpActions
            this.grpActions.Text = "アクション (Actions)";
            this.grpActions.Location = new Point(410, 160);
            this.grpActions.Size = new Size(380, 110);

            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.AccessibleName = "btnSubmit";
            this.btnSubmit.Text = "送信 / 登録 (Submit)";
            this.btnSubmit.Location = new Point(15, 25);
            this.btnSubmit.Size = new Size(130, 35);
            this.btnSubmit.Click += BtnSubmit_Click;

            this.btnClear.Name = "btnClear";
            this.btnClear.AccessibleName = "btnClear";
            this.btnClear.Text = "クリア (Clear)";
            this.btnClear.Location = new Point(155, 25);
            this.btnClear.Size = new Size(100, 35);
            this.btnClear.Click += BtnClear_Click;

            this.btnToggleStatus.Name = "btnProgress";
            this.btnToggleStatus.AccessibleName = "btnProgress";
            this.btnToggleStatus.Text = "進行 (Step)";
            this.btnToggleStatus.Location = new Point(265, 25);
            this.btnToggleStatus.Size = new Size(100, 35);
            this.btnToggleStatus.Click += BtnProgress_Click;

            this.prgStatus.Name = "prgStatus";
            this.prgStatus.AccessibleName = "prgStatus";
            this.prgStatus.Location = new Point(15, 70);
            this.prgStatus.Size = new Size(350, 23);
            this.prgStatus.Value = 40;

            this.grpActions.Controls.Add(this.btnSubmit);
            this.grpActions.Controls.Add(this.btnClear);
            this.grpActions.Controls.Add(this.btnToggleStatus);
            this.grpActions.Controls.Add(this.prgStatus);

            // grpOutput
            this.grpOutput.Text = "実行ログ & データ一覧 (Logs & DataGrid)";
            this.grpOutput.Location = new Point(15, 280);
            this.grpOutput.Size = new Size(775, 320);

            this.txtLog.Name = "txtLog";
            this.txtLog.AccessibleName = "txtLog";
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;
            this.txtLog.ReadOnly = true;
            this.txtLog.Location = new Point(15, 25);
            this.txtLog.Size = new Size(365, 280);
            this.txtLog.Text = "[Ready] アプリケーション待機中...\r\n";

            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.AccessibleName = "dgvUsers";
            this.dgvUsers.Location = new Point(395, 25);
            this.dgvUsers.Size = new Size(365, 280);
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            this.grpOutput.Controls.Add(this.txtLog);
            this.grpOutput.Controls.Add(this.dgvUsers);

            // statusStrip
            this.statusStrip.Items.Add(this.statusLabel);
            this.statusLabel.Text = "ステータス: 準備完了";

            // Controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.grpInputs);
            this.Controls.Add(this.grpRadio);
            this.Controls.Add(this.grpActions);
            this.Controls.Add(this.grpOutput);
            this.Controls.Add(this.statusStrip);

            ((System.ComponentModel.ISupportInitialize)(this.numAge)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SetupSampleData()
        {
            dgvUsers.Columns.Add("Id", "ID");
            dgvUsers.Columns.Add("Name", "Name");
            dgvUsers.Columns.Add("Department", "Dept");
            dgvUsers.Columns.Add("Role", "Role");

            dgvUsers.Rows.Add("1", "Suzuki Ichiro", "開発部", "Leader");
            dgvUsers.Rows.Add("2", "Sato Hanako", "営業部", "Member");
            dgvUsers.Rows.Add("3", "Tanaka Jiro", "情シス", "Admin");
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string dept = cmbDepartment.SelectedItem?.ToString() ?? "未選択";
            decimal age = numAge.Value;
            bool isAdmin = chkIsAdmin.Checked;
            string theme = radThemeLight.Checked ? "Light" : (radThemeDark.Checked ? "Dark" : "System");

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] 登録: {username} ({age}歳), 部署: {dept}, Admin: {isAdmin}, Theme: {theme}\r\n";
            txtLog.AppendText(logEntry);

            int newId = dgvUsers.Rows.Count + 1;
            dgvUsers.Rows.Add(newId.ToString(), username, dept, isAdmin ? "Admin" : "User");

            statusLabel.Text = $"ステータス: {username} を登録しました ({DateTime.Now:HH:mm:ss})";
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            numAge.Value = 20;
            chkIsAdmin.Checked = false;
            chkEnableNotifications.Checked = false;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 入力をクリアしました\r\n");
            statusLabel.Text = "ステータス: フォームをクリアしました";
        }

        private void BtnProgress_Click(object sender, EventArgs e)
        {
            int newVal = (prgStatus.Value + 20) % 120;
            if (newVal > 100) newVal = 0;
            prgStatus.Value = newVal;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] プログレスバー更新: {newVal}%\r\n");
            statusLabel.Text = $"ステータス: 進行度 {newVal}%";
        }
    }
}
