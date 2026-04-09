using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FamilyLedger.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IMembershipRepository membershipRepository,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthTokenResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existing is not null) throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            DisplayName = request.DisplayName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = Hash(request.Password),
            IsSuperAdmin = request.IsSuperAdmin
        };

        await userRepository.AddAsync(user, ct);
        await userRepository.SaveChangesAsync(ct);

        var token = BuildToken(user, null, MemberRole.Viewer);
        return new AuthTokenResponse(token, user.Id, null, "viewer", user.IsSuperAdmin);
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (user.PasswordHash != Hash(request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        MemberRole role = MemberRole.Viewer;
        Guid? activeProfile = null;

        if (request.ProfileId.HasValue)
        {
            var membership = await membershipRepository.GetAsync(request.ProfileId.Value, user.Id, ct)
                ?? throw new UnauthorizedAccessException("User is not a member of this profile.");
            role = membership.Role;
            activeProfile = membership.ProfileId;
        }

        var token = BuildToken(user, activeProfile, role);
        return new AuthTokenResponse(token, user.Id, activeProfile, role.ToString().ToLowerInvariant(), user.IsSuperAdmin);
    }

    public async Task<CreateProfileResponse> CreateProfileForCurrentUserAsync(Guid userId, CreateProfileRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct) ?? throw new UnauthorizedAccessException("User not found.");

        var profile = new Profile { Name = request.Name, Currency = request.Currency };
        await profileRepository.AddAsync(profile, ct);

        await membershipRepository.AddAsync(new Member
        {
            ProfileId = profile.Id,
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = MemberRole.Owner
        }, ct);

        await userRepository.SaveChangesAsync(ct);
        return new CreateProfileResponse(profile.Id, profile.Name, profile.Currency);
    }

    public async Task<IReadOnlyList<ProfileSummary>> GetMyProfilesAsync(Guid userId, CancellationToken ct = default)
    {
        var memberships = await membershipRepository.GetByUserIdAsync(userId, ct);
        var result = new List<ProfileSummary>(memberships.Count);

        foreach (var membership in memberships)
        {
            var profile = await profileRepository.GetByIdAsync(membership.ProfileId, ct);
            result.Add(new ProfileSummary(
                membership.ProfileId,
                profile?.Name ?? "Unknown profile",
                profile?.Currency ?? "EUR",
                membership.Role));
        }

        return result;
    }

    public async Task<AuthTokenResponse> SwitchProfileAsync(Guid userId, Guid profileId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct) ?? throw new UnauthorizedAccessException("User not found.");
        var membership = await membershipRepository.GetAsync(profileId, userId, ct)
            ?? throw new UnauthorizedAccessException("Membership not found.");

        var token = BuildToken(user, profileId, membership.Role);
        return new AuthTokenResponse(token, user.Id, profileId, membership.Role.ToString().ToLowerInvariant(), user.IsSuperAdmin);
    }

    private string BuildToken(User user, Guid? profileId, MemberRole role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("role", role.ToString().ToLowerInvariant()),
            new("is_super_admin", user.IsSuperAdmin.ToString().ToLowerInvariant())
        };

        if (profileId.HasValue) claims.Add(new Claim("profileId", profileId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Hash(string input)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}
