using Revers_planing.DTOs.Task;
using Revers_planing.Models;

namespace Revers_planing.Services;

public interface ITaskService
{
    Task<IEnumerable<Task_>> GetByProjectForTeamAsync(Guid projectId, Guid teamId);
    Task<IEnumerable<Task_>> GetByProjectForTeacherAsync(Guid projectId, Guid subjectId, Guid teacherId);
    Task<Task_> CreateAsync(Guid studentId, Guid subjectId, Guid projectId, CreateTaskDTO dto);
    Task<Task_> UpdateAsync(Guid studentId, Guid subjectId, Guid projectId, Guid taskId, UpdateTaskDTO dto);
    Task DeleteAsync(Guid studentId, Guid subjectId, Guid projectId, Guid taskId, bool cascade);
}

