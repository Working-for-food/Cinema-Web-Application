using Microsoft.AspNetCore.Identity;

namespace Web.Helpers;

public class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new IdentityError { Code = nameof(DefaultError), Description = "Сталася невідома помилка." };

    public override IdentityError ConcurrencyFailure()
        => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Помилка оптимістичної конкуренції, об'єкт було змінено." };

    public override IdentityError PasswordMismatch()
        => new IdentityError { Code = nameof(PasswordMismatch), Description = "Невірний пароль." };

    public override IdentityError InvalidToken()
        => new IdentityError { Code = nameof(InvalidToken), Description = "Невірний токен." };

    public override IdentityError LoginAlreadyAssociated()
        => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Цей користувач вже має прив'язаний акаунт." };

    public override IdentityError InvalidUserName(string userName)
        => new IdentityError { Code = nameof(InvalidUserName), Description = $"Ім'я користувача '{userName}' містить недопустимі символи. Дозволені лише літери та цифри." };

    public override IdentityError InvalidEmail(string email)
        => new IdentityError { Code = nameof(InvalidEmail), Description = $"Email '{email}' є недійсним." };

    public override IdentityError DuplicateUserName(string userName)
        => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Ім'я користувача '{userName}' вже зайняте." };

    public override IdentityError DuplicateEmail(string email)
        => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' вже використовується." };

    public override IdentityError InvalidRoleName(string role)
        => new IdentityError { Code = nameof(InvalidRoleName), Description = $"Ім'я ролі '{role}' є недійсним." };

    public override IdentityError DuplicateRoleName(string role)
        => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Роль '{role}' вже існує." };

    public override IdentityError UserAlreadyHasPassword()
        => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "У користувача вже встановлено пароль." };

    public override IdentityError UserLockoutNotEnabled()
        => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Блокування для цього користувача не ввімкнено." };

    public override IdentityError UserAlreadyInRole(string role)
        => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Користувач вже має роль '{role}'." };

    public override IdentityError UserNotInRole(string role)
        => new IdentityError { Code = nameof(UserNotInRole), Description = $"Користувач не має ролі '{role}'." };

    public override IdentityError PasswordTooShort(int length)
        => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Пароль повинен містити щонайменше {length} символів." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль повинен містити хоча б один спецсимвол (!, @, #, тощо)." };

    public override IdentityError PasswordRequiresDigit()
        => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Пароль повинен містити хоча б одну цифру ('0'-'9')." };

    public override IdentityError PasswordRequiresLower()
        => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Пароль повинен містити хоча б одну малу літеру ('a'-'z')." };

    public override IdentityError PasswordRequiresUpper()
        => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Пароль повинен містити хоча б одну велику літеру ('A'-'Z')." };
}