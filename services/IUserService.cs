using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.models.enums;
using UQuiz.models.interfaces;

namespace UQuiz.services
{
    public interface IUserService
    {
        User Register(string fullNameOrOrgName, string email, string password, UserType userType);
        User Login(string email, string password, UserType userType);
        User GetUserById(int id);
        List<User> GetAllUsers();
        List<User> GetUsersByType(UserType userType);
        bool EmailExists(string email);
    }
}
