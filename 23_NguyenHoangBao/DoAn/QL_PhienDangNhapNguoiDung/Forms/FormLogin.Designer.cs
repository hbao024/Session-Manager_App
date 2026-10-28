using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnClear;
        private Label lblError;
        private Panel panelCard;
        private Label lblSubtitle;
        private Panel pnlUserInput;
        private Panel pnlPassInput;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // -- Form ----------------------------------------------
                        this.Text = "Đăng nhập — Session Manager";
            this.Size = new Size(500, 640);
            this.MinimumSize = new Size(460, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.FromArgb(248, 250, 252);   // slate-950
            this.Font = new Font("Segoe UI", 9.5f);

            // -- Outer card (dark glass effect) -------------------
            panelCard = new Panel
            {
                Size = new Size(400, 500),
                Location = new Point(50, 50),
                BackColor = Color.FromArgb(255, 255, 255),     // slate-800
                Padding = new Padding(0)
            };
            // Rounded-rect illusion via Paint
            panelCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // subtle border glow
                using var borderPen = new Pen(Color.FromArgb(37, 99, 235), 1.5f); // indigo
                g.DrawRectangle(borderPen, 0, 0, panelCard.Width - 1, panelCard.Height - 1);
            };

            // -- Gradient header strip ----------------------------
            var pnlHeader = new Panel
            {
                Size = new Size(400, 100),
                Location = new Point(0, 0),
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
                Text = "SESSION MANAGER",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(380, 40),
                Location = new Point(10, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblSubtitle = new Label
            {
                                Text = "Hệ thống Quản lý Phiên Đăng nhập NoSQL",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(220, 220, 255),
                AutoSize = false,
                Size = new Size(380, 22),
                Location = new Point(10, 62),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // -- Username label -----------------------------------
            lblUsername = new Label
            {
                                Text = "TÊN ĐĂNG NHẬP",
                Location = new Point(30, 125),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Username input container (simulates bordered input)
            pnlUserInput = new Panel
            {
                Location = new Point(30, 145),
                Size = new Size(340, 44),
                BackColor = Color.FromArgb(226, 232, 240),
            };
            pnlUserInput.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(37, 99, 235), 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnlUserInput.Width - 1, pnlUserInput.Height - 1);
            };
            txtUsername = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(316, 24),
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(31, 41, 55),
            };
            pnlUserInput.Controls.Add(txtUsername);

            // -- Password label -----------------------------------
            lblPassword = new Label
            {
                                Text = "MẬT KHẨU",
                Location = new Point(30, 205),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            pnlPassInput = new Panel
            {
                Location = new Point(30, 225),
                Size = new Size(340, 44),
                BackColor = Color.FromArgb(226, 232, 240),
            };
            pnlPassInput.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(37, 99, 235), 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnlPassInput.Width - 1, pnlPassInput.Height - 1);
            };
            txtPassword = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(316, 24),
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(31, 41, 55),
                UseSystemPasswordChar = true
            };
            pnlPassInput.Controls.Add(txtPassword);

            // -- Error label --------------------------------------
            lblError = new Label
            {
                Text = "",
                Location = new Point(30, 278),
                Size = new Size(340, 40),
                ForeColor = Color.FromArgb(252, 165, 165),  // red-300
                Font = new Font("Segoe UI", 8.5f),
                Visible = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // -- Login button -------------------------------------
            btnLogin = new Button
            {
                                Text = "ĐĂNG NHẬP",
                Location = new Point(30, 328),
                Size = new Size(340, 46),
                BackColor = Color.FromArgb(37, 99, 235),   // indigo-500
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);  // indigo-600
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);  // indigo-700
            btnLogin.Click += btnLogin_Click;

            // -- Clear button -------------------------------------
            btnClear = new Button
            {
                                Text = "Xóa trắng",
                Location = new Point(30, 385),
                Size = new Size(340, 34),
                BackColor = Color.FromArgb(255, 255, 255),
                ForeColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(100, 116, 139);
            btnClear.FlatAppearance.BorderSize = 1;
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            btnClear.Click += btnClear_Click;

            // -- Hint label ---------------------------------------
            var lblHint = new Label
            {
                                Text = "admin / Admin@123   •   nguyenvana / User@123",
                Location = new Point(30, 430),
                Size = new Size(340, 22),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // -- Assemble -----------------------------------------
            panelCard.Controls.AddRange(new Control[]
            {
                pnlHeader,
                lblUsername, pnlUserInput,
                lblPassword, pnlPassInput,
                lblError, btnLogin, btnClear, lblHint
            });

            this.Controls.Add(panelCard);
            this.AcceptButton = btnLogin;
            this.Resize += FormLogin_Resize;
            FormLogin_Resize(this, EventArgs.Empty);
        }
    }
}
