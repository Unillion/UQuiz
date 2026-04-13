using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQuiz.data.entities;


namespace UQuiz.database
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<TeacherEntity> Teachers { get; set; }
        public DbSet<StudentEntity> Students { get; set; }
        public DbSet<OrganizationEntity> Organizations { get; set; }
        public DbSet<ConnectionRequestEntity> ConnectionRequests { get; set; }
        public DbSet<TeacherOrganizationEntity> TeacherOrganizations { get; set; }
        public DbSet<TeacherStudentEntity> TeacherStudents { get; set; }
        public DbSet<SurveyEntity> Surveys { get; set; }
        public DbSet<QuestionEntity> Questions { get; set; }
        public DbSet<AnswerOptionEntity> AnswerOptions { get; set; }
        public DbSet<SurveyAssignmentEntity> SurveyAssignments { get; set; }
        public DbSet<SurveyResponseEntity> SurveyResponses { get; set; }
        public DbSet<AnswerEntity> Answers { get; set; }
        public DbSet<AnswerChoiceEntity> AnswerChoices { get; set; }

        public AppDbContext() : base("name=UQuizDbConnectionString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ========== UserEntity ==========
            modelBuilder.Entity<UserEntity>()
                .ToTable("Users")
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.Login)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute() { IsUnique = true }));

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute() { IsUnique = true }));

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.RegistrationDate)
                .IsRequired();

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.UserType)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.FullName)
                .HasMaxLength(100);

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.Phone)
                .HasMaxLength(20);

            // ========== TeacherEntity ==========
            modelBuilder.Entity<TeacherEntity>()
                .ToTable("Teachers")
                .HasKey(e => e.UserId);

            modelBuilder.Entity<TeacherEntity>()
                .Property(e => e.Department)
                .HasMaxLength(100);

            modelBuilder.Entity<TeacherEntity>()
                .Property(e => e.Bio)
                .HasMaxLength(500);

            modelBuilder.Entity<TeacherEntity>()
                .HasRequired(e => e.User)
                .WithOptional(u => u.Teacher);

            // ========== StudentEntity ==========
            modelBuilder.Entity<StudentEntity>()
                .ToTable("Students")
                .HasKey(e => e.UserId);

            modelBuilder.Entity<StudentEntity>()
                .Property(e => e.StudentCode)
                .HasMaxLength(20);

            modelBuilder.Entity<StudentEntity>()
                .Property(e => e.Class)
                .HasMaxLength(20);

            modelBuilder.Entity<StudentEntity>()
                .HasRequired(e => e.User)
                .WithOptional(u => u.Student);

            // ========== OrganizationEntity ==========
            modelBuilder.Entity<OrganizationEntity>()
                .ToTable("Organizations")
                .HasKey(e => e.UserId);

            modelBuilder.Entity<OrganizationEntity>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<OrganizationEntity>()
                .Property(e => e.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<OrganizationEntity>()
                .Property(e => e.Address)
                .HasMaxLength(200);

            modelBuilder.Entity<OrganizationEntity>()
                .Property(e => e.LogoUrl)
                .HasMaxLength(200);

            modelBuilder.Entity<OrganizationEntity>()
                .HasRequired(e => e.User)
                .WithOptional(u => u.Organization);


         

            // ========== ConnectionRequestEntity ==========
            modelBuilder.Entity<ConnectionRequestEntity>()
                .ToTable("ConnectionRequests")
                .HasKey(e => e.Id);

            modelBuilder.Entity<ConnectionRequestEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<ConnectionRequestEntity>()
                .Property(e => e.FromUserId)
                .IsRequired();

            modelBuilder.Entity<ConnectionRequestEntity>()
                .Property(e => e.ToUserId)
                .IsRequired();

            modelBuilder.Entity<ConnectionRequestEntity>()
                .Property(e => e.RequestType)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<ConnectionRequestEntity>()
                .Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<ConnectionRequestEntity>()
                .Property(e => e.CreatedDate)
                .IsRequired();

            modelBuilder.Entity<ConnectionRequestEntity>()
                .HasRequired(e => e.FromUser)
                .WithMany(u => u.SentRequests)
                .HasForeignKey(e => e.FromUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ConnectionRequestEntity>()
                .HasRequired(e => e.ToUser)
                .WithMany(u => u.ReceivedRequests)
                .HasForeignKey(e => e.ToUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ConnectionRequestEntity>()
                .HasOptional(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .WillCascadeOnDelete(false);

            // ========== TeacherOrganizationEntity ==========
            modelBuilder.Entity<TeacherOrganizationEntity>()
                .ToTable("TeacherOrganizations")
                .HasKey(e => e.Id);

            modelBuilder.Entity<TeacherOrganizationEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<TeacherOrganizationEntity>()
                .Property(e => e.TeacherId)
                .IsRequired();

            modelBuilder.Entity<TeacherOrganizationEntity>()
                .Property(e => e.OrganizationId)
                .IsRequired();

            modelBuilder.Entity<TeacherOrganizationEntity>()
                .Property(e => e.JoinedDate)
                .IsRequired();

            modelBuilder.Entity<TeacherOrganizationEntity>()
                .HasRequired(e => e.Teacher)
                .WithMany(t => t.TeacherOrganizations)
                .HasForeignKey(e => e.TeacherId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeacherOrganizationEntity>()
                .HasRequired(e => e.Organization)
                .WithMany(o => o.TeacherOrganizations)
                .HasForeignKey(e => e.OrganizationId)
                .WillCascadeOnDelete(false);

            // ========== TeacherStudentEntity ==========
            modelBuilder.Entity<TeacherStudentEntity>()
                .ToTable("TeacherStudents")
                .HasKey(e => e.Id);

            modelBuilder.Entity<TeacherStudentEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<TeacherStudentEntity>()
                .Property(e => e.TeacherId)
                .IsRequired();

            modelBuilder.Entity<TeacherStudentEntity>()
                .Property(e => e.StudentId)
                .IsRequired();

            modelBuilder.Entity<TeacherStudentEntity>()
                .Property(e => e.OrganizationId)
                .IsRequired();

            modelBuilder.Entity<TeacherStudentEntity>()
                .Property(e => e.AssignedDate)
                .IsRequired();

            modelBuilder.Entity<TeacherStudentEntity>()
                .HasRequired(e => e.Teacher)
                .WithMany(t => t.TeacherStudents)
                .HasForeignKey(e => e.TeacherId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeacherStudentEntity>()
                .HasRequired(e => e.Student)
                .WithMany(s => s.TeacherStudents)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeacherStudentEntity>()
                .HasRequired(e => e.Organization)
                .WithMany(o => o.TeacherStudents)
                .HasForeignKey(e => e.OrganizationId)
                .WillCascadeOnDelete(false);

            // ========== SurveyEntity ==========
            modelBuilder.Entity<SurveyEntity>()
                .ToTable("Surveys")
                .HasKey(e => e.Id);

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.TeacherId)
                .IsRequired();

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.OrganizationId)
                .IsRequired();

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.CreatedDate)
                .IsRequired();

            modelBuilder.Entity<SurveyEntity>()
                .Property(e => e.IsActive)
                .IsRequired();

            modelBuilder.Entity<SurveyEntity>()
                .HasRequired(e => e.Teacher)
                .WithMany(t => t.Surveys)
                .HasForeignKey(e => e.TeacherId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SurveyEntity>()
                .HasRequired(e => e.Organization)
                .WithMany(o => o.Surveys)
                .HasForeignKey(e => e.OrganizationId)
                .WillCascadeOnDelete(false);

            // ========== QuestionEntity ==========
            modelBuilder.Entity<QuestionEntity>()
                .ToTable("Questions")
                .HasKey(e => e.Id);

            modelBuilder.Entity<QuestionEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<QuestionEntity>()
                .Property(e => e.SurveyId)
                .IsRequired();

            modelBuilder.Entity<QuestionEntity>()
                .Property(e => e.QuestionText)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<QuestionEntity>()
                .Property(e => e.QuestionType)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<QuestionEntity>()
                .Property(e => e.OrderNumber)
                .IsRequired();

            modelBuilder.Entity<QuestionEntity>()
                .Property(e => e.Points)
                .IsRequired()
                .HasPrecision(5, 2);

            modelBuilder.Entity<QuestionEntity>()
                .HasRequired(e => e.Survey)
                .WithMany(s => s.Questions)
                .HasForeignKey(e => e.SurveyId)
                .WillCascadeOnDelete(true);

            // ========== AnswerOptionEntity ==========
            modelBuilder.Entity<AnswerOptionEntity>()
                .ToTable("AnswerOptions")
                .HasKey(e => e.Id);

            modelBuilder.Entity<AnswerOptionEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<AnswerOptionEntity>()
                .Property(e => e.QuestionId)
                .IsRequired();

            modelBuilder.Entity<AnswerOptionEntity>()
                .Property(e => e.OptionText)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<AnswerOptionEntity>()
                .Property(e => e.OrderNumber)
                .IsRequired();

            modelBuilder.Entity<AnswerOptionEntity>()
                .Property(e => e.IsCorrect)
                .IsRequired();

            modelBuilder.Entity<AnswerOptionEntity>()
                .HasRequired(e => e.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(e => e.QuestionId)
                .WillCascadeOnDelete(true);

            // ========== SurveyAssignmentEntity ==========
            modelBuilder.Entity<SurveyAssignmentEntity>()
                .ToTable("SurveyAssignments")
                .HasKey(e => e.Id);

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .Property(e => e.SurveyId)
                .IsRequired();

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .Property(e => e.StudentId)
                .IsRequired();

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .Property(e => e.AssignedDate)
                .IsRequired();

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .Property(e => e.IsCompleted)
                .IsRequired();

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .HasRequired(e => e.Survey)
                .WithMany(s => s.Assignments)
                .HasForeignKey(e => e.SurveyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SurveyAssignmentEntity>()
                .HasRequired(e => e.Student)
                .WithMany(s => s.SurveyAssignments)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(false);

            // ========== SurveyResponseEntity ==========
            modelBuilder.Entity<SurveyResponseEntity>()
                .ToTable("SurveyResponses")
                .HasKey(e => e.Id);

            modelBuilder.Entity<SurveyResponseEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<SurveyResponseEntity>()
                .Property(e => e.SurveyId)
                .IsRequired();

            modelBuilder.Entity<SurveyResponseEntity>()
                .Property(e => e.StudentId)
                .IsRequired();

            modelBuilder.Entity<SurveyResponseEntity>()
                .Property(e => e.StartedDate)
                .IsRequired();

            modelBuilder.Entity<SurveyResponseEntity>()
                .Property(e => e.TotalScore)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SurveyResponseEntity>()
                .HasRequired(e => e.Survey)
                .WithMany(s => s.Responses)
                .HasForeignKey(e => e.SurveyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SurveyResponseEntity>()
                .HasRequired(e => e.Student)
                .WithMany(s => s.SurveyResponses)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SurveyResponseEntity>()
                .HasOptional(e => e.Assignment)
                .WithOptionalPrincipal(a => a.Response);

            // ========== AnswerEntity ==========
            modelBuilder.Entity<AnswerEntity>()
                .ToTable("Answers")
                .HasKey(e => e.Id);

            modelBuilder.Entity<AnswerEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<AnswerEntity>()
                .Property(e => e.ResponseId)
                .IsRequired();

            modelBuilder.Entity<AnswerEntity>()
                .Property(e => e.QuestionId)
                .IsRequired();

            modelBuilder.Entity<AnswerEntity>()
                .Property(e => e.Score)
                .HasPrecision(5, 2);

            modelBuilder.Entity<AnswerEntity>()
                .HasRequired(e => e.Response)
                .WithMany(r => r.Answers)
                .HasForeignKey(e => e.ResponseId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<AnswerEntity>()
                .HasRequired(e => e.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(e => e.QuestionId)
                .WillCascadeOnDelete(false);

            // ========== AnswerChoiceEntity ==========
            modelBuilder.Entity<AnswerChoiceEntity>()
                .ToTable("AnswerChoices")
                .HasKey(e => e.Id);

            modelBuilder.Entity<AnswerChoiceEntity>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<AnswerChoiceEntity>()
                .Property(e => e.AnswerId)
                .IsRequired();

            modelBuilder.Entity<AnswerChoiceEntity>()
                .Property(e => e.OptionId)
                .IsRequired();

            modelBuilder.Entity<AnswerChoiceEntity>()
                .HasRequired(e => e.Answer)
                .WithMany(a => a.Choices)
                .HasForeignKey(e => e.AnswerId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<AnswerChoiceEntity>()
                .HasRequired(e => e.Option)
                .WithMany()
                .HasForeignKey(e => e.OptionId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}