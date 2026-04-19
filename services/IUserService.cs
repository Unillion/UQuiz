using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.models.enums;
using UQuiz.models.interfaces;
using static UQuiz.services.UserService;

namespace UQuiz.services
{
    public interface IUserService
    {
        User Register(string fullNameOrOrgName, string email, string password, UserType userType);
        User Login(string email, string password, UserType userType);
        User GetUserById(int id);
        List<User> GetAllUsers();
        List<User> GetUsersByType(UserType userType);
        List<OrganizationInfo> GetTeacherOrganizations(int teacherId);

        bool EmailExists(string email);
        TeacherProfile GetTeacherProfile(int teacherId);
        void UpdateTeacherProfile(int teacherId, string phone, string subject);

        List<TeacherInfo> GetAllTeachers();
        List<TeacherInfo> GetTeachersByOrganization(int organizationId);
        List<ConnectionRequestInfo> GetSentRequests(int organizationId);
        void SendConnectionRequest(int organizationId, int teacherId);
        void CancelConnectionRequest(int requestId);

        List<OrganizationRequestInfo> GetPendingRequestsForTeacher(int teacherId);
        void AcceptOrganizationRequest(int requestId);
        void RejectOrganizationRequest(int requestId);

        List<StudentInfo> GetAllStudents();
        List<StudentInfo> GetStudentsByTeacher(int teacherId, int organizationId);
        List<StudentRequestInfo> GetPendingRequestsForStudent(int studentId);
        List<StudentRequestInfo> GetSentRequestsForTeacher(int teacherId);
        void SendRequestToStudent(int teacherId, int studentId, int organizationId);
        void AcceptStudentRequest(int requestId);
        void RejectStudentRequest(int requestId);
        void CancelStudentRequest(int requestId);

        List<TeacherInfo> GetTeachersByStudent(int studentId);
    }

    public class TeacherInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public int SurveysCount { get; set; }
    }

    public class ConnectionRequestInfo
    {
        public int Id { get; set; }
        public string TeacherName { get; set; }
        public string TeacherEmail { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
    }

    public class TeacherProfile
    {
        public string Phone { get; set; }
        public string Subject { get; set; }
    }
    public class StudentInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Class { get; set; }
    }

    public class StudentRequestInfo
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherEmail { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string OrganizationName { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
    }
}
