using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Enums;

namespace FamilyLedger.Application.Services;

public sealed class AdminService(
    IAdminStatsRepository statsRepository,
    IProfileRepository profileRepository,
    IUserRepository userRepository,
    IMembershipRepository membershipRepository) : IAdminService
{
    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken ct = default)
        => new(
            await statsRepository.CountUsersAsync(ct),
            await statsRepository.CountProfilesAsync(ct),
            await statsRepository.CountMembershipsAsync(ct),
            await statsRepository.CountTransactionsAsync(ct));

    public async Task<CreateProfileResponse> CreateProfileAsync(AdminCreateProfileRequest request, CancellationToken ct = default)
    {
        var owner = await userRepository.GetByIdAsync(request.OwnerUserId, ct) ?? throw new InvalidOperationException("Owner user not found.");
        var profile = new Profile { Name = request.Name, Currency = request.Currency };

        await profileRepository.AddAsync(profile, ct);
        await membershipRepository.AddAsync(new Member
        {
            ProfileId = profile.Id,
            UserId = owner.Id,
            DisplayName = owner.DisplayName,
            Email = owner.Email,
            Role = MemberRole.Owner
        }, ct);

        await userRepository.SaveChangesAsync(ct);
        return new CreateProfileResponse(profile.Id, profile.Name, profile.Currency);
    }

    public async Task AssignMemberAsync(Guid profileId, Guid userId, MemberRole role, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("User not found.");
        var existing = await membershipRepository.GetAsync(profileId, userId, ct);

        if (existing is null)
        {
            await membershipRepository.AddAsync(new Member
            {
                ProfileId = profileId,
                UserId = userId,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = role
            }, ct);
        }
        else
        {
            existing.Role = role;
        }

        await userRepository.SaveChangesAsync(ct);
    }

    public async Task RevokeMemberAsync(Guid profileId, Guid userId, CancellationToken ct = default)
    {
        var existing = await membershipRepository.GetAsync(profileId, userId, ct)
            ?? throw new InvalidOperationException("Membership not found.");
        await membershipRepository.RemoveAsync(existing, ct);
        await userRepository.SaveChangesAsync(ct);
    }
}
