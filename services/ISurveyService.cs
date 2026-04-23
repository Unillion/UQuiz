using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQuiz.services
{
    public interface ISurveyService
    {
        Survey SaveSurvey(SurveyData surveyData);
        List<Survey> GetSurveysByTeacher(int teacherId);
        Survey GetSurveyById(int surveyId);
        void DeleteSurvey(int surveyId);
        void AssignSurveyToStudents(int surveyId, List<int> studentIds);
        List<Survey> GetAvailableSurveysForStudent(int studentId);
        List<Survey> GetCompletedSurveysForStudent(int studentId);
    }

    public class SurveyData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        public int OrganizationId { get; set; }
        public List<QuestionData> Questions { get; set; }
    }

    public class QuestionData
    {
        public int OrderNumber { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public decimal Points { get; set; }
        public List<OptionData> Options { get; set; }
    }

    public class OptionData
    {
        public int OrderNumber { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public decimal Points { get; set; }
    }

    public class Survey
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        public int OrganizationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int QuestionsCount { get; set; }
        public string TeacherName { get; set; }
        public string Score { get; set; }
    }
}
