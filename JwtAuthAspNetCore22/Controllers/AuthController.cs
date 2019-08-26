using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtAuthAspNetCore22.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthAspNetCore22.Controllers
{
   [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;



        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
               
        /// <summary>
        /// Allow user to register.
        /// endpoint: verb:POST, url:https://localhost:6600/api/auth/register
        /// </summary>
        /// <param name="model">{"Username":"username@hotmail.com","Password":"userpassword"}</param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        public async Task<ActionResult> InsertUser([FromBody] RegisterViewModel model)
        {
            var user = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
            }
            return Ok(new { Username = user.UserName });
        }


        /// <summary>
        /// Allow user to login.
        /// endpoint: verb:POST, url:https://localhost:6600/api/auth/login
        /// </summary>
        /// <param name="model">{"Username":"username@hotmail.com","Password":"userpassword"}</param>
        /// <returns></returns>
        [Route("login")] 
        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            var seconds= DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var claim = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim("id",user.Id),
                   // new Claim(JwtRegisteredClaimNames.AuthTime, Convert.ToString(seconds)),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Role, "Reader"),
                   new Claim("Custom_Claim", "Custom_Claim")
            };

                var signinKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));

                int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);

                var token = new JwtSecurityToken(
                  issuer: _configuration["Jwt:Site"],
                  audience: _configuration["Jwt:Site"],
                  expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                  signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                  ,claims:claim //add claims to token
                );

                return Ok(
                  new
                  {
                      token = new JwtSecurityTokenHandler().WriteToken(token),
                      expiration = token.ValidTo
                  });
            }
            return Unauthorized();
        }


        /// <summary>
        /// This method demonstrate how to create and return jwt token asp.net core api.
        /// endpoint: verb:POST, url:https://localhost:6600/api/auth/token
        /// </summary>
        /// <returns></returns>
        /// JWT Authorization in ASP.NET Core 2.1 Web API - C# https://www.youtube.com/watch?v=7tgLuJ__ZKU
        [HttpPost("token")]
        public ActionResult GetToken()
        {
            /* security key */
            var signinKey = _configuration["Jwt:SigningKey"];
            
            /* symmetric security key */
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signinKey));

            /* signing credentials */
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            /* add claims */
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            claims.Add(new Claim(ClaimTypes.Role, "Reader"));
            claims.Add(new Claim("Custom_Claim", "Custom_Claim"));



            /* create token */
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt: Site"],
                audience: _configuration["Jwt:Site"],
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: signingCredentials,
                claims:claims //assign claims
                );

            /* return token with expiration value (token : token, expiration: token_expiration */
            return Ok(
                new
                {
                    token =new JwtSecurityTokenHandler().WriteToken(token),
                    expiration=token.ValidTo
                });            
        }

       



    }
}
