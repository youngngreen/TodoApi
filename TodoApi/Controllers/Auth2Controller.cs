using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth2Controller : ControllerBase
    {
        
        //public static ApiUser user = new ApiUser();
        private readonly IConfiguration _configuration;
        private readonly todoDBContext _context;
        //private readonly IUserService1 _userService;

        public Auth2Controller(IConfiguration configuration, todoDBContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiUser>> GetApiUser(int id)
        {
            var apiUser = await _context.ApiUsers.FindAsync(id);

            if (apiUser == null)
            {
                return NotFound();
            }

            return apiUser;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiUser>> Register(ApiUserDTO request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            ApiUser user = new ApiUser();

            user.UserName = request.UserName;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.ApiUsers.Add(user);
            await _context.SaveChangesAsync();

            //CreatedAtAction("Register", user);
            //return Ok(user);
            //return Ok(await _context.ApiUsers.ToListAsync());

            return CreatedAtAction("GetApiUser", new { id = user.Id }, user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(ApiUserDTO request)
        {
            var rs = await _context.ApiUsers.ToListAsync();
            foreach (var user in rs)
            {
                if (user.UserName != request.UserName)
                {
                    return BadRequest("User not found.");
                }
                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest("wrong password.");
                }
                string token = CreateToken(user);
                var refreshtoken = GenerateRefreshToken();
                //SetRefreshToken(refreshtoken);
                user.RefreshToken = refreshtoken.Token;
                user.TokenExpires = refreshtoken.Expires;
                user.TokenCreated = refreshtoken.Created;

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(token);
            }
            return BadRequest("login failed");

        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var rs = await _context.ApiUsers.ToListAsync();
            foreach(var user in rs)
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (!user.RefreshToken.Equals(refreshToken))
                {
                    return Unauthorized("Invalid Refresh Token.");
                }
                else if (user.TokenExpires < DateTime.Now)
                {
                    return Unauthorized("Token expried.");
                }
                string token = CreateToken(user);
                //var newRefreshToken = GenerateRefreshToken();
                //SetRefreshToken(newRefreshToken);
                return Ok(token);
            }
            return BadRequest("refesh token failed");
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            return refreshToken;
        }

        //private void SetRefreshToken(RefreshToken newRefreshToken)
        //{
        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Expires = newRefreshToken.Expires
        //    };
        //    Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
        //    user.RefreshToken = newRefreshToken.Token;
        //    user.TokenCreated = newRefreshToken.Created;
        //    user.TokenExpires = newRefreshToken.Expires;
        //}
        private string CreateToken(ApiUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
