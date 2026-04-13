using System;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.ViewModels.Base;
using UQuiz.models.enums;
using UQuiz.services;
using UQuiz.models.interfaces;
using UQuiz.views;

namespace UQuiz.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private string _email;
        private string _password;
        private UserType _selectedUserType;
        private string _errorMessage;
        private bool _isLoading;

        public LoginViewModel()
        {
            _userService = new UserService();

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            CloseCommand = new RelayCommand(ExecuteClose);

            SelectedUserType = UserType.Teacher;
            Email = "teacher@uquiz.edu";
            Password = "12345678";
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

        public UserType SelectedUserType
        {
            get => _selectedUserType;
            set => SetProperty(ref _selectedUserType, value);
        }

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

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                var user = _userService.Login(Email, Password, SelectedUserType);

                string userTypeText = GetUserTypeText(user.UserType);

                MessageBox.Show($"Вход выполнен успешно!\n\n" +
                              $"Тип: {userTypeText}\n" +
                              $"Логин: {user.Login}\n" +
                              $"Email: {user.Email}\n" +
                              $"Дата регистрации: {user.RegistrationDate:dd.MM.yyyy}",
                              "Успешный вход",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                OpenMainWindow(user, parameter as Window);
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

        private void ExecuteRegister(object parameter)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }

        private void ExecuteMinimize(object parameter)
        {
            if (parameter is Window window)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void ExecuteClose(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            else
            {
                Application.Current.Shutdown();
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

        private void OpenMainWindow(User user, Window currentWindow)
        {
            // Позже создадим разные окна для разных типов пользователей
            switch (user.UserType)
            {
                case UserType.Teacher:
                    MessageBox.Show("Открытие окна учителя", "Учитель", MessageBoxButton.OK, MessageBoxImage.Information);
                    // var teacherWindow = new TeacherWindow(user);
                    // teacherWindow.Show();
                    break;
                case UserType.Organization:
                    MessageBox.Show("Открытие окна организации", "Организация", MessageBoxButton.OK, MessageBoxImage.Information);
                    // var orgWindow = new OrganizationWindow(user);
                    // orgWindow.Show();
                    break;
                case UserType.RegularUser:
                    MessageBox.Show("Открытие окна ученика", "Ученик", MessageBoxButton.OK, MessageBoxImage.Information);
                    // var studentWindow = new StudentWindow(user);
                    // studentWindow.Show();
                    break;
            }

            currentWindow?.Close();
        }
    }
}