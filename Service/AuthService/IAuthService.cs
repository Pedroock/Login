using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login.Dtos.User;
using Login.Models;

namespace Login.Service.AuthService
{
    public interface IAuthService
    {
        ServiceResponse<string> Login(LoginUserDto request);
        ServiceResponse<GetUserDto> Register(RegisterUserDto request);
        ServiceResponse<GetUserDto> ResetPasword(ResetUserPasswordDto request);
        bool UserExists(string username);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateToken(User user);
    }
}