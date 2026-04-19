using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.models.users;
using UQuiz.services;
using UQuiz.ViewModels.Base;
using UQuiz.ViewModels.builder;

namespace UQuiz.ViewModels
{
    public class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly Teacher _teacher;
        private readonly IUserService _userService;
        private readonly ISurveyService _surveyService;
        private string _title;
        private string _description;
        private ObservableCollection<QuestionViewModel> _questions;
        private QuestionViewModel _selectedQuestion;

        public SurveyBuilderViewModel(Teacher teacher)
        {
            _teacher = teacher;
            _userService = new UserService();
            _surveyService = new SurveyService();

            Questions = new ObservableCollection<QuestionViewModel>();

            AddQuestionCommand = new RelayCommand(ExecuteAddQuestion);
            RemoveQuestionCommand = new RelayCommand(ExecuteRemoveQuestion);
            RemoveOptionCommand = new RelayCommand(ExecuteRemoveOption);
            SaveSurveyCommand = new RelayCommand(ExecuteSaveSurvey, CanExecuteSaveSurvey);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ObservableCollection<QuestionViewModel> Questions
        {
            get => _questions;
            set => SetProperty(ref _questions, value);
        }

        public QuestionViewModel SelectedQuestion
        {
            get => _selectedQuestion;
            set => SetProperty(ref _selectedQuestion, value);
        }

        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand RemoveOptionCommand { get; }
        public ICommand SaveSurveyCommand { get; }
        public ICommand CancelCommand { get; }

        private void ExecuteAddQuestion(object parameter)
        {
            var question = new QuestionViewModel
            {
                OrderNumber = Questions.Count + 1,
                QuestionType = "Text"
            };
            Questions.Add(question);
            SelectedQuestion = question;
        }

        private void ExecuteRemoveQuestion(object parameter)
        {
            if (parameter is QuestionViewModel question)
            {
                Questions.Remove(question);
                for (int i = 0; i < Questions.Count; i++)
                {
                    Questions[i].OrderNumber = i + 1;
                }
            }
        }

        private void ExecuteRemoveOption(object parameter)
        {
            if (parameter is OptionViewModel option)
            {
                foreach (var question in Questions)
                {
                    if (question.Options.Contains(option))
                    {
                        question.Options.Remove(option);
                        for (int i = 0; i < question.Options.Count; i++)
                        {
                            question.Options[i].OrderNumber = i + 1;
                        }
                        break;
                    }
                }
            }
        }

        private bool CanExecuteSaveSurvey(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Title) && Questions.Count > 0;
        }

        private void ExecuteSaveSurvey(object parameter)
        {
            try
            {
                var organizations = _userService.GetTeacherOrganizations(_teacher.Id);
                int organizationId;

                if (organizations.Count == 0)
                {
                    // Если нет организаций, используем 0 или создаём "Личную" организацию
                    var result = MessageBox.Show(
                        "У вас нет привязанных организаций. Создать опрос в личном кабинете?",
                        "Нет организаций",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                        return;

                    organizationId = 0; // 0 означает личный опрос учителя
                }
                else if (organizations.Count == 1)
                {
                    organizationId = organizations.First().Id;
                }
                else
                {
                    // Если несколько организаций - пока берём первую (потом можно сделать выбор)
                    organizationId = organizations.First().Id;
                }

                var surveyData = new SurveyData
                {
                    Title = Title,
                    Description = Description,
                    TeacherId = _teacher.Id,
                    OrganizationId = organizationId,
                    Questions = new List<QuestionData>()
                };

                foreach (var q in Questions)
                {
                    var questionData = new QuestionData
                    {
                        OrderNumber = q.OrderNumber,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        Options = new List<OptionData>()
                    };

                    foreach (var opt in q.Options)
                    {
                        questionData.Options.Add(new OptionData
                        {
                            OrderNumber = opt.OrderNumber,
                            OptionText = opt.OptionText,
                            IsCorrect = opt.IsCorrect,
                            Points = opt.Points
                        });
                    }

                    surveyData.Questions.Add(questionData);
                }

                _surveyService.SaveSurvey(surveyData);

                MessageBox.Show($"Опрос \"{Title}\" сохранён!\nВопросов: {Questions.Count}",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);

                Console.WriteLine(ex.ToString());
            }
        }

        private void ExecuteCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}