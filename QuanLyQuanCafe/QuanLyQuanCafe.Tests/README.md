# QuanLyQuanCafe.Tests

Bộ kiểm thử Unit Test cho **6 chức năng cốt lõi** của hệ thống QuanLyQuanCafe.

## Kết quả kiểm thử

| STT | Tên chức năng | Pass | Fail | Untested | N/A | Tổng test case |
|-----|---------------|------|------|----------|-----|----------------|
| 1 | Đăng ký tài khoản (RegistrationService) | 10 | 0 | 0 | 0 | 10 |
| 2 | Đăng nhập (AuthService) | 10 | 0 | 0 | 0 | 10 |
| 3 | Quản lý giỏ hàng (CartService) | 10 | 0 | 0 | 0 | 10 |
| 4 | Đánh giá sản phẩm (DanhGiaService) | 10 | 0 | 0 | 0 | 10 |
| 5 | Tạo và xem hóa đơn (OrderService) | 9 | 0 | 0 | 0 | 9 |
| 6 | Phương thức thanh toán (PaymentService) | 10 | 0 | 0 | 0 | 10 |
| | **Tổng cộng** | **59** | **0** | **0** | **0** | **59** |

## File test tương ứng

| File | Service | Số test |
|------|---------|---------|
| `RegistrationServiceTests.cs` | RegistrationService | 10 |
| `AuthServiceTests.cs` | AuthService | 10 |
| `CartServiceTests.cs` | CartService | 10 |
| `DanhGiaServiceTests.cs` | DanhGiaService | 10 |
| `OrderServiceTests.cs` | OrderService | 9 |
| `PaymentServiceTests.cs` | PaymentService | 10 |

## Công nghệ

- **xUnit** — Framework test
- **EF Core InMemory** — Database giả lập
- **Moq** — Mock IHttpContextAccessor

## Cách chạy

```bash
cd "WebQuanLyQuanCaPhe/QuanLyQuanCafe"

dotnet test QuanLyQuanCafe.Tests/QuanLyQuanCafe.Tests.csproj --filter "FullyQualifiedName~RegistrationServiceTests|FullyQualifiedName~AuthServiceTests|FullyQualifiedName~CartServiceTests|FullyQualifiedName~DanhGiaServiceTests|FullyQualifiedName~OrderServiceTests|FullyQualifiedName~PaymentServiceTests"
```