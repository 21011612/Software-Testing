-- Chạy trên DB TooruCoffee (bổ sung cho AdminOrderService)
USE TooruCoffee;
GO

CREATE OR ALTER PROCEDURE sp_ApDungKhuyenMai
    @MaHD INT,
    @MaKM INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TrangThai NVARCHAR(30), @TongTruocGiam DECIMAL(18,2);
    SELECT @TrangThai = TrangThai, @TongTruocGiam = TongTienTruocGiam FROM HoaDon WHERE MaHD = @MaHD;
    IF @TrangThai IS NULL
    BEGIN RAISERROR(N'Hóa đơn không tồn tại.', 16, 1); RETURN; END
    IF @TrangThai <> N'Chờ thanh toán'
    BEGIN RAISERROR(N'Chỉ áp dụng KM cho hóa đơn chờ thanh toán.', 16, 1); RETURN; END

    DECLARE @LoaiGiam NVARCHAR(20), @GiaTriGiam DECIMAL(18,2), @DKTT DECIMAL(18,2);
    DECLARE @NgayBatDau DATE, @NgayKetThuc DATE, @SoLuongToiDa INT, @DaSuDung INT, @TrangThaiKM BIT;
    SELECT @LoaiGiam = LoaiGiam, @GiaTriGiam = GiaTriGiam, @DKTT = DieuKienToiThieu,
           @NgayBatDau = NgayBatDau, @NgayKetThuc = NgayKetThuc,
           @SoLuongToiDa = SoLuongToiDa, @DaSuDung = DaSuDung, @TrangThaiKM = TrangThai
    FROM KhuyenMai WHERE MaKM = @MaKM;

    IF @LoaiGiam IS NULL
    BEGIN RAISERROR(N'Khuyến mãi không tồn tại.', 16, 1); RETURN; END
    IF @TrangThaiKM = 0 OR CAST(GETDATE() AS DATE) NOT BETWEEN @NgayBatDau AND @NgayKetThuc
    BEGIN RAISERROR(N'Khuyến mãi không còn hiệu lực.', 16, 1); RETURN; END
    IF @DaSuDung >= @SoLuongToiDa
    BEGIN RAISERROR(N'Khuyến mãi đã hết lượt.', 16, 1); RETURN; END
    IF @TongTruocGiam < @DKTT
    BEGIN RAISERROR(N'Đơn chưa đủ điều kiện tối thiểu.', 16, 1); RETURN; END

    DECLARE @Giam DECIMAL(18,2);
    IF @LoaiGiam = N'PhanTram'
        SET @Giam = ROUND(@TongTruocGiam * @GiaTriGiam / 100.0, 0);
    ELSE
        SET @Giam = @GiaTriGiam;
    IF @Giam > @TongTruocGiam SET @Giam = @TongTruocGiam;

    UPDATE HoaDon
    SET MaKM = @MaKM, GiamGia = @Giam, TongThanhToan = TongTienTruocGiam - @Giam
    WHERE MaHD = @MaHD;

    UPDATE KhuyenMai SET DaSuDung = DaSuDung + 1 WHERE MaKM = @MaKM;
END;
GO

CREATE OR ALTER PROCEDURE sp_HuyHoaDon
    @MaHD INT,
    @MaNV INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TrangThai NVARCHAR(30), @MaBan INT;
    SELECT @TrangThai = TrangThai, @MaBan = MaBan FROM HoaDon WHERE MaHD = @MaHD;
    IF @TrangThai IS NULL
    BEGIN RAISERROR(N'Hóa đơn không tồn tại.', 16, 1); RETURN; END
    IF @TrangThai <> N'Chờ thanh toán'
    BEGIN RAISERROR(N'Chỉ hủy được hóa đơn chờ thanh toán.', 16, 1); RETURN; END

    UPDATE HoaDon SET TrangThai = N'Đã hủy', MaKM = NULL, GiamGia = 0, TongThanhToan = TongTienTruocGiam
    WHERE MaHD = @MaHD;

    IF @MaBan IS NOT NULL
        UPDATE Ban SET TrangThai = N'Trống' WHERE MaBan = @MaBan AND TrangThai = N'Đang phục vụ';
END;
GO