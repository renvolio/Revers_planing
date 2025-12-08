namespace Revers_planing.Services;

public interface ITaskService
{
    Task<IEnumerable<Task_>> GetByProjectForTeamAsync(Guid projectId, Guid teamId);
    Task<Task_> CreateAsync(Guid studentId, Guid subjectId, Guid projectId, CreateTaskDTO dto);
    Task<Task_> UpdateAsync(Guid studentId, Guid subjectId, Guid taskId, UpdateTaskDTO dto);
    Task DeleteAsync(Guid studentId, Guid subjectId, Guid taskId, bool cascade);
}


