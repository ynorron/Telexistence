
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Telexistence.Models;

namespace Telexistence.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase {
        private readonly IConfiguration _config;
        public LoginController(IConfiguration config) => _config = config;

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request) {
            if (request.Username == "admin" && request.Password == "password") {
                var claims = new[] 
                { 
                    new Claim(ClaimTypes.Name, request.Username  ),
                    new Claim(ClaimTypes.Role, "Admin" )

                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return Unauthorized();
        }
    }
}
