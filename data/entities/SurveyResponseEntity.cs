using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class SurveyResponseEntity
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public int StudentId { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? TotalScore { get; set; }

        public virtual SurveyEntity Survey { get; set; }
        public virtual StudentEntity Student { get; set; }
        public virtual SurveyAssignmentEntity Assignment { get; set; }
        public virtual ICollection<AnswerEntity> Answers { get; set; }
    }
}
