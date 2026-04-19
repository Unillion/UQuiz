using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.models.users;
using UQuiz.services;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels
{
    public class StudentViewModel : ViewModelBase
    {
        private readonly RegularUser _student;
        private readonly ISurveyService _surveyService;
        private readonly IUserService _userService;
        private string _pageTitle;
        private string _selectedMenu;
        private ObservableCollection<StudentSurveyCardViewModel> _availableSurveys;
        private ObservableCollection<StudentSurveyCardViewModel> _completedSurveys;
        private ObservableCollection<StudentRequestViewModel> _pendingRequests;
        private ObservableCollection<TeacherItemViewModel> _myTeachers;
        private string _studentClass;
        private string _profileMessage;
        private bool _isProfileLoading;
        public string FullName { get; }


        public StudentViewModel(RegularUser student)
        {
            _student = student;
            _surveyService = new SurveyService();
            _userService = new UserService();

            FullName = student.FullName;
            StudentEmail = student.Email;

            NavigateCommand = new RelayCommand(ExecuteNavigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            StartSurveyCommand = new RelayCommand(ExecuteStartSurvey);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            MaximizeCommand = new RelayCommand(ExecuteMaximize);
            CloseCommand = new RelayCommand(ExecuteClose);

            AvailableSurveys = new ObservableCollection<StudentSurveyCardViewModel>();
            CompletedSurveys = new ObservableCollection<StudentSurveyCardViewModel>();

            PendingRequests = new ObservableCollection<StudentRequestViewModel>();
            AcceptRequestCommand = new RelayCommand(ExecuteAcceptRequest);
            RejectRequestCommand = new RelayCommand(ExecuteRejectRequest);

            MyTeachers = new ObservableCollection<TeacherItemViewModel>();
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile, CanExecuteSaveProfile);
            LoadProfileData();


            SelectedMenu = "Available";
            LoadSurveys();
        }

        public string StudentName { get; }
        public string StudentEmail { get; }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }
        public ObservableCollection<TeacherItemViewModel> MyTeachers
        {
            get => _myTeachers;
            set => SetProperty(ref _myTeachers, value);
        }

        public string SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (SetProperty(ref _selectedMenu, value))
                    UpdatePageForMenu(value);
            }
        }
        private bool CanExecuteSaveProfile(object parameter) => !IsProfileLoading;

        public ObservableCollection<StudentRequestViewModel> PendingRequests
        {
            get => _pendingRequests;
            set => SetProperty(ref _pendingRequests, value);
        }

        public ICommand AcceptRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }


        public ObservableCollection<StudentSurveyCardViewModel> AvailableSurveys
        {
            get => _availableSurveys;
            set => SetProperty(ref _availableSurveys, value);
        }

        public ObservableCollection<StudentSurveyCardViewModel> CompletedSurveys
        {
            get => _completedSurveys;
            set => SetProperty(ref _completedSurveys, value);
        }

        private StudentSurveyCardViewModel _selectedSurvey;
        public StudentSurveyCardViewModel SelectedSurvey
        {
            get => _selectedSurvey;
            set => SetProperty(ref _selectedSurvey, value);
        }
        public string StudentClass
        {
            get => _studentClass;
            set => SetProperty(ref _studentClass, value);
        }

        public string ProfileMessage
        {
            get => _profileMessage;
            set => SetProperty(ref _profileMessage, value);
        }

        public bool IsProfileLoading
        {
            get => _isProfileLoading;
            set => SetProperty(ref _isProfileLoading, value);
        }

        public ICommand SaveProfileCommand { get; }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand StartSurveyCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }

        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string menu)
            {
                SelectedMenu = menu;
            }
        }

        private void ExecuteLogout(object parameter)
        {
            new LoginWindow().Show();
            (parameter as Window)?.Close();
        }

        private void ExecuteStartSurvey(object parameter)
        {
            if (SelectedSurvey != null)
            {
                MessageBox.Show($"Начинаем опрос: {SelectedSurvey.Title}",
                    "Опрос", MessageBoxButton.OK, MessageBoxImage.Information);
                // Здесь будет открытие окна прохождения опроса
            }
        }

        private void ExecuteMinimize(object parameter)
        {
            if (parameter is Window window)
                window.WindowState = WindowState.Minimized;
        }

        private void ExecuteMaximize(object parameter)
        {
            if (parameter is Window window)
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal : WindowState.Maximized;
        }

        private void ExecuteClose(object parameter)
        {
            if (parameter is Window window)
                window.Close();
            else
                Application.Current.Shutdown();
        }

        private void UpdatePageForMenu(string menu)
        {
            switch (menu)
            {
                case "Available":
                    PageTitle = "Доступные опросы";
                    break;
                case "Completed":
                    PageTitle = "Пройденные опросы";
                    break;
                case "Teachers":
                    PageTitle = "Мои учителя";
                    LoadMyTeachers();
                    break;
                case "Requests":
                    PageTitle = "Входящие заявки";
                    LoadPendingRequests();
                    break;
                case "Profile":
                    PageTitle = "Профиль";
                    break;
            }
        }

        private void ExecuteAcceptRequest(object parameter)
        {
            if (parameter is StudentRequestViewModel request)
            {
                try
                {
                    _userService.AcceptStudentRequest(request.Id);
                    LoadPendingRequests();
                    MessageBox.Show($"Заявка от {request.TeacherName} принята", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteRejectRequest(object parameter)
        {
            if (parameter is StudentRequestViewModel request)
            {
                try
                {
                    _userService.RejectStudentRequest(request.Id);
                    LoadPendingRequests();
                    MessageBox.Show($"Заявка от {request.TeacherName} отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadMyTeachers()
        {
            MyTeachers.Clear();
            var teachers = _userService.GetTeachersByStudent(_student.Id);
            foreach (var t in teachers)
            {
                MyTeachers.Add(new TeacherItemViewModel
                {
                    Id = t.Id,
                    Name = t.FullName,
                    Email = t.Email,
                    Subject = t.Subject
                });
            }
        }

        private void ExecuteSaveProfile(object parameter)
        {
            IsProfileLoading = true;
            ProfileMessage = string.Empty;

            try
            {
                _userService.UpdateStudentProfile(_student.Id, StudentClass);
                ProfileMessage = "Данные успешно сохранены!";
                MessageBox.Show("Профиль обновлён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ProfileMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProfileLoading = false;
            }
        }

        private void LoadProfileData()
        {
            try
            {
                var profile = _userService.GetStudentProfile(_student.Id);
                StudentClass = profile.Class;
            }
            catch
            {
                StudentClass = string.Empty;
            }
        }

        private void LoadPendingRequests()
        {
            PendingRequests.Clear();
            var requests = _userService.GetPendingRequestsForStudent(_student.Id);
            foreach (var r in requests)
            {
                PendingRequests.Add(new StudentRequestViewModel
                {
                    Id = r.Id,
                    TeacherName = r.TeacherName,
                    TeacherEmail = r.TeacherEmail,
                    OrganizationName = r.OrganizationName,
                    CreatedDate = r.CreatedDate
                });
            }
        }

        private void LoadSurveys()
        {
            AvailableSurveys.Clear();
            AvailableSurveys.Add(new StudentSurveyCardViewModel
            {
                Id = 1,
                Title = "Математика 10 класс",
                Description = "Контрольная работа по алгебре",
                QuestionsCount = 15,
                CreatedDate = "10.04.2026",
                TeacherName = "Иванов И.И."
            });
            AvailableSurveys.Add(new StudentSurveyCardViewModel
            {
                Id = 2,
                Title = "История России",
                Description = "Тест по эпохе Петра I",
                QuestionsCount = 20,
                CreatedDate = "08.04.2026",
                TeacherName = "Петрова А.С."
            });

            CompletedSurveys.Clear();
            CompletedSurveys.Add(new StudentSurveyCardViewModel
            {
                Id = 3,
                Title = "Физика 9 класс",
                Description = "Законы Ньютона",
                QuestionsCount = 10,
                CreatedDate = "05.04.2026",
                TeacherName = "Сидоров В.П.",
                Score = "8/10"
                });
            }
        }


    

    public class StudentSurveyCardViewModel : ViewModelBase
    {
        private int _id;
        private string _title;
        private string _description;
        private int _questionsCount;
        private string _createdDate;
        private string _teacherName;
        private string _score;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public int QuestionsCount { get => _questionsCount; set => SetProperty(ref _questionsCount, value); }
        public string CreatedDate { get => _createdDate; set => SetProperty(ref _createdDate, value); }
        public string TeacherName { get => _teacherName; set => SetProperty(ref _teacherName, value); }
        public string Score { get => _score; set => SetProperty(ref _score, value); }
    }
   
}