using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    partial class FormLoginHistory
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvHistory;
        private ListBox lstRedisLog;
        private Label lblTitle, lblTotal, lblRedisTitle;
        private Button btnRefresh, btnBack;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Text = "L\u1ECBch s\u1EED \u0111\u0103ng nh\u1EADp";
            Size = new Size(1000, 700);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);
            Load += FormLoginHistory_Load;

            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1000, 72),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height),
                    Color.FromArgb(30, 64, 175), Color.FromArgb(37, 99, 235),
                    LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
            };
            lblTitle = new Label
            {
                Location = new Point(18, 14),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Text = "L\u1ECBch s\u1EED \u0111\u0103ng nh\u1EADp",
                BackColor = Color.Transparent
            };
            lblTotal = new Label
            {
                Location = new Point(20, 47),
                Size = new Size(940, 20),
                ForeColor = Color.FromArgb(219, 234, 254),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblTotal });

            var lblSqlSection = new Label
            {
                Location = new Point(14, 82),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Text = "SQL SERVER - L\u1ECBch s\u1EED l\u01B0u tr\u1EEF v\u0129nh vi\u1EC5n",
                BackColor = Color.Transparent
            };
            dgvHistory = new DataGridView
            {
                Location = new Point(0, 100),
                Size = new Size(1000, 320),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 9.5f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvHistory.RowTemplate.Height = 38;
            dgvHistory.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvHistory.ColumnHeadersHeight = 42;
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(30, 64, 175),
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvHistory.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(30, 64, 175),
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "username", HeaderText = "T\u00E0i kho\u1EA3n", FillWeight = 14 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "login", HeaderText = "\u0110\u0103ng nh\u1EADp l\u00FAc", FillWeight = 19 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "logout", HeaderText = "\u0110\u0103ng xu\u1EA5t l\u00FAc", FillWeight = 19 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "duration", HeaderText = "Th\u1EDDi gian", FillWeight = 11 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ip", HeaderText = "\u0110\u1ECBa ch\u1EC9 IP", FillWeight = 14 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "status", HeaderText = "K\u1EBFt qu\u1EA3", FillWeight = 11 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "note", HeaderText = "Ghi ch\u00FA", FillWeight = 18 });

            var pnlRedisSection = new Panel
            {
                Location = new Point(0, 428),
                Size = new Size(1000, 34),
                BackColor = Color.FromArgb(226, 232, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lblRedisTitle = new Label
            {
                Location = new Point(14, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(185, 28, 28),
                Text = "REDIS LIST - Log \u0111\u0103ng nh\u1EADp g\u1EA7n \u0111\u00E2y (10 m\u1EE5c m\u1EDBi nh\u1EA5t)",
                BackColor = Color.Transparent
            };
            pnlRedisSection.Controls.Add(lblRedisTitle);

            lstRedisLog = new ListBox
            {
                Location = new Point(0, 462),
                Size = new Size(1000, 156),
                Font = new Font("Consolas", 9f),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(22, 101, 52),
                BorderStyle = BorderStyle.None,
                ItemHeight = 20,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var pnlBottom = new Panel
            {
                Location = new Point(0, 618),
                Size = new Size(1000, 44),
                BackColor = Color.White,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            btnBack = new Button
            {
                Text = "Quay l\u1EA1i",
                Location = new Point(720, 4),
                Size = new Size(128, 36),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnRefresh = new Button
            {
                Text = "L\u00E0m m\u1EDBi",
                Location = new Point(860, 4),
                Size = new Size(128, 36),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnBack.Click += btnBack_Click;
            btnRefresh.Click += btnRefresh_Click;
            pnlBottom.Controls.AddRange(new Control[] { btnBack, btnRefresh });

            Resize += (s, e) =>
            {
                int width = ClientSize.Width;
                dgvHistory.Width = width;
                pnlRedisSection.Width = width;
                lstRedisLog.Width = width;
                pnlBottom.Width = width;
                btnRefresh.Left = Math.Max(10, width - 140);
                btnBack.Left = Math.Max(10, width - 280);
                lstRedisLog.Height = Math.Max(80, pnlBottom.Top - lstRedisLog.Top - 8);
            };

            Controls.AddRange(new Control[] { pnlHeader, lblSqlSection, dgvHistory, pnlRedisSection, lstRedisLog, pnlBottom });
        }
    }
}
