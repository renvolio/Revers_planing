using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Revers_planing.Data;
using Revers_planing.DTOs.Task;
using Revers_planing.Models;
using Revers_planing.Services;

namespace Revers_planing.Controllers;

[ApiController]
[Route("api/subjects/{subjectId:guid}/projects/{projectId:guid}/tasks")]
[Authorize(Roles = "Student")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ApplicationDbContext _context;

    public TaskController(ITaskService taskService, ApplicationDbContext context)
    {
        _taskService = taskService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid subjectId, Guid projectId)
    {
        var team = await GetStudentTeam(subjectId);
        var tasks = await _taskService.GetByProjectForTeamAsync(projectId, team.Id);
        return Ok(tasks.Select(ToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid subjectId, Guid projectId, [FromBody] CreateTaskDTO dto)
    {
        var studentId = GetUserId();
        var task = await _taskService.CreateAsync(studentId, subjectId, projectId, dto);
        return Ok(ToDto(task));
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid subjectId, Guid projectId, Guid taskId, [FromBody] UpdateTaskDTO dto)
    {
        var studentId = GetUserId();
        var task = await _taskService.UpdateAsync(studentId, subjectId, taskId, dto);
        return Ok(ToDto(task));
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid subjectId, Guid projectId, Guid taskId, [FromQuery] bool cascade = false)
    {
        var studentId = GetUserId();
        await _taskService.DeleteAsync(studentId, subjectId, taskId, cascade);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) throw new UnauthorizedAccessException("пользователя нет в токене");
        return Guid.Parse(userId);
    }

    private async Task<Team> GetStudentTeam(Guid subjectId)
    {
        var studentId = GetUserId();
        var student = await _context.Students
            .Include(s => s.Team)
            .FirstOrDefaultAsync(s => s.Id == studentId)
            ?? throw new InvalidOperationException("студент не найден");

        if (student.Team == null || student.Team.SubjectId != subjectId)
        {
            throw new UnauthorizedAccessException("студент не состоит в команде предмета");
        }

        return student.Team;
    }

    private static TaskDTO ToDto(Task_ task) => new()
    {
        Id = task.Id,
        Name = task.Name,
        Description = task.Description,
        DeadlineAssessment = task.DeadlineAssessment,
        EndDate = task.EndDate,
        StartDate = task.StartDate,
        TeamId = task.TeamId,
        ProjectId = task.ProjectId,
        ParentTaskId = task.ParentTaskId,
        Status = task.Status,
        ResponsibleStudentId = task.ResponsibleStudentId
    };
}


