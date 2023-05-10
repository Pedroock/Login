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
        ServiceResponse<GetUserDto> ResetEmail(string email);
        Task SendValidationEmail();
        ServiceResponse<GetUserDto> EnterValidationCode(string code);
        bool UserExists(string username);
        bool EmailExists(string email);
        bool EmailIsValid(string email);
        VoidServiceResponse PasswordIsValid(string password);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateToken(User user);
        string  CreateRandomCode();
    }
}