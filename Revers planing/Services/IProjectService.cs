using Revers_planing.DTOs.Project;
using Revers_planing.Models;

namespace Revers_planing.Services;

public interface IProjectService
{
    Task<Project> CreateAsync(Guid teacherId, Guid subjectId, CreateProjectDTO dto);
    Task<IEnumerable<Project>> GetBySubjectForTeacherAsync(Guid teacherId, Guid subjectId);
    Task<IEnumerable<Project>> GetBySubjectForStudentAsync(Guid subjectId, int groupNumber);
    Task<Project> UpdateAsync(Guid teacherId, Guid subjectId, Guid projectId, UpdateProjectDTO dto);
    Task DeleteAsync(Guid teacherId, Guid subjectId, Guid projectId);
}


