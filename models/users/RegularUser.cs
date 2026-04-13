using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.models.enums;
using UQuiz.models.interfaces;

namespace UQuiz.models.users
{
    public class RegularUser : User
    {
        public override UserType UserType => UserType.RegularUser;
    }
}
