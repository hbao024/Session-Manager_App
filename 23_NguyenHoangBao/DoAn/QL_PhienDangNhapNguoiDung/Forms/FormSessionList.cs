using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Models;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public partial class FormSessionList : Form
    {
        private readonly int _userId;
        private readonly string _currentSessionId;
        private readonly SessionManager _sessionManager;
        private readonly LoginHistoryService _historyService;

        public FormSessionList(int userId, string currentSessionId)
        {
            InitializeComponent();
            _userId           = userId;
            _currentSessionId = currentSessionId;
            _sessionManager   = new SessionManager();
            _historyService   = new LoginHistoryService(AppConfig.SQL_CONNECTION);
        }

        private void FormSessionList_Load(object sender, EventArgs e)
        {
            LoadSessions();
        }

        private void LoadSessions()
        {
            var sessions = _sessionManager.GetUserActiveSessions(_userId);
            dgvSessions.Rows.Clear();

            foreach (var s in sessions)
            {
                bool isCurrent = s.SessionId == _currentSessionId;
                string shortSid = s.SessionId.Length >= 8 ? s.SessionId.Substring(0, 8) : s.SessionId;
                int row = dgvSessions.Rows.Add(
                    isCurrent ? "Hi\u1EC7n t\u1EA1i" : "Thi\u1EBFt b\u1ECB kh\u00E1c",
                    shortSid + "...",
                    s.IpAddress,
                    s.LoginTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    s.LastActivity.ToString("HH:mm:ss"),
                    $"{s.TtlSeconds / 60} ph\u00FAt {s.TtlSeconds % 60}s",
                    s.SessionId  // ?n d? dÃƒÂ¯Ã‚Â¿Ã‚Â½ng khi xÃƒÂ¯Ã‚Â¿Ã‚Â½a
                );
                if (isCurrent)
                    dgvSessions.Rows[row].DefaultCellStyle.BackColor = Color.FromArgb(220, 240, 255);
            }

            lblCount.Text = $"T\u1ED5ng s\u1ED1 phi\u00EAn \u0111ang ho\u1EA1t \u0111\u1ED9ng: {sessions.Count}";
        }

        private void btnLogoutSelected_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0) return;

            var row = dgvSessions.SelectedRows[0];
            string sid = row.Cells[6].Value?.ToString() ?? "";

            if (sid == _currentSessionId)
            {
                MessageBox.Show("Kh\u00F4ng th\u1EC3 \u0111\u0103ng xu\u1EA5t phi\u00EAn hi\u1EC7n t\u1EA1i t\u1EEB \u0111\u00E2y.\nD\u00F9ng n\u00FAt \u0110\u0103ng xu\u1EA5t \u1EDF m\u00E0n h\u00ECnh ch\u00EDnh.",
                    "Th\u00F4ng b\u00E1o", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("\u0110\u0103ng xu\u1EA5t phi\u00EAn \u0111\u00E3 ch\u1ECDn?", "X\u00E1c nh\u1EADn",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _sessionManager.Logout(sid);
                _historyService.LogLogout(sid);
                LoadSessions();
                MessageBox.Show("\u0110\u00E3 \u0111\u0103ng xu\u1EA5t phi\u00EAn th\u00E0nh c\u00F4ng!", "Th\u00E0nh c\u00F4ng",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnLogoutAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("\u0110\u0103ng xu\u1EA5t T\u1EA4T C\u1EA2 thi\u1EBFt b\u1ECB kh\u00E1c (gi\u1EEF phi\u00EAn hi\u1EC7n t\u1EA1i)?",
                "X\u00E1c nh\u1EADn", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var sessions = _sessionManager.GetUserActiveSessions(_userId);
                var others = sessions.Where(s => s.SessionId != _currentSessionId).ToList();

                foreach (var s in others)
                {
                    _sessionManager.Logout(s.SessionId);
                    _historyService.LogLogout(s.SessionId);
                }

                LoadSessions();
                MessageBox.Show($"\u0110\u00E3 \u0111\u0103ng xu\u1EA5t {others.Count} thi\u1EBFt b\u1ECB kh\u00E1c!", "Th\u00E0nh c\u00F4ng",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadSessions();

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
