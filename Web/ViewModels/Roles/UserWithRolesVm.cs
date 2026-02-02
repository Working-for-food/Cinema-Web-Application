namespace Web.ViewModels.Roles;

public class UserWithRolesVm
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; } 
}