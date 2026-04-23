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

        public void AssignSurveyToStudents(int surveyId, List<int> studentIds)
        {
            using (var context = new AppDbContext())
            {
                try
                {
                    foreach (var studentId in studentIds)
                    {
                        var exists = context.SurveyAssignments
                            .Any(sa => sa.SurveyId == surveyId && sa.StudentId == studentId);

                        if (!exists)
                        {
                            var assignment = new SurveyAssignmentEntity
                            {
                                SurveyId = surveyId,
                                StudentId = studentId,
                                AssignedDate = DateTime.Now,
                                IsCompleted = false
                            };
                            context.SurveyAssignments.Add(assignment);
                        }
                    }
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        error += "\n" + inner.Message;
                        inner = inner.InnerException;
                    }
                    throw new Exception(error);
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

        public List<Survey> GetAvailableSurveysForStudent(int studentId)
        {
            using (var context = new AppDbContext())
            {
                var surveys = from sa in context.SurveyAssignments
                              join s in context.Surveys on sa.SurveyId equals s.Id
                              join u in context.Users on s.TeacherId equals u.Id
                              where sa.StudentId == studentId && !sa.IsCompleted
                              select new
                              {
                                  s.Id,
                                  s.Title,
                                  s.Description,
                                  s.CreatedDate,
                                  TeacherName = u.FullName ?? u.Login,
                                  QuestionsCount = context.Questions.Count(q => q.SurveyId == s.Id)
                              };

                var result = new List<Survey>();
                foreach (var s in surveys.ToList())
                {
                    result.Add(new Survey
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        CreatedDate = s.CreatedDate,
                        TeacherName = s.TeacherName,
                        QuestionsCount = s.QuestionsCount
                    });
                }
                return result;
            }
        }

        public List<Survey> GetCompletedSurveysForStudent(int studentId)
        {
            using (var context = new AppDbContext())
            {
                var surveys = from sa in context.SurveyAssignments
                              join s in context.Surveys on sa.SurveyId equals s.Id
                              join u in context.Users on s.TeacherId equals u.Id
                              join sr in context.SurveyResponses on new { sa.SurveyId, sa.StudentId } equals new { sr.SurveyId, sr.StudentId } into srJoin
                              from sr in srJoin.DefaultIfEmpty()
                              where sa.StudentId == studentId && sa.IsCompleted
                              select new
                              {
                                  s.Id,
                                  s.Title,
                                  s.Description,
                                  s.CreatedDate,
                                  TeacherName = u.FullName ?? u.Login,
                                  QuestionsCount = context.Questions.Count(q => q.SurveyId == s.Id),
                                  Score = sr != null ? sr.TotalScore : null
                              };

                var result = new List<Survey>();
                foreach (var s in surveys.ToList())
                {
                    result.Add(new Survey
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        CreatedDate = s.CreatedDate,
                        TeacherName = s.TeacherName,
                        QuestionsCount = s.QuestionsCount,
                        Score = s.Score.HasValue ? $"{s.Score}/{s.QuestionsCount}" : null
                    });
                }
                return result;
            }
        }

        public void SubmitSurveyResponse(int surveyId, int studentId, List<AnswerData> answers)
        {
            using (var context = new AppDbContext())
            {
                try
                {
                    // Создаём запись о прохождении
                    var response = new SurveyResponseEntity
                    {
                        SurveyId = surveyId,
                        StudentId = studentId,
                        StartedDate = DateTime.Now,
                        CompletedDate = DateTime.Now
                    };
                    context.SurveyResponses.Add(response);
                    context.SaveChanges();

                    // Сохраняем ответы
                    foreach (var answer in answers)
                    {
                        var answerEntity = new AnswerEntity
                        {
                            ResponseId = response.Id,
                            QuestionId = answer.QuestionId,
                            AnswerText = answer.TextAnswer
                        };
                        context.Answers.Add(answerEntity);
                        context.SaveChanges();

                        // Сохраняем выбранные варианты
                        if (answer.SelectedOptionIds != null)
                        {
                            foreach (var optionId in answer.SelectedOptionIds)
                            {
                                var choice = new AnswerChoiceEntity
                                {
                                    AnswerId = answerEntity.Id,
                                    OptionId = optionId
                                };
                                context.AnswerChoices.Add(choice);
                            }
                            context.SaveChanges();
                        }
                    }

                    // Помечаем назначение как выполненное
                    var assignment = context.SurveyAssignments
                        .FirstOrDefault(sa => sa.SurveyId == surveyId && sa.StudentId == studentId);
                    if (assignment != null)
                    {
                        assignment.IsCompleted = true;
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        error += "\n" + inner.Message;
                        inner = inner.InnerException;
                    }
                    throw new Exception(error);
                }
            }
        }
    }
}
