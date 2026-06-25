USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TooruCoffee')
BEGIN
    ALTER DATABASE TooruCoffee SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TooruCoffee;
END
GO
CREATE DATABASE TooruCoffee COLLATE Vietnamese_CI_AS;
GO
USE TooruCoffee;
GO
CREATE TABLE LoaiSanPham (
    MaLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(500),
    HinhAnh NVARCHAR(255),
    ThuTuHienThi INT DEFAULT 0,
    TrangThai BIT DEFAULT 1
);
GO
CREATE TABLE SanPham (
    MaSP INT IDENTITY(1,1) PRIMARY KEY,
    TenSP NVARCHAR(150) NOT NULL,
    MaLoai INT NOT NULL,
    Gia DECIMAL(18,2) NOT NULL CHECK (Gia >= 0),
    MoTa NVARCHAR(1000),
    HinhAnh NVARCHAR(255) NOT NULL,
    DonViTinh NVARCHAR(20) DEFAULT N'Ly',
    KichCo NVARCHAR(50) DEFAULT N'M (Vừa)',
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE(),
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai)
);
GO
CREATE TABLE Ban (
    MaBan INT IDENTITY(1,1) PRIMARY KEY,
    TenBan NVARCHAR(50) NOT NULL UNIQUE,
    KhuVuc NVARCHAR(50) DEFAULT N'Tầng trệt',
    SoCho INT DEFAULT 4 CHECK (SoCho > 0),
    TrangThai NVARCHAR(30) DEFAULT N'Trống' CONSTRAINT CK_Ban_TrangThai CHECK (TrangThai IN (N'Trống', N'Đang phục vụ', N'Đã đặt trước', N'Bảo trì')),
    GhiChu NVARCHAR(200)
);
GO
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
    SDT VARCHAR(15) UNIQUE NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    DiaChi NVARCHAR(255),
    ChucVu NVARCHAR(50) NOT NULL,
    NgayVaoLam DATE DEFAULT GETDATE(),
    LuongCoBan DECIMAL(18,2) DEFAULT 0,
    TrangThai BIT DEFAULT 1
);
GO
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15) UNIQUE NOT NULL,
    Email NVARCHAR(100),
    DiaChi NVARCHAR(255),
    NgaySinh DATE,
    DiemTichLuy INT DEFAULT 0 CHECK (DiemTichLuy >= 0),
    HangThanhVien NVARCHAR(20) DEFAULT N'Thường' CONSTRAINT CK_KH_Hang CHECK (HangThanhVien IN (N'Thường', N'Bạc', N'Vàng', N'Kim Cương')),
    NgayTao DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(300)
);
GO
CREATE TABLE TaiKhoan (
    MaTK INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhauHash NVARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(20) NOT NULL,
    MaNV INT NULL,
    MaKH INT NULL,
    TrangThai BIT DEFAULT 1,
    LanDangNhapCuoi DATETIME,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT CK_TK_VaiTro CHECK (VaiTro IN ('Admin', 'NhanVien', 'KhachHang')),
    CONSTRAINT CK_TK_MotLoai CHECK (
        (VaiTro = 'NhanVien' AND MaNV IS NOT NULL AND MaKH IS NULL) OR
        (VaiTro = 'KhachHang' AND MaKH IS NOT NULL AND MaNV IS NULL) OR
        (VaiTro = 'Admin' AND MaNV IS NULL AND MaKH IS NULL)
    )
);
GO
CREATE TABLE KhuyenMai (
    MaKM INT IDENTITY(1,1) PRIMARY KEY,
    TenKM NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(500),
    LoaiGiam NVARCHAR(20) DEFAULT N'PhanTram' CONSTRAINT CK_KM_Loai CHECK (LoaiGiam IN (N'PhanTram', N'SoTienCoDinh')),
    GiaTriGiam DECIMAL(18,2) NOT NULL CHECK (GiaTriGiam >= 0),
    DieuKienToiThieu DECIMAL(18,2) DEFAULT 0,
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    SoLuongToiDa INT DEFAULT 9999,
    DaSuDung INT DEFAULT 0,
    TrangThai BIT DEFAULT 1,
    CONSTRAINT CK_KM_Ngay CHECK (NgayKetThuc >= NgayBatDau)
);
GO
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaBan INT NULL,
    MaNV INT NOT NULL,
    MaKH INT NULL,
    MaKM INT NULL,
    NgayLap DATETIME DEFAULT GETDATE(),
    TongTienTruocGiam DECIMAL(18,2) DEFAULT 0,
    GiamGia DECIMAL(18,2) DEFAULT 0,
    TongThanhToan DECIMAL(18,2) DEFAULT 0,
    TrangThai NVARCHAR(30) DEFAULT N'Chờ thanh toán' CONSTRAINT CK_HD_TrangThai CHECK (TrangThai IN (N'Chờ thanh toán', N'Đã thanh toán', N'Đã hủy', N'Hoàn thành', N'Đang chuẩn bị')),
    LoaiDon NVARCHAR(20) DEFAULT N'Tại quán' CONSTRAINT CK_HD_LoaiDon CHECK (LoaiDon IN (N'Tại quán', N'Mang về', N'Giao hàng')),
    TenKhach NVARCHAR(100),
    SDT VARCHAR(15),
    DiaChiGiao NVARCHAR(255),
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaBan) REFERENCES Ban(MaBan),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaKM) REFERENCES KhuyenMai(MaKM)
);
GO
CREATE TABLE ChiTietHoaDon (
    MaCT INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT NOT NULL,
    MaSP INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    GhiChu NVARCHAR(300),
    ThanhTien AS (CAST(SoLuong AS DECIMAL(18,2)) * DonGia) PERSISTED,
    FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD) ON DELETE CASCADE,
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO
CREATE TABLE ThanhToan (
    MaTT INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT NOT NULL UNIQUE,
    NgayThanhToan DATETIME DEFAULT GETDATE(),
    SoTien DECIMAL(18,2) NOT NULL,
    PhuongThuc NVARCHAR(30) NOT NULL CONSTRAINT CK_TT_PhuongThuc CHECK (PhuongThuc IN (N'Tiền mặt', N'Thẻ', N'Chuyển khoản', N'Momo', N'ZaloPay', N'ShopeePay', N'Khác')),
    MaGiaoDich NVARCHAR(100),
    TrangThai NVARCHAR(20) DEFAULT N'Thành công',
    FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD)
);
GO
CREATE TABLE DatBan (
    MaDatBan INT IDENTITY(1,1) PRIMARY KEY,
    MaBan INT NOT NULL,
    MaKH INT NULL,
    TenKhach NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15) NOT NULL,
    NgayDat DATE NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME,
    SoNguoi INT DEFAULT 2 CHECK (SoNguoi > 0),
    TrangThai NVARCHAR(20) DEFAULT N'Đã xác nhận' CONSTRAINT CK_DB_TrangThai CHECK (TrangThai IN (N'Đã xác nhận', N'Đã đến', N'Đã hủy', N'Hoàn tất', N'Quá hạn')),
    GhiChu NVARCHAR(300),
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaBan) REFERENCES Ban(MaBan),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);
GO
CREATE TABLE NhaCungCap (
    MaNCC INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    SDT VARCHAR(15),
    Email NVARCHAR(100),
    DiaChi NVARCHAR(255),
    NguoiLienHe NVARCHAR(100),
    TrangThai BIT DEFAULT 1
);
GO
CREATE TABLE NguyenLieu (
    MaNL INT IDENTITY(1,1) PRIMARY KEY,
    TenNL NVARCHAR(100) NOT NULL,
    DonViTinh NVARCHAR(20) DEFAULT N'kg',
    SoLuongTon DECIMAL(18,3) DEFAULT 0,
    GiaNhapTrungBinh DECIMAL(18,2),
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    MaNCC INT NULL,
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);
GO
CREATE TABLE PhieuNhap (
    MaPN INT IDENTITY(1,1) PRIMARY KEY,
    MaNCC INT NOT NULL,
    MaNV INT NOT NULL,
    NgayNhap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) DEFAULT 0,
    GhiChu NVARCHAR(300),
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);
GO
CREATE TABLE ChiTietPhieuNhap (
    MaCTPN INT IDENTITY(1,1) PRIMARY KEY,
    MaPN INT NOT NULL,
    MaNL INT NOT NULL,
    SoLuong DECIMAL(18,3) NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia) PERSISTED,
    FOREIGN KEY (MaPN) REFERENCES PhieuNhap(MaPN) ON DELETE CASCADE,
    FOREIGN KEY (MaNL) REFERENCES NguyenLieu(MaNL)
);
GO
CREATE TABLE CongThuc (
    MaCongThuc INT IDENTITY(1,1) PRIMARY KEY,
    MaSP INT NOT NULL,
    MaNL INT NOT NULL,
    SoLuongCan DECIMAL(10,3) NOT NULL CHECK (SoLuongCan > 0),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP),
    FOREIGN KEY (MaNL) REFERENCES NguyenLieu(MaNL),
    CONSTRAINT UQ_CongThuc UNIQUE (MaSP, MaNL)
);
GO
CREATE TABLE DanhGia (
    MaDG INT IDENTITY(1,1) PRIMARY KEY,
    MaSP INT NOT NULL,
    MaKH INT NULL,
    HoTen NVARCHAR(100),
    SoSao INT NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
    BinhLuan NVARCHAR(1000),
    NgayDG DATETIME DEFAULT GETDATE(),
    TrangThai BIT DEFAULT 1,
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);
GO
CREATE TABLE LichSuHoatDong (
    MaLS INT IDENTITY(1,1) PRIMARY KEY,
    LoaiHanhDong NVARCHAR(30) NOT NULL,
    MaThamChieu INT,
    BangThamChieu NVARCHAR(50),
    MoTa NVARCHAR(500),
    MaNV INT NULL,
    ThoiGian DATETIME DEFAULT GETDATE(),
    IP NVARCHAR(50),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);
