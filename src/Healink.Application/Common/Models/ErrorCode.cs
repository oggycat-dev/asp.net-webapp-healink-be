namespace Healink.Application.Common.Models;

/// <summary>
/// Enumeration of error codes used throughout the application
/// </summary>
public enum ErrorCode
{
    // General errors
    InternalError = 1000,
    InternalServerError = 1000, // Alias for InternalError
    ValidationFailed = 1001,
    InvalidInput = 1002,
    NotFound = 1003,
    Unauthorized = 1004,
    Forbidden = 1005,
    
    // Authentication & Authorization errors
    InvalidCredentials = 2001,
    InvalidToken = 2002,
    TokenExpired = 2003,
    
    // User-related errors
    UserNotFound = 3001,
    UserAlreadyExists = 3002,
    AccountLocked = 3003,
    AccountDeactivated = 3004,
    
    // Data-related errors
    DuplicateEntry = 4001,
    ResourceConflict = 4002,
    ConcurrencyError = 4003,
    
    // Database errors
    DatabaseError = 5001,
    DatabaseConnectionError = 5002,
    DatabaseTimeout = 5003,
    
    // File & Storage errors
    FileNotFound = 6001,
    FileUploadError = 6002,
    StorageError = 6003,
    FileSizeExceeded = 6004,
    InvalidFileFormat = 6005,
    
    // External service errors
    ExternalServiceError = 7001,
    NetworkError = 7002,
    ServiceUnavailable = 7003,
    
    // Business rule errors
    BusinessRuleViolation = 8001,
    InsufficientPermissions = 8002,
    OperationNotAllowed = 8003,
    
    // Staff-related errors
    StaffNotFound = 9001,
    StaffAlreadyExists = 9002,
    InvalidStaffOperation = 9003,
    
    // Healthcare-specific errors
    PatientNotFound = 10001,
    AppointmentNotFound = 10002,
    MedicalRecordNotFound = 10003,
    InvalidMedicalData = 10004
} 