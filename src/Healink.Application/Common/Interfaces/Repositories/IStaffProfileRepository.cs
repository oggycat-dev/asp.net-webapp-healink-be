using Healink.Domain.Entities;
using Healink.Domain.Enums;

namespace Healink.Application.Common.Interfaces.Repositories;

/// <summary>
/// Repository interface for StaffProfile entity
/// </summary>
public interface IStaffProfileRepository : IGenericRepository<StaffProfile>
{
    /// <summary>
    /// Get staff profile by AppUser ID
    /// </summary>
    Task<StaffProfile?> GetByAppUserIdAsync(string appUserId);
    
    /// <summary>
    /// Get staff profile by license number
    /// </summary>
    Task<StaffProfile?> GetByLicenseNumberAsync(string licenseNumber);
    
    /// <summary>
    /// Get staff profile by email
    /// </summary>
    Task<StaffProfile?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Get filtered staff profiles with pagination
    /// </summary>
    Task<List<StaffProfile>> GetFilteredAsync(
        string? searchTerm = null,
        string? department = null,
        string? position = null,
        EntityStatus? status = null,
        string? sortBy = null,
        bool isAscending = true,
        int pageNumber = 1,
        int pageSize = 10);
    
    /// <summary>
    /// Get staff profiles by department
    /// </summary>
    Task<List<StaffProfile>> GetByDepartmentAsync(string department);
    
    /// <summary>
    /// Search staff profiles
    /// </summary>
    Task<List<StaffProfile>> SearchAsync(string searchTerm, int limit = 50);
    
    /// <summary>
    /// Get total count of staff profiles
    /// </summary>
    Task<int> GetTotalCountAsync();
    
    /// <summary>
    /// Check if license number exists
    /// </summary>
    Task<bool> LicenseNumberExistsAsync(string licenseNumber);
    
    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
} 