using Revers_planing.DTOs.Project;
using Revers_planing.DTOs.Subject;
using Revers_planing.Models;

namespace Revers_planing.Services;

public interface ISubjectService
{
    Task<Subject> CreateAsync(Guid teacherId, CreateSubjectDTO dto);
    Task<IEnumerable<Subject>> GetForTeacherAsync(Guid teacherId);
    Task<IEnumerable<Subject>> GetForStudentAsync(Guid studentId, int groupNumber);
    Task<Subject?> GetByIdAsync(Guid subjectId);
    Task<Subject> UpdateAsync(Guid subjectId, UpdateSubjectDTO dto, Guid teacherId);
    Task DeleteAsync(Guid subjectId, Guid teacherId);
    Task<(Team Team, Subject Subject)> JoinSubjectAsync(Guid subjectId, Guid studentId, int teamNumber, string? teamName, int groupNumber);
}


