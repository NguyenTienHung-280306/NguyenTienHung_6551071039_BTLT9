CREATE DATABASE QL_ChiTieu;
GO
USE QL_ChiTieu;
GO

CREATE TABLE ChiTieu (
    MaCT INT IDENTITY(1,1) PRIMARY KEY,
    NgayChi DATE NOT NULL,
    NoiDung NVARCHAR(100) NOT NULL,
    LoaiChi NVARCHAR(30) NOT NULL,
    SoTien DECIMAL(18,0) NOT NULL,
    GhiChu NVARCHAR(200) NULL
);
INSERT INTO ChiTieu (NgayChi, NoiDung, LoaiChi, SoTien, GhiChu) VALUES
('2026-10-01', N'Cơm trưa', N'Ăn uống', 35000, N''),
('2026-10-02', N'Xăng xe', N'Di chuyển', 60000, N''),
('2026-10-03', N'Photo giáo trình', N'Học tập', 25000, N''),
('2026-10-04', N'Xem phim', N'Giải trí', 90000, N''),
('2026-10-05', N'Trà sữa', N'Ăn uống', 40000, N''),
('2026-10-06', N'Mua sắm', N'Cá nhân', 150000, N''),
('2026-10-07', N'Tiền điện', N'Hóa đơn', 500000, N''),
('2026-10-08', N'Ăn sáng', N'Ăn uống', 25000, N''),
('2026-10-09', N' gửi xe', N'Di chuyển', 5000, N''),
('2026-10-10', N'Mua bút', N'Học tập', 10000, N'');