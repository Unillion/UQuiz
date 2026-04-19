using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.data.entities;
using UQuiz.database;

namespace UQuiz.services
{
    public class SurveyService : ISurveyService
    {
        public Survey SaveSurvey(SurveyData surveyData)
        {
            using (var context = new AppDbContext())
            {
                try
                {
                    var surveyEntity = new SurveyEntity
                    {
                        Title = surveyData.Title,
                        Description = surveyData.Description,
                        TeacherId = surveyData.TeacherId,
                        OrganizationId = surveyData.OrganizationId > 0 ? surveyData.OrganizationId : (int?)null,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    context.Surveys.Add(surveyEntity);
                    context.SaveChanges();

                    foreach (var q in surveyData.Questions)
                    {
                        var questionEntity = new QuestionEntity
                        {
                            SurveyId = surveyEntity.Id,
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            OrderNumber = q.OrderNumber,
                            Points = q.Points
                        };

                        context.Questions.Add(questionEntity);
                        context.SaveChanges();

                        if (q.Options != null)
                        {
                            foreach (var opt in q.Options)
                            {
                                var optionEntity = new AnswerOptionEntity
                                {
                                    QuestionId = questionEntity.Id,
                                    OptionText = opt.OptionText,
                                    OrderNumber = opt.OrderNumber,
                                    IsCorrect = opt.IsCorrect
                                };
                                context.AnswerOptions.Add(optionEntity);
                            }
                            context.SaveChanges();
                        }
                    }

                    return new Survey
                    {
                        Id = surveyEntity.Id,
                        Title = surveyEntity.Title,
                        Description = surveyEntity.Description,
                        TeacherId = surveyEntity.TeacherId,
                        OrganizationId = surveyEntity.OrganizationId ?? 0,
                        CreatedDate = surveyEntity.CreatedDate,
                        IsActive = surveyEntity.IsActive,
                        QuestionsCount = surveyData.Questions.Count
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("=== ОШИБКА СОХРАНЕНИЯ ОПРОСА ===");
                    Console.WriteLine($"Message: {ex.Message}");

                    Exception inner = ex.InnerException;
                    int level = 1;

                    while (inner != null)
                    {
                        Console.WriteLine($"\n--- Inner Exception Level {level} ---");
                        Console.WriteLine($"Message: {inner.Message}");
                        Console.WriteLine($"Type: {inner.GetType().Name}");
                        Console.WriteLine($"Stack Trace: {inner.StackTrace}");

                        inner = inner.InnerException;
                        level++;
                    }

                    Console.WriteLine("=== КОНЕЦ ОШИБКИ ===");

                    throw;
                }
            }
        }

        public List<Survey> GetSurveysByTeacher(int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var surveys = context.Surveys
                    .Where(s => s.TeacherId == teacherId)
                    .OrderByDescending(s => s.CreatedDate)
                    .ToList();

                var result = new List<Survey>();
                foreach (var s in surveys)
                {
                    var questionsCount = context.Questions.Count(q => q.SurveyId == s.Id);

                    result.Add(new Survey
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        TeacherId = s.TeacherId,
                        OrganizationId = s.OrganizationId ?? 0,
                        CreatedDate = s.CreatedDate,
                        IsActive = s.IsActive,
                        QuestionsCount = questionsCount
                    });
                }
                return result;
            }
        }

        public Survey GetSurveyById(int surveyId)
        {
            using (var context = new AppDbContext())
            {
                var s = context.Surveys.Find(surveyId);
                if (s == null) return null;

                var questionsCount = context.Questions.Count(q => q.SurveyId == s.Id);

                return new Survey
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    TeacherId = s.TeacherId,
                    OrganizationId = s.OrganizationId ?? 0,
                    CreatedDate = s.CreatedDate,
                    IsActive = s.IsActive,
                    QuestionsCount = questionsCount
                };
            }
        }

        public void DeleteSurvey(int surveyId)
        {
            using (var context = new AppDbContext())
            {
                var survey = context.Surveys.Find(surveyId);
                if (survey != null)
                {
                    context.Surveys.Remove(survey);
                    context.SaveChanges();
                }
            }
        }
    }
}
