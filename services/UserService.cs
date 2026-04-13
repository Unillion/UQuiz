using System;
using System.Collections.Generic;
using System.Linq;
using UQuiz.database;
using UQuiz.data.entities;
using UQuiz.models.enums;
using UQuiz.models.users;
using UQuiz.models.interfaces;
using System.Windows;

namespace UQuiz.services
{
    public class UserService : IUserService
    {
        public User Register(string fullNameOrOrgName, string email, string password, UserType userType)
        {
            using (var context = new AppDbContext())
            {
                try
                {
                    if (context.Users.Any(u => u.Email == email))
                        throw new Exception("Пользователь с таким Email уже существует");

                    string passwordHash = HashPassword(password);

                    // Генерируем логин из email (всё что до @)
                    string login = email.Split('@')[0];

                    var userEntity = new UserEntity
                    {
                        Login = login,
                        Email = email,
                        PasswordHash = passwordHash,
                        RegistrationDate = DateTime.Now,
                        UserType = userType.ToString(),
                        FullName = fullNameOrOrgName
                    };

                    context.Users.Add(userEntity);
                    context.SaveChanges();

                    switch (userType)
                    {
                        case UserType.Teacher:
                            context.Teachers.Add(new TeacherEntity { UserId = userEntity.Id });
                            break;
                        case UserType.RegularUser:
                            context.Students.Add(new StudentEntity { UserId = userEntity.Id });
                            break;
                        case UserType.Organization:
                            context.Organizations.Add(new OrganizationEntity
                            {
                                UserId = userEntity.Id,
                                Name = fullNameOrOrgName
                            });
                            break;
                    }
                    context.SaveChanges();

                    return MapToDomainUser(userEntity);
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        errorMessage += "\n\nInner: " + inner.Message;
                        inner = inner.InnerException;
                    }
                    MessageBox.Show(errorMessage, "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            }
        }

        public User Login(string email, string password, UserType userType)
        {
            using (var context = new AppDbContext())
            {
                string passwordHash = HashPassword(password);

                var userEntity = context.Users
                    .FirstOrDefault(u => u.Email == email &&
                                         u.PasswordHash == passwordHash &&
                                         u.UserType == userType.ToString());

                if (userEntity == null)
                    throw new Exception("Неверный Email, пароль или тип пользователя");

                return MapToDomainUser(userEntity);
            }
        }

        public User GetUserById(int id)
        {
            using (var context = new AppDbContext())
            {
                var userEntity = context.Users.Find(id);
                return userEntity != null ? MapToDomainUser(userEntity) : null;
            }
        }

        public List<User> GetAllUsers()
        {
            using (var context = new AppDbContext())
            {
                return context.Users.ToList().Select(MapToDomainUser).ToList();
            }
        }

        public List<User> GetUsersByType(UserType userType)
        {
            using (var context = new AppDbContext())
            {
                return context.Users
                    .Where(u => u.UserType == userType.ToString())
                    .ToList()
                    .Select(MapToDomainUser)
                    .ToList();
            }
        }

        public bool EmailExists(string email)
        {
            using (var context = new AppDbContext())
            {
                return context.Users.Any(u => u.Email == email);
            }
        }

        public bool LoginExists(string login)
        {
            using (var context = new AppDbContext())
            {
                return context.Users.Any(u => u.Login == login);
            }
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
            );
        }

        private User MapToDomainUser(UserEntity entity)
        {
            User user = null;

            if (Enum.TryParse<UserType>(entity.UserType, out var userType))
            {
                switch (userType)
                {
                    case UserType.Teacher:
                        user = new Teacher();
                        break;
                    case UserType.RegularUser:
                        user = new RegularUser();
                        break;
                    case UserType.Organization:
                        user = new Organization();
                        break;
                }
            }

            if (user != null)
            {
                user.Id = entity.Id;
                user.Login = entity.Login;
                user.Email = entity.Email;
                user.Password = entity.PasswordHash;
                user.RegistrationDate = entity.RegistrationDate;
            }

            return user;
        }
    }
}