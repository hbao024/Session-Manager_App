using System;
using System.Drawing;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public sealed class FormMfaCode : Form
    {
        private readonly int _userId;
        private readonly MfaService _mfaService;
        private readonly TextBox _txtCode;

        public FormMfaCode(int userId, MfaService mfaService)
        {
            _userId = userId;
            _mfaService = mfaService;
            Text = "X\u00E1c th\u1EF1c MFA";
            ClientSize = new Size(390, 190);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(new Label
            {
                Text = "Nh\u1EADp m\u00E3 6 s\u1ED1 t\u1EEB Google Authenticator",
                Location = new Point(20, 22),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 64, 175)
            });
            _txtCode = new TextBox
            {
                Location = new Point(20, 64),
                Size = new Size(350, 34),
                MaxLength = 6,
                Font = new Font("Consolas", 15f, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };
            Controls.Add(_txtCode);

            var verify = MakeButton("\u0110\u0102NG NH\u1EACP", new Point(20, 120), 170, Color.FromArgb(37, 99, 235));
            verify.Click += Verify_Click;
            var cancel = MakeButton("H\u1EE6Y", new Point(200, 120), 170, Color.FromArgb(100, 116, 139));
            cancel.DialogResult = DialogResult.Cancel;
            Controls.AddRange(new Control[] { verify, cancel });
            AcceptButton = verify;
            CancelButton = cancel;
        }

        private void Verify_Click(object? sender, EventArgs e)
        {
            if (!_mfaService.VerifyCode(_userId, _txtCode.Text))
            {
                MessageBox.Show("M\u00E3 MFA kh\u00F4ng \u0111\u00FAng ho\u1EB7c \u0111\u00E3 h\u1EBFt h\u1EA1n.",
                    "X\u00E1c th\u1EF1c th\u1EA5t b\u1EA1i", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCode.SelectAll();
                _txtCode.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static Button MakeButton(string text, Point location, int width, Color color)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(width, 38),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
        }
    }
}
