using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class AnswerOptionEntity
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
        public int OrderNumber { get; set; }
        public bool IsCorrect { get; set; }

        public virtual QuestionEntity Question { get; set; }
    }
}
