# Environment Configuration Guide

## Cấu hình Environment Variables cho Healink Project

Project này hỗ trợ cấu hình thông qua file `.env` hoặc system environment variables. Dưới đây là các biến môi trường cần thiết:

## Required Environment Variables

### Database Configuration
```
CONNECTION_STRING=Server=localhost,1433;Database=HealinkDb;User Id=sa;Password=Healink123!;TrustServerCertificate=true;Encrypt=false;
```

### JWT Configuration
```
JWT_SECRET_KEY=HealinkSuperSecretKeyForJWTTokenGenerationAndValidation12345
JWT_ISSUER=Healink
JWT_AUDIENCE=HealinkUsers
JWT_EXPIRY_MINUTES=60
```

### Admin Account Configuration
```
ADMIN_EMAIL=admin@healink.com
ADMIN_PASSWORD=Admin123!
ADMIN_USERNAME=admin
```

### Storage Configuration
```
STORAGE_PROVIDER_TYPE=LocalStorage
STORAGE_BASE_URL=https://localhost:7041
```

### Local Storage Settings
```
LOCAL_STORAGE_ROOT_PATH=wwwroot/uploads
LOCAL_STORAGE_MAX_FILE_SIZE=10485760
LOCAL_STORAGE_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.pdf,.doc,.docx,.epub
```

### Logging Configuration
```
LOG_LEVEL_DEFAULT=Information
LOG_LEVEL_ASPNETCORE=Warning
```

### CORS Configuration
```
ALLOWED_HOSTS=*
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:4200,https://localhost:3000,https://localhost:4200
```

## Optional Cloud Storage Settings

### Amazon S3 Settings
```
AWS_ACCESS_KEY=your-access-key
AWS_SECRET_KEY=your-secret-key
AWS_BUCKET_NAME=healink-bucket
AWS_REGION=us-east-1
AWS_USE_HTTPS=true
AWS_MAX_FILE_SIZE=52428800
AWS_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.pdf,.doc,.docx,.epub
```

### Azure Blob Settings
```
AZURE_CONNECTION_STRING=your-azure-connection-string
AZURE_CONTAINER_NAME=healink-files
AZURE_MAX_FILE_SIZE=52428800
AZURE_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.pdf,.doc,.docx,.epub
```

## Cách sử dụng

### Option 1: Tạo file .env (Recommended)
1. Tạo file `.env` ở root directory của project
2. Copy tất cả các variables ở trên vào file `.env`
3. Chỉnh sửa các giá trị phù hợp với môi trường của bạn

### Option 2: System Environment Variables
Set các biến môi trường trực tiếp trong hệ thống hoặc trong IDE của bạn.

### Option 3: appsettings.json (Fallback)
Nếu không có environment variables, hệ thống sẽ sử dụng giá trị từ `appsettings.json`

## Features được hỗ trợ

- **Environment Configuration**: Đọc cấu hình từ file .env hoặc system environment
- **Storage Options**: LocalStorage, Amazon S3, Azure Blob Storage
- **JWT Authentication**: Cấu hình JWT token với các thông số tùy chỉnh
- **CORS Configuration**: Cấu hình CORS origins động
- **Logging**: Cấu hình log levels
- **Admin Account**: Auto-create admin account khi khởi động

## Lưu ý bảo mật

- **KHÔNG** commit file `.env` vào Git repository
- Sử dụng secret management tools trong production
- Thay đổi JWT_SECRET_KEY trong production
- Sử dụng strong passwords cho admin account 