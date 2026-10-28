-- ============================================================
-- ĐỒ ÁN: QUẢN LÝ PHIÊN ĐĂNG NHẬP NGƯỜI DÙNG
-- CSDL: dbSessionManager
--
-- Script an toàn: không xóa USERS hoặc LOGIN_HISTORY.
-- Có thể chạy lại nhiều lần để sửa/cập nhật dữ liệu mẫu.
-- ============================================================

USE master;
GO

IF DB_ID(N'dbSessionManager') IS NULL
BEGIN
    CREATE DATABASE dbSessionManager;
END
GO

USE dbSessionManager;
GO

-- ============================================================
-- TẠO BẢNG NẾU CHƯA CÓ
-- ============================================================

IF OBJECT_ID(N'dbo.USERS', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.USERS (
        user_id      INT IDENTITY(1,1) PRIMARY KEY,
        username     NVARCHAR(50)  NOT NULL UNIQUE,
        password_hash NVARCHAR(255) NOT NULL,
        email        NVARCHAR(100) NOT NULL,
        full_name    NVARCHAR(100) NOT NULL,
        role         NVARCHAR(20)  NOT NULL DEFAULT 'user',
        is_active    BIT           NOT NULL DEFAULT 1,
        created_at   DATETIME      NOT NULL DEFAULT GETDATE(),
        last_login   DATETIME      NULL
    );
END
GO

IF OBJECT_ID(N'dbo.LOGIN_HISTORY', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LOGIN_HISTORY (
        id           INT IDENTITY(1,1) PRIMARY KEY,
        user_id      INT           NULL,
        username     NVARCHAR(50)  NOT NULL,
        session_id   NVARCHAR(100) NULL,
        login_time   DATETIME      NOT NULL DEFAULT GETDATE(),
        logout_time  DATETIME      NULL,
        ip_address   NVARCHAR(50)  NULL,
        status       NVARCHAR(20)  NOT NULL,
        note         NVARCHAR(200) NULL,
        CONSTRAINT FK_LOGIN_HISTORY_USERS
            FOREIGN KEY (user_id) REFERENCES dbo.USERS(user_id)
    );
END
GO

-- ============================================================
-- DỮ LIỆU MẪU VÀ SỬA HASH CŨ
-- ============================================================

DECLARE @AdminHash NVARCHAR(64) =
    N'240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9';
DECLARE @UserHash NVARCHAR(64) =
    N'e606e38b0d8c19b24cf0ee3808183162ea7cd63ff7912dbb22b5e803286b4446';

IF NOT EXISTS (SELECT 1 FROM dbo.USERS WHERE username = N'admin')
BEGIN
    INSERT INTO dbo.USERS (username, password_hash, email, full_name, role, is_active)
    VALUES (N'admin', @AdminHash, N'admin@sessionapp.com', N'Quản trị viên', N'admin', 1);
END

-- Sửa hash admin cũ bị thiếu ký tự cuối, không ghi đè mật khẩu khác.
UPDATE dbo.USERS
SET password_hash = @AdminHash
WHERE username = N'admin'
  AND password_hash = N'240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a';

IF NOT EXISTS (SELECT 1 FROM dbo.USERS WHERE username = N'nguyenvana')
BEGIN
    INSERT INTO dbo.USERS (username, password_hash, email, full_name, role, is_active)
    VALUES (N'nguyenvana', @UserHash, N'vana@gmail.com', N'Nguyễn Văn A', N'user', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.USERS WHERE username = N'tranthib')
BEGIN
    INSERT INTO dbo.USERS (username, password_hash, email, full_name, role, is_active)
    VALUES (N'tranthib', @UserHash, N'thib@gmail.com', N'Trần Thị B', N'user', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.USERS WHERE username = N'levanc')
BEGIN
    INSERT INTO dbo.USERS (username, password_hash, email, full_name, role, is_active)
    VALUES (N'levanc', @UserHash, N'vanc@gmail.com', N'Lê Văn C', N'user', 0);
END

-- Chỉ sửa các hash sai do script cũ, không ghi đè mật khẩu người dùng đã đổi.
UPDATE dbo.USERS
SET password_hash = @UserHash
WHERE username IN (N'nguyenvana', N'tranthib', N'levanc')
  AND password_hash IN (
      N'9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08',
      N'173c7498bb17a3e63a5e27e00cefd0faeafab406a9fcf64f3ba1dec4a8c83b9b'
  );
GO

SELECT user_id, username, full_name, role, is_active, created_at
FROM dbo.USERS
ORDER BY user_id;
GO
