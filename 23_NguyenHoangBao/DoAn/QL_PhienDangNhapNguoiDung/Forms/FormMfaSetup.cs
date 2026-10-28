using System;
using System.Drawing;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public sealed class FormMfaSetup : Form
    {
        private readonly int _userId;
        private readonly MfaService _mfaService;
        private readonly TextBox _txtCode;

        public FormMfaSetup(int userId, MfaService mfaService)
        {
            _userId = userId;
            _mfaService = mfaService;
            Text = "Thi\u1EBFt l\u1EADp MFA cho Admin";
            ClientSize = new Size(520, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);

            var title = new Label
            {
                Text = "X\u00C1C TH\u1EF0C HAI L\u1EDAP (TOTP)",
                Location = new Point(20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 64, 175)
            };
            var instructions = new Label
            {
                Text = "L\u01B0u secret b\u00EAn d\u01B0\u1EDBi v\u00E0 nh\u1EADp m\u00E3 6 s\u1ED1 t\u1EEB Google Authenticator.",
                Location = new Point(20, 54),
                Size = new Size(480, 24),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            var secret = new TextBox
            {
                Text = _mfaService.GetOrCreateSecret(_userId),
                Location = new Point(20, 92),
                Size = new Size(480, 36),
                ReadOnly = true,
                Font = new Font("Consolas", 12f, FontStyle.Bold),
                BackColor = Color.White
            };
            var labelCode = new Label
            {
                Text = "M\u00E3 x\u00E1c th\u1EF1c:",
                Location = new Point(20, 148),
                AutoSize = true
            };
            _txtCode = new TextBox
            {
                Location = new Point(130, 144),
                Size = new Size(140, 30),
                MaxLength = 6,
                Font = new Font("Consolas", 12f, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };
            var enable = MakeButton("B\u1EACT MFA", new Point(20, 210), 150, Color.FromArgb(5, 150, 105));
            enable.Click += Enable_Click;
            var disable = MakeButton("T\u1EAET MFA", new Point(180, 210), 150, Color.FromArgb(220, 38, 38));
            disable.Click += (s, e) =>
            {
                _mfaService.Disable(_userId);
                MessageBox.Show("MFA \u0111\u00E3 \u0111\u01B0\u1EE3c t\u1EAFt.", "Th\u00E0nh c\u00F4ng",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            var close = MakeButton("\u0110\u00D3NG", new Point(340, 210), 160, Color.FromArgb(100, 116, 139));
            close.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { title, instructions, secret, labelCode, _txtCode, enable, disable, close });
            AcceptButton = enable;
            CancelButton = close;
        }

        private void Enable_Click(object? sender, EventArgs e)
        {
            if (!_mfaService.Enable(_userId, _txtCode.Text))
            {
                MessageBox.Show("M\u00E3 MFA kh\u00F4ng \u0111\u00FAng ho\u1EB7c \u0111\u00E3 h\u1EBFt h\u1EA1n.",
                    "Kh\u00F4ng th\u00E0nh c\u00F4ng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("MFA \u0111\u00E3 \u0111\u01B0\u1EE3c b\u1EADt cho t\u00E0i kho\u1EA3n Admin.",
                "Th\u00E0nh c\u00F4ng", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
