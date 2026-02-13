using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Entities;
using Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
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
                UserName = model.Username,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, code = token },
                    protocol: HttpContext.Request.Scheme);

                await _emailService.SendEmailAsync(model.Email, "Підтвердження реєстрації",
                    $"Будь ласка, підтвердіть ваш акаунт, натиснувши <a href='{callbackUrl}'>тут</a>.");

                return View("RegisterConfirmation");
            }

            foreach (var error in result.Errors)
            {
                string errorMessage = error.Description;
                switch (error.Code)
                {
                    case "DuplicateUserName":
                        errorMessage = $"Ім'я користувача '{model.Username}' вже зайняте.";
                        ModelState.AddModelError("Username", errorMessage);
                        break;

                    case "DuplicateEmail":
                        errorMessage = $"Електронна пошта '{model.Email}' вже використовується.";
                        ModelState.AddModelError("Email", errorMessage);
                        break;

                    case "PasswordTooShort":
                        errorMessage = "Пароль має містити щонайменше 8 символів.";
                        ModelState.AddModelError("Password", errorMessage);
                        break;
                    case "PasswordRequiresDigit":
                        errorMessage = "Пароль повинен містити хоча б одну цифру ('0'-'9').";
                        ModelState.AddModelError("Password", errorMessage);
                        break;
                    case "PasswordRequiresLower":
                        errorMessage = "Пароль повинен містити хоча б одну малу літеру ('a'-'z').";
                        ModelState.AddModelError("Password", errorMessage);
                        break;
                    case "PasswordRequiresUpper":
                        errorMessage = "Пароль повинен містити хоча б одну велику літеру ('A'-'Z').";
                        ModelState.AddModelError("Password", errorMessage);
                        break;
                    case "PasswordRequiresNonAlphanumeric":
                        errorMessage = "Пароль повинен містити хоча б один спеціальний символ.";
                        ModelState.AddModelError("Password", errorMessage);
                        break;

                    default:
                        ModelState.AddModelError(string.Empty, errorMessage);
                        break;
                }
            }
        }

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

                return RedirectToAction("Index", "Afisha");
            }

            if (result.IsNotAllowed)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    ViewData["ShowEmailConfirmationError"] = true;
                    ModelState.AddModelError(string.Empty, "Ви не підтвердили електронну пошту. Перевірте вашу скриньку.");
                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, "Невірна спроба входу (неправильний логін або пароль).");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Afisha");
    }

    // GET: /Account/ChangePassword
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST: /Account/ChangePassword
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVm model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.NewPassword == model.CurrentPassword)
        {
            ModelState.AddModelError("NewPassword", "Новий пароль не може співпадати з поточним.");
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Ваш пароль успішно змінено.";
            return RedirectToAction("Profile");
        }

        foreach (var error in result.Errors)
        {
            string errorMessage = error.Description;
            if (error.Code == "PasswordMismatch") errorMessage = "Неправильний поточний пароль.";

            ModelState.AddModelError(string.Empty, errorMessage);
        }

        return View(model);
    }

    private string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts[0].Length <= 2) return $"*@{parts[1]}";
        return $"{parts[0].Substring(0, 2)}***{parts[0].Substring(parts[0].Length - 1)}@{parts[1]}";
    }

    // GET: /Account/ForgotPassword
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    // POST: /Account/ForgotPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
    {
        if (!ModelState.IsValid) return View(model);

        if (string.Equals(model.Username.Trim(), "superadmin", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Супер-адміністратор не може відновлювати пароль через цю форму. Зверніться до розробника.");
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.Username);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Користувача з таким логіном не знайдено.");
            return View(model);
        }

        if (!(await _userManager.IsEmailConfirmedAsync(user)))
        {
            ModelState.AddModelError(string.Empty, "Електронна пошта цього акаунту не підтверджена. Відновлення неможливе.");
            return View(model);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action("ResetPassword", "Account",
            new { userId = user.Id, code = token }, protocol: HttpContext.Request.Scheme);

        await _emailService.SendEmailAsync(user.Email, "Відновлення пароля",
            $"Для скидання пароля натисніть <a href='{callbackUrl}'>тут</a>.");

        return View("ForgotPasswordConfirmation", MaskEmail(user.Email));
    }

    // GET: /Account/ResetPassword
    [HttpGet]
    public IActionResult ResetPassword(string userId, string code)
    {
        return userId == null || code == null ? View("Error") : View(new ResetPasswordVm { UserId = userId, Code = code });
    }

    // POST: /Account/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Паролі не співпадають.");
            return View(model);
        }

        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return RedirectToAction("ResetPasswordConfirmation");

        if (await _userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Новий пароль не може бути таким самим, як старий.");
            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

        if (result.Succeeded)
        {
            return View("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            string msg = error.Description;
            if (error.Code.Contains("Password")) msg = "Пароль недостатньо складний (див. вимоги).";

            ModelState.AddModelError(string.Empty, msg);
        }

        return View(model);
    }

    // GET: /Account/Profile
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Користувача не знайдено");

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

        bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");
        string adminMode = HttpContext.Session.GetString("AdminMode");

        if (isAdmin && adminMode != "user")
        {
            return View("AdminProfile", model);
        }

        return View(model);
    }

    // POST: /Account/UpdateProfile
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileVm model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Користувача не знайдено");

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
            TempData["StatusMessage"] = "Ваш профіль успішно оновлено.";
            return RedirectToAction("Profile");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.Roles = await _userManager.GetRolesAsync(user);

        if (User.IsInRole("admin"))
        {
            return View("AdminProfile", model);
        }

        return View("Profile", model);
    }

    // POST: /Account/ToggleAdminMode
    [HttpPost]
    [Authorize(Roles = "admin")]
    public IActionResult ToggleAdminMode()
    {
        var currentMode = HttpContext.Session.GetString("AdminMode");

        if (currentMode == "user")
        {
            HttpContext.Session.Remove("AdminMode");
        }
        else
        {
            HttpContext.Session.SetString("AdminMode", "user");
        }
        return Redirect(Request.Headers["Referer"].ToString());
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

    // Handles the link click from the email
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Не вдалося знайти користувача з ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            return View("ConfirmEmail");
        }
        else
        {
            return View("Error");
        }
    }

    [HttpPost]
    [Authorize]
    [IgnoreAntiforgeryToken] 
    public async Task<IActionResult> CheckCurrentPassword(string password)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(false);

        bool isCorrect = await _userManager.CheckPasswordAsync(user, password);

        return Json(isCorrect);
    }

}