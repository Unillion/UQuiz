using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class TeacherStudentEntity
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public int OrganizationId { get; set; }
        public DateTime AssignedDate { get; set; }

        public virtual TeacherEntity Teacher { get; set; }
        public virtual StudentEntity Student { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
    }
}
