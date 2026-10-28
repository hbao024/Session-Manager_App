using System;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Models;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public partial class FormMain : Form
    {
        private readonly string _sessionId;
        private readonly UserService _userService;
        private readonly SessionManager _sessionManager;
        private readonly LoginHistoryService _historyService;
        private SessionInfo? _currentSession;
        private System.Windows.Forms.Timer _refreshTimer = null!;

        public FormMain(string sessionId)
        {
            InitializeComponent();
            _sessionId      = sessionId;
            _userService    = new UserService(AppConfig.SQL_CONNECTION);
            _sessionManager = new SessionManager();
            _historyService = new LoginHistoryService(AppConfig.SQL_CONNECTION);
        }

        private void FormMain_Resize(object? sender, EventArgs e)
        {
            if (panelHeader == null || panelInfo == null || panelActions == null) return;

            int contentWidth = Math.Max(420, ClientSize.Width - 36);
            panelHeader.Width = ClientSize.Width;
            panelInfo.Width = contentWidth;
            panelActions.Width = contentWidth;
            panelActions.Left = 18;
            panelActions.Top = Math.Max(456, ClientSize.Height - panelActions.Height - 18);
            panelInfo.Height = Math.Max(300, panelActions.Top - panelInfo.Top - 12);

            foreach (Control control in panelInfo.Controls)
            {
                if (control is Label label)
                    label.Width = Math.Max(100, panelInfo.ClientSize.Width - label.Left - 20);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadSessionInfo();
            StartSessionTimer();
        }

        private void LoadSessionInfo()
        {
            _currentSession = _sessionManager.ValidateAndRefresh(_sessionId);
            if (_currentSession == null)
            {
                MessageBox.Show("Phi\u00EAn \u0111\u0103ng nh\u1EADp \u0111\u00E3 h\u1EBFt h\u1EA1n!", "Th\u00F4ng b\u00E1o",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RedirectToLogin();
                return;
            }

            UserModel? account = _userService.GetUserById(_currentSession.UserId);
            if (account == null || !account.IsActive)
            {
                HandleSessionTermination(
                    "T\u00E0i kho\u1EA3n \u0111\u00E3 b\u1ECB v\u00F4 hi\u1EC7u h\u00F3a ho\u1EB7c kh\u00F4ng c\u00F2n t\u1ED3n t\u1EA1i.");
                return;
            }

            // Hi?n th? thÃƒÂ¯Ã‚Â¿Ã‚Â½ng tin user
            lblWelcome.Text       = $"Xin ch\u00E0o, {_currentSession.FullName}!";
            lblUsername.Text      = $"T\u00E0i kho\u1EA3n: {_currentSession.Username}";
            lblRole.Text          = $"Quy\u1EC1n: {(_currentSession.Role == "admin" ? "Qu\u1EA3n tr\u1ECB vi\u00EAn" : "Ng\u01B0\u1EDDi d\u00F9ng")}";
            lblEmail.Text         = $"Email: {_currentSession.Email}";
            lblLoginTime.Text    = $"\u0110\u0103ng nh\u1EADp l\u00FAc: {_currentSession.LoginTime:dd/MM/yyyy HH:mm:ss}";
            lblIP.Text           = $"\u0110\u1ECBa ch\u1EC9 IP: {_currentSession.IpAddress}";
            string shortSid = _currentSession.SessionId.Length >= 8 ? _currentSession.SessionId.Substring(0, 8) : _currentSession.SessionId;
            lblSessionId.Text    = $"Session ID: {shortSid}...";

            // Hi?n th? / ?n nÃƒÂ¯Ã‚Â¿Ã‚Â½t Admin
            btnAdmin.Visible = _currentSession.Role == "admin";
        }

        private void StartSessionTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 10000; // c?p nh?t m?i 10 giÃƒÂ¯Ã‚Â¿Ã‚Â½y
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            var session = _sessionManager.GetSessionInfo(_sessionId);
            if (session == null)
            {
                _refreshTimer.Stop();
                MessageBox.Show("Phi\u00EAn \u0111\u0103ng nh\u1EADp \u0111\u00E3 h\u1EBFt h\u1EA1n!", "H\u1EBFt phi\u00EAn",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RedirectToLogin();
                return;
            }

            UserModel? account = _userService.GetUserById(session.UserId);
            if (account == null || !account.IsActive)
            {
                HandleSessionTermination(
                    "T\u00E0i kho\u1EA3n \u0111\u00E3 b\u1ECB v\u00F4 hi\u1EC7u h\u00F3a. Phi\u00EAn l\u00E0m vi\u1EC7c \u0111\u00E3 b\u1ECB thu h\u1ED3i.");
                return;
            }

            lblTTL.Text          = $"C\u00F2n l\u1EA1i: {session.TtlSeconds / 60} ph\u00FAt {session.TtlSeconds % 60} gi\u00E2y";
            lblLastActivity.Text = $"Ho\u1EA1t \u0111\u1ED9ng cu\u1ED1i: {session.LastActivity:HH:mm:ss}";
        }

        private void HandleSessionTermination(string message)
        {
            _refreshTimer?.Stop();
            _sessionManager.Logout(_sessionId);
            try
            {
                _historyService.LogLogout(_sessionId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            MessageBox.Show(message, "Phi\u00EAn \u0111\u0103ng nh\u1EADp \u0111\u00E3 k\u1EBFt th\u00FAc",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RedirectToLogin();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("B\u1EA1n c\u00F3 ch\u1EAFc mu\u1ED1n \u0111\u0103ng xu\u1EA5t?", "X\u00E1c nh\u1EADn",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _refreshTimer.Stop();
                _sessionManager.Logout(_sessionId);
                _historyService.LogLogout(_sessionId);
                RedirectToLogin();
            }
        }

        private void btnSessions_Click(object sender, EventArgs e)
        {
            if (_currentSession == null) return;
            var form = new FormSessionList(_currentSession.UserId, _sessionId);
            form.ShowDialog(this);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (_currentSession == null) return;
            var form = new FormLoginHistory(_currentSession.UserId, _currentSession.Role);
            form.ShowDialog(this);
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            var form = new FormAdmin(_sessionId);
            form.ShowDialog(this);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Gia h?n phiÃƒÂ¯Ã‚Â¿Ã‚Â½n th? cÃƒÂ¯Ã‚Â¿Ã‚Â½ng
            _sessionManager.ValidateAndRefresh(_sessionId);
            LoadSessionInfo();


        }

        private void RedirectToLogin()
        {
            AppConfig.CurrentSessionId = null;
            var login = new FormLogin();
            login.Show();
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            base.OnFormClosing(e);
        }
    }
}
