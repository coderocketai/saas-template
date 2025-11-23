using CodeRocket.Common.Dto;
using CodeRocket.Common.Models.Users;
using CodeRocket.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CodeRocket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService service, ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<User>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<User>>> GetAllUsers([FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await service.GetAllUsersAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all users");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving users");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        try
        {
            var user = await service.GetUserByIdAsync(id);
            
            if (user == null)
            {
                logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound($"User with ID {id} not found");
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving user");
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    /// <param name="user">User data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUser = await service.CreateUserAsync(user);
            
            logger.LogInformation("User created with ID {UserId}", createdUser.Id);
            
            return CreatedAtAction(
                nameof(GetUserById), 
                new { id = createdUser.Id }, 
                createdUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating user");
        }
    }

    /// <summary>
    /// Update existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="user">Updated user data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existingUser = await service.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                logger.LogWarning("User with ID {UserId} not found for update", id);
                return NotFound($"User with ID {id} not found");
            }

            var updatedUser = await service.UpdateUserAsync(user);
            
            logger.LogInformation("User with ID {UserId} updated", id);
            
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating user");
        }
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await service.DeleteUserAsync(id);
            
            if (!result)
            {
                logger.LogWarning("User with ID {UserId} not found for deletion", id);
                return NotFound($"User with ID {id} not found");
            }
            
            logger.LogInformation("User with ID {UserId} deleted", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting user");
        }
    }
}