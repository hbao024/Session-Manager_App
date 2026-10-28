using System;
using System.Linq;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Models;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public partial class FormLogin : Form
    {
        private readonly UserService _userService;
        private readonly SessionManager _sessionManager;
        private readonly LoginHistoryService _historyService;
        private readonly RiskAssessmentService _riskService;
        private readonly SessionEventService _eventService;
        private readonly MfaService _mfaService;

        public FormLogin()
        {
            InitializeComponent();
            _userService    = new UserService(AppConfig.SQL_CONNECTION);
            _sessionManager = new SessionManager();
            _historyService = new LoginHistoryService(AppConfig.SQL_CONNECTION);
            _riskService    = new RiskAssessmentService();
            _eventService   = new SessionEventService();
            _mfaService     = new MfaService();
        }

        private void FormLogin_Resize(object? sender, EventArgs e)
        {
            if (panelCard == null) return;
            panelCard.Location = new System.Drawing.Point(
                Math.Max(0, (ClientSize.Width - panelCard.Width) / 2),
                Math.Max(0, (ClientSize.Height - panelCard.Height) / 2));
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowError("Vui l\u00F2ng nh\u1EADp \u0111\u1EA7y \u0111\u1EE7 t\u00EAn \u0111\u0103ng nh\u1EADp v\u00E0 m\u1EADt kh\u1EA9u.");
                    return;
                }

                if (_sessionManager.IsLocked(username))
                {
                    long remaining = _sessionManager.GetLockRemainingSeconds(username);
                    ShowError($"T\u00E0i kho\u1EA3n t\u1EA1m th\u1EDDi b\u1ECB kh\u00F3a do \u0111\u0103ng nh\u1EADp sai nhi\u1EC1u l\u1EA7n.\nTh\u1EED l\u1EA1i sau {remaining} gi\u00E2y.");
                    TryLogLogin(null, username, null, "locked", "Brute-force detected");
                    return;
                }

                UserModel? user = _userService.Authenticate(username, password);

                if (user == null)
                {
                    long failed = _sessionManager.IncrementFailedAttempts(username);
                    int remaining = Math.Max(0, 5 - (int)failed);
                    string msg = remaining > 0
                        ? $"Sai t\u00EAn \u0111\u0103ng nh\u1EADp ho\u1EB7c m\u1EADt kh\u1EA9u. C\u00F2n {remaining} l\u1EA7n th\u1EED."
                        : "T\u00E0i kho\u1EA3n \u0111\u00E3 b\u1ECB kh\u00F3a 15 ph\u00FAt do \u0111\u0103ng nh\u1EADp sai qu\u00E1 nhi\u1EC1u l\u1EA7n.";
                    ShowError(msg);
                    TryLogLogin(null, username, null, "failed");
                    return;
                }

                if (!user.IsActive)
                {
                    ShowError("T\u00E0i kho\u1EA3n c\u1EE7a b\u1EA1n \u0111\u00E3 b\u1ECB v\u00F4 hi\u1EC7u h\u00F3a. Vui l\u00F2ng li\u00EAn h\u1EC7 qu\u1EA3n tr\u1ECB vi\u00EAn.");
                    TryLogLogin(user.UserId, username, null, "failed", "Account disabled");
                    return;
                }

                string ipAddress = GetLocalIP();
                string deviceId = Environment.MachineName;
                LoginRiskResult risk = _riskService.Assess(user, ipAddress, deviceId);

                if (risk.Score > 0)
                {
                    _eventService.Publish(new SessionEvent
                    {
                        EventType = risk.Blocked ? "RISK_BLOCKED" : "RISK_DETECTED",
                        UserId = user.UserId,
                        Username = user.Username,
                        IpAddress = ipAddress,
                        RiskScore = risk.Score,
                        RiskLevel = risk.Level,
                        Reason = risk.ReasonText
                    });
                }

                if (risk.Blocked)
                {
                    ShowError($"Đăng nhập bị từ chối do rủi ro cao ({risk.Score} điểm).");
                    TryLogLogin(user.UserId, username, null, "failed", risk.ReasonText);
                    return;
                }

                bool mfaEnabled = user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                    _mfaService.IsEnabled(user.UserId);
                if (risk.RequiresMfa && !mfaEnabled)
                {
                    ShowError("Đăng nhập có rủi ro trung bình/cao. Admin cần thiết lập MFA trước.");
                    TryLogLogin(user.UserId, username, null, "failed", "MFA required by risk policy");
                    return;
                }

                if (mfaEnabled)
                {
                    using var mfaDialog = new FormMfaCode(user.UserId, _mfaService);
                    if (mfaDialog.ShowDialog(this) != DialogResult.OK)
                    {
                        TryLogLogin(user.UserId, username, null, "failed", "MFA verification failed");
                        return;
                    }
                }

                _sessionManager.ClearFailedAttempts(username);
                _riskService.RememberLogin(user, ipAddress, deviceId);
                string sessionId = _sessionManager.CreateSession(user, ipAddress, risk, deviceId);
                _userService.UpdateLastLogin(user.UserId);
                TryLogLogin(user.UserId, username, sessionId, "success");

                AppConfig.CurrentSessionId = sessionId;

                var formMain = new FormMain(sessionId);
                formMain.Show();
                Hide();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowError("Kh\u00F4ng th\u1EC3 \u0111\u0103ng nh\u1EADp. H\u00E3y ki\u1EC3m tra SQL Server v\u00E0 Redis r\u1ED3i th\u1EED l\u1EA1i.");
            }
        }

        private void TryLogLogin(int? userId, string username, string? sessionId, string status, string? note = null)
        {
            try
            {
                _historyService.LogLogin(userId, username, sessionId, GetLocalIP(), status, note);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();
            lblError.Visible = false;
            txtUsername.Focus();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private string GetLocalIP()
        {
            return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                .AddressList
                .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "127.0.0.1";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (Application.OpenForms.Count == 0)
                Application.Exit();
        }
    }
}
