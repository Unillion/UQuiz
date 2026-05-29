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


        public void SubmitSurveyResponse(int surveyId, int studentId, List<AnswerData> answers)
        {
            using (var context = new AppDbContext())
            {
                // Проверка на повторную отправку
                var existingResponse = context.SurveyResponses
                    .Any(sr => sr.SurveyId == surveyId && sr.StudentId == studentId);

                if (existingResponse)
                    throw new Exception("Вы уже отправили этот опрос");

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

                decimal totalScore = 0;

                foreach (var answer in answers)
                {
                    // Получаем вопрос
                    var question = context.Questions.Find(answer.QuestionId);
                    if (question == null) continue;

                    // Создаём ответ
                    var answerEntity = new AnswerEntity
                    {
                        ResponseId = response.Id,
                        QuestionId = answer.QuestionId,
                        AnswerText = answer.TextAnswer ?? ""
                    };

                    // АВТОПРОВЕРКА
                    if (question.QuestionType == "SingleChoice" || question.QuestionType == "MultipleChoice")
                    {
                        // Загружаем ВСЕ варианты для этого вопроса
                        var allOptions = context.AnswerOptions
                            .Where(o => o.QuestionId == question.Id)
                            .ToList();

                        // Находим правильные
                        var correctOptionIds = allOptions
                            .Where(o => o.IsCorrect)
                            .Select(o => o.Id)
                            .ToList();

                        // Выбранные учеником
                        var selectedOptionIds = answer.SelectedOptionIds ?? new List<int>();

                        // ОТЛАДКА
                        System.Diagnostics.Debug.WriteLine($"=== Вопрос {question.Id}: {question.QuestionText} ===");
                        System.Diagnostics.Debug.WriteLine($"Тип: {question.QuestionType}, Баллы: {question.Points}");
                        System.Diagnostics.Debug.WriteLine($"Всего вариантов: {allOptions.Count}");
                        foreach (var opt in allOptions)
                            System.Diagnostics.Debug.WriteLine($"  Option {opt.Id}: '{opt.OptionText}' IsCorrect={opt.IsCorrect}");
                        System.Diagnostics.Debug.WriteLine($"Правильных ID: [{string.Join(",", correctOptionIds)}]");
                        System.Diagnostics.Debug.WriteLine($"Выбрано ID: [{string.Join(",", selectedOptionIds)}]");

                        // Вычисляем баллы
                        decimal score = 0;

                        if (question.QuestionType == "SingleChoice")
                        {
                            if (selectedOptionIds.Count == 1 && correctOptionIds.Contains(selectedOptionIds[0]))
                            {
                                score = question.Points;
                                System.Diagnostics.Debug.WriteLine($"SingleChoice: ПРАВИЛЬНО! Баллы: {score}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"SingleChoice: НЕПРАВИЛЬНО. Баллы: 0");
                            }
                        }
                        else if (question.QuestionType == "MultipleChoice")
                        {
                            bool allCorrectSelected = correctOptionIds.Count > 0 &&
                                                      correctOptionIds.All(c => selectedOptionIds.Contains(c));
                            bool noIncorrectSelected = selectedOptionIds.All(s => correctOptionIds.Contains(s));

                            if (allCorrectSelected && noIncorrectSelected)
                            {
                                score = question.Points;
                                System.Diagnostics.Debug.WriteLine($"MultipleChoice: ВСЁ ПРАВИЛЬНО! Баллы: {score}");
                            }
                            else if (selectedOptionIds.Any(s => correctOptionIds.Contains(s)))
                            {
                                score = question.Points / 2;
                                System.Diagnostics.Debug.WriteLine($"MultipleChoice: ЧАСТИЧНО ПРАВИЛЬНО. Баллы: {score}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"MultipleChoice: НЕПРАВИЛЬНО. Баллы: 0");
                            }
                        }

                        answerEntity.Score = score;
                        totalScore += score;
                    }
                    // Текстовые вопросы — без автопроверки

                    context.Answers.Add(answerEntity);
                    context.SaveChanges();

                    // Сохраняем выбранные варианты
                    if (answer.SelectedOptionIds != null && answer.SelectedOptionIds.Count > 0)
                    {
                        foreach (var optionId in answer.SelectedOptionIds)
                        {
                            context.AnswerChoices.Add(new AnswerChoiceEntity
                            {
                                AnswerId = answerEntity.Id,
                                OptionId = optionId
                            });
                        }
                        context.SaveChanges();
                    }
                }

                // Сохраняем общий балл
                response.TotalScore = totalScore;
                context.SaveChanges();

                // Помечаем как выполненное
                var assignment = context.SurveyAssignments
                    .FirstOrDefault(sa => sa.SurveyId == surveyId && sa.StudentId == studentId);
                if (assignment != null)
                {
                    assignment.IsCompleted = true;
                    context.SaveChanges();
                }

                System.Diagnostics.Debug.WriteLine($"=== ИТОГО: {totalScore} баллов ===");
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
                    var assignedCount = context.SurveyAssignments.Count(sa => sa.SurveyId == s.Id);
                    var completedCount = context.SurveyAssignments.Count(sa => sa.SurveyId == s.Id && sa.IsCompleted);

                    result.Add(new Survey
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        TeacherId = s.TeacherId,
                        OrganizationId = s.OrganizationId ?? 0,
                        CreatedDate = s.CreatedDate,
                        IsActive = s.IsActive,
                        QuestionsCount = questionsCount,
                        AssignedCount = assignedCount,
                        CompletedCount = completedCount
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
                int addedCount = 0;
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
                        addedCount++;
                    }
                }
                context.SaveChanges();

                if (addedCount == 0 && studentIds.Count > 0)
                    throw new Exception("Опрос уже отправлен всем выбранным ученикам");
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

        public List<StudentResponseInfo> GetStudentResponses(int surveyId)
        {
            using (var context = new AppDbContext())
            {
                var responses = from sr in context.SurveyResponses
                                join u in context.Users on sr.StudentId equals u.Id
                                join s in context.Students on sr.StudentId equals s.UserId
                                where sr.SurveyId == surveyId
                                select new StudentResponseInfo
                                {
                                    ResponseId = sr.Id,
                                    StudentId = sr.StudentId,
                                    StudentName = u.FullName ?? u.Login,
                                    StudentEmail = u.Email,
                                    StudentClass = s.Class ?? "—",
                                    CompletedDate = sr.CompletedDate,
                                    TotalScore = sr.TotalScore,
                                    TotalQuestions = context.Questions.Count(q => q.SurveyId == surveyId)
                                };
                return responses.ToList();
            }
        }

        public StudentResponseDetail GetStudentResponseDetail(int responseId)
        {
            using (var context = new AppDbContext())
            {
                var response = context.SurveyResponses.Find(responseId);
                if (response == null) return null;

                var student = context.Users.Find(response.StudentId);
                var survey = context.Surveys.Find(response.SurveyId);
                var answers = context.Answers.Where(a => a.ResponseId == responseId).ToList();

                var detail = new StudentResponseDetail
                {
                    ResponseId = response.Id,
                    StudentId = response.StudentId,
                    StudentName = student?.FullName ?? student?.Login,
                    SurveyTitle = survey?.Title,
                    Answers = new List<AnswerDetail>()
                };

                foreach (var answer in answers)
                {
                    var question = context.Questions.Find(answer.QuestionId);
                    var choices = context.AnswerChoices
                        .Where(c => c.AnswerId == answer.Id)
                        .Select(c => c.Option.OptionText)
                        .ToList();

                    var correctOptions = context.AnswerOptions
                        .Where(o => o.QuestionId == question.Id && o.IsCorrect)
                        .Select(o => o.OptionText)
                        .ToList();

                    detail.Answers.Add(new AnswerDetail
                    {
                        AnswerId = answer.Id,
                        QuestionId = answer.QuestionId,
                        QuestionText = question?.QuestionText,
                        QuestionType = question?.QuestionType,
                        Points = question?.Points ?? 0,
                        StudentAnswer = answer.AnswerText,
                        SelectedOptionIds = answer.Choices?.Select(c => c.OptionId).ToList(),
                        SelectedOptionTexts = choices,
                        CorrectOptionTexts = correctOptions,
                        Score = answer.Score,
                        CorrectAnswer = question?.CorrectAnswer,
                        IsAutoChecked = question?.QuestionType == "SingleChoice" || question?.QuestionType == "MultipleChoice"
                    });
                }

                return detail;
            }
        }

        public void UpdateAnswerScore(int answerId, decimal score, string correctAnswer)
        {
            using (var context = new AppDbContext())
            {
                var answer = context.Answers.Find(answerId);
                if (answer != null)
                {
                    answer.Score = score;

                    // Обновляем правильный ответ в вопросе, если передан
                    if (!string.IsNullOrEmpty(correctAnswer))
                    {
                        var question = context.Questions.Find(answer.QuestionId);
                        if (question != null)
                        {
                            question.CorrectAnswer = correctAnswer;
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        public void UpdateResponseTotalScore(int responseId)
        {
            using (var context = new AppDbContext())
            {
                var response = context.SurveyResponses
                    .Include("Answers")
                    .FirstOrDefault(r => r.Id == responseId);

                if (response != null)
                {
                    response.TotalScore = response.Answers.Sum(a => a.Score ?? 0);
                    context.SaveChanges();
                }
            }
        }

        public SurveyAnalytics GetSurveyAnalytics(int surveyId)
        {
            using (var context = new AppDbContext())
            {
                var survey = context.Surveys.Find(surveyId);
                if (survey == null) return null;

                var responses = context.SurveyResponses.Where(r => r.SurveyId == surveyId).ToList();
                var questions = context.Questions.Where(q => q.SurveyId == surveyId).OrderBy(q => q.OrderNumber).ToList();

                var analytics = new SurveyAnalytics
                {
                    Title = survey.Title,
                    TotalStudents = context.SurveyAssignments.Count(sa => sa.SurveyId == surveyId),
                    CompletedCount = responses.Count,
                    MaxScore = questions.Sum(q => q.Points),
                    AverageScore = responses.Any() ? responses.Average(r => r.TotalScore ?? 0) : 0,
                    QuestionStats = new List<QuestionStats>(),
                    ScoreDistribution = new List<ScoreDistribution>()
                };

                // Статистика по вопросам
                foreach (var q in questions)
                {
                    var answers = context.Answers.Where(a => a.QuestionId == q.Id).ToList();
                    analytics.QuestionStats.Add(new QuestionStats
                    {
                        QuestionNumber = q.OrderNumber,
                        QuestionText = q.QuestionText.Length > 50 ? q.QuestionText.Substring(0, 50) + "..." : q.QuestionText,
                        MaxPoints = q.Points,
                        AverageScore = answers.Any() ? answers.Average(a => a.Score ?? 0) : 0,
                        TotalAnswers = answers.Count,
                        CorrectAnswers = answers.Count(a => (a.Score ?? 0) == q.Points)
                    });
                }

                // Распределение оценок
                if (analytics.MaxScore > 0)
                {
                    var maxScore = analytics.MaxScore;
                    var ranges = new[] { 0.9m, 0.75m, 0.5m, 0.25m, 0 };
                    var labels = new[] { "Отлично (90-100%)", "Хорошо (75-89%)", "Удовл. (50-74%)", "Неуд. (25-49%)", "Плохо (0-24%)" };

                    for (int i = 0; i < ranges.Length; i++)
                    {
                        var threshold = maxScore * ranges[i];
                        var nextThreshold = i > 0 ? maxScore * ranges[i - 1] : decimal.MaxValue;

                        analytics.ScoreDistribution.Add(new ScoreDistribution
                        {
                            Label = labels[i],
                            Count = responses.Count(r => (r.TotalScore ?? 0) >= threshold && (r.TotalScore ?? 0) < nextThreshold)
                        });
                    }
                }

                return analytics;
            }
        }

        public TeacherAnalytics GetTeacherAnalytics(int teacherId)
        {
            using (var context = new AppDbContext())
            {
                var surveys = context.Surveys.Where(s => s.TeacherId == teacherId).ToList();

                // Выносим Id в примитивный список
                var surveyIds = surveys.Select(s => s.Id).ToList();

                var allResponses = context.SurveyResponses
                    .Where(r => surveyIds.Contains(r.SurveyId))
                    .ToList();

                // Собираем уникальных учеников через назначения
                var totalStudents = context.SurveyAssignments
                    .Where(sa => surveyIds.Contains(sa.SurveyId))
                    .Select(sa => sa.StudentId)
                    .Distinct()
                    .Count();

                var analytics = new TeacherAnalytics
                {
                    TotalSurveys = surveys.Count,
                    TotalStudents = totalStudents,
                    OverallAverageScore = allResponses.Any() ? allResponses.Average(r => r.TotalScore ?? 0) : 0,
                    SurveyScores = new List<SurveyScoreInfo>()
                };

                foreach (var s in surveys.OrderBy(s => s.CreatedDate))
                {
                    var responses = allResponses.Where(r => r.SurveyId == s.Id).ToList();
                    analytics.SurveyScores.Add(new SurveyScoreInfo
                    {
                        Title = s.Title.Length > 30 ? s.Title.Substring(0, 30) + "..." : s.Title,
                        AverageScore = responses.Any() ? responses.Average(r => r.TotalScore ?? 0) : 0,
                        CreatedDate = s.CreatedDate
                    });
                }

                return analytics;
            }
        }

    }
}
