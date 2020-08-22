using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiJWTDemo.DTOS;
using WebApiJWTDemo.Models;
using WebApiJWTDemo.Services;

namespace WebApiJWTDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService userService;
        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SignupRequestDTO signupRequest)
        {
            var user = new User();
            user.FirstName = signupRequest.FirstName;
            user.LastName = signupRequest.LastName;
            user.Email = signupRequest.Email;
            user.PhoneNumber = signupRequest.PhoneNumber;
            user.UserName = signupRequest.Email.Split('@')[0];
            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;
            var accessToken = await userService.Signup(user, signupRequest.Password);
            if (accessToken != null)
            {
                return Ok(accessToken);
            }

            return BadRequest("something went wrong");
            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm]IFormCollection keyValues)
        {
            

            var accessToken = await userService.Authenticate(keyValues["email"].ToString(),keyValues["password"].ToString());
            if (accessToken != null)
            {
                return Ok(accessToken);
            }

            return BadRequest("something went wrong");

        }
    }
}
