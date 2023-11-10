using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockTradeApi3.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860


namespace StockTradeApi3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class LoginRegisterController : ControllerBase
    {
        private readonly JwtSettings jwtSetting;
        private Db1Context db1;

        public LoginRegisterController (IOptions<JwtSettings> jwtsetting,Db1Context db)
        {
            jwtSetting = jwtsetting.Value;
            db1 = db;
        }

        [HttpPost("Login")]
        public IActionResult login([FromBody]User user)
        {
            var user2 = CheckCredentials(user);
            if (user2 == null)
                return NotFound("user couldn't find");

            var token = CreateToken(user2);
            return Ok(token);
        }


        [HttpPost("Register")]
        public User Add([FromBody] User user)
        {
            User user2 = user;
            user.Role = "user";
            db1.Users.Add(user2);
            db1.SaveChanges();
            return user2;
        }




        private string CreateToken(User user)
        {
            if (jwtSetting.Key == null)
                throw new Exception("Key cannot be null");
            var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key));
            var credentials = new SigningCredentials(SecurityKey,SecurityAlgorithms.HmacSha256);

            var claimSeries = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Username!),
                new Claim(ClaimTypes.Role , user.Role!)
            };
            var token = new JwtSecurityToken(jwtSetting.Issuer, jwtSetting.Audience, claimSeries,expires:DateTime.Now.AddHours(8) ,signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private User? CheckCredentials(User user)
        {

            return db1.Users.FirstOrDefault(x => x.Username!.ToLower() == user.Username && x.Password == user.Password);
        }

    }
}

