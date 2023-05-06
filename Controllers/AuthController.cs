using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Login.Service.AuthService;
using Login.Models;
using Login.Dtos.User;
using Microsoft.AspNetCore.Authorization;

namespace Login.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public ActionResult<ServiceResponse<GetUserDto>> Register(RegisterUserDto request)
        {
            var response = (_authService.Register(request));
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Login")]
        public ActionResult<ServiceResponse<string>> Login(LoginUserDto request)
        {
            var response = (_authService.Login(request));
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [Authorize]
        [HttpPost("Reset Password")]
        public ActionResult<ServiceResponse<GetUserDto>> ResetPassword(ResetUserPasswordDto request)
        {
            var response = _authService.ResetPasword(request);
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}