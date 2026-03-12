using System;
using System.Reactive;
using ReactiveUI;

namespace KitBox.ViewModels;

public sealed class ManagerLoginViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    //Identifiants pré-décidés
    private const string AllowedEmail = "manager@kitbox.com";
    private const string AllowedPassword = "manager123";

    private string? _email;
    public string? Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    private string? _password;
    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ManagerLoginViewModel(MainViewModel main)
    {
        _main = main;

        LoginCommand = ReactiveCommand.Create(Login);
        CancelCommand = ReactiveCommand.Create(() => _main.NavigateTo(new HomeViewModel(_main)));
    }

    private void Login()
    {
        var emailOk = string.Equals(Email?.Trim(), AllowedEmail, StringComparison.OrdinalIgnoreCase);
        var pwdOk = string.Equals(Password ?? "", AllowedPassword, StringComparison.Ordinal);

        if (emailOk && pwdOk)
        {
            HasError = false;
            ErrorMessage = "";
            _main.NavigateTo(new ManagerDashboardViewModel(_main));
            return;
        }

        // Cases mis à vide + message
        Email = "";
        Password = "";
        HasError = true;
        ErrorMessage = "Email ou mot de passe incorrect. Veuillez réessayer.";
    }
}