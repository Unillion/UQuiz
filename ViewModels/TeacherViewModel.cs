using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.models.users;
using UQuiz.services;
using UQuiz.ViewModels.Base;
using UQuiz.views;

namespace UQuiz.ViewModels
{
    public class TeacherViewModel : ViewModelBase
    {
        private readonly Teacher _teacher;
        private readonly IUserService _userService;
        private string _pageTitle;
        private bool _isCreateButtonVisible;
        private string _selectedMenu;
        private ObservableCollection<SurveyCardViewModel> _surveys;
        private string _phone;
        private string _subject;
        private string _profileMessage;
        private bool _isProfileLoading;
        private readonly ISurveyService _surveyService;
        private ObservableCollection<OrganizationRequestViewModel> _pendingRequests;
        private ObservableCollection<OrganizationItemViewModel> _myOrganizations;
        private string _searchStudentEmail;
        private ObservableCollection<StudentItemViewModel> _allStudents;
        private ObservableCollection<StudentItemViewModel> _myStudents;
        private ObservableCollection<StudentItemViewModel> _studentSearchResults;
        private ObservableCollection<StudentRequestViewModel> _sentStudentRequests;
        private OrganizationItemViewModel _selectedOrganizationForRequest;
        public ICommand AcceptRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }
        public ICommand OpenSurveyCommand { get; }

        public TeacherViewModel(Teacher teacher)
        {
            _teacher = teacher;
            _userService = new UserService();
            _surveyService = new SurveyService();

            Surveys = new ObservableCollection<SurveyCardViewModel>();
            TeacherName = teacher.Login;
            TeacherEmail = teacher.Email;

            NavigateCommand = new RelayCommand(ExecuteNavigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            CreateSurveyCommand = new RelayCommand(ExecuteCreateSurvey);
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile, CanExecuteSaveProfile);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            MaximizeCommand = new RelayCommand(ExecuteMaximize);
            CloseCommand = new RelayCommand(ExecuteClose);
            AcceptRequestCommand = new RelayCommand(ExecuteAcceptRequest);
            RejectRequestCommand = new RelayCommand(ExecuteRejectRequest);
            PendingRequests = new ObservableCollection<OrganizationRequestViewModel>();
            MyOrganizations = new ObservableCollection<OrganizationItemViewModel>();
            AllStudents = new ObservableCollection<StudentItemViewModel>();
            MyStudents = new ObservableCollection<StudentItemViewModel>();
            StudentSearchResults = new ObservableCollection<StudentItemViewModel>();
            SentStudentRequests = new ObservableCollection<StudentRequestViewModel>();

            SearchStudentCommand = new RelayCommand(ExecuteSearchStudent);
            SendStudentRequestCommand = new RelayCommand(ExecuteSendStudentRequest);
            CancelStudentRequestCommand = new RelayCommand(ExecuteCancelStudentRequest);
            OpenSurveyCommand = new RelayCommand(ExecuteOpenSurvey);



            LoadProfileData();
            SelectedMenu = "Surveys";
            LoadSurveys();
        }

        public string TeacherName { get; }
        public string TeacherEmail { get; }

        public ObservableCollection<OrganizationRequestViewModel> PendingRequests
        {
            get => _pendingRequests;
            set => SetProperty(ref _pendingRequests, value);
        }

