using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Models;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    /// <summary>
    /// Dialog dùng chung cho thêm mới và sửa tài khoản.
    /// </summary>
    public class FormAddUser : Form
    {
        public string NewUsername { get; private set; } = "";
        public string NewPassword { get; private set; } = "";
        public string NewEmail { get; private set; } = "";
        public string NewFullName { get; private set; } = "";
        public string NewRole { get; private set; } = "user";

        private readonly bool _editMode;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private TextBox txtEmail = null!;
        private TextBox txtFullName = null!;
        private ComboBox cmbRole = null!;
        private Button btnOK = null!;
        private Button btnCancel = null!;

        public FormAddUser() : this(null) { }

        public FormAddUser(UserModel? user)
        {
            _editMode = user != null;
            Text = _editMode ? "S\u1EEDa th\u00F4ng tin t\u00E0i kho\u1EA3n" : "Th\u00EAm t\u00E0i kho\u1EA3n m\u1EDBi";
            ClientSize = new Size(520, 620);
            MinimumSize = new Size(520, 620);
            MaximumSize = new Size(520, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5f);

            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, 78),
                BackColor = Color.Transparent
            };
            pnlHeader.Paint += (s, e) =>
            {
                Color c1 = _editMode ? Color.FromArgb(37, 99, 235) : Color.FromArgb(5, 150, 105);
                Color c2 = _editMode ? Color.FromArgb(79, 70, 229) : Color.FromArgb(14, 116, 144);
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height),
                    c1, c2, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
            };

            var lblHeaderTitle = new Label
            {
                Text = _editMode ? "C\u1EACP NH\u1EACT T\u00C0I KHO\u1EA2N" : "T\u1EA0O T\u00C0I KHO\u1EA2N M\u1EDAI",
                Location = new Point(20, 14),
                Size = new Size(ClientSize.Width - 40, 30),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            var lblHeaderSub = new Label
            {
                Text = _editMode ? "Ch\u1EC9nh s\u1EEDa th\u00F4ng tin ng\u01B0\u1EDDi d\u00F9ng" : "\u0110i\u1EC1n \u0111\u1EA7y \u0111\u1EE7 th\u00F4ng tin b\u00EAn d\u01B0\u1EDBi",
                Location = new Point(22, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(220, 240, 255),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSub });

            var card = new Panel
            {
                Location = new Point(18, 92),
                Size = new Size(ClientSize.Width - 36, 440),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(203, 213, 225), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            int fieldWidth = card.Width - 32;
            int y = 18;

            card.Controls.Add(MakeFieldLabel("T\u00CAN \u0110\u0102NG NH\u1EACP", y));
            var pnlUser = MakeInputPanel(y + 22, fieldWidth);
            txtUsername = MakeInnerTextBox(pnlUser);
            txtUsername.ReadOnly = _editMode;
            if (_editMode)
            {
                txtUsername.BackColor = Color.FromArgb(241, 245, 249);
                txtUsername.ForeColor = Color.FromArgb(100, 116, 139);
                pnlUser.BackColor = Color.FromArgb(241, 245, 249);
            }
            if (user != null) txtUsername.Text = user.Username;
            card.Controls.Add(pnlUser);
            y += 78;

            card.Controls.Add(MakeFieldLabel(
                _editMode ? "M\u1EACT KH\u1EA8U M\u1EDAI (T\u00D9Y CH\u1ECCN)" : "M\u1EACT KH\u1EA8U", y));
            var pnlPass = MakeInputPanel(y + 22, fieldWidth);
            txtPassword = MakeInnerTextBox(pnlPass);
            txtPassword.UseSystemPasswordChar = true;
            card.Controls.Add(pnlPass);
            y += 78;

            card.Controls.Add(MakeFieldLabel("EMAIL", y));
            var pnlEmail = MakeInputPanel(y + 22, fieldWidth);
            txtEmail = MakeInnerTextBox(pnlEmail);
            if (user != null) txtEmail.Text = user.Email;
            card.Controls.Add(pnlEmail);
            y += 78;

            card.Controls.Add(MakeFieldLabel("H\u1ECC V\u00C0 T\u00CAN", y));
            var pnlName = MakeInputPanel(y + 22, fieldWidth);
            txtFullName = MakeInnerTextBox(pnlName);
            if (user != null) txtFullName.Text = user.FullName;
            card.Controls.Add(pnlName);
            y += 78;

            card.Controls.Add(MakeFieldLabel("QUY\u1EC0N TRUY C\u1EACP", y));
            cmbRole = new ComboBox
            {
                Location = new Point(16, y + 22),
                Size = new Size(fieldWidth, 38),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(31, 41, 55),
                FlatStyle = FlatStyle.Flat
            };
            cmbRole.Items.AddRange(new object[] { "user", "admin" });
            cmbRole.SelectedItem = user?.Role ?? "user";
            if (cmbRole.SelectedIndex < 0) cmbRole.SelectedIndex = 0;
            card.Controls.Add(cmbRole);

            Controls.AddRange(new Control[] { pnlHeader, card });

            Color okColor = _editMode ? Color.FromArgb(37, 99, 235) : Color.FromArgb(5, 150, 105);
            int buttonTop = ClientSize.Height - 52;
            btnOK = MakeButton(
                _editMode ? "L\u01AFU THAY \u0110\u1ED4I" : "TH\u00CAM T\u00C0I KHO\u1EA2N",
                new Point(ClientSize.Width - 318, buttonTop), 170, okColor);
            btnOK.Click += BtnOK_Click;

            btnCancel = MakeButton("H\u1EE6Y", new Point(ClientSize.Width - 136, buttonTop), 118,
                Color.FromArgb(100, 116, 139));
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.AddRange(new Control[] { btnOK, btnCancel });
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                (!_editMode && string.IsNullOrWhiteSpace(txtPassword.Text)) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show(
                    "Vui l\u00F2ng \u0111i\u1EC1n \u0111\u1EA7y \u0111\u1EE7 th\u00F4ng tin b\u1EAFt bu\u1ED9c.",
                    "Thi\u1EBFu th\u00F4ng tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            NewUsername = txtUsername.Text.Trim();
            NewPassword = txtPassword.Text;
            NewEmail = txtEmail.Text.Trim();
            NewFullName = txtFullName.Text.Trim();
            NewRole = cmbRole.SelectedItem?.ToString() ?? "user";
            DialogResult = DialogResult.OK;
            Close();
        }

        private Label MakeFieldLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(16, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = Color.Transparent
            };
        }

        private Panel MakeInputPanel(int y, int width)
        {
            return new Panel
            {
                Location = new Point(16, y),
                Size = new Size(width, 44),
                BackColor = Color.FromArgb(226, 232, 240)
            };
        }

        private TextBox MakeInnerTextBox(Panel container)
        {
            var textBox = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(container.Width - 20, 24),
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            container.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(148, 163, 184), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, container.Width - 1, container.Height - 1);
            };
            container.Controls.Add(textBox);
            return textBox;
        }

        private Button MakeButton(string text, Point location, int width, Color color)
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
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }
    }
}
