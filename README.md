# Healink - Healthcare Management System


## 🏗️ Kiến trúc

Dự án được tổ chức theo Clean Architecture với 4 layers:

```
src/
├── Healink.Domain/           # Domain Layer - Entities, Enums, Interfaces
├── Healink.Application/      # Application Layer - Business Logic, DTOs, Features
├── Healink.Infrastructure/   # Infrastructure Layer - Data Access, External Services
└── Healink.API/              # Presentation Layer - Controllers, API Endpoints
```

## 🛠️ Công nghệ sử dụng

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server với Entity Framework Core 8.0.13
- **Authentication**: ASP.NET Core Identity
- **Documentation**: Swagger/OpenAPI
- **Patterns**: 
  - Clean Architecture
  - CQRS với MediatR
  - Repository Pattern
  - Unit of Work Pattern

## 📦 Packages chính

- **MediatR** - CQRS Pattern
- **AutoMapper** - Object Mapping
- **FluentValidation** - Input Validation
- **Entity Framework Core** - Data Access
- **ASP.NET Core Identity** - Authentication & Authorization

## 🚀 Bắt đầu

### Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Instance)
- Visual Studio 2022 hoặc VS Code

### Cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd asp.net-webapp-healink-be
```

2. **Restore packages**
```bash
dotnet restore
```

3. **Cập nhật connection string**
   - Mở `src/Healink.API/appsettings.json`
   - Cập nhật `DefaultConnection` theo môi trường của bạn

4. **Tạo database**
```bash
dotnet ef database update --project src/Healink.Infrastructure --startup-project src/Healink.API
```

5. **Chạy ứng dụng**
```bash
dotnet run --project src/Healink.API
```

6. **Truy cập ứng dụng**
   - API: `https://localhost:7xxx` hoặc `http://localhost:5xxx`
   - Swagger UI: `https://localhost:7xxx/swagger`

## 👥 Tài khoản mặc định

Hệ thống sẽ tự động tạo tài khoản admin khi khởi động:

- **Email**: admin@healink.com
- **Password**: Admin123!
- **Role**: Admin

## 🔗 API Endpoints

### Staff Management

- `GET /api/staff` - Lấy danh sách staff (với phân trang và tìm kiếm)
- `POST /api/staff` - Tạo staff mới

#### Tạo Staff mới

```json
POST /api/staff
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@healink.com",
  "phoneNumber": "0123456789",
  "dateOfBirth": "1990-01-01",
  "gender": 1,
  "address": "123 Main St",
  "department": "IT",
  "position": "Developer",
  "employeeId": "EMP001",
  "hireDate": "2023-01-01",
  "password": "Password123!"
}
```

#### Lấy danh sách Staff

```
GET /api/staff?pageNumber=1&pageSize=10&searchTerm=john&department=IT&status=1
```

## 🗄️ Cấu trúc Database

### Bảng chính

- **AspNetUsers** - Thông tin đăng nhập (Identity)
- **AspNetRoles** - Phân quyền hệ thống
- **StaffProfiles** - Thông tin chi tiết staff

### Roles

- **Admin**: Quản trị viên hệ thống
- **Staff**: Nhân viên

## 🔧 Cấu hình

### JWT Settings (appsettings.json)

```json
{
  "Jwt": {
    "Key": "your-secret-key-here-must-be-at-least-32-characters-long",
    "Issuer": "Healink.API",
    "Audience": "Healink.Client",
    "DurationInMinutes": 60,
    "RefreshTokenDurationInDays": 7
  }
}
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HealinkDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## 📝 Migrations

### Tạo migration mới
```bash
dotnet ef migrations add MigrationName --project src/Healink.Infrastructure --startup-project src/Healink.API
```

### Cập nhật database
```bash
dotnet ef database update --project src/Healink.Infrastructure --startup-project src/Healink.API
```

### Xóa migration
```bash
dotnet ef migrations remove --project src/Healink.Infrastructure --startup-project src/Healink.API
```

## 🏃 Development

### Build solution
```bash
dotnet build
```

### Run với watch mode
```bash
dotnet watch run --project src/Healink.API
```

### Run tests
```bash
dotnet test
```

## 📋 TODO

- [ ] Thêm Authentication JWT
- [ ] Thêm Authorization middleware
- [ ] Thêm logging
- [ ] Thêm unit tests
- [ ] Thêm integration tests
- [ ] Thêm API versioning
- [ ] Thêm health checks

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 📞 Liên hệ

Dự án được tạo ra cho mục đích học tập và nghiên cứu. 