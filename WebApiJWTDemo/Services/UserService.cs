using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApiJWTDemo.Helpers;
using WebApiJWTDemo.Models;

namespace WebApiJWTDemo.Services
{
    public interface IUserService
    {
        public Task<string> Authenticate(string email,string password);
        public Task<string> Signup(User user,string password);
    }
    public class UserService:IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        
        public UserService(ApplicationDbContext db, IOptions<AppSettings> appSettings, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _db = db;
            _appSettings = appSettings.Value;
            this.signInManager = signInManager;
            this.userManager = userManager;
           

        }

        public async Task<string> Authenticate(string email,string password)
        {
            var user = _db.Users.Where(u => u.Email == email).FirstOrDefault();

            if (user == null)
            {
                return null;
            }
            // return null if user not found

            
            var result = await signInManager.CheckPasswordSignInAsync(user, password,false);
            

            if (result.Succeeded)
            {
                
                
                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.NameIdentifier,user.Id)
                    }),

                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };
                tokenDescriptor.Expires = DateTime.Now.AddYears(2);
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(token);

                return accessToken;
            }

            return null;
        }

        public async Task<string> Signup(User user,string password)
        {
            var signupResult = await userManager.CreateAsync(user,password);
            if (signupResult.Succeeded)
            {
                var jwtToken = await Authenticate(user.Email, password);
                return jwtToken;
            }
            return null;
        }
    }
}
