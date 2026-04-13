using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.models.enums;

namespace UQuiz.models.interfaces
{
    public abstract class User : IUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegistrationDate { get; set; }
        public abstract UserType UserType { get; }

        protected User()
        {
            RegistrationDate = DateTime.Now;
        }

        public virtual bool ValidatePassword(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 8;
        }

        public virtual void HashPassword()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                Password = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password))
                );
            }
        }


    }
}
