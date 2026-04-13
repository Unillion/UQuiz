using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class SurveyEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        public int OrganizationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int? TimeLimit { get; set; }

        public virtual TeacherEntity Teacher { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
        public virtual ICollection<QuestionEntity> Questions { get; set; }
        public virtual ICollection<SurveyAssignmentEntity> Assignments { get; set; }
        public virtual ICollection<SurveyResponseEntity> Responses { get; set; }
    }
}
