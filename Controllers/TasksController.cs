using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pomodoro_api.Data;
using pomodoro_api.Models;
using System.Collections;
using System.Security.Claims;

namespace pomodoro_api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable>> GetTasks()
    {
        var userId = GetUserId();
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetTask(int id)
    {
        var userId = GetUserId();
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult> CreateTask(TaskItem task)
    {
        task.UserId = GetUserId();
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(int id, TaskItem task)
    {
        var userId = GetUserId();

        if (id != task.Id)
            return BadRequest();

        var existingTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (existingTask == null)
            return NotFound();

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Priority = task.Priority;
        existingTask.Category = task.Category;
        existingTask.IsCompleted = task.IsCompleted;

        if (task.IsCompleted && existingTask.CompletedAt == null)
            existingTask.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingTask);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
