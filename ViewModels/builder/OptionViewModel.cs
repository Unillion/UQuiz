using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels.builder
{
    public class OptionViewModel : ViewModelBase
    {
        private int _orderNumber;
        private string _optionText;
        private bool _isCorrect;
        private decimal _points;

        public int OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        public string OptionText
        {
            get => _optionText;
            set => SetProperty(ref _optionText, value);
        }

        public bool IsCorrect
        {
            get => _isCorrect;
            set => SetProperty(ref _isCorrect, value);
        }

        public decimal Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }
}
