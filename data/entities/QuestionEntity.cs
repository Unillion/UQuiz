using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class QuestionEntity
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public int OrderNumber { get; set; }
        public decimal Points { get; set; }
        public string CorrectAnswer { get; set; }

        public virtual SurveyEntity Survey { get; set; }
        public virtual ICollection<AnswerOptionEntity> Options { get; set; }
        public virtual ICollection<AnswerEntity> Answers { get; set; }
    }
}
