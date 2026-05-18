using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.services;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels
{
    public class SurveyPassingViewModel : ViewModelBase
    {
        private string _surveyTitle;
        private string _surveyDescription;
        private ObservableCollection<QuestionPassingViewModel> _questions;
        private int _currentQuestionIndex;
        private QuestionPassingViewModel _currentQuestion;
        private bool _isFirstQuestion;
        private bool _isLastQuestion;
        private string _progress;
        private DateTime _startTime;
        private readonly int _surveyId;
        private readonly int _studentId;
        private readonly ISurveyService _surveyService;

        public SurveyPassingViewModel(int surveyId, int studentId)
        {
            _surveyId = surveyId;
            _studentId = studentId;
            _startTime = DateTime.Now;
            _surveyService = new SurveyService();
            var surveyDetail = _surveyService.GetSurveyDetail(surveyId);


            if (surveyDetail != null)
            {
                _surveyTitle = surveyDetail.Title;
                _surveyDescription = surveyDetail.Description;

                Questions = new ObservableCollection<QuestionPassingViewModel>();

                foreach (var q in surveyDetail.Questions)
                {
                    var question = new QuestionPassingViewModel
                    {
                        Id = q.Id,
                        OrderNumber = q.OrderNumber,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        Options = new ObservableCollection<OptionPassingViewModel>()
                    };

                    if (q.Options != null)
                    {
                        foreach (var o in q.Options)
                        {
                            question.Options.Add(new OptionPassingViewModel
                            {
                                Id = o.Id,
                                Text = o.OptionText
                            });
                        }
                    }

                    Questions.Add(question);
                }
            }

            NextQuestionCommand = new RelayCommand(ExecuteNextQuestion, CanExecuteNextQuestion);
            PreviousQuestionCommand = new RelayCommand(ExecutePreviousQuestion, CanExecutePreviousQuestion);
            FinishSurveyCommand = new RelayCommand(ExecuteFinishSurvey);
            CancelCommand = new RelayCommand(ExecuteCancel);

            if (Questions.Count > 0)
            {
                CurrentQuestionIndex = 0;
                UpdateCurrentQuestion();
            }
        }

        public string SurveyTitle => _surveyTitle;
        public string SurveyDescription => _surveyDescription;

        public ObservableCollection<QuestionPassingViewModel> Questions
        {
            get => _questions;
            set => SetProperty(ref _questions, value);
        }

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                if (SetProperty(ref _currentQuestionIndex, value))
                {
                    UpdateCurrentQuestion();
                    IsFirstQuestion = value == 0;
                    IsLastQuestion = value == Questions.Count - 1;
                    Progress = $"Вопрос {value + 1} из {Questions.Count}";
                }
            }
        }

        public QuestionPassingViewModel CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
        }

        public bool IsFirstQuestion
        {
            get => _isFirstQuestion;
            set => SetProperty(ref _isFirstQuestion, value);
        }

        public bool IsLastQuestion
        {
            get => _isLastQuestion;
            set => SetProperty(ref _isLastQuestion, value);
        }

        public string Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }
        public ICommand FinishSurveyCommand { get; }
        public ICommand CancelCommand { get; }

        private bool CanExecuteNextQuestion(object parameter)
        {
            return CurrentQuestionIndex < Questions.Count - 1;
        }

        private void ExecuteNextQuestion(object parameter)
        {
            if (CurrentQuestionIndex < Questions.Count - 1)
            {
                CurrentQuestionIndex++;
            }
        }

        private bool CanExecutePreviousQuestion(object parameter)
        {
            return CurrentQuestionIndex > 0;
        }

        private void ExecutePreviousQuestion(object parameter)
        {
            if (CurrentQuestionIndex > 0)
            {
                CurrentQuestionIndex--;
            }
        }

        private void ExecuteFinishSurvey(object parameter)
        {
            var unanswered = Questions.Where(q => !q.IsAnswered).ToList();

            if (unanswered.Count > 0)
            {
                var result = MessageBox.Show(
                    $"У вас есть {unanswered.Count} неотвеченных вопросов. Всё равно завершить опрос?",
                    "Завершение опроса",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            try
            {
                var answers = new List<AnswerData>();
                foreach (var q in Questions)
                {
                    var answer = new AnswerData
                    {
                        QuestionId = q.Id,
                        TextAnswer = q.IsText ? q.TextAnswer : null,
                        SelectedOptionIds = q.IsChoice ? q.Options.Where(o => o.IsSelected).Select(o => o.Id).ToList() : null
                    };
                    answers.Add(answer);
                }

                _surveyService.SubmitSurveyResponse(_surveyId, _studentId, answers);

                var answeredCount = Questions.Count(q => q.IsAnswered);
                MessageBox.Show($"Опрос завершён!\nОтвечено: {answeredCount} из {Questions.Count}\nВремя: {(DateTime.Now - _startTime).TotalMinutes:F1} мин.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                if (parameter is Window window)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти? Все ответы будут потеряны.",
                "Отмена", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && parameter is Window window)
            {
                window.Close();
            }
        }

        private void UpdateCurrentQuestion()
        {
            if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < Questions.Count)
            {
                CurrentQuestion = Questions[CurrentQuestionIndex];
            }
        }


    }

    public class QuestionPassingViewModel : ViewModelBase
    {
        private int _id;
        private int _orderNumber;
        private string _questionText;
        private string _questionType;
        private decimal _points;
        private ObservableCollection<OptionPassingViewModel> _options;
        private string _textAnswer;
        public bool IsSingleChoice => QuestionType == "SingleChoice";
        public bool IsMultipleChoice => QuestionType == "MultipleChoice";
        public bool IsText_q => QuestionType == "Text";
        public bool IsChoice_q => QuestionType == "SingleChoice" || QuestionType == "MultipleChoice";
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public int OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        public string QuestionText
        {
            get => _questionText;
            set => SetProperty(ref _questionText, value);
        }

        public string QuestionType
        {
            get => _questionType;
            set
            {
                if (SetProperty(ref _questionType, value))
                {
                    OnPropertyChanged(nameof(IsText_q));
                    OnPropertyChanged(nameof(IsChoice_q));
                    OnPropertyChanged(nameof(IsSingleChoice));
                    OnPropertyChanged(nameof(IsMultipleChoice));
                }
            }
        }

        public decimal Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        public ObservableCollection<OptionPassingViewModel> Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        public string TextAnswer
        {
            get => _textAnswer;
            set
            {
                if (SetProperty(ref _textAnswer, value))
                {
                    OnPropertyChanged(nameof(IsAnswered));
                }
            }
        }

        public bool IsText => QuestionType == "Text";
        public bool IsChoice => QuestionType == "SingleChoice" || QuestionType == "MultipleChoice";

        public bool IsAnswered
        {
            get
            {
                if (IsText)
                    return !string.IsNullOrWhiteSpace(TextAnswer);
                if (IsChoice)
                    return Options != null && Options.Any(o => o.IsSelected);
                return false;
            }
        }
    }

    public class OptionPassingViewModel : ViewModelBase
    {
        private int _id;
        private string _text;
        private bool _isSelected;


        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}