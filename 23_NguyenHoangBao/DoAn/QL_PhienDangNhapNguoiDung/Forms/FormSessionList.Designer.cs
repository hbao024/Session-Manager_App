using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    partial class FormSessionList
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvSessions;
        private Button btnLogoutSelected, btnLogoutAll, btnRefresh, btnBack;
        private Label lblTitle, lblCount;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Phi\u00EAn \u0111\u0103ng nh\u1EADp c\u1EE7a t\u00F4i";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(860, 540);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);
            this.Load += FormSessionList_Load;

            // -- Gradient header -----------------------------------
            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(860, 72),
                BackColor = Color.Transparent
            };
            pnlHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height),
                    Color.FromArgb(14, 116, 144),   // cyan-500
                    Color.FromArgb(37, 99, 235),  // indigo-500
                    LinearGradientMode.Horizontal);
                g.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
            };

            lblTitle = new Label
            {
                Text = "Phi\u00EAn \u0111\u0103ng nh\u1EADp c\u1EE7a b\u1EA1n",
                Location = new Point(18, 14),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblCount = new Label
            {
                Text = "...",
                Location = new Point(20, 47),
                AutoSize = true,
                ForeColor = Color.FromArgb(207, 250, 254),  // cyan-100
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblCount });

            // -- DataGridView --------------------------------------
            dgvSessions = new DataGridView
            {
                Location = new Point(0, 72),
                Size = new Size(860, 388),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(255, 255, 255),
                Font = new Font("Segoe UI", 9.5f),
                RowTemplate = { Height = 40 }
            };
            dgvSessions.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvSessions.ColumnHeadersHeight = 42;
            dgvSessions.EnableHeadersVisualStyles = false;
            dgvSessions.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(14, 116, 144),  // cyan dark
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgvSessions.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(31, 41, 55),
                SelectionBackColor = Color.FromArgb(14, 116, 144),
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0)
            };

            dgvSessions.Columns.Add("status",   "Tr\u1EA1ng th\u00E1i");
            dgvSessions.Columns.Add("session",  "Session ID");
            dgvSessions.Columns.Add("ip",       "\u0110\u1ECBa ch\u1EC9 IP");
            dgvSessions.Columns.Add("login",    "\u0110\u0103ng nh\u1EADp l\u00FAc");
            dgvSessions.Columns.Add("activity", "Ho\u1EA1t \u0111\u1ED9ng cu\u1ED1i");
            dgvSessions.Columns.Add("ttl",      "C\u00F2n l\u1EA1i");
            var hiddenCol = new DataGridViewTextBoxColumn { Name = "sid", Visible = false };
            dgvSessions.Columns.Add(hiddenCol);

            dgvSessions.Columns["status"].FillWeight   = 16;
            dgvSessions.Columns["session"].FillWeight  = 16;
            dgvSessions.Columns["ip"].FillWeight       = 18;
            dgvSessions.Columns["login"].FillWeight    = 22;
            dgvSessions.Columns["activity"].FillWeight = 15;
            dgvSessions.Columns["ttl"].FillWeight      = 13;

            // -- Bottom toolbar ------------------------------------
            var pnlBottom = new Panel
            {
                Location = new Point(0, 460),
                Size = new Size(860, 52),
                BackColor = Color.FromArgb(255, 255, 255)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(203, 213, 225), 1);
                e.Graphics.DrawLine(p, 0, 0, pnlBottom.Width, 0);
            };

            btnRefresh        = MakeBtn("L\u00E0m m\u1EDBi", new Point(12, 8), 128, Color.FromArgb(14, 116, 144));
            btnBack           = MakeBtn("Quay l\u1EA1i", new Point(152, 8), 128, Color.FromArgb(100, 116, 139));
            btnLogoutSelected = MakeBtn("\u0110\u0103ng xu\u1EA5t phi\u00EAn ch\u1ECDn", new Point(490, 8), 185, Color.FromArgb(217, 119, 6));
            btnLogoutAll      = MakeBtn("\u0110\u0103ng xu\u1EA5t t\u1EA5t c\u1EA3", new Point(687, 8), 162, Color.FromArgb(220, 38, 38));

            btnRefresh.FlatAppearance.MouseOverBackColor        = Color.FromArgb(8, 145, 178);
            btnBack.FlatAppearance.MouseOverBackColor           = Color.FromArgb(31, 41, 55);
            btnLogoutSelected.FlatAppearance.MouseOverBackColor = Color.FromArgb(217, 119, 6);
            btnLogoutAll.FlatAppearance.MouseOverBackColor      = Color.FromArgb(220, 38, 38);

            btnRefresh.Click        += btnRefresh_Click;
            btnBack.Click           += btnBack_Click;
            btnLogoutSelected.Click += btnLogoutSelected_Click;
            btnLogoutAll.Click      += btnLogoutAll_Click;

            pnlBottom.Controls.AddRange(new Control[] { btnRefresh, btnBack, btnLogoutSelected, btnLogoutAll });

            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvSessions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlBottom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.AddRange(new Control[] { pnlHeader, dgvSessions, pnlBottom });
        }

        private Button MakeBtn(string text, Point loc, int width, Color color) => new Button
        {
            Text = text,
            Location = loc,
            Size = new Size(width, 36),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }
}
