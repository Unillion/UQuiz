using System;
using System.Windows;
using System.Windows.Input;
using UQuiz.ViewModels.Base;
using UQuiz.commands;
using UQuiz.models.enums;
using UQuiz.services;

namespace UQuiz.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private string _fullNameOrOrgName;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private UserType _selectedUserType;
        private string _errorMessage;
        private bool _isLoading;

        public RegisterViewModel()
        {
            _userService = new UserService();

            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            CancelCommand = new RelayCommand(ExecuteCancel);
            SelectedUserType = UserType.Teacher;
        }

        public string FullNameOrOrgName
        {
            get => _fullNameOrOrgName;
            set
            {
                if (SetProperty(ref _fullNameOrOrgName, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public UserType SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                if (SetProperty(ref _selectedUserType, value))
                {
                    OnPropertyChanged(nameof(FieldLabel));
                    OnPropertyChanged(nameof(IsOrganization));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string FieldLabel
        {
            get
            {
                switch (SelectedUserType)
                {
                    case UserType.Teacher:
                    case UserType.RegularUser:
                        return "ФИО";
                    case UserType.Organization:
                        return "Название организации";
                    default:
                        return "Имя";
                }
            }
        }

        public bool IsOrganization => SelectedUserType == UserType.Organization;

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        private bool CanExecuteRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(FullNameOrOrgName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   Email.Contains("@") &&
                   Password.Length >= 8 &&
                   !IsLoading;
        }

        private void ExecuteRegister(object parameter)
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Пароли не совпадают";
                    return;
                }

                var user = _userService.Register(FullNameOrOrgName, Email, Password, SelectedUserType);

                string userTypeText = GetUserTypeText(user.UserType);

                MessageBox.Show($"Регистрация успешна!\n\n" +
                              $"{FieldLabel}: {FullNameOrOrgName}\n" +
                              $"Email: {user.Email}\n" +
                              $"Тип: {userTypeText}\n" +
                              $"Дата регистрации: {user.RegistrationDate:dd.MM.yyyy}",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                if (parameter is Window window)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }

        private string GetUserTypeText(UserType userType)
        {
            switch (userType)
            {
                case UserType.Teacher:
                    return "Учитель";
                case UserType.Organization:
                    return "Организация";
                case UserType.RegularUser:
                    return "Ученик";
                default:
                    return "Неизвестно";
            }
        }
    }
}