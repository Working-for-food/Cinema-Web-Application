using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Entities;
using Web.ViewModels.Roles;

namespace Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public IActionResult Index() => View(_roleManager.Roles.ToList());

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View((object)name);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        IdentityRole role = await _roleManager.FindByIdAsync(id);
        if (role != null)
        {
            await _roleManager.DeleteAsync(role);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> UserList()
    {
        string superAdminEmail = _configuration["SuperAdmin:Email"];
        var users = _userManager.Users.ToList();
        var model = new List<UserWithRolesVm>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new UserWithRolesVm
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Roles = roles
            });
        }

        var sortedModel = model
            .OrderByDescending(u => u.Email == superAdminEmail)
            .ThenBy(u => u.Username)
            .ToList();

        return View(sortedModel);
    }

    public async Task<IActionResult> Edit(string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            string superAdminEmail = _configuration["SuperAdmin:Email"];
            if (user.Email == superAdminEmail)
            {
                return RedirectToAction("UserList");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();

            ChangeRoleVm model = new ChangeRoleVm
            {
                UserId = user.Id,
                UserEmail = user.Email ?? "",
                UserRoles = userRoles,
                Username = user.UserName,
                AllRoles = allRoles
            };
            return View(model);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string userId, List<string> roles)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            string superAdminEmail = _configuration["SuperAdmin:Email"];
            if (user.Email == superAdminEmail)
            {
                return RedirectToAction("UserList");
            }

            if (roles.Contains("admin") && !roles.Contains("user"))
            {
                roles.Add("user");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var addedRoles = roles.Except(userRoles);
            var removedRoles = userRoles.Except(roles);

            await _userManager.AddToRolesAsync(user, addedRoles);
            await _userManager.RemoveFromRolesAsync(user, removedRoles);

            return RedirectToAction("UserList");
        }

        return NotFound();
    }

    // GET: /Roles/RoleEdit/{id}
    [HttpGet]
    public async Task<IActionResult> RoleEdit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();
        return View(role);
    }

    // POST: /Roles/RoleEdit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleEdit(string id, string name)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        if (!string.IsNullOrEmpty(name))
        {
            role.Name = name;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData["Success"] = "Назву ролі успішно змінено.";
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(role);
    }
}