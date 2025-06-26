using FluentValidation;
using Healink.Application.Common.DTOs.Staff;

namespace Healink.Application.Features.Staff.Commands.CreateStaffProfile;

/// <summary>
/// Validator for CreateStaffProfileDto
/// </summary>
public class CreateStaffProfileCommandValidator : AbstractValidator<CreateStaffProfileDto>
{
    public CreateStaffProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[\+]?[0-9\s\-\(\)]{10,15}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.Now.AddYears(-16))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Staff member must be at least 16 years old.");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.Position)
            .MaximumLength(100).WithMessage("Position cannot exceed 100 characters.");

        RuleFor(x => x.EmployeeId)
            .MaximumLength(20).WithMessage("Employee ID cannot exceed 20 characters.");

        RuleFor(x => x.HireDate)
            .LessThanOrEqualTo(DateTime.Now)
            .When(x => x.HireDate.HasValue)
            .WithMessage("Hire date cannot be in the future.");

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Salary.HasValue)
            .WithMessage("Salary must be non-negative.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");
    }
} 