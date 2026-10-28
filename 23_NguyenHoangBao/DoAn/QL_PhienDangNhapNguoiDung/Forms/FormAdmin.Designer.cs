using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    partial class FormAdmin
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvUsers;
        private Button btnToggleActive, btnForceLogout, btnAddUser, btnEditUser, btnDeleteUser, btnRefresh, btnBack,
            btnExportRedis, btnImportRedis, btnMfa;
        private Label lblTitle, lblUserCount, lblOnlineStats;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Text = "Qu\u1EA3n tr\u1ECB h\u1EC7 th\u1ED1ng \u2014 Admin Panel";
            Size = new Size(1200, 700);
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);
            Load += FormAdmin_Load;

            // -- Gradient Header -----------------------------------
            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1200, 90),
                BackColor = Color.Transparent
            };
            pnlHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height),
                    Color.FromArgb(30, 64, 175),
                    Color.FromArgb(37, 99, 235),
                    LinearGradientMode.Horizontal);
                g.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
            };

            lblTitle = new Label
            {
                Text = "ADMIN PANEL \u2014 Qu\u1EA3n l\u00FD t\u00E0i kho\u1EA3n",
                Location = new Point(20, 16),
                Size = new Size(700, 34),
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            lblOnlineStats = new Label
            {
                Text = "...",
                Location = new Point(22, 56),
                Size = new Size(600, 22),
                ForeColor = Color.FromArgb(254, 243, 199), // amber-100
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            lblUserCount = new Label
            {
                Text = "...",
                Location = new Point(850, 34),
                Size = new Size(330, 24),
                ForeColor = Color.FromArgb(255, 255, 255),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblOnlineStats, lblUserCount });

            // -- Toolbar -------------------------------------------
            var pnlToolbar = new Panel
            {
                Location = new Point(0, 90),
                Size = new Size(1200, 96),
                BackColor = Color.FromArgb(255, 255, 255)
            };
            pnlToolbar.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(203, 213, 225), 1);
                e.Graphics.DrawLine(p, 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1);
            };

            btnRefresh      = MakeBtn("L\u00E0m m\u1EDBi", new Point(12, 10), 120, Color.FromArgb(14, 116, 144));
            btnAddUser      = MakeBtn("Th\u00EAm t\u00E0i kho\u1EA3n", new Point(144, 10), 165, Color.FromArgb(5, 150, 105));
            btnEditUser     = MakeBtn("S\u1EEDa th\u00F4ng tin", new Point(321, 10), 155, Color.FromArgb(37, 99, 235));
            btnDeleteUser   = MakeBtn("X\u00F3a t\u00E0i kho\u1EA3n", new Point(488, 10), 160, Color.FromArgb(220, 38, 38));
            btnToggleActive = MakeBtn("K\u00EDch ho\u1EA1t / Kh\u00F3a", new Point(660, 10), 185, Color.FromArgb(217, 119, 6));
            btnForceLogout  = MakeBtn("Bu\u1ED9c \u0111\u0103ng xu\u1EA5t", new Point(12, 54), 175, Color.FromArgb(168, 85, 247));
            btnBack         = MakeBtn("Quay l\u1EA1i", new Point(199, 54), 112, Color.FromArgb(100, 116, 139));
            btnMfa          = MakeBtn("Thi\u1EBFt l\u1EADp MFA", new Point(331, 54), 170, Color.FromArgb(8, 145, 178));

            btnRefresh.FlatAppearance.MouseOverBackColor      = Color.FromArgb(8, 145, 178);
            btnAddUser.FlatAppearance.MouseOverBackColor      = Color.FromArgb(5, 150, 105);
            btnEditUser.FlatAppearance.MouseOverBackColor     = Color.FromArgb(29, 78, 216);
            btnDeleteUser.FlatAppearance.MouseOverBackColor   = Color.FromArgb(220, 38, 38);
            btnToggleActive.FlatAppearance.MouseOverBackColor = Color.FromArgb(217, 119, 6);
            btnForceLogout.FlatAppearance.MouseOverBackColor  = Color.FromArgb(126, 34, 206);
            btnBack.FlatAppearance.MouseOverBackColor         = Color.FromArgb(31, 41, 55);

            btnRefresh.Click      += btnRefresh_Click;
            btnAddUser.Click      += btnAddUser_Click;
            btnEditUser.Click     += btnEditUser_Click;
            btnDeleteUser.Click   += btnDeleteUser_Click;
            btnToggleActive.Click += btnToggleActive_Click;
            btnForceLogout.Click  += btnForceLogout_Click;
            btnBack.Click         += btnBack_Click;
            btnMfa.Click          += btnMfa_Click;

            pnlToolbar.Controls.AddRange(new Control[]
            {
                btnRefresh, btnAddUser, btnEditUser, btnDeleteUser,
                btnToggleActive, btnForceLogout, btnBack, btnMfa
            });

            // -- DataGridView --------------------------------------
            dgvUsers = new DataGridView
            {
                Location = new Point(0, 186),
                Size = new Size(1200, 426),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(255, 255, 255),
                Font = new Font("Segoe UI", 9.5f),
                RowTemplate = { Height = 40 }
            };
            dgvUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Header style
            dgvUsers.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvUsers.ColumnHeadersHeight = 42;
            dgvUsers.EnableHeadersVisualStyles = false;

            // Row style
            dgvUsers.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(30, 64, 175),  // indigo dark
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvUsers.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(30, 64, 175),
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0)
            };

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", HeaderText = "ID", FillWeight = 5 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "username", HeaderText = "T\u00E0i kho\u1EA3n", FillWeight = 13 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "fullname", HeaderText = "H\u1ECD t\u00EAn", FillWeight = 17 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "email", HeaderText = "Email", FillWeight = 19 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "role", HeaderText = "Quy\u1EC1n", FillWeight = 10 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "status", HeaderText = "Tr\u1EA1ng th\u00E1i", FillWeight = 13 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "online", HeaderText = "Phi\u00EAn online", FillWeight = 12 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "lastlogin", HeaderText = "\u0110\u0103ng nh\u1EADp cu\u1ED1i", FillWeight = 16 });

            // -- Bottom Redis bar -----------------------------------
            var pnlBottom = new Panel
            {
                Location = new Point(0, 612),
                Size = new Size(1200, 50),
                BackColor = Color.FromArgb(255, 255, 255)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(203, 213, 225), 1);
                e.Graphics.DrawLine(p, 0, 0, pnlBottom.Width, 0);
            };

            btnExportRedis = MakeBtn("Export Redis", new Point(12, 8), 155, Color.FromArgb(100, 116, 139));
            btnImportRedis = MakeBtn("Import Redis", new Point(179, 8), 155, Color.FromArgb(100, 116, 139));
            btnExportRedis.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 41, 55);
            btnImportRedis.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 41, 55);
            btnExportRedis.Click += btnExportRedis_Click;
            btnImportRedis.Click += btnImportRedis_Click;

            var lblBottomNote = new Label
            {
                Text = "Backup / Restore d\u1EEF li\u1EC7u Redis (Hash \u00B7 Set \u00B7 List \u00B7 String)",
                Location = new Point(348, 14),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                BackColor = Color.Transparent
            };

            pnlBottom.Controls.AddRange(new Control[] { btnExportRedis, btnImportRedis, lblBottomNote });

            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlToolbar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlBottom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Controls.AddRange(new Control[]
            {
                pnlHeader, pnlToolbar, dgvUsers, pnlBottom
            });
        }

        private Button MakeBtn(string text, Point location, int width, Color color) => new Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 38),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }
}
