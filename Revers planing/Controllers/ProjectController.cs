using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Revers_planing.Data;
using Revers_planing.DTOs.Project;
using Revers_planing.Models;
using Revers_planing.Services;

namespace Revers_planing.Controllers;

[ApiController]
[Route("api/subjects/{subjectId:guid}/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ApplicationDbContext _context;

    public ProjectController(IProjectService projectService, ApplicationDbContext context)
    {
        _projectService = projectService;
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get(Guid subjectId)
    {
        if (User.IsInRole("Teacher"))
        {
            var teacherId = GetUserId();
            var projects = await _projectService.GetBySubjectForTeacherAsync(teacherId, subjectId);
            return Ok(projects.Select(ToDto));
        }

        var studentId = GetUserId();
        var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId)
                      ?? throw new InvalidOperationException(" студент не найден");
        var studentProjects = await _projectService.GetBySubjectForStudentAsync(subjectId, student.GroupNumber);
        return Ok(studentProjects.Select(ToDto));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create(Guid subjectId, [FromBody] CreateProjectDTO dto)
    {
        var teacherId = GetUserId();
        var project = await _projectService.CreateAsync(teacherId, subjectId, dto);
        return Ok(ToDto(project));
    }

    [HttpPut("{projectId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(Guid subjectId, Guid projectId, [FromBody] UpdateProjectDTO dto)
    {
        var teacherId = GetUserId();
        var project = await _projectService.UpdateAsync(teacherId, subjectId, projectId, dto);
        return Ok(ToDto(project));
    }

    [HttpDelete("{projectId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(Guid subjectId, Guid projectId)
    {
        var teacherId = GetUserId();
        await _projectService.DeleteAsync(teacherId, subjectId, projectId);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) throw new UnauthorizedAccessException("пользователя нет в токене");
        return Guid.Parse(userId);
    }

    private static ProjectDTO ToDto(Project project) =>
        new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            SubjectId = project.SubjectId,
            TeacherId = project.TeacherId,
            StartDate = project.StartDate,
            EndDate = project.EndDate
        };
}


