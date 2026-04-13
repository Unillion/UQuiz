using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UQuiz.data.entities;

namespace UQuiz.data.entities
{
    public class StudentEntity
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(20)]
        public string StudentCode { get; set; }

        [StringLength(20)]
        public string Class { get; set; }

        [Required]
        public virtual UserEntity User { get; set; }

        public virtual ICollection<TeacherStudentEntity> TeacherStudents { get; set; }
        public virtual ICollection<SurveyAssignmentEntity> SurveyAssignments { get; set; }
        public virtual ICollection<SurveyResponseEntity> SurveyResponses { get; set; }
    }
}