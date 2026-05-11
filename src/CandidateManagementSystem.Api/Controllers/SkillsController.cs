using CandidateManagementSystem.Api.DTOs.Skill;
using CandidateManagementSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CandidateManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController(ISkillService skillService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var skills = await skillService.GetAllAsync(ct);
        return Ok(skills);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var skill = await skillService.GetByIdAsync(id, ct);
        return skill is null ? NotFound() : Ok(skill);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSkillRequest request, CancellationToken ct)
    {
        var skill = await skillService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = skill.Id }, skill);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await skillService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
