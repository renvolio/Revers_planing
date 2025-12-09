using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Revers_planing.Data;
using Revers_planing.Models;

namespace Revers_planing.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teachers = await _context.Teachers
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Email,
                Role = "Teacher",
                Position = t.Position
            })
            .ToListAsync();

        var students = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Email,
                Role = "Student",
                GroupNumber = s.GroupNumber
            })
            .ToListAsync();

        return Ok(new
        {
            Teachers = teachers,
            Students = students
        });
    }
}