        public ObservableCollection<OrganizationItemViewModel> MyOrganizations
        {
            get => _myOrganizations;
            set => SetProperty(ref _myOrganizations, value);
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public bool IsCreateButtonVisible
        {
            get => _isCreateButtonVisible;
            set => SetProperty(ref _isCreateButtonVisible, value);
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

        public ObservableCollection<SurveyCardViewModel> Surveys
        {
            get => _surveys;
            set => SetProperty(ref _surveys, value);
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (SetProperty(ref _phone, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Subject
        {
            get => _subject;
            set
            {
                if (SetProperty(ref _subject, value))
                    CommandManager.InvalidateRequerySuggested();
            }
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

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CreateSurveyCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }

        public string SearchStudentEmail
        {
            get => _searchStudentEmail;
            set => SetProperty(ref _searchStudentEmail, value);
        }

        public ObservableCollection<StudentItemViewModel> AllStudents
        {
            get => _allStudents;
            set => SetProperty(ref _allStudents, value);
        }

        public ObservableCollection<StudentItemViewModel> MyStudents
        {
            get => _myStudents;
            set => SetProperty(ref _myStudents, value);
        }

        public ObservableCollection<StudentItemViewModel> StudentSearchResults
        {
            get => _studentSearchResults;
            set => SetProperty(ref _studentSearchResults, value);
        }

        public ObservableCollection<StudentRequestViewModel> SentStudentRequests
        {
            get => _sentStudentRequests;
            set => SetProperty(ref _sentStudentRequests, value);
        }

        public ObservableCollection<OrganizationItemViewModel> OrganizationsForRequest
        {
            get => _myOrganizations;
        }

        public OrganizationItemViewModel SelectedOrganizationForRequest
        {
            get => _selectedOrganizationForRequest;
            set => SetProperty(ref _selectedOrganizationForRequest, value);
        }

        public ICommand SearchStudentCommand { get; }
        public ICommand SendStudentRequestCommand { get; }
        public ICommand CancelStudentRequestCommand { get; }

        private void LoadSurveys()
        {
            if (Surveys == null)
            {
                Surveys = new ObservableCollection<SurveyCardViewModel>();
            }
            else
            {
                Surveys.Clear();
            }

            var surveys = _surveyService.GetSurveysByTeacher(_teacher.Id);

            // Отладка
            System.Diagnostics.Debug.WriteLine($"Загружено опросов: {surveys.Count}");

            foreach (var s in surveys)
            {
                System.Diagnostics.Debug.WriteLine($"Опрос: {s.Title}, Вопросов: {s.QuestionsCount}");

                Surveys.Add(new SurveyCardViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    QuestionsCount = s.QuestionsCount,
                    CompletedCount = "0/0",
                    CreatedDate = s.CreatedDate.ToString("dd.MM.yyyy")
                });
            }

            // Принудительно уведомляем об изменении
            OnPropertyChanged(nameof(Surveys));
        }
        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string menu)
            {
                SelectedMenu = menu;
                ProfileMessage = string.Empty;
            }
        }

        private void ExecuteLogout(object parameter)
        {
            new LoginWindow().Show();
            (parameter as Window)?.Close();
        }

        private void ExecuteCreateSurvey(object parameter)
        {
            var builderWindow = new SurveyBuilderWindow(_teacher);
            builderWindow.Owner = parameter as Window;
            if (builderWindow.ShowDialog() == true)
            {
                LoadSurveys();
            }
        }

        private bool CanExecuteSaveProfile(object parameter) => !IsProfileLoading;

        private void ExecuteSaveProfile(object parameter)
        {
            IsProfileLoading = true;
            ProfileMessage = string.Empty;

            try
            {
                _userService.UpdateTeacherProfile(_teacher.Id, Phone, Subject);
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

        private void ExecuteMinimize(object parameter)
        {
            if (parameter is Window window)
                window.WindowState = WindowState.Minimized;
        }

        private void ExecuteMaximize(object parameter)
        {
            if (parameter is Window window)
                window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void ExecuteClose(object parameter)
        {
            if (parameter is Window window)
                window.Close();
            else
                Application.Current.Shutdown();
        }

        private void LoadProfileData()
        {
            try
            {
                var profile = _userService.GetTeacherProfile(_teacher.Id);
                Phone = profile.Phone;
                Subject = profile.Subject;
            }
            catch
            {
                Phone = string.Empty;
                Subject = string.Empty;
            }
        }
        private void ExecuteAcceptRequest(object parameter)
        {
            if (parameter is OrganizationRequestViewModel request)
            {
                try
                {
                    _userService.AcceptOrganizationRequest(request.Id);
                    LoadPendingRequests();
                    LoadMyOrganizations();
                    MessageBox.Show($"Заявка от {request.OrganizationName} принята", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteRejectRequest(object parameter)
        {
            if (parameter is OrganizationRequestViewModel request)
            {
                try
                {
                    _userService.RejectOrganizationRequest(request.Id);
                    LoadPendingRequests();
                    MessageBox.Show($"Заявка от {request.OrganizationName} отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteOpenSurvey(object parameter)
        {
            if (parameter is SurveyCardViewModel survey)
            {
                var manageWindow = new SurveyManageWindow(_teacher, survey.Id, survey.Title);
                manageWindow.Owner = Application.Current.MainWindow;
                manageWindow.ShowDialog();
            }
        }
        private void LoadPendingRequests()
        {
            PendingRequests.Clear();
            var requests = _userService.GetPendingRequestsForTeacher(_teacher.Id);
            foreach (var r in requests)
            {
                PendingRequests.Add(new OrganizationRequestViewModel
                {
                    Id = r.Id,
                    OrganizationName = r.OrganizationName,
                    OrganizationEmail = r.OrganizationEmail,
                    CreatedDate = r.CreatedDate
                });
            }
        }

        private void LoadMyOrganizations()
        {
            MyOrganizations.Clear();
            var orgs = _userService.GetTeacherOrganizations(_teacher.Id);
            foreach (var o in orgs)
            {
                MyOrganizations.Add(new OrganizationItemViewModel
                {
                    Id = o.Id,
                    Name = o.Name
                });
            }
        }

        private void UpdatePageForMenu(string menu)
        {
            switch (menu)
            {
                case "Surveys":
                    PageTitle = "Мои опросы";
                    IsCreateButtonVisible = true;
                    break;
                case "Analytics":
                    PageTitle = "Аналитика";
                    IsCreateButtonVisible = false;
                    break;
                case "Students":
                    PageTitle = "Ученики";
                    IsCreateButtonVisible = false;
                    LoadAllStudents();
                    LoadMyOrganizations();
                    if (MyOrganizations.Count > 0)
                        SelectedOrganizationForRequest = MyOrganizations[0];
                    LoadSentStudentRequests();
                    break;
                case "Organizations":
                    PageTitle = "Организации";
                    IsCreateButtonVisible = false;
                    LoadPendingRequests();
                    LoadMyOrganizations();
                    break;
                case "Profile":
                    PageTitle = "Профиль учителя";
                    IsCreateButtonVisible = false;
                    break;
            }
        }

        private void LoadTestSurveys()
        {
            Surveys = new ObservableCollection<SurveyCardViewModel>
            {
                new SurveyCardViewModel { Title = "Математика 10 класс", Description = "Контрольная работа по алгебре", QuestionsCount = 15, CompletedCount = "32/45", CreatedDate = "10.04.2026" },
                new SurveyCardViewModel { Title = "История России", Description = "Тест по эпохе Петра I", QuestionsCount = 20, CompletedCount = "28/45", CreatedDate = "08.04.2026" },
                new SurveyCardViewModel { Title = "Физика 9 класс", Description = "Законы Ньютона", QuestionsCount = 10, CompletedCount = "40/45", CreatedDate = "05.04.2026" }
            };
        }

        private void ExecuteSearchStudent(object parameter)
        {
            StudentSearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(SearchStudentEmail))
            {
                foreach (var student in AllStudents)
                {
                    if (student.Email.ToLower().Contains(SearchStudentEmail.ToLower()))
                    {
                        StudentSearchResults.Add(student);
                    }
                }

                if (StudentSearchResults.Count == 0)
                {
                    MessageBox.Show("Ученик с таким Email не найден", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                foreach (var student in AllStudents)
                {
                    StudentSearchResults.Add(student);
                }
            }
        }

        private void ExecuteSendStudentRequest(object parameter)
        {
            if (parameter is StudentItemViewModel student)
            {
                if (SelectedOrganizationForRequest == null)
                {
                    MessageBox.Show("Выберите организацию для прикрепления ученика", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    _userService.SendRequestToStudent(_teacher.Id, student.Id, SelectedOrganizationForRequest.Id);
                    MessageBox.Show($"Заявка ученику {student.Name} отправлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSentStudentRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ExecuteCancelStudentRequest(object parameter)
        {
            if (parameter is StudentRequestViewModel request)
            {
                try
                {
                    _userService.CancelStudentRequest(request.Id);
                    LoadSentStudentRequests();
                    MessageBox.Show("Заявка отменена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadAllStudents()
        {
            AllStudents.Clear();
            var students = _userService.GetAllStudents();
            foreach (var s in students)
            {
                AllStudents.Add(new StudentItemViewModel
                {
                    Id = s.Id,
                    Name = s.FullName,
                    Email = s.Email,
                    Class = s.Class
                });
            }
        }

        private void LoadMyStudents()
        {
            if (SelectedOrganizationForRequest == null) return;

            MyStudents.Clear();
            var students = _userService.GetStudentsByTeacher(_teacher.Id, SelectedOrganizationForRequest.Id);
            foreach (var s in students)
            {
                MyStudents.Add(new StudentItemViewModel
                {
                    Id = s.Id,
                    Name = s.FullName,
                    Email = s.Email,
                    Class = s.Class
                });
            }
        }

        private void LoadSentStudentRequests()
        {
            SentStudentRequests.Clear();
            var requests = _userService.GetSentRequestsForTeacher(_teacher.Id);
            foreach (var r in requests)
            {
                SentStudentRequests.Add(new StudentRequestViewModel
                {
                    Id = r.Id,
                    StudentName = r.StudentName,
                    StudentEmail = r.StudentEmail,
                    OrganizationName = r.OrganizationName,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate
                });
            }
        }
    }

    public class StudentItemViewModel : ViewModelBase
    {
        private int _id;
        private string _name;
        private string _email;
        private string _class;
        private bool _isSelected;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Class { get => _class; set => SetProperty(ref _class, value); }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class StudentRequestViewModel : ViewModelBase
    {
        private int _id;
        private string _teacherName;
        private string _teacherEmail;
        private string _studentName;
        private string _studentEmail;
        private string _organizationName;
        private string _status;
        private string _createdDate;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string TeacherName { get => _teacherName; set => SetProperty(ref _teacherName, value); }
        public string TeacherEmail { get => _teacherEmail; set => SetProperty(ref _teacherEmail, value); }
        public string StudentName { get => _studentName; set => SetProperty(ref _studentName, value); }
        public string StudentEmail { get => _studentEmail; set => SetProperty(ref _studentEmail, value); }
        public string OrganizationName { get => _organizationName; set => SetProperty(ref _organizationName, value); }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
        public string CreatedDate { get => _createdDate; set => SetProperty(ref _createdDate, value); }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "Ожидает";
                    case "Accepted": return "Принята";
                    case "Rejected": return "Отклонена";
                    default: return Status;
                }
            }
        }
    }

    public class OrganizationRequestViewModel : ViewModelBase
    {
        private int _id;
        private string _organizationName;
        private string _organizationEmail;
        private string _createdDate;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string OrganizationName { get => _organizationName; set => SetProperty(ref _organizationName, value); }
        public string OrganizationEmail { get => _organizationEmail; set => SetProperty(ref _organizationEmail, value); }
        public string CreatedDate { get => _createdDate; set => SetProperty(ref _createdDate, value); }
    }

    public class OrganizationItemViewModel : ViewModelBase
    {
        private int _id;
        private string _name;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
    }
    public class SurveyCardViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public ICommand OpenSurveyCommand { get; set; }
        private string _title, _description, _completedCount, _createdDate;
        private int _questionsCount;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public int QuestionsCount { get => _questionsCount; set => SetProperty(ref _questionsCount, value); }
        public string CompletedCount { get => _completedCount; set => SetProperty(ref _completedCount, value); }
        public string CreatedDate { get => _createdDate; set => SetProperty(ref _createdDate, value); }
    }
}