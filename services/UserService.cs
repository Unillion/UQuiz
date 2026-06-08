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
                    string login = email.Split('@')[0];

                    string dbUserType;
                    switch (userType)
                    {
                        case UserType.Teacher:
                            dbUserType = "Teacher";
                            break;
                        case UserType.RegularUser:
                            dbUserType = "Student";
                            break;
                        case UserType.Organization:
                            dbUserType = "Organization";
                            break;
                        default:
                            throw new Exception("Неизвестный тип пользователя");
                    }

                    var userEntity = new UserEntity
                    {
                        Login = login,
                        Email = email,
                        PasswordHash = passwordHash,
                        RegistrationDate = DateTime.Now,
                        UserType = dbUserType,
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

                string dbUserType;
                switch (userType)
                {
                    case UserType.Teacher:
                        dbUserType = "Teacher";
                        break;
                    case UserType.RegularUser:
                        dbUserType = "Student";
                        break;
                    case UserType.Organization:
                        dbUserType = "Organization";
                        break;
                    default:
                        throw new Exception("Неизвестный тип пользователя");
                }

                var userEntity = context.Users
                    .FirstOrDefault(u => u.Email == email &&
                                         u.PasswordHash == passwordHash &&
                                         u.UserType == dbUserType);

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

        public TeacherProfile GetTeacherProfile(int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var user = context.Users.Find(teacherId);
                var teacher = context.Teachers.FirstOrDefault(t => t.UserId == teacherId);

                return new TeacherProfile
                {
                    Phone = user?.Phone ?? string.Empty,
                    Subject = teacher?.Department ?? string.Empty
                };
            }
        }

        public void UpdateTeacherProfile(int teacherId, string phone, string subject)
        {
            using (var context = new AppDbContext())
            {
                var user = context.Users.Find(teacherId);
                var teacher = context.Teachers.FirstOrDefault(t => t.UserId == teacherId);

                if (user != null)
                {
                    user.Phone = phone;
                }

                if (teacher != null)
                {
                    teacher.Department = subject;
                }

                context.SaveChanges();
            }
        }
        private User MapToDomainUser(UserEntity entity)
        {
            User user = null;

            switch (entity.UserType)
            {
                case "Teacher":
                    user = new Teacher();
                    break;
                case "Student":
                    user = new RegularUser();
                    break;
                case "Organization":
                    user = new Organization();
                    break;
            }

            if (user != null)
            {
                user.Id = entity.Id;
                user.Login = entity.Login;
                user.Email = entity.Email;
                user.Password = entity.PasswordHash;
                user.RegistrationDate = entity.RegistrationDate;
                user.FullName = entity.FullName;
            }

            return user;
        }

        public List<OrganizationInfo> GetTeacherOrganizations(int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var orgs = from to in context.TeacherOrganizations
                           join o in context.Organizations on to.OrganizationId equals o.UserId
                           where to.TeacherId == teacherId
                           select new OrganizationInfo
                           {
                               Id = o.UserId,
                               Name = o.Name
                           };
                return orgs.ToList();
            }
        }

        public List<TeacherInfo> GetAllTeachers()
        {
            using (var context = new AppDbContext())
            {
                var teachers = from u in context.Users
                               join t in context.Teachers on u.Id equals t.UserId
                               where u.UserType == "Teacher"
                               select new TeacherInfo
                               {
                                   Id = u.Id,
                                   FullName = u.FullName ?? u.Login,
                                   Email = u.Email,
                                   Subject = t.Department ?? "Не указан"
                               };
                return teachers.ToList();
            }
        }

        public List<TeacherInfo> GetTeachersByOrganization(int organizationId)
        {
            using (var context = new AppDbContext())
            {
                var teachers = from to in context.TeacherOrganizations
                               join t in context.Teachers on to.TeacherId equals t.UserId
                               join u in context.Users on t.UserId equals u.Id
                               where to.OrganizationId == organizationId
                               select new TeacherInfo
                               {
                                   Id = u.Id,
                                   FullName = u.FullName ?? u.Login,
                                   Email = u.Email,
                                   Subject = t.Department ?? "Не указан",
                                   SurveysCount = context.Surveys.Count(s => s.TeacherId == t.UserId)
                               };
                return teachers.ToList();
            }
        }

        public List<ConnectionRequestInfo> GetSentRequests(int organizationId)
        {
            using (var context = new AppDbContext())
            {
                var requests = from cr in context.ConnectionRequests
                               join tu in context.Users on cr.ToUserId equals tu.Id
                               where cr.FromUserId == organizationId && cr.RequestType == "OrgToTeacher"
                               select new
                               {
                                   cr.Id,
                                   TeacherName = tu.FullName ?? tu.Login,
                                   TeacherEmail = tu.Email,
                                   cr.Status,
                                   cr.CreatedDate
                               };

                var result = new List<ConnectionRequestInfo>();
                foreach (var r in requests.ToList())
                {
                    result.Add(new ConnectionRequestInfo
                    {
                        Id = r.Id,
                        TeacherName = r.TeacherName,
                        TeacherEmail = r.TeacherEmail,
                        Status = r.Status,
                        CreatedDate = r.CreatedDate.ToString("dd.MM.yyyy")
                    });
                }
                return result;
            }
        }

        public void SendConnectionRequest(int organizationId, int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var existing = context.ConnectionRequests
                    .FirstOrDefault(cr => cr.FromUserId == organizationId
                                       && cr.ToUserId == teacherId
                                       && cr.RequestType == "OrgToTeacher"
                                       && cr.Status == "Pending");

                if (existing != null)
                    throw new Exception("Заявка этому учителю уже отправлена");

                var alreadyAttached = context.TeacherOrganizations
                    .Any(to => to.OrganizationId == organizationId && to.TeacherId == teacherId);

                if (alreadyAttached)
                    throw new Exception("Учитель уже прикреплён к организации");

                var request = new ConnectionRequestEntity
                {
                    FromUserId = organizationId,
                    ToUserId = teacherId,
                    RequestType = "OrgToTeacher",
                    Status = "Pending",
                    CreatedDate = DateTime.Now
                };

                context.ConnectionRequests.Add(request);
                context.SaveChanges();
            }
        }

        public void CancelConnectionRequest(int requestId)
        {
            using (var context = new AppDbContext())
            {
                var request = context.ConnectionRequests.Find(requestId);
                if (request != null && request.Status == "Pending")
                {
                    context.ConnectionRequests.Remove(request);
                    context.SaveChanges();
                }
            }
        }

        public SurveyDetail GetSurveyDetail(int surveyId)
        {
            using (var context = new AppDbContext())
            {
                var survey = context.Surveys.Find(surveyId);
                if (survey == null) return null;

                var questions = context.Questions
                    .Where(q => q.SurveyId == surveyId)
                    .OrderBy(q => q.OrderNumber)
                    .ToList();

                var detail = new SurveyDetail
                {
                    Id = survey.Id,
                    Title = survey.Title,
                    Description = survey.Description,
                    Questions = new List<QuestionDetail>()
                };

                foreach (var q in questions)
                {
                    var questionDetail = new QuestionDetail
                    {
                        Id = q.Id,
                        OrderNumber = q.OrderNumber,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        Options = new List<OptionDetail>()
                    };

                    if (q.QuestionType == "SingleChoice" || q.QuestionType == "MultipleChoice")
                    {
                        var options = context.AnswerOptions
                            .Where(o => o.QuestionId == q.Id)
                            .OrderBy(o => o.OrderNumber)
                            .ToList();

                        foreach (var o in options)
                        {
                            questionDetail.Options.Add(new OptionDetail
                            {
                                Id = o.Id,
                                OptionText = o.OptionText,
                                OrderNumber = o.OrderNumber
                            });
                        }
                    }

                    detail.Questions.Add(questionDetail);
                }

                return detail;
            }
        }

        public class OrganizationInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public List<OrganizationRequestInfo> GetPendingRequestsForTeacher(int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var requests = from cr in context.ConnectionRequests
                               join org in context.Users on cr.FromUserId equals org.Id
                               where cr.ToUserId == teacherId
                                  && cr.RequestType == "OrgToTeacher"
                                  && cr.Status == "Pending"
                               select new
                               {
                                   cr.Id,
                                   OrganizationName = org.FullName ?? org.Login,
                                   OrganizationEmail = org.Email,
                                   cr.CreatedDate
                               };

                var result = new List<OrganizationRequestInfo>();
                foreach (var r in requests.ToList())
                {
                    result.Add(new OrganizationRequestInfo
                    {
                        Id = r.Id,
                        OrganizationName = r.OrganizationName,
                        OrganizationEmail = r.OrganizationEmail,
                        CreatedDate = r.CreatedDate.ToString("dd.MM.yyyy")
                    });
                }
                return result;
            }
        }

        public void AcceptOrganizationRequest(int requestId)
        {
            using (var context = new AppDbContext())
            {
                var request = context.ConnectionRequests.Find(requestId);
                if (request != null && request.Status == "Pending")
                {
                    request.Status = "Accepted";
                    request.ProcessedDate = DateTime.Now;

                    var teacherOrg = new TeacherOrganizationEntity
                    {
                        TeacherId = request.ToUserId,
                        OrganizationId = request.FromUserId,
                        JoinedDate = DateTime.Now
                    };
                    context.TeacherOrganizations.Add(teacherOrg);

                    context.SaveChanges();
                }
            }
        }

        public void RejectOrganizationRequest(int requestId)
        {
            using (var context = new AppDbContext())
            {
                var request = context.ConnectionRequests.Find(requestId);
                if (request != null && request.Status == "Pending")
                {
                    request.Status = "Rejected";
                    request.ProcessedDate = DateTime.Now;
                    context.SaveChanges();
                }
            }
        }

        public class OrganizationRequestInfo
        {
            public int Id { get; set; }
            public string OrganizationName { get; set; }
            public string OrganizationEmail { get; set; }
            public string CreatedDate { get; set; }
        }

        public List<StudentInfo> GetAllStudents()
        {
            using (var context = new AppDbContext())
            {
                var students = from u in context.Users
                               join s in context.Students on u.Id equals s.UserId
                               where u.UserType == "Student"
                               select new StudentInfo
                               {
                                   Id = u.Id,
                                   FullName = u.FullName ?? u.Login,
                                   Email = u.Email,
                                   Class = s.Class ?? "—"
                               };
                return students.ToList();
            }
        }

        public List<StudentInfo> GetStudentsByTeacher(int teacherId, int organizationId)
        {
            using (var context = new AppDbContext())
            {
                var students = from ts in context.TeacherStudents
                               join s in context.Students on ts.StudentId equals s.UserId
                               join u in context.Users on s.UserId equals u.Id
                               where ts.TeacherId == teacherId && ts.OrganizationId == organizationId
                               select new StudentInfo
                               {
                                   Id = u.Id,
                                   FullName = u.FullName ?? u.Login,
                                   Email = u.Email,
                                   Class = s.Class ?? "—"
                               };
                return students.ToList();
            }
        }

        public List<StudentRequestInfo> GetPendingRequestsForStudent(int studentId)
        {
            using (var context = new AppDbContext())
            {
                var requests = from cr in context.ConnectionRequests
                               join tu in context.Users on cr.FromUserId equals tu.Id
                               join o in context.Organizations on cr.OrganizationId equals o.UserId
                               where cr.ToUserId == studentId
                                  && cr.RequestType == "TeacherToStudent"
                                  && cr.Status == "Pending"
                               select new
                               {
                                   cr.Id,
                                   TeacherName = tu.FullName ?? tu.Login,
                                   TeacherEmail = tu.Email,
                                   OrganizationName = o.Name,
                                   cr.CreatedDate
                               };

                var result = new List<StudentRequestInfo>();
                foreach (var r in requests.ToList())
                {
                    result.Add(new StudentRequestInfo
                    {
                        Id = r.Id,
                        TeacherName = r.TeacherName,
                        TeacherEmail = r.TeacherEmail,
                        OrganizationName = r.OrganizationName,
                        CreatedDate = r.CreatedDate.ToString("dd.MM.yyyy")
                    });
                }
                return result;
            }
        }

        public List<StudentRequestInfo> GetSentRequestsForTeacher(int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var requests = from cr in context.ConnectionRequests
                               join su in context.Users on cr.ToUserId equals su.Id
                               join o in context.Organizations on cr.OrganizationId equals o.UserId
                               where cr.FromUserId == teacherId
                                  && cr.RequestType == "TeacherToStudent"
                               select new
                               {
                                   cr.Id,
                                   StudentName = su.FullName ?? su.Login,
                                   StudentEmail = su.Email,
                                   OrganizationName = o.Name,
                                   cr.Status,
                                   cr.CreatedDate
                               };

                var result = new List<StudentRequestInfo>();
                foreach (var r in requests.ToList())
                {
                    result.Add(new StudentRequestInfo
                    {
                        Id = r.Id,
                        StudentName = r.StudentName,
                        StudentEmail = r.StudentEmail,
                        OrganizationName = r.OrganizationName,
                        Status = r.Status,
                        CreatedDate = r.CreatedDate.ToString("dd.MM.yyyy")
                    });
                }
                return result;
            }
        }

        public void SendRequestToStudent(int teacherId, int studentId, int organizationId)
        {
            using (var context = new AppDbContext())
            {
                var existing = context.ConnectionRequests
                    .FirstOrDefault(cr => cr.FromUserId == teacherId
                                       && cr.ToUserId == studentId
                                       && cr.RequestType == "TeacherToStudent"
                                       && cr.Status == "Pending");

                if (existing != null)
                    throw new Exception("Заявка этому ученику уже отправлена");

                var alreadyAttached = context.TeacherStudents
                    .Any(ts => ts.TeacherId == teacherId
                            && ts.StudentId == studentId
                            && ts.OrganizationId == organizationId);

                if (alreadyAttached)
                    throw new Exception("Ученик уже прикреплён");

                var request = new ConnectionRequestEntity
                {
                    FromUserId = teacherId,
                    ToUserId = studentId,
                    OrganizationId = organizationId,
                    RequestType = "TeacherToStudent",
                    Status = "Pending",
                    CreatedDate = DateTime.Now
                };

                context.ConnectionRequests.Add(request);
                context.SaveChanges();
            }
        }

        public void AcceptStudentRequest(int requestId)
        {
            using (var context = new AppDbContext())
            {
                var request = context.ConnectionRequests.Find(requestId);
                if (request != null && request.Status == "Pending" && request.RequestType == "TeacherToStudent")
                {
                    request.Status = "Accepted";
                    request.ProcessedDate = DateTime.Now;

                    var teacherStudent = new TeacherStudentEntity
                    {
                        TeacherId = request.FromUserId,
                        StudentId = request.ToUserId,
                        OrganizationId = request.OrganizationId.Value,
                        AssignedDate = DateTime.Now
                    };
                    context.TeacherStudents.Add(teacherStudent);

                    context.SaveChanges();
                }
            }
        }

        public void RejectStudentRequest(int requestId)
        {
            using (var context = new AppDbContext())
            {
                var request = context.ConnectionRequests.Find(requestId);
                if (request != null && request.Status == "Pending" && request.RequestType == "TeacherToStudent")
                {
                    request.Status = "Rejected";
                    request.ProcessedDate = DateTime.Now;
                    context.SaveChanges();
                }
            }
        }

        public void CancelStudentRequest(int requestId)
        {
            using (var context = new AppDbContext())
            {
                var request = context.ConnectionRequests.Find(requestId);
                if (request != null && request.Status == "Pending" && request.RequestType == "TeacherToStudent")
                {
                    context.ConnectionRequests.Remove(request);
                    context.SaveChanges();
                }
            }
        }

        public List<TeacherInfo> GetTeachersByStudent(int studentId)
        {
            using (var context = new AppDbContext())
            {
                var teachers = from ts in context.TeacherStudents
                               join t in context.Teachers on ts.TeacherId equals t.UserId
                               join u in context.Users on t.UserId equals u.Id
                               where ts.StudentId == studentId
                               select new TeacherInfo
                               {
                                   Id = u.Id,
                                   FullName = u.FullName ?? u.Login,
                                   Email = u.Email,
                                   Subject = t.Department ?? "Не указан"
                               };
                return teachers.ToList();
            }
        }
        public StudentProfile GetStudentProfile(int studentId)
        {
            using (var context = new AppDbContext())
            {
                var user = context.Users.Find(studentId);
                var student = context.Students.FirstOrDefault(s => s.UserId == studentId);

                return new StudentProfile
                {
                    FullName = user?.FullName ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    Class = student?.Class ?? string.Empty
                };
            }
        }

        public void UpdateStudentProfile(int studentId, string studentClass)
        {
            using (var context = new AppDbContext())
            {
                var student = context.Students.FirstOrDefault(s => s.UserId == studentId);
                if (student != null)
                {
                    student.Class = studentClass;
                    context.SaveChanges();
                }
            }
        }

        public class StudentProfile
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Class { get; set; }
        }
    }
}