GO
CREATE INDEX IX_SanPham_TenSP ON SanPham(TenSP);
CREATE INDEX IX_SanPham_MaLoai ON SanPham(MaLoai);
CREATE INDEX IX_HoaDon_NgayLap ON HoaDon(NgayLap);
CREATE INDEX IX_HoaDon_TrangThai ON HoaDon(TrangThai);
CREATE INDEX IX_HoaDon_MaBan ON HoaDon(MaBan);
CREATE INDEX IX_ChiTietHoaDon_MaHD ON ChiTietHoaDon(MaHD);
CREATE INDEX IX_DatBan_NgayGio ON DatBan(NgayDat, GioBatDau);
CREATE INDEX IX_KhachHang_SDT ON KhachHang(SDT);
CREATE INDEX IX_NguyenLieu_Ten ON NguyenLieu(TenNL);
GO
INSERT INTO LoaiSanPham (TenLoai, MoTa, ThuTuHienThi, TrangThai) VALUES
(N'Cà phê', N'Các loại cà phê đặc sản Tooru Coffee', 1, 1),
(N'Trà', N'Trà trái cây, trà signature', 2, 1),
(N'Sinh tố & Yogurt', N'Sinh tố tươi mát, yogurt lành mạnh', 3, 1),
(N'Nước giải khát & Soda', N'Nước ngọt, soda, ép trái cây', 4, 1),
(N'Đồ ăn & Món ăn', N'Món ăn nhẹ, cơm, rau, hải sản', 5, 1);
GO
INSERT INTO SanPham (TenSP, MaLoai, Gia, MoTa, HinhAnh, DonViTinh, KichCo, TrangThai) VALUES
(N'Americano Tiki', 1, 49000, N'Cà phê Americano đậm vị signature Tooru', N'images/Americano_Tiki.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Sữa Đá', 1, 39000, N'Cà phê sữa đá chuẩn vị Việt', N'images/Ca_phe_sua_da.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Muối', 1, 55000, N'Cà phê muối độc đáo', N'images/Ca_phe_muoi.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Kem Tươi', 1, 58000, N'Cà phê với kem tươi đánh bông', N'images/Ca_phe_kem_tuoi.jpg', N'Ly', N'L (Lớn)', 1),
(N'Cà Phê Latte Art', 1, 62000, N'Latte art đẹp mắt', N'images/Ca_phe_latte_art.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Sữa Nóng', 1, 42000, N'Cà phê sữa nóng', N'images/Ca_phe_sua_nong.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Đá Chén', 1, 36000, N'Cà phê đá chén truyền thống', N'images/Ca_phe_da_chen.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Bạch Nâu', 1, 47000, N'Cà phê Bạch Nâu', N'images/Ca_phe_Bach_Nau.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cà Phê Sữa Đá Lớp', 1, 44000, N'Cà phê sữa đá lớp', N'images/Ca_phe_sua_da_lop.jpg', N'Ly', N'L (Lớn)', 1),
(N'Bạc Xỉu', 1, 41000, N'Bạc xỉu đá', N'images/Ca_phe_sua_da.jpg', N'Ly', N'M (Vừa)', 1),
(N'Espresso', 1, 35000, N'Espresso nguyên chất', N'images/Ca_phe_da_chen.jpg', N'Ly', N'Nhỏ', 1),
(N'Cà Phê Cốt Dừa', 1, 53000, N'Cà phê cốt dừa mát lạnh', N'images/Ca_phe_kem_tuoi.jpg', N'Ly', N'L (Lớn)', 1),
(N'Trà Đào Cam Sả', 2, 42000, N'Trà đào cam sả', N'images/Tra_cam_da_cocktail.jpg', N'Ly', N'L (Lớn)', 1),
(N'Trà Vải Thiều', 2, 44000, N'Trà vải thiều', N'images/Tra_cam_da_cocktail.jpg', N'Ly', N'L (Lớn)', 1),
(N'Trà Cam Đá Cocktail', 2, 39000, N'Trà cam cocktail', N'images/Tra_cam_da_cocktail.jpg', N'Ly', N'M (Vừa)', 1),
(N'Sinh Tố Xoài', 3, 49000, N'Sinh tố xoài chín', N'images/Sinh_to_xoai.jpg', N'Ly', N'L (Lớn)', 1),
(N'Sinh Tố Bơ Sáp', 3, 52000, N'Sinh tố bơ sáp', N'images/Sinh_to_xoai.jpg', N'Ly', N'L (Lớn)', 1),
(N'Yogurt Bạc Hà', 3, 46000, N'Yogurt bạc hà mát lạnh', N'images/Yogurt_bac_ha.jpg', N'Ly', N'M (Vừa)', 1),
(N'Yogurt Xoài', 3, 47000, N'Yogurt xoài', N'images/Sinh_to_xoai.jpg', N'Ly', N'L (Lớn)', 1),
(N'7Up Lon', 4, 25000, N'7Up lon', N'images/7Up_Lon.jpg', N'Lon', N'330ml', 1),
(N'Soda Chanh Bạc Hà', 4, 36000, N'Soda chanh bạc hà', N'images/Yogurt_bac_ha.jpg', N'Ly', N'M (Vừa)', 1),
(N'Nước Ép Cam', 4, 39000, N'Nước ép cam tươi', N'images/Tra_cam_da_cocktail.jpg', N'Ly', N'M (Vừa)', 1),
(N'Cocktail Vàng Biển', 4, 68000, N'Cocktail vàng biển signature', N'images/Cocktail_vang_bien.jpg', N'Ly', N'L (Lớn)', 1),
(N'Chai Đen LED', 4, 33000, N'Chai đen LED đặc biệt', N'images/Chai_den_led.jpg', N'Chai', N'500ml', 1),
(N'Cà Kho Tộ', 5, 75000, N'Cà kho tộ đậm đà', N'images/Ca_kho_to.jpg', N'Phần', N'1 người', 1),
(N'Rau Muống Xào Tỏi', 5, 29000, N'Rau muống xào tỏi', N'images/Rau_muong_xao_toi.jpg', N'Phần', N'1 người', 1),
(N'Gà Chiên Bơ Tỏi', 5, 68000, N'Gà chiên bơ tỏi giòn', N'images/Ca_kho_to.jpg', N'Phần', N'1 người', 1),
(N'Bánh Mì Trứng Pate', 5, 39000, N'Bánh mì trứng pate', N'images/Ca_phe_sua_da_lop.jpg', N'Ổ', N'1 ổ', 1),
(N'Salad Rau Tươi', 5, 42000, N'Salad rau củ tươi', N'images/Rau_muong_xao_toi.jpg', N'Phần', N'1 người', 1),
(N'Flan Caramen', 5, 29000, N'Flan caramen', N'images/Ca_phe_muoi.jpg', N'Phần', N'1 cái', 1);
GO
INSERT INTO Ban (TenBan, KhuVuc, SoCho, TrangThai, GhiChu) VALUES
(N'Bàn 01', N'Tầng trệt', 2, N'Trống', NULL),
(N'Bàn 02', N'Tầng trệt', 4, N'Trống', NULL),
(N'Bàn 03', N'Tầng trệt', 4, N'Trống', NULL),
(N'Bàn 04', N'Tầng trệt', 6, N'Trống', NULL),
(N'Bàn VIP 1', N'Tầng 2', 6, N'Trống', N'Phòng riêng, có máy lạnh'),
(N'Bàn VIP 2', N'Tầng 2', 8, N'Trống', N'Phòng riêng, có máy lạnh'),
(N'Sân vườn 1', N'Sân vườn', 4, N'Trống', N'Không gian thoáng mát'),
(N'Sân vườn 2', N'Sân vườn', 4, N'Trống', N'Không gian thoáng mát'),
(N'Bàn 05', N'Tầng trệt', 2, N'Trống', NULL),
(N'Bàn 06', N'Tầng trệt', 4, N'Trống', NULL),
(N'Bàn 07', N'Tầng trệt', 4, N'Trống', NULL),
(N'Bàn 08', N'Sân vườn', 6, N'Trống', NULL);
GO
INSERT INTO NhanVien (HoTen, NgaySinh, GioiTinh, SDT, Email, DiaChi, ChucVu, NgayVaoLam, LuongCoBan, TrangThai) VALUES
(N'Nguyễn Văn Admin', '1990-05-15', N'Nam', '0901234567', 'admin@toorucoffee.vn', N'123 Đường Cafe, Phường Vĩnh Hải, Nha Trang', N'Quản lý', '2023-01-01', 15000000, 1),
(N'Trần Thị Barista', '1995-08-20', N'Nữ', '0912345678', 'barista@toorucoffee.vn', N'45 Nguyễn Trãi, Phường Tân Lập, Nha Trang', N'Barista', '2023-03-15', 8500000, 1),
(N'Lê Văn Phục Vụ', '1998-11-10', N'Nam', '0923456789', 'phucvu@toorucoffee.vn', N'78 Lê Lợi, Phường Vĩnh Hải, Nha Trang', N'Phục vụ', '2024-01-20', 6500000, 1),
(N'Phạm Thị Thu Ngân', '1993-02-28', N'Nữ', '0934567890', 'thungan@toorucoffee.vn', N'12 Pasteur, Phường Tân Lập, Nha Trang', N'Thu ngân', '2023-06-01', 9000000, 1),
(N'Võ Minh Tuấn', '1997-09-05', N'Nam', '0945678901', 'tuanvo@toorucoffee.vn', N'34 Hai Bà Trưng, Phường Vĩnh Hải, Nha Trang', N'Phục vụ', '2024-04-10', 6200000, 1);
GO
INSERT INTO KhachHang (HoTen, SDT, Email, DiaChi, NgaySinh, DiemTichLuy, HangThanhVien, NgayTao) VALUES
(N'Nguyễn Thị Khách Hàng', '0987654321', 'khach1@gmail.com', N'56 Hai Bà Trưng, Phường Vĩnh Hải, Nha Trang', '1995-07-12', 1250, N'Vàng', '2024-01-05'),
(N'Trần Minh Tuấn', '0976543210', 'tuantran@gmail.com', N'89 Võ Văn Tần, Phường Tân Lập, Nha Trang', '1988-03-22', 320, N'Thường', '2024-02-10'),
(N'Lê Thị Hương', '0965432109', 'huongle@yahoo.com', N'101 Nguyễn Đình Chiểu, Phường Vĩnh Hải, Nha Trang', '2000-12-05', 780, N'Bạc', '2024-03-01'),
(N'Phạm Quốc Bảo', '0954321098', 'baopham@gmail.com', N'22 Lê Duẩn, Phường Tân Lập, Nha Trang', '1992-04-18', 2450, N'Kim Cương', '2023-11-20'),
(N'Đặng Thị Mai', '0943210987', 'maidd@gmail.com', N'67 Nguyễn Huệ, Phường Vĩnh Hải, Nha Trang', '1999-08-30', 560, N'Bạc', '2024-05-02');
GO
INSERT INTO TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaNV, MaKH, TrangThai) VALUES
(N'admin', N'admin@Tooru2026!', 'Admin', NULL, NULL, 1),
(N'barista01', N'barista@123', 'NhanVien', 2, NULL, 1),
(N'thungan01', N'thungan@123', 'NhanVien', 4, NULL, 1),
(N'phucvu01', N'phucvu@123', 'NhanVien', 3, NULL, 1),
(N'khachhang1', N'khach@123', 'KhachHang', NULL, 1, 1),
(N'khachhang2', N'khach@123', 'KhachHang', NULL, 2, 1),
(N'khachhang3', N'khach@123', 'KhachHang', NULL, 4, 1);
GO
INSERT INTO KhuyenMai (TenKM, MoTa, LoaiGiam, GiaTriGiam, DieuKienToiThieu, NgayBatDau, NgayKetThuc, SoLuongToiDa, DaSuDung, TrangThai) VALUES
(N'Giảm 10% cho đơn từ 150k', N'Áp dụng tất cả sản phẩm, không kèm KM khác', N'PhanTram', 10, 150000, '2026-06-01', '2026-12-31', 500, 12, 1),
(N'Giảm 20k cho đơn từ 80k', N'Áp dụng cho khách thành viên Bạc trở lên', N'SoTienCoDinh', 20000, 80000, '2026-06-15', '2026-07-15', 200, 45, 1),
(N'Happy Hour 15h-17h giảm 15%', N'Chỉ áp dụng đồ uống (trừ nước ngọt)', N'PhanTram', 15, 0, '2026-06-01', '2026-06-30', 9999, 89, 1);
GO
INSERT INTO NhaCungCap (TenNCC, SDT, Email, DiaChi, NguoiLienHe, TrangThai) VALUES
(N'Công ty TNHH Cà Phê Việt', '0281234567', 'contact@capheviet.vn', N'Khu công nghiệp Suối Dầu, Nha Trang', N'Ông Nguyễn Văn Cà', 1),
(N'Nhà cung cấp Trái cây Tươi Sạch', '0289876543', 'traicay@tuoisach.com', N'Chợ đầu mối Nha Trang, Phường Tân Lập', N'Chị Trần Thị Trái', 1),
(N'Nước giải khát ABC', '0285556666', 'sales@nuocabc.vn', N'Phường Vĩnh Hải, Nha Trang', N'Anh Lê Văn Nước', 1);
GO
INSERT INTO NguyenLieu (TenNL, DonViTinh, SoLuongTon, GiaNhapTrungBinh, MaNCC) VALUES
(N'Cà phê bột nguyên chất', N'kg', 45.500, 180000, 1),
(N'Sữa đặc có đường', N'lon', 120, 28000, 1),
(N'Xoài cát', N'kg', 18.000, 45000, 2),
(N'Bơ sáp', N'kg', 12.500, 95000, 2),
(N'Chanh tươi', N'kg', 8.200, 32000, 2),
(N'Đào tươi', N'kg', 6.800, 68000, 2),
(N'7Up lon 330ml', N'lon', 240, 8500, 3),
(N'Trứng gà ta', N'quả', 150, 4500, 2),
(N'Cá basa fillet', N'kg', 22.000, 115000, 2),
(N'Rau muống', N'kg', 15.000, 18000, 2),
(N'Kem tươi', N'kg', 9.500, 125000, 2),
(N'Dừa tươi', N'quả', 35, 28000, 2);
GO
INSERT INTO PhieuNhap (MaNCC, MaNV, NgayNhap, TongTien, GhiChu) VALUES
(1, 1, '2026-05-20 08:30:00', 4500000, N'Nhập định kỳ đầu tháng'),
(2, 2, '2026-05-22 09:15:00', 1850000, N'Nhập trái cây tươi');
GO
INSERT INTO ChiTietPhieuNhap (MaPN, MaNL, SoLuong, DonGia) VALUES
(1, 1, 20, 180000),
(1, 2, 50, 28000),
(2, 3, 15, 45000),
(2, 4, 8, 95000),
(2, 5, 5, 32000);
GO
UPDATE NguyenLieu SET SoLuongTon = SoLuongTon + 20 WHERE MaNL = 1;
UPDATE NguyenLieu SET SoLuongTon = SoLuongTon + 50 WHERE MaNL = 2;
UPDATE NguyenLieu SET SoLuongTon = SoLuongTon + 15 WHERE MaNL = 3;
UPDATE NguyenLieu SET SoLuongTon = SoLuongTon + 8 WHERE MaNL = 4;
UPDATE NguyenLieu SET SoLuongTon = SoLuongTon + 5 WHERE MaNL = 5;
GO
INSERT INTO CongThuc (MaSP, MaNL, SoLuongCan) VALUES
(1, 1, 0.018),
(2, 1, 0.015),
(2, 2, 0.030),
(4, 11, 0.025),
(16, 3, 0.120),
(25, 9, 0.250);
GO
INSERT INTO HoaDon (MaBan, MaNV, MaKH, MaKM, NgayLap, TongTienTruocGiam, GiamGia, TongThanhToan, TrangThai, LoaiDon, TenKhach, SDT, GhiChu) VALUES
(1, 3, NULL, NULL, '2026-06-01 09:15:00', 153000, 0, 153000, N'Đã thanh toán', N'Tại quán', N'Khách lẻ', '0909999888', N'Khách quen'),
(NULL, 4, 1, 1, '2026-06-01 14:40:00', 206000, 20600, 185400, N'Đã thanh toán', N'Mang về', N'Nguyễn Thị Khách Hàng', '0987654321', NULL),
(5, 2, 3, NULL, '2026-06-02 10:05:00', 185000, 0, 185000, N'Chờ thanh toán', N'Tại quán', N'Lê Thị Hương', '0965432109', N'Khách đặt trước'),
(3, 5, 2, 2, '2026-06-02 11:20:00', 119000, 20000, 99000, N'Đã thanh toán', N'Tại quán', N'Trần Minh Tuấn', '0976543210', NULL),
(2, 3, 4, NULL, '2026-06-02 16:30:00', 260000, 0, 260000, N'Đã thanh toán', N'Tại quán', N'Phạm Quốc Bảo', '0954321098', N'Khách VIP'),
(6, 4, NULL, NULL, '2026-06-03 08:45:00', 76000, 0, 76000, N'Chờ thanh toán', N'Mang về', N'Khách vãng lai', '0911223344', NULL),
(4, 5, 5, 3, '2026-06-03 15:10:00', 175000, 26250, 148750, N'Đã thanh toán', N'Tại quán', N'Đặng Thị Mai', '0943210987', N'Happy hour');
GO
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia, GhiChu) VALUES
(1, 2, 2, 39000, N'Ít đá, nhiều sữa'),
(1, 25, 1, 75000, NULL),
(2, 4, 1, 58000, N'Size L'),
(2, 16, 2, 49000, N'Không đường'),
(2, 20, 2, 25000, NULL),
(3, 3, 2, 55000, N'Cafe Muối - ít muối'),
(3, 25, 1, 75000, NULL),
(4, 9, 1, 44000, NULL),
(4, 18, 1, 46000, N'Ít ngọt'),
(4, 26, 1, 29000, NULL),
(5, 5, 2, 62000, N'Latte Art'),
(5, 23, 1, 68000, N'Không đá'),
(5, 27, 1, 68000, NULL),
(6, 8, 1, 47000, NULL),
(6, 30, 1, 29000, NULL),
(7, 13, 2, 42000, N'Nhiều cam'),
(7, 17, 1, 52000, NULL),
(7, 28, 1, 39000, N'Không hành');
GO
UPDATE Ban SET TrangThai = N'Đang phục vụ' WHERE MaBan IN (1,2,3,4,5);
GO
INSERT INTO ThanhToan (MaHD, NgayThanhToan, SoTien, PhuongThuc, MaGiaoDich, TrangThai) VALUES
(1, '2026-06-01 09:30:00', 153000, N'Tiền mặt', NULL, N'Thành công'),
(2, '2026-06-01 14:45:00', 185400, N'Momo', N'MOMO202606011445001', N'Thành công'),
(4, '2026-06-02 11:35:00', 99000, N'Chuyển khoản', N'BIDV987654', N'Thành công'),
(5, '2026-06-02 17:00:00', 260000, N'Thẻ', N'VISA39281', N'Thành công'),
(7, '2026-06-03 15:40:00', 148750, N'ZaloPay', N'ZLP20260603154099', N'Thành công');
GO
INSERT INTO DatBan (MaBan, MaKH, TenKhach, SDT, NgayDat, GioBatDau, GioKetThuc, SoNguoi, TrangThai, GhiChu) VALUES
(5, 1, N'Nguyễn Thị Khách Hàng', '0987654321', '2026-06-03', '18:00', '20:00', 5, N'Đã xác nhận', N'Sinh nhật, cần bánh kem'),
(7, NULL, N'Hoàng Anh', '0911222333', '2026-06-04', '15:30', '17:30', 3, N'Đã xác nhận', NULL),
(6, 4, N'Phạm Quốc Bảo', '0954321098', '2026-06-05', '19:00', '21:30', 6, N'Đã xác nhận', N'Khách VIP');
GO
INSERT INTO DanhGia (MaSP, MaKH, HoTen, SoSao, BinhLuan, NgayDG, TrangThai) VALUES
(2, 1, N'Nguyễn Thị Khách Hàng', 5, N'Cà phê sữa ngon, đúng vị truyền thống, sẽ ủng hộ quán tiếp!', '2026-05-28 11:20:00', 1),
(4, 3, N'Lê Thị Hương', 4, N'Cà phê kem tươi rất độc đáo, hơi đắt nhưng đáng tiền', '2026-05-30 16:45:00', 1),
(16, NULL, N'Khách Google', 5, N'Sinh tố xoài mát lạnh, xoài chín rất ngon', '2026-06-01 08:10:00', 1),
(25, 4, N'Phạm Quốc Bảo', 5, N'Cà kho tộ ngon, ăn kèm cơm trắng rất hợp', '2026-06-02 12:30:00', 1),
(5, 2, N'Trần Minh Tuấn', 4, N'Latte art đẹp, cà phê không quá đắng', '2026-06-02 14:15:00', 1);
GO
UPDATE KhachHang SET HangThanhVien = N'Vàng' WHERE MaKH = 1;
UPDATE KhachHang SET HangThanhVien = N'Bạc' WHERE MaKH = 3;
UPDATE KhachHang SET HangThanhVien = N'Kim Cương' WHERE MaKH = 4;
UPDATE KhachHang SET HangThanhVien = N'Bạc' WHERE MaKH = 5;
GO
INSERT INTO LichSuHoatDong (LoaiHanhDong, MaThamChieu, BangThamChieu, MoTa, MaNV) VALUES
(N'Tạo_HĐ', 1, N'HoaDon', N'Tạo hóa đơn mới', 3),
(N'ThanhToan', 1, N'HoaDon', N'Thanh toán thành công - Tiền mặt', 3),
(N'Tạo_HĐ', 2, N'HoaDon', N'Tạo hóa đơn mới', 4),
(N'ThanhToan', 2, N'HoaDon', N'Thanh toán thành công - Momo', 4);
GO
CREATE OR ALTER VIEW vw_MenuSanPham AS
SELECT sp.MaSP, sp.TenSP, lsp.TenLoai, sp.Gia, sp.MoTa, sp.HinhAnh, sp.DonViTinh, sp.KichCo, sp.TrangThai
FROM SanPham sp
JOIN LoaiSanPham lsp ON sp.MaLoai = lsp.MaLoai
WHERE sp.TrangThai = 1 AND lsp.TrangThai = 1;
GO
CREATE OR ALTER VIEW vw_HoaDonChiTiet AS
SELECT hd.MaHD, hd.NgayLap, hd.LoaiDon, hd.TrangThai AS TrangThaiHoaDon, b.TenBan, nv.HoTen AS NhanVien,
ISNULL(kh.HoTen, hd.TenKhach) AS KhachHang, sp.TenSP, ct.SoLuong, ct.DonGia, ct.GhiChu AS GhiChuMon, ct.ThanhTien,
hd.TongTienTruocGiam, hd.GiamGia, hd.TongThanhToan
FROM HoaDon hd
LEFT JOIN Ban b ON hd.MaBan = b.MaBan
JOIN NhanVien nv ON hd.MaNV = nv.MaNV
LEFT JOIN KhachHang kh ON hd.MaKH = kh.MaKH
JOIN ChiTietHoaDon ct ON hd.MaHD = ct.MaHD
JOIN SanPham sp ON ct.MaSP = sp.MaSP;
GO
CREATE OR ALTER VIEW vw_DoanhThuTheoNgay AS
SELECT CAST(NgayLap AS DATE) AS Ngay, COUNT(MaHD) AS SoHoaDon, SUM(TongTienTruocGiam) AS TongDoanhThuTruocGiam,
SUM(GiamGia) AS TongGiamGia, SUM(TongThanhToan) AS TongThanhToan,
SUM(CASE WHEN TrangThai = N'Đã thanh toán' OR TrangThai = N'Hoàn thành' THEN TongThanhToan ELSE 0 END) AS DoanhThuThucTe
FROM HoaDon
GROUP BY CAST(NgayLap AS DATE);
GO
CREATE OR ALTER VIEW vw_TonKho AS
SELECT nl.MaNL, nl.TenNL, nl.DonViTinh, nl.SoLuongTon, nl.GiaNhapTrungBinh,
(nl.SoLuongTon * ISNULL(nl.GiaNhapTrungBinh, 0)) AS GiaTriTon, ncc.TenNCC
FROM NguyenLieu nl
LEFT JOIN NhaCungCap ncc ON nl.MaNCC = ncc.MaNCC;
GO
CREATE OR ALTER VIEW vw_BanTrong AS
SELECT MaBan, TenBan, KhuVuc, SoCho, TrangThai
FROM Ban
WHERE TrangThai = N'Trống';
GO
CREATE OR ALTER PROCEDURE sp_TaoHoaDon
    @MaBan INT = NULL,
    @MaNV INT,
    @MaKH INT = NULL,
    @MaKM INT = NULL,
    @LoaiDon NVARCHAR(20) = N'Tại quán',
    @TenKhach NVARCHAR(100) = NULL,
    @SDT VARCHAR(15) = NULL,
    @DiaChiGiao NVARCHAR(255) = NULL,
    @GhiChu NVARCHAR(500) = NULL,
    @MaHD INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO HoaDon (MaBan, MaNV, MaKH, MaKM, LoaiDon, TenKhach, SDT, DiaChiGiao, GhiChu, TrangThai)
    VALUES (@MaBan, @MaNV, @MaKH, @MaKM, @LoaiDon, @TenKhach, @SDT, @DiaChiGiao, @GhiChu, N'Chờ thanh toán');
    SET @MaHD = SCOPE_IDENTITY();
    IF @MaBan IS NOT NULL
    BEGIN
        UPDATE Ban SET TrangThai = N'Đang phục vụ' WHERE MaBan = @MaBan AND TrangThai = N'Trống';
    END
    IF @MaKM IS NOT NULL
        EXEC sp_ApDungKhuyenMai @MaHD, @MaKM;
    INSERT INTO LichSuHoatDong (LoaiHanhDong, MaThamChieu, BangThamChieu, MoTa, MaNV)
    VALUES (N'Tạo_HĐ', @MaHD, N'HoaDon', N'Tạo hóa đơn mới', @MaNV);
