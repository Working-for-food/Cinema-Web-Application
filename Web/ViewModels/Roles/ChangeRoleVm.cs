using Microsoft.AspNetCore.Identity;

namespace Web.ViewModels.Roles;

public class ChangeRoleVm
{
    public string UserId { get; set; } 
    public string UserEmail { get; set; } 
    public string Username { get; set; } 
    public List<IdentityRole> AllRoles { get; set; } = new();
    public IList<string> UserRoles { get; set; } = new List<string>();
}