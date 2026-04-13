using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class AnswerEntity
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }
        public decimal? Score { get; set; }

        public virtual SurveyResponseEntity Response { get; set; }
        public virtual QuestionEntity Question { get; set; }
        public virtual ICollection<AnswerChoiceEntity> Choices { get; set; }
    }
}
