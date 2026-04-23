using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.models.users;
using UQuiz.services;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels
{
    public class SurveyManageViewModel : ViewModelBase
    {
        private readonly Teacher _teacher;
        private readonly IUserService _userService;
        private readonly ISurveyService _surveyService;
        private readonly int _surveyId;
        private string _surveyTitle;
        private string _selectedSection;
        private ObservableCollection<StudentItemViewModel> _attachedStudents;
        private ObservableCollection<StudentItemViewModel> _selectedStudents;
        private ObservableCollection<string> _availableClasses;
        private string _selectedClass;
        private bool _sendToAll;
        private string _message;

        public SurveyManageViewModel(Teacher teacher, int surveyId, string surveyTitle)
        {
            _teacher = teacher;
            _userService = new UserService();
            _surveyService = new SurveyService();
            _surveyId = surveyId;
            _surveyTitle = surveyTitle;

            NavigateCommand = new RelayCommand(ExecuteNavigate);
            SendSurveyCommand = new RelayCommand(ExecuteSendSurvey, CanExecuteSendSurvey);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            DeselectAllCommand = new RelayCommand(ExecuteDeselectAll);
            BackCommand = new RelayCommand(ExecuteBack);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            MaximizeCommand = new RelayCommand(ExecuteMaximize);
            CloseCommand = new RelayCommand(ExecuteClose);

            AttachedStudents = new ObservableCollection<StudentItemViewModel>();
            SelectedStudents = new ObservableCollection<StudentItemViewModel>();
            AvailableClasses = new ObservableCollection<string>();

            SelectedSection = "Send";
            LoadStudents();
        }

        public string SurveyTitle => _surveyTitle;

        public string SelectedSection
        {
            get => _selectedSection;
            set
            {
                if (SetProperty(ref _selectedSection, value))
                    UpdatePageForSection(value);
            }
        }

        public ObservableCollection<StudentItemViewModel> AttachedStudents
        {
            get => _attachedStudents;
            set => SetProperty(ref _attachedStudents, value);
        }

        public ObservableCollection<StudentItemViewModel> SelectedStudents
        {
            get => _selectedStudents;
            set => SetProperty(ref _selectedStudents, value);
        }

        public ObservableCollection<string> AvailableClasses
        {
            get => _availableClasses;
            set => SetProperty(ref _availableClasses, value);
        }

        public string SelectedClass
        {
            get => _selectedClass;
            set
            {
                if (SetProperty(ref _selectedClass, value))
                {
                    if (SendToAll && !string.IsNullOrEmpty(value))
                    {
                        SelectStudentsByClass(value);
                    }
                }
            }
        }

        public bool SendToAll
        {
            get => _sendToAll;
            set
            {
                if (SetProperty(ref _sendToAll, value))
                {
                    OnPropertyChanged(nameof(IsManualSelectionEnabled));
                    if (value && !string.IsNullOrEmpty(SelectedClass))
                    {
                        SelectStudentsByClass(SelectedClass);
                    }
                }
            }
        }

        public bool IsManualSelectionEnabled => !SendToAll;

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand SendSurveyCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }

        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string section)
            {
                SelectedSection = section;
            }
        }

        private bool CanExecuteSendSurvey(object parameter)
        {
            if (SendToAll) return true;
            return AttachedStudents.Any(s => s.IsSelected);
        }

        private void ExecuteSendSurvey(object parameter)
        {
            Message = string.Empty;

            try
            {
                var studentIds = SendToAll
                    ? AttachedStudents.Select(s => s.Id).ToList()
                    : AttachedStudents.Where(s => s.IsSelected).Select(s => s.Id).ToList();

                if (studentIds.Count == 0)
                {
                    MessageBox.Show("Нет выбранных учеников", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _surveyService.AssignSurveyToStudents(_surveyId, studentIds);

                Message = $"Опрос отправлен {studentIds.Count} ученикам!";
                MessageBox.Show(Message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSelectAll(object parameter)
        {
            foreach (var student in AttachedStudents)
            {
                student.IsSelected = true;
            }
            UpdateSelectedStudents();
        }

        private void ExecuteDeselectAll(object parameter)
        {
            foreach (var student in AttachedStudents)
            {
                student.IsSelected = false;
            }
            UpdateSelectedStudents();
        }

        private void ExecuteBack(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
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

        private void UpdatePageForSection(string section)
        {
            switch (section)
            {
                case "Send":
                    break;
                case "Check":
                    break;
            }
        }

        private void LoadStudents()
        {
            var organizations = _userService.GetTeacherOrganizations(_teacher.Id);
            var allStudents = new System.Collections.Generic.List<StudentInfo>();

            foreach (var org in organizations)
            {
                var students = _userService.GetStudentsByTeacher(_teacher.Id, org.Id);
                allStudents.AddRange(students);
            }

            var uniqueStudents = allStudents.GroupBy(s => s.Id).Select(g => g.First()).ToList();

            AttachedStudents.Clear();
            AvailableClasses.Clear();

            foreach (var s in uniqueStudents)
            {
                AttachedStudents.Add(new StudentItemViewModel
                {
                    Id = s.Id,
                    Name = s.FullName,
                    Email = s.Email,
                    Class = s.Class,
                    IsSelected = false
                });

                if (!string.IsNullOrEmpty(s.Class) && !AvailableClasses.Contains(s.Class))
                {
                    AvailableClasses.Add(s.Class);
                }
            }
        }

        private void SelectStudentsByClass(string className)
        {
            foreach (var student in AttachedStudents)
            {
                if (student.Class == className)
                {
                    student.IsSelected = true;
                }
                else
                {
                    student.IsSelected = false;
                }
            }
            UpdateSelectedStudents();
        }

        private void UpdateSelectedStudents()
        {
            SelectedStudents.Clear();
            foreach (var student in AttachedStudents)
            {
                if (student.IsSelected)
                {
                    SelectedStudents.Add(student);
                }
            }
        }
    }
}