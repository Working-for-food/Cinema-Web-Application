using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Entities;
using Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterVm());
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username, // Identity саме перевірить унікальність
                Email = model.Email,       // Identity саме перевірить унікальність
                DateOfBirth = model.DateOfBirth
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // --- ОБРОБКА ПОМИЛОК ---
            foreach (var error in result.Errors)
            {
                // error.Description вже буде українською завдяки UkrainianIdentityErrorDescriber!

                switch (error.Code)
                {
                    case "DuplicateUserName":
                        // Помилка піде під поле "Ім'я користувача"
                        ModelState.AddModelError("Username", error.Description);
                        break;

                    case "DuplicateEmail":
                        // Помилка піде під поле "Електронна пошта"
                        ModelState.AddModelError("Email", error.Description);
                        break;

                    // (Додатково) Можна виводити помилки пароля прямо під полем пароля
                    case "PasswordTooShort":
                    case "PasswordRequiresDigit":
                    case "PasswordRequiresLower":
                    case "PasswordRequiresUpper":
                    case "PasswordRequiresNonAlphanumeric":
                        ModelState.AddModelError("Password", error.Description);
                        break;

                    default:
                        // Всі інші помилки (загальні) йдуть наверх сторінки
                        ModelState.AddModelError(string.Empty, error.Description);
                        break;
                }
            }
        }

        // Якщо щось пішло не так, повертаємо форму з даними та помилками
        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Невірна спроба входу (логін або пароль).");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Profile
    [HttpGet]
    [Authorize] 
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("User not found");

        var model = new ProfileVm
        {
            Username = user.UserName,
            Email = user.Email,
            IsEmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            TwoFactorEnabled = user.TwoFactorEnabled,
            Roles = await _userManager.GetRolesAsync(user),
            StatusMessage = TempData["StatusMessage"] as string
        };

        return View(model);
    }

    // POST: /Account/UpdateProfile
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileVm model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("User not found");

        if (!ModelState.IsValid)
        {
            model.Roles = await _userManager.GetRolesAsync(user);
            model.Username = user.UserName;
            model.Email = user.Email;
            model.TwoFactorEnabled = user.TwoFactorEnabled;
            return View("Profile", model);
        }

        user.PhoneNumber = model.PhoneNumber;
        user.DateOfBirth = model.DateOfBirth;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction("Profile");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.Roles = await _userManager.GetRolesAsync(user);
        return View("Profile", model);
    }

    // POST: /Account/Toggle2FA
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.TwoFactorEnabled = !user.TwoFactorEnabled;
        await _userManager.UpdateAsync(user);

        TempData["StatusMessage"] = user.TwoFactorEnabled
            ? "Two-Factor Authentication enabled."
            : "Two-Factor Authentication disabled.";

        return RedirectToAction("Profile");
    }

    // REMOTE VALIDATION: Email Check
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> VerifyEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            return Json($"Електронна пошта '{email}' вже використовується.");
        }

        return Json(true);
    }

    // REMOTE VALIDATION: Username Check
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> VerifyUsername(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user != null)
        {
            return Json($"Ім'я користувача '{username}' вже зайняте.");
        }

        return Json(true);
    }
}

