-- Bảng đánh giá sao & bình luận sản phẩm (Tooru Coffee)
USE TooruCoffee;
GO

IF OBJECT_ID(N'dbo.DanhGiaBinhLuan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhGiaBinhLuan (
        MaDG INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaSP INT NOT NULL,
        MaKH INT NULL,
        HoTenHienThi NVARCHAR(100) NOT NULL,
        SoSao TINYINT NOT NULL,
        NoiDung NVARCHAR(500) NOT NULL,
        NgayTao DATETIME NOT NULL CONSTRAINT DF_DanhGia_NgayTao DEFAULT (GETDATE()),
        TrangThai BIT NOT NULL CONSTRAINT DF_DanhGia_TrangThai DEFAULT (1),
        CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5),
        CONSTRAINT FK_DanhGia_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP) ON DELETE CASCADE,
        CONSTRAINT FK_DanhGia_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE SET NULL
    );

    CREATE INDEX IX_DanhGia_MaSP ON dbo.DanhGiaBinhLuan(MaSP);
    CREATE UNIQUE INDEX UQ_DanhGia_MaSP_MaKH ON dbo.DanhGiaBinhLuan(MaSP, MaKH) WHERE MaKH IS NOT NULL;
END
GO