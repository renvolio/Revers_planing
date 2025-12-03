using Microsoft.EntityFrameworkCore;
using Revers_planing.Models;

namespace Revers_planing.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Task_> Tasks { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Наследование: User -> Student, Teacher (TPH с дискриминатором)
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Student>("Student")
            .HasValue<Teacher>("Teacher");
        
        // Уникальный индекс на Email для быстрого поиска при аутентификации
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // 1) Команда - Проект: ассоциация many-to-many
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Teams)
            .WithMany(t => t.Projects);

        // 2) Команда (композиция, ромб у команды) - Task_ (many)
        // Удаление команды каскадно удаляет задачи
        modelBuilder.Entity<Task_>()
            .HasOne(t => t.Team)
            .WithMany(team => team.Tasks)
            .HasForeignKey(t => t.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3) Команда (many) - Subject (1, ромб у Subject)
        // Удаление предмета каскадно удаляет команды
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Subject)
            .WithMany(s => s.Teams)
            .HasForeignKey(t => t.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4) Subject (1, композиция) - Project (many)
        // Удаление предмета каскадно удаляет проекты
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Subject)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5) Project (агрегация, 1) - Task_ (many)
        // Task_ может жить без проекта, при удалении проекта ссылка очищается
        modelBuilder.Entity<Task_>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // 6) Самоагрегация Task_: 1 ко many (ParentTask - Children)
        // Не даём каскадно удалять дочерние задачи, чтобы явно контролировать это в коде
        modelBuilder.Entity<Task_>()
            .HasOne(t => t.ParentTask)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // 7) Subject (many) - Teacher (many), ассоциация
        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.Subjects)
            .WithMany(s => s.Teachers);

        // Subject (many) - Student (many), ассоциация
        modelBuilder.Entity<Student>()
            .HasMany(st => st.Subjects)
            .WithMany(s => s.Students);

        // Task_ (many) - Student (many), ассоциация
        modelBuilder.Entity<Task_>()
            .HasMany(t => t.Students)
            .WithMany(s => s.Tasks);

        // 8) Project (many) - Teacher (1), ассоциация
        // При удалении преподавателя проекты остаются, ссылка очищается
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Teacher)
            .WithMany(t => t.Projects)
            .HasForeignKey(p => p.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}