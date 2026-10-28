using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblWelcome, lblUsername, lblRole, lblEmail;
        private Label lblLoginTime, lblIP, lblSessionId, lblTTL, lblLastActivity;
        private Button btnLogout, btnSessions, btnHistory, btnAdmin, btnRefresh;
        private Panel panelHeader, panelInfo, panelActions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Text = "Session Manager - Trang ch\u1EE7";
            Size = new Size(700, 700);
            MinimumSize = new Size(620, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);
            Load += FormMain_Load;

            panelHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(700, 110),
                BackColor = Color.Transparent
            };
            panelHeader.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, panelHeader.Width, panelHeader.Height),
                    Color.FromArgb(30, 64, 175), Color.FromArgb(37, 99, 235),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, panelHeader.Width, panelHeader.Height);
                }
            };

            lblWelcome = new Label
            {
                Location = new Point(22, 18),
                Size = new Size(650, 40),
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Xin ch\u00E0o!",
                BackColor = Color.Transparent
            };
            var lblHeaderSub = new Label
            {
                Location = new Point(24, 62),
                Size = new Size(650, 22),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(220, 235, 255),
                Text = "Phi\u00EAn \u0111\u0103ng nh\u1EADp \u0111ang ho\u1EA1t \u0111\u1ED9ng",
                BackColor = Color.Transparent
            };
            panelHeader.Controls.AddRange(new Control[] { lblWelcome, lblHeaderSub });

            panelInfo = new Panel
            {
                Location = new Point(18, 124),
                Size = new Size(664, 320),
                BackColor = Color.White
            };
            panelInfo.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(203, 213, 225), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, panelInfo.Width - 1, panelInfo.Height - 1);
            };

            var lblSection = new Label
            {
                Location = new Point(20, 14),
                Size = new Size(620, 20),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Text = "TH\u00D4NG TIN PHI\u00CAN \u0110\u0102NG NH\u1EACP",
                BackColor = Color.Transparent
            };
            var pnlDivider = new Panel
            {
                Location = new Point(20, 36),
                Size = new Size(620, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            };

            int y = 50;
            lblUsername = MakeInfoLabel(y, "T\u00E0i kho\u1EA3n", Color.FromArgb(31, 41, 55)); y += 36;
            lblRole = MakeInfoLabel(y, "Quy\u1EC1n", Color.FromArgb(5, 150, 105)); y += 36;
            lblEmail = MakeInfoLabel(y, "Email", Color.FromArgb(31, 41, 55)); y += 36;
            lblLoginTime = MakeInfoLabel(y, "\u0110\u0103ng nh\u1EADp l\u00FAc", Color.FromArgb(31, 41, 55)); y += 36;
            lblIP = MakeInfoLabel(y, "\u0110\u1ECBa ch\u1EC9 IP", Color.FromArgb(31, 41, 55)); y += 36;
            lblSessionId = MakeInfoLabel(y, "Session ID", Color.FromArgb(79, 70, 229)); y += 36;
            lblTTL = MakeInfoLabel(y, "C\u00F2n l\u1EA1i", Color.FromArgb(220, 38, 38)); y += 36;
            lblLastActivity = MakeInfoLabel(y, "Ho\u1EA1t \u0111\u1ED9ng cu\u1ED1i", Color.FromArgb(5, 150, 105));
            panelInfo.Controls.AddRange(new Control[]
            {
                lblSection, pnlDivider, lblUsername, lblRole, lblEmail,
                lblLoginTime, lblIP, lblSessionId, lblTTL, lblLastActivity
            });

            panelActions = new Panel
            {
                Location = new Point(18, 456),
                Size = new Size(664, 120),
                BackColor = Color.White
            };
            panelActions.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(203, 213, 225), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, panelActions.Width - 1, panelActions.Height - 1);
            };

            btnRefresh = MakeBtn("Gia h\u1EA1n", new Point(14, 14), 110, Color.FromArgb(5, 150, 105));
            btnSessions = MakeBtn("Phi\u00EAn c\u1EE7a t\u00F4i", new Point(136, 14), 148, Color.FromArgb(14, 116, 144));
            btnHistory = MakeBtn("L\u1ECBch s\u1EED", new Point(296, 14), 124, Color.FromArgb(79, 70, 229));
            btnAdmin = MakeBtn("Qu\u1EA3n tr\u1ECB", new Point(432, 14), 132, Color.FromArgb(217, 119, 6));
            btnLogout = MakeBtn("\u0110\u0102NG XU\u1EA4T", new Point(14, 66), 636, Color.FromArgb(220, 38, 38));

            btnRefresh.Click += btnRefresh_Click;
            btnSessions.Click += btnSessions_Click;
            btnHistory.Click += btnHistory_Click;
            btnAdmin.Click += btnAdmin_Click;
            btnLogout.Click += btnLogout_Click;
            panelActions.Controls.AddRange(new Control[] { btnRefresh, btnSessions, btnHistory, btnAdmin, btnLogout });

            panelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelActions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.AddRange(new Control[] { panelHeader, panelInfo, panelActions });
            Resize += FormMain_Resize;
            FormMain_Resize(this, System.EventArgs.Empty);
        }

        private Label MakeInfoLabel(int y, string name, Color color)
        {
            return new Label
            {
                Location = new Point(20, y),
                Size = new Size(620, 28),
                Font = new Font("Segoe UI", 10f),
                ForeColor = color,
                Text = name + ": ...",
                BackColor = Color.Transparent
            };
        }

        private Button MakeBtn(string text, Point loc, int width, Color color)
        {
            return new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(width, 40),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }
    }
}
