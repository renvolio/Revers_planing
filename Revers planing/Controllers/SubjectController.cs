using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Revers_planing.Data;
using Revers_planing.DTOs.Subject;
using Revers_planing.DTOs.Subject.Team;
using Revers_planing.Models;
using Revers_planing.Services;

namespace Revers_planing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly ApplicationDbContext _context;

    public SubjectController(ISubjectService subjectService, ApplicationDbContext context)
    {
        _subjectService = subjectService;
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateSubjectDTO dto)
    {
        var teacherId = GetUserId();
        var subject = await _subjectService.CreateAsync(teacherId, dto);
        return Ok(ToDto(subject));
    }

    [HttpGet("teacher")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetForTeacher()
    {
        var teacherId = GetUserId();
        var subjects = await _subjectService.GetForTeacherAsync(teacherId);
        return Ok(subjects.Select(ToDto));
    }

    [HttpGet("available")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetForStudent()
    {
        var studentId = GetUserId();
        var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId)
                      ?? throw new InvalidOperationException("студент не найден");

        var subjects = await _subjectService.GetForStudentAsync(studentId, student.GroupNumber);
        return Ok(subjects.Select(ToDto));
    }

    [HttpGet("{subjectId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid subjectId)
    {
        var subject = await _subjectService.GetByIdAsync(subjectId)
                      ?? throw new InvalidOperationException("предмет не найден");

        if (User.IsInRole("Student"))
        {
            var studentId = GetUserId();
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId)
                          ?? throw new InvalidOperationException("студент не найден");

            if (!subject.AllowedGroups.Contains(student.GroupNumber))
            {
                return Forbid();
            }
        }
        else if (User.IsInRole("Teacher"))
        {
            var teacherId = GetUserId();
            if (!subject.Teachers.Any(t => t.Id == teacherId))
            {
                return Forbid();
            }
        }

        return Ok(ToDto(subject));
    }

    [HttpPut("{subjectId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(Guid subjectId, [FromBody] UpdateSubjectDTO dto)
    {
        var teacherId = GetUserId();
        var subject = await _subjectService.UpdateAsync(subjectId, dto, teacherId);
        return Ok(ToDto(subject));
    }

    [HttpDelete("{subjectId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(Guid subjectId)
    {
        var teacherId = GetUserId();
        await _subjectService.DeleteAsync(subjectId, teacherId);
        return NoContent();
    }

    [HttpPost("{subjectId:guid}/join")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> JoinSubject(Guid subjectId, [FromBody] JoinSubjectDTO dto)
    {
        var studentId = GetUserId();
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId)
                      ?? throw new InvalidOperationException("студент не найден");

        var (team, subject) = await _subjectService.JoinSubjectAsync(subjectId, studentId, dto.TeamNumber, dto.TeamName, student.GroupNumber);

        return Ok(new
        {
            Subject = ToDto(subject),
            Team = new { team.Id, team.Number, team.Name }
        });
    }

    [HttpGet("{subjectId:guid}/team/members")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetTeamMembers(Guid subjectId)
    {
        var studentId = GetUserId();

        var student = await _context.Students
            .Include(s => s.Team)
            .ThenInclude(t => t!.Students)
            .FirstOrDefaultAsync(s => s.Id == studentId)
            ?? throw new InvalidOperationException("студент не найден");

        if (student.Team == null || student.Team.SubjectId != subjectId)
        {
            // Return empty list instead of throwing error
            return Ok(new List<TeamMemberDTO>());
        }

        var members = student.Team.Students
            .Select(s => new TeamMemberDTO
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email
            })
            .ToList();

        return Ok(members);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (userId == null) throw new UnauthorizedAccessException("пользователь не найден в токене");
        return Guid.Parse(userId);
    }

    private static SubjectDTO ToDto(Subject subject) =>
        new()
        {
            Id = subject.Id,
            Name = subject.Name,
            StartDate = subject.StartDate,
            EndDate = subject.EndDate,
            Discription = subject.Discription,
            AllowedGroups = subject.AllowedGroups
        };
}