END;
GO
CREATE OR ALTER PROCEDURE sp_ThemChiTietHoaDon
    @MaHD INT,
    @MaSP INT,
    @SoLuong INT,
    @DonGia DECIMAL(18,2),
    @GhiChu NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia, GhiChu)
    VALUES (@MaHD, @MaSP, @SoLuong, @DonGia, @GhiChu);
END;
GO
CREATE OR ALTER PROCEDURE sp_ThanhToanHoaDon
    @MaHD INT,
    @PhuongThuc NVARCHAR(30),
    @SoTien DECIMAL(18,2),
    @MaGiaoDich NVARCHAR(100) = NULL,
    @MaNVThanhToan INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF NOT EXISTS (SELECT 1 FROM HoaDon WHERE MaHD = @MaHD AND TrangThai = N'Chờ thanh toán')
        BEGIN
            RAISERROR(N'Hóa đơn không tồn tại hoặc đã thanh toán/hủy.', 16, 1);
            RETURN;
        END
        DECLARE @TongThanhToan DECIMAL(18,2);
        SELECT @TongThanhToan = TongThanhToan FROM HoaDon WHERE MaHD = @MaHD;
        IF @SoTien < @TongThanhToan
        BEGIN
            RAISERROR(N'Số tiền thanh toán không đủ.', 16, 1);
            RETURN;
        END
        INSERT INTO ThanhToan (MaHD, SoTien, PhuongThuc, MaGiaoDich, TrangThai)
        VALUES (@MaHD, @SoTien, @PhuongThuc, @MaGiaoDich, N'Thành công');
        UPDATE HoaDon SET TrangThai = N'Đã thanh toán' WHERE MaHD = @MaHD;
        DECLARE @MaBan INT;
        SELECT @MaBan = MaBan FROM HoaDon WHERE MaHD = @MaHD;
        IF @MaBan IS NOT NULL
            UPDATE Ban SET TrangThai = N'Trống' WHERE MaBan = @MaBan;
        DECLARE @MaKH INT;
        SELECT @MaKH = MaKH FROM HoaDon WHERE MaHD = @MaHD;
        IF @MaKH IS NOT NULL
            UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + (CAST(@TongThanhToan AS INT) / 10000) WHERE MaKH = @MaKH;
        INSERT INTO LichSuHoatDong (LoaiHanhDong, MaThamChieu, BangThamChieu, MoTa, MaNV)
        VALUES (N'ThanhToan', @MaHD, N'HoaDon', N'Thanh toán thành công - ' + @PhuongThuc, @MaNVThanhToan);
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO
CREATE OR ALTER PROCEDURE sp_ThongKeDoanhThu
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(hd.NgayLap AS DATE) AS Ngay, COUNT(DISTINCT hd.MaHD) AS SoDon,
    SUM(hd.TongTienTruocGiam) AS DoanhThuGoc, SUM(hd.GiamGia) AS TongGiam,
    SUM(hd.TongThanhToan) AS DoanhThuThuc, AVG(hd.TongThanhToan) AS TrungBinhDon
    FROM HoaDon hd
    WHERE CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
      AND hd.TrangThai IN (N'Đã thanh toán', N'Hoàn thành')
    GROUP BY CAST(hd.NgayLap AS DATE)
    ORDER BY Ngay;
    SELECT COUNT(DISTINCT MaHD) AS TongSoDon, SUM(TongTienTruocGiam) AS TongDoanhThuGoc,
    SUM(GiamGia) AS TongGiamGia, SUM(TongThanhToan) AS TongDoanhThuThuc
    FROM HoaDon
    WHERE CAST(NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
      AND TrangThai IN (N'Đã thanh toán', N'Hoàn thành');
END;
GO
CREATE OR ALTER PROCEDURE sp_CapNhatTrangThaiBan
    @MaBan INT,
    @TrangThaiMoi NVARCHAR(30)
AS
BEGIN
    UPDATE Ban SET TrangThai = @TrangThaiMoi WHERE MaBan = @MaBan;
END;
GO
CREATE OR ALTER PROCEDURE sp_ApDungKhuyenMai
    @MaHD INT,
    @MaKM INT,
    @MaNV INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TongTien DECIMAL(18,2), @LoaiGiam NVARCHAR(20), @GiaTriGiam DECIMAL(18,2), @DieuKien DECIMAL(18,2);
    SELECT @TongTien = TongTienTruocGiam FROM HoaDon WHERE MaHD = @MaHD;
    SELECT @LoaiGiam = LoaiGiam, @GiaTriGiam = GiaTriGiam, @DieuKien = DieuKienToiThieu
    FROM KhuyenMai
    WHERE MaKM = @MaKM AND TrangThai = 1 AND GETDATE() BETWEEN NgayBatDau AND NgayKetThuc;
    IF @TongTien >= @DieuKien
    BEGIN
        DECLARE @GiamGia DECIMAL(18,2) = CASE
            WHEN @LoaiGiam = N'PhanTram' THEN @TongTien * @GiaTriGiam / 100
            ELSE @GiaTriGiam END;
        UPDATE HoaDon
        SET MaKM = @MaKM, GiamGia = @GiamGia, TongThanhToan = TongTienTruocGiam - @GiamGia
        WHERE MaHD = @MaHD;
        UPDATE KhuyenMai SET DaSuDung = DaSuDung + 1 WHERE MaKM = @MaKM;
        INSERT INTO LichSuHoatDong (LoaiHanhDong, MaThamChieu, BangThamChieu, MoTa, MaNV)
        VALUES (N'ApDung_KM', @MaHD, N'HoaDon', N'Áp dụng KM', @MaNV);
    END
END;
GO
CREATE OR ALTER TRIGGER trg_CapNhatTongTienHoaDon
ON ChiTietHoaDon
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AffectedHDs TABLE (MaHD INT);
    INSERT INTO @AffectedHDs (MaHD)
    SELECT DISTINCT MaHD FROM inserted
    UNION
    SELECT DISTINCT MaHD FROM deleted;
    UPDATE hd
    SET TongTienTruocGiam = ISNULL((SELECT SUM(ThanhTien) FROM ChiTietHoaDon WHERE MaHD = hd.MaHD), 0)
    FROM HoaDon hd
    WHERE hd.MaHD IN (SELECT MaHD FROM @AffectedHDs);
    UPDATE HoaDon
    SET TongThanhToan = TongTienTruocGiam - ISNULL(GiamGia, 0)
    WHERE MaHD IN (SELECT MaHD FROM @AffectedHDs);
END;
GO
CREATE OR ALTER TRIGGER trg_SanPham_NgayCapNhat
ON SanPham
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SanPham
    SET NgayCapNhat = GETDATE()
    FROM SanPham sp
    JOIN inserted i ON sp.MaSP = i.MaSP;
END;
GO
CREATE OR ALTER TRIGGER trg_CapNhatTonKho_Nhap
ON ChiTietPhieuNhap
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE nl
    SET SoLuongTon = nl.SoLuongTon + i.SoLuong,
        NgayCapNhat = GETDATE()
    FROM NguyenLieu nl
    JOIN inserted i ON nl.MaNL = i.MaNL;
END;
GO
CREATE OR ALTER TRIGGER trg_LogHuyHoaDon
ON HoaDon
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(TrangThai)
    BEGIN
        INSERT INTO LichSuHoatDong (LoaiHanhDong, MaThamChieu, BangThamChieu, MoTa, MaNV)
        SELECT N'Hủy_HĐ', i.MaHD, N'HoaDon',
        N'Hóa đơn bị hủy. Trạng thái cũ: ' + d.TrangThai + N' -> ' + i.TrangThai, i.MaNV
        FROM inserted i
        JOIN deleted d ON i.MaHD = d.MaHD
        WHERE i.TrangThai = N'Đã hủy' AND d.TrangThai <> N'Đã hủy';
    END
END;
GO
CREATE OR ALTER TRIGGER trg_TruTonKhoKhiThanhToan
ON HoaDon
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(TrangThai)
    BEGIN
        DECLARE @MaHD INT, @TrangThaiMoi NVARCHAR(30), @TrangThaiCu NVARCHAR(30);
        SELECT @MaHD = MaHD, @TrangThaiMoi = TrangThai FROM inserted;
        SELECT @TrangThaiCu = TrangThai FROM deleted WHERE MaHD = @MaHD;
        IF @TrangThaiMoi IN (N'Đã thanh toán', N'Hoàn thành') AND @TrangThaiCu NOT IN (N'Đã thanh toán', N'Hoàn thành')
        BEGIN
            UPDATE nl
            SET SoLuongTon = nl.SoLuongTon - (ct.SoLuong * c.SoLuongCan),
                NgayCapNhat = GETDATE()
            FROM NguyenLieu nl
            JOIN ChiTietHoaDon ct ON ct.MaHD = @MaHD
            JOIN CongThuc c ON c.MaSP = ct.MaSP AND c.MaNL = nl.MaNL;
        END
    END
END;
GO
CREATE OR ALTER FUNCTION fn_TinhHangThanhVien (@Diem INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @Hang NVARCHAR(20);
    IF @Diem >= 5000 SET @Hang = N'Kim Cương';
    ELSE IF @Diem >= 2000 SET @Hang = N'Vàng';
    ELSE IF @Diem >= 500 SET @Hang = N'Bạc';
    ELSE SET @Hang = N'Thường';
    RETURN @Hang;
END;
GO
UPDATE KhachHang SET HangThanhVien = dbo.fn_TinhHangThanhVien(DiemTichLuy);
GO