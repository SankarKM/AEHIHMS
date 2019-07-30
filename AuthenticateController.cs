using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IHMS.Data.Model;
using IHMS.Data.Repository;
using IHMS.Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class AuthenticateController : Controller
    {
        private IConfiguration _config;

        public AuthenticateController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult CreateToken([FromBody]LoginModel login)
        {
            //var test = Request.Body;
            //var test1 = Request.Form;
            //LoginModel email = new LoginModel();
            IActionResult response = Unauthorized();
            UserModel user = null;

            if (login.Username == "venu" && login.Password == "pass")
                user = new UserModel { Name = "Venu", Email = "venu@google.com" };

            if (user != null)
            {
                var tokenString = BuildToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string BuildToken(UserModel user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

           var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"], claims, notBefore: DateTime.Now,
              expires: DateTime.Now.AddHours(1),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private class UserModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public DateTime Birthdate { get; set; }
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
