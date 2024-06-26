using JwtRoleAuthentication.Data;
using JwtRoleAuthentication.Enums;
using JwtRoleAuthentication.Models;
using JwtRoleAuthentication.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtRoleAuthentication.Controllers;

[ApiController]
[Route("/api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;

    public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        TokenService tokenService, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }


    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if the phone number already exists
        var userWithSamePhoneNumber = await _context.Users.FirstOrDefaultAsync(user => user.PhoneNumber == request.PhoneNumber);
        if (userWithSamePhoneNumber != null)
        {
            return BadRequest(new { message = "A user with this phone number already exists." });
        }

        var result = await _userManager.CreateAsync(
            new ApplicationUser { UserName = request.Username, Email = request.Email, Role = request.Role, PhoneNumber = request.PhoneNumber },
            request.Password!
        );

        if (result.Succeeded)
        {
            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email, role = Role.User }, request);
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }


    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.Users.SingleOrDefaultAsync(user => user.PhoneNumber == request.PhoneNumber);

        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password!);

        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }

        var userInDb = _context.Users.FirstOrDefault(user => user.PhoneNumber == request.PhoneNumber);

        if (userInDb is null)
        {
            return Unauthorized();
        }

        var accessToken = _tokenService.CreateToken(userInDb);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Username = userInDb.UserName,
            Email = userInDb.Email,
            PhoneNumber = userInDb.PhoneNumber,
            Token = accessToken,
        });
    }
}