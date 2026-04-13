using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UQuiz.data.entities;

namespace UQuiz.data.entities
{
    public class OrganizationEntity
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(200)]
        public string LogoUrl { get; set; }

        [Required]
        public virtual UserEntity User { get; set; }

        public virtual ICollection<TeacherOrganizationEntity> TeacherOrganizations { get; set; }
        public virtual ICollection<TeacherStudentEntity> TeacherStudents { get; set; }
        public virtual ICollection<SurveyEntity> Surveys { get; set; }
    }
}