using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_PhienDangNhapNguoiDung.Models;
using QL_PhienDangNhapNguoiDung.Services;

namespace QL_PhienDangNhapNguoiDung.Forms
{
    public partial class FormLoginHistory : Form
    {
        private readonly int _userId;
        private readonly string _role;
        private readonly LoginHistoryService _historyService;
        private readonly SessionManager _sessionManager;

        public FormLoginHistory(int userId, string role)
        {
            InitializeComponent();
            _userId         = userId;
            _role           = role;
            _historyService = new LoginHistoryService(AppConfig.SQL_CONNECTION);
            _sessionManager = new SessionManager();
        }

        private void FormLoginHistory_Load(object sender, EventArgs e)
        {
            LoadHistory();
            LoadRecentRedisLog();
        }

        private void LoadHistory()
        {
            List<LoginHistoryModel> records = _role == "admin"
                ? _historyService.GetAll()
                : _historyService.GetByUser(_userId);

            dgvHistory.Rows.Clear();
            foreach (var r in records)
            {
                int row = dgvHistory.Rows.Add(
                    r.Username,
                    r.LoginTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    r.LogoutTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "-",
                    r.Duration,
                    r.IpAddress ?? "-",
                    r.Status,
                    r.Note ?? "-"
                );

                // MÃƒÆ’Ã‚Â¯Ãƒâ€šÃ‚Â¿Ãƒâ€šÃ‚Â½u theo status
                var color = r.Status switch
                {
                    "success" => Color.FromArgb(220, 250, 230),
                    "failed"  => Color.FromArgb(255, 230, 230),
                    "locked"  => Color.FromArgb(255, 200, 170),
                    _ => Color.White
                };
                dgvHistory.Rows[row].DefaultCellStyle.BackColor = color;
            }

            lblTotal.Text = $"T\u1ED5ng: {records.Count} b\u1EA3n ghi | " +
                $"Th\u00E0nh c\u00F4ng: {records.Count(r => r.Status == "success")} | " +
                $"Th\u1EA5t b\u1EA1i: {records.Count(r => r.Status == "failed")} | " +
                $"B\u1ECB kh\u00F3a: {records.Count(r => r.Status == "locked")}";
        }

        private void LoadRecentRedisLog()
        {
            var logs = _sessionManager.GetRecentLoginLog(_userId, 10);
            lstRedisLog.Items.Clear();
            foreach (var log in logs)
                lstRedisLog.Items.Add(log);

            if (logs.Count == 0)
                lstRedisLog.Items.Add("(Ch\u01B0a c\u00F3 log trong Redis)");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadHistory();
            LoadRecentRedisLog();
        }

        private void btnBack_Click(object sender, EventArgs e) => Close();
    }
}
