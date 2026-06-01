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
        SurveyDetail GetSurveyDetail(int surveyId);
        void SubmitSurveyResponse(int surveyId, int studentId, List<AnswerData> answers);
        List<StudentResponseInfo> GetStudentResponses(int surveyId);
        StudentResponseDetail GetStudentResponseDetail(int responseId);
        void UpdateAnswerScore(int answerId, decimal score, string correctAnswer);
        void UpdateResponseTotalScore(int responseId);
        SurveyAnalytics GetSurveyAnalytics(int surveyId);
        TeacherAnalytics GetTeacherAnalytics(int teacherId);
        OrganizationAnalytics GetOrganizationAnalytics(int organizationId);

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
        public int AssignedCount { get; set; }
        public int CompletedCount { get; set; }
    }
    public class SurveyDetail
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<QuestionDetail> Questions { get; set; }
    }

    public class QuestionDetail
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public decimal Points { get; set; }
        public List<OptionDetail> Options { get; set; }
    }

    public class OptionDetail
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public int OrderNumber { get; set; }
    }

    public class AnswerData
    {
        public int QuestionId { get; set; }
        public string TextAnswer { get; set; }
        public List<int> SelectedOptionIds { get; set; }
    }

    public class StudentResponseInfo
    {
        public int ResponseId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentClass { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? TotalScore { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class StudentResponseDetail
    {
        public int ResponseId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string SurveyTitle { get; set; }
        public List<AnswerDetail> Answers { get; set; }
    }

    public class AnswerDetail
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public decimal Points { get; set; }
        public string StudentAnswer { get; set; }
        public List<int> SelectedOptionIds { get; set; }
        public List<string> SelectedOptionTexts { get; set; }
        public List<string> CorrectOptionTexts { get; set; }   // Добавить
        public decimal? Score { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsAutoChecked { get; set; }                 // Добавить
    }

    public class SurveyAnalytics
    {
        public string Title { get; set; }
        public int TotalStudents { get; set; }
        public int CompletedCount { get; set; }
        public decimal AverageScore { get; set; }
        public decimal MaxScore { get; set; }
        public List<QuestionStats> QuestionStats { get; set; }
        public List<ScoreDistribution> ScoreDistribution { get; set; }
        public List<TimeDistribution> TimeDistribution { get; set; }
    }
    public class TimeDistribution
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }
    public class QuestionStats
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public decimal AverageScore { get; set; }
        public decimal MaxPoints { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
    }

    public class ScoreDistribution
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class TeacherAnalytics
    {
        public int TotalSurveys { get; set; }
        public int TotalStudents { get; set; }
        public decimal OverallAverageScore { get; set; }
        public List<SurveyScoreInfo> SurveyScores { get; set; }
    }

    public class SurveyScoreInfo
    {
        public string Title { get; set; }
        public decimal AverageScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class OrganizationAnalytics
    {
        public int TotalTeachers { get; set; }
        public int TotalSurveys { get; set; }
        public int TotalStudents { get; set; }
        public decimal OverallAverageScore { get; set; }
        public List<TeacherAnalyticsItem> TeacherStats { get; set; }
    }

    public class TeacherAnalyticsItem
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string Subject { get; set; }
        public int SurveysCount { get; set; }
        public int StudentsCount { get; set; }
        public decimal AverageScore { get; set; }
        public int CompletedSurveys { get; set; }
    }
}
