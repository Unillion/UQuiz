using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.data.entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string UserType { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }

        public virtual TeacherEntity Teacher { get; set; }
        public virtual StudentEntity Student { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
        public virtual ICollection<ConnectionRequestEntity> SentRequests { get; set; }
        public virtual ICollection<ConnectionRequestEntity> ReceivedRequests { get; set; }
    }
}
