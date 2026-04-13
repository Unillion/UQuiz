using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.models.enums;

namespace UQuiz.models.interfaces
{
    public interface IUser
    {
        int Id { get; set; }
        string Login { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        DateTime RegistrationDate { get; set; }
        UserType UserType { get; }
    }
}
