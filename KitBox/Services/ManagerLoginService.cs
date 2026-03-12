namespace KitBox.Services;

public static class ManagerAuthService
{
    public static bool Validate(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return false;

        return email.Trim().ToLowerInvariant() == "manager@kitbox.com"
               && password == "manager123";
    }
}