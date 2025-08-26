using MediatR;
using Microsoft.Extensions.Logging;
using Healink.Application.Common.DTOs.Auth;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Auth.Commands.Register;

/// <summary>
/// Enhanced handler for register command using Unit of Work pattern
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserRegistrationResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserRegistrationResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting user registration with Unit of Work for: {Username} with email: {Email}", 
                request.Username, request.Email);

            // Start coordinated transaction across both contexts
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Check if passwords match
                if (request.Password != request.ConfirmPassword)
                {
                    _logger.LogWarning("Password confirmation mismatch for user: {Username}", request.Username);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<UserRegistrationResponse>.Failure("Password and confirmation password do not match", ErrorCode.ValidationFailed);
                }

                // Check if user already exists
                var existingUser = await _identityService.FindUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed - user already exists with email: {Email}", request.Email);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<UserRegistrationResponse>.Failure("A user with this email already exists", ErrorCode.DuplicateEntry);
                }

                // Check if username already exists
                var existingUserByUsername = await _identityService.FindUserByUsernameAsync(request.Username);
                if (existingUserByUsername != null)
                {
                    _logger.LogWarning("Registration failed - username already exists: {Username}", request.Username);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<UserRegistrationResponse>.Failure("Username is already taken", ErrorCode.DuplicateEntry);
                }

                // Create the user (this will affect ApplicationDbContext)
                var registrationResult = await _identityService.CreateUserAsync(
                    request.Username, 
                    request.Email, 
                    request.Password,
                    request.PhoneNumber);

                if (!registrationResult.IsSuccess)
                {
                    _logger.LogWarning("User registration failed for: {Username}. Reason: {Error}", 
                        request.Username, registrationResult.Message);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<UserRegistrationResponse>.Failure(
                        registrationResult.Message ?? "User registration failed", 
                        ErrorCode.BadRequest);
                }

                var user = registrationResult.Data!;

                // Assign default role (Patient or User) - this affects ApplicationDbContext
                await _identityService.AddToRoleAsync(user, "Patient");

                // Save application context changes (Identity data) explicitly
                await _unitOfWork.SaveApplicationChangesAsync(cancellationToken);

                // If you have any business logic that affects BusinessContext, do it here
                // For example: creating related profile data
                // await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit both transactions
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var response = new UserRegistrationResponse
                {
                    UserId = user.Id,
                    Username = user.UserName!,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = user.EmailConfirmed,
                    IsSuccess = true,
                    Message = "User registered successfully with Unit of Work coordination"
                };

                _logger.LogInformation("User registered successfully with Unit of Work: {Username} with ID: {UserId}", 
                    request.Username, user.Id);

                return Result<UserRegistrationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transaction for user: {Username}", request.Username);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while registering user with Unit of Work: {Username}", request.Username);
            return Result<UserRegistrationResponse>.Failure("An error occurred during registration", ErrorCode.InternalServerError);
        }
    }
}
