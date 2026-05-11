using CandidateManagementSystem.Api.DTOs.Candidate;
using CandidateManagementSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CandidateManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController(ICandidateService candidateService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? name,
        [FromQuery] List<int>? skillIds,
        CancellationToken ct)
    {
        var request = new CandidateSearchRequest(name, skillIds);
        var candidates = await candidateService.SearchAsync(request, ct);
        return Ok(candidates);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var candidate = await candidateService.GetByIdAsync(id, ct);
        return candidate is null ? NotFound() : Ok(candidate);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCandidateRequest request, CancellationToken ct)
    {
        var candidate = await candidateService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = candidate.Id }, candidate);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCandidateRequest request, CancellationToken ct)
    {
        var candidate = await candidateService.UpdateAsync(id, request, ct);
        return candidate is null ? NotFound() : Ok(candidate);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await candidateService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> AddSkill(int id, int skillId, CancellationToken ct)
    {
        var candidate = await candidateService.AddSkillAsync(id, skillId, ct);
        if (candidate is null) return NotFound();
        return Ok(candidate);
    }

    [HttpDelete("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkill(int id, int skillId, CancellationToken ct)
    {
        var candidate = await candidateService.RemoveSkillAsync(id, skillId, ct);
        if (candidate is null) return NotFound();
        return Ok(candidate);
    }
}
