using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class TeacherOrganizationEntity
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int OrganizationId { get; set; }
        public DateTime JoinedDate { get; set; }

        public virtual TeacherEntity Teacher { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
    }
}
