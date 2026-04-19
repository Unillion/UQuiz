using System.Collections.ObjectModel;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.ViewModels.Base;
using UQuiz.ViewModels.builder;

public class QuestionViewModel : ViewModelBase
{
    private int _orderNumber;
    private string _questionText;
    private string _questionType;
    private decimal _points;
    private ObservableCollection<OptionViewModel> _options;

    public QuestionViewModel()
    {
        Options = new ObservableCollection<OptionViewModel>();
        Points = 1;
        QuestionType = "Text"; // По умолчанию текстовый
        AddOptionCommand = new RelayCommand(ExecuteAddOption);
        RemoveOptionCommand = new RelayCommand(ExecuteRemoveOption, CanExecuteRemoveOption);
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
                OnPropertyChanged(nameof(HasOptions));
                OnPropertyChanged(nameof(IsSingleChoice));
                OnPropertyChanged(nameof(IsMultipleChoice));
                OnPropertyChanged(nameof(IsText));
                // Если тип текстовый - очищаем варианты
                if (value == "Text")
                {
                    Options.Clear();
                }
                // Если переключились на тип с вариантами и их нет - добавляем пустые
                if ((value == "SingleChoice" || value == "MultipleChoice") && Options.Count == 0)
                {
                    Options.Add(new OptionViewModel { OrderNumber = 1, OptionText = "Вариант 1" });
                    Options.Add(new OptionViewModel { OrderNumber = 2, OptionText = "Вариант 2" });
                }
            }
        }
    }

    public decimal Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }

    public ObservableCollection<OptionViewModel> Options
    {
        get => _options;
        set => SetProperty(ref _options, value);
    }

    public ICommand AddOptionCommand { get; }
    public ICommand RemoveOptionCommand { get; }

    private OptionViewModel _selectedOption;
    public OptionViewModel SelectedOption
    {
        get => _selectedOption;
        set => SetProperty(ref _selectedOption, value);
    }

    public bool HasOptions => QuestionType == "SingleChoice" || QuestionType == "MultipleChoice";
    public bool IsSingleChoice => QuestionType == "SingleChoice";
    public bool IsMultipleChoice => QuestionType == "MultipleChoice";
    public bool IsText => QuestionType == "Text";

    private void ExecuteAddOption(object parameter)
    {
        var option = new OptionViewModel
        {
            OrderNumber = Options.Count + 1,
            OptionText = $"Вариант {Options.Count + 1}"
        };
        Options.Add(option);
    }

    private bool CanExecuteRemoveOption(object parameter)
    {
        return SelectedOption != null;
    }

    private void ExecuteRemoveOption(object parameter)
    {
        if (SelectedOption != null)
        {
            Options.Remove(SelectedOption);
            for (int i = 0; i < Options.Count; i++)
            {
                Options[i].OrderNumber = i + 1;
            }
        }
    }


}