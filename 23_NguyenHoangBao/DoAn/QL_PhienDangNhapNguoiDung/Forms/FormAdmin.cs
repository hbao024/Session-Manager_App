using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Models;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public partial class FormAdmin : Form
    {
        private readonly string _sessionId;
        private readonly UserService _userService;
        private readonly SessionManager _sessionManager;
        private readonly LoginHistoryService _historyService;
        private readonly SessionEventService _eventService;
        private readonly MfaService _mfaService;
        private IDisposable? _eventSubscription;

        public FormAdmin(string sessionId)
        {
            InitializeComponent();
            _sessionId      = sessionId;
            _userService    = new UserService(AppConfig.SQL_CONNECTION);
            _sessionManager = new SessionManager();
            _historyService = new LoginHistoryService(AppConfig.SQL_CONNECTION);
            _eventService   = new SessionEventService();
            _mfaService     = new MfaService();
            FormClosed += (s, e) => _eventSubscription?.Dispose();
        }

        private void FormAdmin_Load(object sender, EventArgs e)
        {
            _eventSubscription = _eventService.Subscribe(OnSessionEvent);
            LoadUsers();
            UpdateOnlineStats();
        }

        private void OnSessionEvent(SessionEvent sessionEvent)
        {
            if (IsDisposed || Disposing) return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed || Disposing) return;
                    LoadUsers();
                    UpdateOnlineStats();
                }));
            }
            catch (InvalidOperationException)
            {
                // The form can close while a Pub/Sub callback is being delivered.
            }
        }

        private void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            dgvUsers.Rows.Clear();
            var sessionCounts = users.ToDictionary(u => u.UserId,
                u => _sessionManager.GetUserActiveSessions(u.UserId).Count);

            foreach (var u in users)
            {
                int activeSessions = sessionCounts[u.UserId];
                int row = dgvUsers.Rows.Add(
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Role == "admin" ? "Admin" : "User",
                    u.IsActive ? "Ho\u1EA1t \u0111\u1ED9ng" : "B\u1ECB kh\u00F3a",
                    activeSessions > 0 ? $"{activeSessions} phi\u00EAn" : "Offline",
                    u.LastLogin?.ToString("dd/MM/yyyy HH:mm") ?? "Ch\u01B0a \u0111\u0103ng nh\u1EADp"
                );

                if (!u.IsActive)
                    dgvUsers.Rows[row].DefaultCellStyle.ForeColor = Color.Gray;
                if (activeSessions > 0)
                    dgvUsers.Rows[row].DefaultCellStyle.BackColor = Color.FromArgb(225, 245, 225);
            }

            lblUserCount.Text = $"T\u1ED5ng: {users.Count} t\u00E0i kho\u1EA3n | Online: {sessionCounts.Count(x => x.Value > 0)}";
        }

        private void UpdateOnlineStats()
        {
            int total = _sessionManager.GetTotalOnlineCount();
            lblOnlineStats.Text = $"T\u1ED5ng phi\u00EAn \u0111ang online to\u00E0n h\u1EC7 th\u1ED1ng: {total}";
        }

        private void btnToggleActive_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            var row = dgvUsers.SelectedRows[0];
            int userId   = (int)row.Cells[0].Value;
            string status = row.Cells[5].Value?.ToString() ?? "";
            bool isActive = status.Contains("Ho\u1EA1t \u0111\u1ED9ng");

            string action = isActive ? "v\u00F4 hi\u1EC7u h\u00F3a" : "k\u00EDch ho\u1EA1t";
            if (MessageBox.Show($"B\u1EA1n c\u00F3 mu\u1ED1n {action} t\u00E0i kho\u1EA3n n\u00E0y?", "X\u00E1c nh\u1EADn",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _userService.SetUserActive(userId, !isActive);
                // N?u vÃƒÂ¯Ã‚Â¿Ã‚Â½ hi?u hÃƒÂ¯Ã‚Â¿Ã‚Â½a ? dang xu?t t?t c? phiÃƒÂ¯Ã‚Â¿Ã‚Â½n c?a user dÃƒÂ¯Ã‚Â¿Ã‚Â½
                if (isActive)
                {
                    var sessions = _sessionManager.GetUserActiveSessions(userId);
                    int count = _sessionManager.LogoutAll(userId);
                    _historyService.LogLogoutMany(sessions.Select(s => s.SessionId));
                    MessageBox.Show($"\u0110\u00E3 {action} t\u00E0i kho\u1EA3n v\u00E0 \u0111\u0103ng xu\u1EA5t {count} phi\u00EAn.", "Th\u00E0nh c\u00F4ng",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"\u0110\u00E3 {action} t\u00E0i kho\u1EA3n.", "Th\u00E0nh c\u00F4ng",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LoadUsers();
            }
        }

        private void btnForceLogout_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            var row = dgvUsers.SelectedRows[0];
            int userId   = (int)row.Cells[0].Value;
            string uname = row.Cells[1].Value?.ToString() ?? "";

            if (MessageBox.Show($"Bu\u00F4c \u0111\u0103ng xu\u1EA5t t\u1EA5t c\u1EA3 phi\u00EAn c\u1EE7a '{uname}'?", "X\u00E1c nh\u1EADn",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // L?y danh sÃƒÂ¯Ã‚Â¿Ã‚Â½ch session tru?c khi xÃƒÂ¯Ã‚Â¿Ã‚Â½a d? ghi log
                var sessions = _sessionManager.GetUserActiveSessions(userId);
                int count = _sessionManager.LogoutAll(userId);
                _historyService.LogLogoutMany(sessions.Select(s => s.SessionId));

                MessageBox.Show($"\u0110\u00E3 \u0111\u0103ng xu\u1EA5t {count} phi\u00EAn c\u1EE7a '{uname}'.", "Th\u00E0nh c\u00F4ng",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUsers();
                UpdateOnlineStats();
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            using var dlg = new FormAddUser();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _userService.AddUser(dlg.NewUsername, dlg.NewPassword,
                        dlg.NewEmail, dlg.NewFullName, dlg.NewRole);
                    LoadUsers();
                    MessageBox.Show("Th\u00EAm t\u00E0i kho\u1EA3n th\u00E0nh c\u00F4ng!", "Th\u00E0nh c\u00F4ng",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ShowDatabaseError(ex, "Kh\u00F4ng th\u1EC3 th\u00EAm t\u00E0i kho\u1EA3n");
                }
            }
        }

        private void btnEditUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("H\u00E3y ch\u1ECDn t\u00E0i kho\u1EA3n c\u1EA7n s\u1EEDa.", "Ch\u01B0a ch\u1ECDn t\u00E0i kho\u1EA3n",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int userId = (int)dgvUsers.SelectedRows[0].Cells[0].Value;
            var user = _userService.GetUserById(userId);
            if (user == null) return;

            using var dlg = new FormAddUser(user);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _userService.UpdateUser(userId, dlg.NewEmail, dlg.NewFullName,
                        dlg.NewRole, dlg.NewPassword);
                    LoadUsers();
                    MessageBox.Show("C\u1EADp nh\u1EADt t\u00E0i kho\u1EA3n th\u00E0nh c\u00F4ng!", "Th\u00E0nh c\u00F4ng",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ShowDatabaseError(ex, "Kh\u00F4ng th\u1EC3 c\u1EADp nh\u1EADt t\u00E0i kho\u1EA3n");
                }
            }
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("H\u00E3y ch\u1ECDn t\u00E0i kho\u1EA3n c\u1EA7n x\u00F3a.", "Ch\u01B0a ch\u1ECDn t\u00E0i kho\u1EA3n",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = dgvUsers.SelectedRows[0];
            int userId = (int)row.Cells[0].Value;
            string username = row.Cells[1].Value?.ToString() ?? "";
            var current = _sessionManager.GetSessionInfo(_sessionId);
            if (current != null && current.UserId == userId)
            {
                MessageBox.Show("Kh\u00F4ng th\u1EC3 x\u00F3a t\u00E0i kho\u1EA3n \u0111\u0103ng \u0111\u0103ng nh\u1EADp.", "Kh\u00F4ng th\u1EC3 th\u1EF1c hi\u1EC7n",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                $"X\u00F3a t\u00E0i kho\u1EA3n '{username}' v\u00E0 to\u00E0n b\u1ED9 l\u1ECBch s\u1EED \u0111\u0103ng nh\u1EADp c\u1EE7a t\u00E0i kho\u1EA3n n\u00E0y?\nThao t\u00E1c kh\u00F4ng th\u1EC3 ho\u00E0n t\u00E1c.",
                "X\u00E1c nh\u1EADn x\u00F3a", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                var sessions = _sessionManager.GetUserActiveSessions(userId);
                _sessionManager.LogoutAll(userId);
                _historyService.LogLogoutMany(sessions.Select(s => s.SessionId));
                _userService.DeleteUser(userId);
                LoadUsers();
                UpdateOnlineStats();
                MessageBox.Show("\u0110\u00E3 x\u00F3a t\u00E0i kho\u1EA3n.", "Th\u00E0nh c\u00F4ng",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex, "Kh\u00F4ng th\u1EC3 x\u00F3a t\u00E0i kho\u1EA3n");
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();

        private void btnMfa_Click(object sender, EventArgs e)
        {
            var current = _sessionManager.GetSessionInfo(_sessionId);
            if (current == null || !string.Equals(current.Role, "admin", StringComparison.OrdinalIgnoreCase))
                return;

            using var dialog = new FormMfaSetup(current.UserId, _mfaService);
            dialog.ShowDialog(this);
        }

        private void ShowDatabaseError(Exception ex, string title)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show($"{title}.\nKi\u1EC3m tra d\u1EEF li\u1EC7u v\u00E0 k\u1EBFt n\u1ED1i SQL Server.", "L\u1ED7i",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
            UpdateOnlineStats();
        }

        private void btnExportRedis_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Export d\u1EEF li\u1EC7u Redis",
                Filter = "Redis backup (*.redisbackup)|*.redisbackup|T\u1EA5t c\u1EA3 file (*.*)|*.*",
                FileName = $"redis-session-backup-{DateTime.Now:yyyyMMdd-HHmmss}.redisbackup",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                int count = new RedisBackupService().Export(dialog.FileName);
                MessageBox.Show($"\u0110\u00E3 export {count} key Redis.\nFile: {dialog.FileName}",
                    "Export th\u00E0nh c\u00F4ng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Kh\u00F4ng th\u1EC3 export Redis. H\u00E3y ki\u1EC3m tra Redis \u0111\u0103ng ch\u1EA1y v\u00E0 th\u1EED l\u1EA1i.",
                    "L\u1ED7i Redis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportRedis_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Import d\u1EEF li\u1EC7u Redis",
                Filter = "Redis backup (*.redisbackup)|*.redisbackup|T\u1EA5t c\u1EA3 file (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            if (MessageBox.Show(
                    "C\u00E1c key tr\u00F9ng t\u00EAn trong Redis s\u1EBD b\u1ECB thay th\u1EBF b\u1EDFi d\u1EEF li\u1EC7u trong file backup. Ti\u1EBFp t\u1EE5c?",
                    "X\u00E1c nh\u1EADn import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                int count = new RedisBackupService().Import(dialog.FileName);
                LoadUsers();
                UpdateOnlineStats();
                MessageBox.Show($"\u0110\u00E3 import {count} key Redis. H\u00E3y l\u00E0m m\u1EDBi c\u00E1c m\u00E0n h\u00ECnh phi\u00EAn n\u1EBFu c\u1EA7n.",
                    "Import th\u00E0nh c\u00F4ng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Kh\u00F4ng th\u1EC3 import Redis. File c\u00F3 th\u1EC3 sai \u0111\u1ECBnh d\u1EA1ng ho\u1EB7c Redis kh\u00F4ng kh\u1EA3 d\u1EE5ng.",
                    "L\u1ED7i Redis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
