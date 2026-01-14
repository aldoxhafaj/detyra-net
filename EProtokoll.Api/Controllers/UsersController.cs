using EProtokoll.Api.Dtos;
using EProtokoll.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _auth.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _auth.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var user = await _auth.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        var user = await _auth.UpdateUserAsync(id, request);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost("{id:int}/roles")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] string role)
    {
        var user = await _auth.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        user.Role = role;
        await _auth.UpdateUserAsync(id, new UpdateUserRequest(user.FullName, role, user.IsActive, user.DepartmentId));
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _auth.DeleteUserAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
