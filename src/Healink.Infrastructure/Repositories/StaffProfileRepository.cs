using Microsoft.EntityFrameworkCore;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Domain.Entities;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Repositories;

/// <summary>
/// Staff profile repository implementation
/// </summary>
public class StaffProfileRepository : GenericRepository<StaffProfile>, IStaffProfileRepository
{
    private readonly HealinkDbContext _context;

    public StaffProfileRepository(HealinkDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Get staff profile by AppUser ID
    /// </summary>
    public async Task<StaffProfile?> GetByAppUserIdAsync(string appUserId)
    {
        return await _context.StaffProfiles
            .Include(s => s.AppUser)
            .FirstOrDefaultAsync(s => s.AppUserId == appUserId);
    }

    /// <summary>
    /// Get staff profiles by department
    /// </summary>
    public async Task<List<StaffProfile>> GetByDepartmentAsync(string department)
    {
        return await _context.StaffProfiles
            .Include(s => s.AppUser)
            .Where(s => s.Department == department && s.Status == EntityStatus.Active)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    /// <summary>
    /// Get active staff profiles with pagination
    /// </summary>
    public async Task<(List<StaffProfile> Items, int TotalCount)> GetActiveStaffPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null)
    {
        var query = _context.StaffProfiles
            .Where(s => s.Status == EntityStatus.Active);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(s => 
                s.FirstName.Contains(searchTerm) || 
                s.LastName.Contains(searchTerm) ||
                s.Email.Contains(searchTerm) ||
                (s.Department != null && s.Department.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<StaffProfile?> GetByEmailAsync(string email)
    {
        return await _context.StaffProfiles
            .Include(s => s.AppUser)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<StaffProfile?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _context.StaffProfiles
            .Include(s => s.AppUser)
            .FirstOrDefaultAsync(s => s.LicenseNumber == licenseNumber);
    }

    public async Task<List<StaffProfile>> GetFilteredAsync(
        string? searchTerm = null,
        string? department = null,
        string? position = null,
        EntityStatus? status = null,
        string? sortBy = null,
        bool isAscending = true,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.StaffProfiles
            .Include(s => s.AppUser)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(s => 
                s.FirstName.ToLower().Contains(lowerSearchTerm) ||
                s.LastName.ToLower().Contains(lowerSearchTerm) ||
                s.Email.ToLower().Contains(lowerSearchTerm) ||
                (s.LicenseNumber != null && s.LicenseNumber.ToLower().Contains(lowerSearchTerm)));
        }

        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(s => s.Department != null && s.Department.Contains(department));
        }

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(s => s.Position != null && s.Position.Contains(position));
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "firstname" => isAscending ? query.OrderBy(s => s.FirstName) : query.OrderByDescending(s => s.FirstName),
            "lastname" => isAscending ? query.OrderBy(s => s.LastName) : query.OrderByDescending(s => s.LastName),
            "email" => isAscending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
            "department" => isAscending ? query.OrderBy(s => s.Department) : query.OrderByDescending(s => s.Department),
            "position" => isAscending ? query.OrderBy(s => s.Position) : query.OrderByDescending(s => s.Position),
            "joindate" => isAscending ? query.OrderBy(s => s.JoinDate) : query.OrderByDescending(s => s.JoinDate),
            "createdat" => isAscending ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
            _ => isAscending ? query.OrderBy(s => s.FirstName).ThenBy(s => s.LastName) : query.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName)
        };

        // Apply pagination
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<StaffProfile>> SearchAsync(string searchTerm, int limit = 50)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.StaffProfiles
            .Include(s => s.AppUser)
            .Where(s => s.FirstName.ToLower().Contains(lowerSearchTerm) ||
                       s.LastName.ToLower().Contains(lowerSearchTerm) ||
                       s.Email.ToLower().Contains(lowerSearchTerm) ||
                       (s.LicenseNumber != null && s.LicenseNumber.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.StaffProfiles.CountAsync();
    }

    public async Task<bool> LicenseNumberExistsAsync(string licenseNumber)
    {
        return await _context.StaffProfiles
            .AnyAsync(s => s.LicenseNumber == licenseNumber);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.StaffProfiles
            .AnyAsync(s => s.Email == email);
    }
} 