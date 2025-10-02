
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DeliveryApp.Web.Controllers
{
    [Route("api/debug")]
    [ApiController]
    [Authorize]
    public class DebugController : ControllerBase
    {
        private readonly Volo.Abp.Identity.IdentityUserManager _userManager;
        
        public DebugController(Volo.Abp.Identity.IdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            var isInAdminRole = User.IsInRole("admin");
            var isInManagerRole = User.IsInRole("manager");
            var claimsIdentity = User.Identity as ClaimsIdentity;
            
            return Ok(new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                AuthenticationType = User.Identity.AuthenticationType,
                NameClaimType = claimsIdentity?.NameClaimType,
                RoleClaimType = claimsIdentity?.RoleClaimType,
                IsInAdminRole = isInAdminRole,
                IsInManagerRole = isInManagerRole,
                Claims = claims
            });
        }

        [HttpPost("assign-admin-role/{email}")]
        [AllowAnonymous] // Temporarily allow anonymous for fixing the role issue
        public async Task<IActionResult> AssignAdminRole(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"User with email {email} not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                if (!roles.Contains("admin"))
                {
                    await _userManager.AddToRoleAsync(user, "admin");
                }
                
                var updatedRoles = await _userManager.GetRolesAsync(user);
                
                return Ok(new 
                { 
                    Message = $"Admin role assigned to {email}",
                    UserRoles = updatedRoles
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("test-admin")]
        [Authorize(Roles = "admin")]
        public IActionResult TestAdmin()
        {
            return Ok(new { Message = "You are an admin!", User = User.Identity.Name });
        }

        [HttpGet("test-admin-or-manager")]
        [Authorize(Roles = "admin,manager")]
        public IActionResult TestAdminOrManager()
        {
            return Ok(new { Message = "You are admin or manager!", User = User.Identity.Name });
        }
    }
}
