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
    public class OrganizationViewModel : ViewModelBase
    {
        private readonly Organization _organization;
        private readonly IUserService _userService;
        private string _pageTitle;
        private string _selectedMenu;
        private string _searchEmail;
        private ObservableCollection<TeacherItemViewModel> _allTeachers;
        private ObservableCollection<TeacherItemViewModel> _myTeachers;
        private ObservableCollection<TeacherItemViewModel> _searchResults;
        private ObservableCollection<SentRequestViewModel> _sentRequests;

        public OrganizationViewModel(Organization organization)
        {
            _organization = organization;
            _userService = new UserService();

            OrganizationName = organization.FullName ?? organization.Login;
            OrganizationEmail = organization.Email;

            NavigateCommand = new RelayCommand(ExecuteNavigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SearchTeacherCommand = new RelayCommand(ExecuteSearchTeacher);
            SendRequestCommand = new RelayCommand(ExecuteSendRequest);
            CancelRequestCommand = new RelayCommand(ExecuteCancelRequest);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            MaximizeCommand = new RelayCommand(ExecuteMaximize);
            CloseCommand = new RelayCommand(ExecuteClose);

            AllTeachers = new ObservableCollection<TeacherItemViewModel>();
            MyTeachers = new ObservableCollection<TeacherItemViewModel>();
            SearchResults = new ObservableCollection<TeacherItemViewModel>();
            SentRequests = new ObservableCollection<SentRequestViewModel>();

            SelectedMenu = "Teachers";
            LoadData();
        }

        public string OrganizationName { get; }
        public string OrganizationEmail { get; }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
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

        public string SearchEmail
        {
            get => _searchEmail;
            set => SetProperty(ref _searchEmail, value);
        }

        public ObservableCollection<TeacherItemViewModel> AllTeachers
        {
            get => _allTeachers;
            set => SetProperty(ref _allTeachers, value);
        }

        public ObservableCollection<TeacherItemViewModel> MyTeachers
        {
            get => _myTeachers;
            set => SetProperty(ref _myTeachers, value);
        }

        public ObservableCollection<TeacherItemViewModel> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public ObservableCollection<SentRequestViewModel> SentRequests
        {
            get => _sentRequests;
            set => SetProperty(ref _sentRequests, value);
        }

        private TeacherItemViewModel _selectedTeacher;
        public TeacherItemViewModel SelectedTeacher
        {
            get => _selectedTeacher;
            set => SetProperty(ref _selectedTeacher, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SearchTeacherCommand { get; }
        public ICommand SendRequestCommand { get; }
        public ICommand CancelRequestCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }

        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string menu)
            {
                SelectedMenu = menu;
                if (menu == "Teachers")
                {
                    SearchResults.Clear();
                    SearchEmail = string.Empty;
                }
            }
        }

        private void ExecuteLogout(object parameter)
        {
            new LoginWindow().Show();
            (parameter as Window)?.Close();
        }

        private void ExecuteSearchTeacher(object parameter)
        {
            SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(SearchEmail))
            {
                // Поиск по email среди всех учителей
                foreach (var teacher in AllTeachers)
                {
                    if (teacher.Email.ToLower().Contains(SearchEmail.ToLower()))
                    {
                        SearchResults.Add(teacher);
                    }
                }

                if (SearchResults.Count == 0)
                {
                    MessageBox.Show("Учитель с таким Email не найден", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Если поле пустое - показываем всех
                foreach (var teacher in AllTeachers)
                {
                    SearchResults.Add(teacher);
                }
            }
        }

        private void ExecuteSendRequest(object parameter)
        {
            if (parameter is TeacherItemViewModel teacher)
            {
                try
                {
                    _userService.SendConnectionRequest(_organization.Id, teacher.Id);
                    MessageBox.Show($"Заявка учителю {teacher.Name} отправлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSentRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ExecuteCancelRequest(object parameter)
        {
            if (parameter is SentRequestViewModel request)
            {
                SentRequests.Remove(request);
                MessageBox.Show("Заявка отменена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                case "Analytics":
                    PageTitle = "Аналитика";
                    break;
                case "Teachers":
                    PageTitle = "Учителя";
                    break;
                case "Requests":
                    PageTitle = "Отправленные заявки";
                    break;
            }
        }

        private void LoadData()
        {
            LoadAllTeachers();
            LoadMyTeachers();
            LoadSentRequests();
        }

        private void LoadAllTeachers()
        {
            AllTeachers.Clear();
            var teachers = _userService.GetAllTeachers();
            foreach (var t in teachers)
            {
                AllTeachers.Add(new TeacherItemViewModel
                {
                    Id = t.Id,
                    Name = t.FullName,
                    Email = t.Email,
                    Subject = t.Subject
                });
            }
        }

        private void LoadMyTeachers()
        {
            MyTeachers.Clear();
            var teachers = _userService.GetTeachersByOrganization(_organization.Id);
            foreach (var t in teachers)
            {
                MyTeachers.Add(new TeacherItemViewModel
                {
                    Id = t.Id,
                    Name = t.FullName,
                    Email = t.Email,
                    Subject = t.Subject,
                    SurveysCount = t.SurveysCount
                });
            }
        }

        private void LoadSentRequests()
        {
            SentRequests.Clear();
            var requests = _userService.GetSentRequests(_organization.Id);
            foreach (var r in requests)
            {
                SentRequests.Add(new SentRequestViewModel
                {
                    Id = r.Id,
                    TeacherName = r.TeacherName,
                    TeacherEmail = r.TeacherEmail,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate
                });
            }
        }

    }

    public class TeacherItemViewModel : ViewModelBase
    {
        private int _id;
        private string _name;
        private string _email;
        private string _subject;
        private int _surveysCount;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Subject { get => _subject; set => SetProperty(ref _subject, value); }
        public int SurveysCount { get => _surveysCount; set => SetProperty(ref _surveysCount, value); }
    }

    public class SentRequestViewModel : ViewModelBase
    {
        private int _id;
        private string _teacherName;
        private string _teacherEmail;
        private string _status;
        private string _createdDate;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string TeacherName { get => _teacherName; set => SetProperty(ref _teacherName, value); }
        public string TeacherEmail { get => _teacherEmail; set => SetProperty(ref _teacherEmail, value); }
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
}