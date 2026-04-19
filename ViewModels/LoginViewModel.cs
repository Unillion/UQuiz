using System;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.ViewModels.Base;
using UQuiz.models.enums;
using UQuiz.services;
using UQuiz.models.interfaces;
using UQuiz.views;
using UQuiz.Views;
using UQuiz.models.users;

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
                OpenMainWindow(user, parameter as Window);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                IsLoading = false;
            }
        }

        private void ExecuteRegister(object parameter)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();

            if (parameter is Window currentWindow)
            {
                currentWindow.Close();
            }
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
            switch (user.UserType)
            {
                case UserType.Teacher:
                    var teacherWindow = new TeacherWindow((Teacher)user);
                    currentWindow?.Close();
                    teacherWindow.Show();
                    break;
                case UserType.Organization:
                    var orgWindow = new OrganizationWindow((Organization)user);
                    currentWindow?.Close();
                    orgWindow.Show();
                    break;
                case UserType.RegularUser:
                    var studentWindow = new StudentWindow((RegularUser)user);
                    currentWindow?.Close();
                    studentWindow.Show();
                    break;
            }
            currentWindow?.Close();
        }
    }
}