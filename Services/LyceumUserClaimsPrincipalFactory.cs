using System.Security.Claims;
using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Lyceum.Services;

public class LyceumUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
{
    public LyceumUserClaimsPrincipalFactory(
        UserManager<User> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        // Add role from the custom enum — Identity's default factory reads from AspNetUserRoles
        // which is unused in this project; roles live on User.Role instead.
        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        return identity;
    }
}
