using FamilyLedger.Domain.Enums;

namespace FamilyLedger.Application.DTOs;

public record RegisterUserRequest(string DisplayName, string Email, string Password, bool IsSuperAdmin = false);
public record LoginRequest(string Email, string Password, Guid? ProfileId = null);
public record AuthTokenResponse(string AccessToken, Guid UserId, Guid? ActiveProfileId, string Role, bool IsSuperAdmin);

public record CreateProfileRequest(string Name, string Currency = "EUR");
public record ProfileSummary(Guid ProfileId, string Name, string Currency, MemberRole Role);
public record CreateProfileResponse(Guid ProfileId, string Name, string Currency);

public record AdminCreateProfileRequest(string Name, string Currency, Guid OwnerUserId);
public record AdminAssignMemberRequest(MemberRole Role);
public record AdminDashboardResponse(int TotalUsers, int TotalProfiles, int TotalMemberships, int TotalTransactions);
