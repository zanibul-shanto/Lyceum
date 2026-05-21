using System.Security.Claims;
using Lyceum.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Lyceum.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly UserService _userService;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ProtectedLocalStorage protectedLocalStorage, UserService userService)
    {
        _protectedLocalStorage = protectedLocalStorage;
        _userService = userService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<string>("userSession");
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                var username = result.Value;
                var allUsers = await _userService.GetAllUsersAsync();
                var user = allUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    var identity = CreateIdentity(user);
                    _currentUser = new ClaimsPrincipal(identity);
                    return new AuthenticationState(_currentUser);
                }
            }
        }
        catch
        {
            // ProtectedLocalStorage will throw an exception during pre-rendering, which is expected.
        }

        return new AuthenticationState(_currentUser);
    }

    public async Task MarkUserAsAuthenticated(User user)
    {
        try
        {
            await _protectedLocalStorage.SetAsync("userSession", user.Username);
        }
        catch
        {
            // Handle if storage isn't accessible
        }

        var identity = CreateIdentity(user);
        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync("userSession");
        }
        catch
        {
        }

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    private ClaimsIdentity CreateIdentity(User user)
    {
        return new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("FullName", user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("RoleDisplay", user.Role.ToString())
        }, "LyceumAuth");
    }
}

