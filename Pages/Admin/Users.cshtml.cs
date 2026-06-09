using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebShop.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserRow> Users { get; private set; } = [];
    public string StatusMessage { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostMakeAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            StatusMessage = "User not found.";
            await LoadUsersAsync();
            return Page();
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        StatusMessage = "User updated to admin.";
        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            StatusMessage = "User not found.";
            await LoadUsersAsync();
            return Page();
        }

        var isLastAdmin = await IsLastAdminAsync(user);
        if (isLastAdmin)
        {
            StatusMessage = "Cannot remove admin role from the last admin account.";
            await LoadUsersAsync();
            return Page();
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        StatusMessage = "Admin role removed.";
        await LoadUsersAsync();
        return Page();
    }

    private async Task<bool> IsLastAdminAsync(IdentityUser targetUser)
    {
        if (!await _userManager.IsInRoleAsync(targetUser, "Admin"))
        {
            return false;
        }

        var adminCount = 0;
        foreach (var user in _userManager.Users)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                adminCount += 1;
            }
        }

        return adminCount <= 1;
    }

    private async Task LoadUsersAsync()
    {
        var list = new List<UserRow>();

        foreach (var user in _userManager.Users.OrderBy(u => u.Email))
        {
            list.Add(new UserRow(
                user.Id,
                user.Email ?? user.UserName ?? "(no email)",
                await _userManager.IsInRoleAsync(user, "Admin"),
                await _userManager.IsInRoleAsync(user, "User")
            ));
        }

        Users = list;
    }

    public record UserRow(string Id, string Email, bool IsAdmin, bool IsUser);
}
