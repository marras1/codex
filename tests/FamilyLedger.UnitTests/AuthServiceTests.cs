using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Application.Services;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FamilyLedger.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IProfileRepository> _profiles = new();
    private readonly Mock<IMembershipRepository> _memberships = new();
    private readonly IAuthService _sut;

    public AuthServiceTests()
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "12345678901234567890123456789012",
            ["Jwt:Issuer"] = "familyledger-api",
            ["Jwt:Audience"] = "familyledger-clients"
        }).Build();

        _sut = new AuthService(_users.Object, _profiles.Object, _memberships.Object, cfg);
    }

    [Fact]
    public async Task Register_CreatesUserAndReturnsToken()
    {
        _users.Setup(x => x.GetByEmailAsync("new@example.com", default)).ReturnsAsync((User?)null);

        var res = await _sut.RegisterAsync(new RegisterUserRequest("New", "new@example.com", "Pass1234!"));

        Assert.NotNull(res.AccessToken);
        _users.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Once);
    }

    [Fact]
    public async Task Login_WithProfile_MintsProfileScopedToken()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var user = new User { Id = userId, Email = "a@a.com", DisplayName = "A", PasswordHash = "A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3" };
        _users.Setup(x => x.GetByEmailAsync("a@a.com", default)).ReturnsAsync(user);
        _memberships.Setup(x => x.GetAsync(profileId, userId, default))
            .ReturnsAsync(new Member { ProfileId = profileId, UserId = userId, Role = MemberRole.Owner });

        var res = await _sut.LoginAsync(new LoginRequest("a@a.com", "123", profileId));

        Assert.Equal(profileId, res.ActiveProfileId);
        Assert.Equal("owner", res.Role);
    }
}
