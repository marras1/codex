using FamilyLedger.Application.DTOs;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Enums;

namespace FamilyLedger.Application.Interfaces;

public interface IAuthService
{
    Task<AuthTokenResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
    Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<CreateProfileResponse> CreateProfileForCurrentUserAsync(Guid userId, CreateProfileRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProfileSummary>> GetMyProfilesAsync(Guid userId, CancellationToken ct = default);
    Task<AuthTokenResponse> SwitchProfileAsync(Guid userId, Guid profileId, CancellationToken ct = default);
}

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken ct = default);
    Task<CreateProfileResponse> CreateProfileAsync(AdminCreateProfileRequest request, CancellationToken ct = default);
    Task AssignMemberAsync(Guid profileId, Guid userId, MemberRole role, CancellationToken ct = default);
    Task RevokeMemberAsync(Guid profileId, Guid userId, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Profile profile, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}

public interface IMembershipRepository
{
    Task<Member?> GetAsync(Guid profileId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Member>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
    Task RemoveAsync(Member member, CancellationToken ct = default);
}

public interface IAdminStatsRepository
{
    Task<int> CountUsersAsync(CancellationToken ct = default);
    Task<int> CountProfilesAsync(CancellationToken ct = default);
    Task<int> CountMembershipsAsync(CancellationToken ct = default);
    Task<int> CountTransactionsAsync(CancellationToken ct = default);
}
