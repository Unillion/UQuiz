using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class SurveyAssignmentEntity
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public int StudentId { get; set; }
        public DateTime AssignedDate { get; set; }
        public bool IsCompleted { get; set; }

        public virtual SurveyEntity Survey { get; set; }
        public virtual StudentEntity Student { get; set; }
        public virtual SurveyResponseEntity Response { get; set; }
    }
}
