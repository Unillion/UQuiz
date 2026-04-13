using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UQuiz.data.entities;

namespace UQuiz.data.entities
{
    public class TeacherEntity
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        [Required]
        public virtual UserEntity User { get; set; }

        public virtual ICollection<TeacherOrganizationEntity> TeacherOrganizations { get; set; }
        public virtual ICollection<TeacherStudentEntity> TeacherStudents { get; set; }
        public virtual ICollection<SurveyEntity> Surveys { get; set; }
    }
}