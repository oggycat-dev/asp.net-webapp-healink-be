# Load Environment Variables for Healink API
Write-Host "Loading Healink environment variables..." -ForegroundColor Green

# Database Configuration
$env:CONNECTION_STRING = "Server=(localdb)\mssqllocaldb;Database=HealinkDb;Trusted_Connection=true;MultipleActiveResultSets=true"

# JWT Configuration
$env:JWT_SECRET = "HealinkSuperSecretKeyForJWTTokenGenerationAndValidation12345"
$env:JWT_SECRET_KEY = "HealinkSuperSecretKeyForJWTTokenGenerationAndValidation12345"
$env:JWT_ISSUER = "Healink"
$env:JWT_AUDIENCE = "HealinkUsers" 
$env:JWT_EXPIRES_IN_MINUTES = "60"

# Admin Account Configuration
$env:ADMIN_EMAIL = "admin@healink.com"
$env:ADMIN_PASSWORD = "Admin123!"
$env:ADMIN_USERNAME = "admin"

# CORS Configuration
$env:FRONTEND_URL = "http://localhost:3000"
$env:CORS_ALLOW_ANY_ORIGIN = "true"
$env:CORS_ALLOWED_ORIGINS = "http://localhost:3000,http://localhost:4200,https://localhost:3000,https://localhost:4200"

# Environment
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Storage Configuration (Local by default)
$env:STORAGE_PROVIDER_TYPE = "LocalStorage"
$env:LOCAL_STORAGE_ROOT_PATH = "wwwroot/uploads"
$env:LOCAL_STORAGE_MAX_FILE_SIZE = "10485760"
$env:LOCAL_STORAGE_ALLOWED_EXTENSIONS = ".jpg,.jpeg,.png,.gif,.bmp,.pdf,.doc,.docx,.xls,.xlsx,.txt"

# Security
$env:REQUIRE_HTTPS = "false"

Write-Host "Environment variables loaded successfully!" -ForegroundColor Green
Write-Host "You can now run: dotnet run" -ForegroundColor Yellow 