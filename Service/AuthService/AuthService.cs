using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Login.Data;
using Login.Models;
using Login.Dtos.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;

namespace Login.Service.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public AuthService(DataContext context, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public ServiceResponse<string> Login(LoginUserDto request)
        {
            var response = new ServiceResponse<string>();
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user is null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }
            if(VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                response.Data = CreateToken(user);
                response.Message = "You have successfully loged in";
                return response;
            }
            response.Success = false;
            response.Message = "Wrong passowrd";
            return response;
        }

        public ServiceResponse<GetUserDto> Register(RegisterUserDto request)
        {
            var response = new ServiceResponse<GetUserDto>();
            if(UserExists(request.Username))
            {
                response.Success = false;
                response.Message = "User Already Exists";
                return response;
            }
            if(EmailExists(request.Email))
            {
                response.Success = false;
                response.Message = "Email Already In Use";
                return response;
            }
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwrodSalt);
            var user = new User
            {
                Username = request.Username,
                Role = request.Role,
                PasswordHash = passwordHash,
                PasswordSalt = passwrodSalt,
                Email = request.Email
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            response.Message = "Your user has been created";
            response.Data = _mapper.Map<GetUserDto>(user);
            return response;
        }
        public ServiceResponse<GetUserDto> ResetPasword(ResetUserPasswordDto request)
        {
            var response = new ServiceResponse<GetUserDto>();
            int userId = int.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if(VerifyPassword(request.currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwrodSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwrodSalt;
                _context.SaveChanges();
                response.Message = "You have changed you password";
                response.Data = _mapper.Map<GetUserDto>(user);
                return response;
            }
            response.Success = false;
            response.Message = "Please insert your old password to confirm identity n shit lol";
            return response;
        }
        public bool UserExists(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            if(user is null)
            {
                return false;
            }
            return true;
        }

        public bool EmailExists(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if(user is null)
            {
                return false;
            }
            return true;
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPassword(string password, byte[] dbPasswordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return hash.SequenceEqual(dbPasswordHash);
            }
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var appSettingsPassword = _configuration.GetSection("AppSettings:Token").Value;
            if (appSettingsPassword is null)
            {
                throw new Exception("AppSettingsPassword wasn't found");
            }
            SymmetricSecurityKey key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(appSettingsPassword));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public string CreateRandomCode()
        {
            Random r = new Random();
            int randomInt = r.Next(1000000);
            string code = randomInt.ToString("D6");
            return code;
        }

        public Task SendValidationEmail()
        {
            int userId = int.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            string code = CreateRandomCode();
            user!.ValidationCode = code;
            _context.SaveChanges();
            string receiver = user!.Email;
            string subject = "Email validation code";
            string message = $"Insert the code {code} to validate your email";
            var client = new SmtpClient("smtp.office365.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("pedroarthurcosta@hotmail.com", "bmnsslihacpmasbc")
            };
            return client.SendMailAsync( new MailMessage(
                from: "pedroarthurcosta@hotmail.com",
                to: receiver,
                subject,
                message
            ));
        }

        public ServiceResponse<GetUserDto> EnterValidationCode(string code)
        {
            var response = new ServiceResponse<GetUserDto>();
            int userId = int.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if(code == user!.ValidationCode)
            {
                user.IsValidated = true;
                response.Message = "Your email has been validated";
                response.Data = _mapper.Map<GetUserDto>(user);
                return response;
            }
            response.Message = "This is the wrong code";
            response.Success = false;
            return response;
        }
    }
}
