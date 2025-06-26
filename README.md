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

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

