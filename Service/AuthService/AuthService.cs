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
using System.Text.RegularExpressions;

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
        // needed
        private int GetCurrentUserId() 
        {
            return int.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private User GetCurrentUser() 
        {
            return _context.Users.FirstOrDefault(u => u.Id == GetCurrentUserId())!;
        }

        // big boios
        public ServiceResponse<string> Login(LoginUserDto request)
        {
            var response = new ServiceResponse<string>();
            var user = _context.Users.FirstOrDefault(u => u.Username == request.UsernameOrEmail);
            if(user is null)
            {
                user = _context.Users.FirstOrDefault(u => u.Email == request.UsernameOrEmail);
                if(user is null)
                {
                    response.Success = false;
                    response.Message = "No user with this username or email";
                    return response;
                }
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
            if(!EmailIsValid(request.Email))
            {
                response.Success = false;
                response.Message = "Email is not valid";
                return response;
            }
            VoidServiceResponse passValid = PasswordIsValid(request.Password);
            if(!passValid.Success)
            {
                response.Success = false;
                response.Message = passValid.Message;
                return response;
            }
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwrodSalt);
            var user = new User
            {
                Username = request.Username.Trim(),
                Role = request.Role,
                PasswordHash = passwordHash,
                PasswordSalt = passwrodSalt,
                Email = request.Email.Trim()
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
            var user = GetCurrentUser();
            if(VerifyPassword(request.currentPassword, user!.PasswordHash, user.PasswordSalt))
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
        
        public ServiceResponse<GetUserDto> ResetEmail(string email)
        {
            ServiceResponse<GetUserDto> response = new ServiceResponse<GetUserDto>();
            User user = GetCurrentUser();
            if(EmailExists(email))
            {
                response.Success = false;
                response.Message = "Email Already In Use";
                return response;
            }
            if(!EmailIsValid(email))
            {
                response.Success = false;
                response.Message = "Email is not valid";
                return response;
            }
            user.Email = email;
            user.IsValidated = false;
            _context.SaveChanges();
            response.Message = "You have changed emails, try revalidating it with the code";
            response.Data = _mapper.Map<GetUserDto>(user);
            return response;
        }

        public Task SendValidationEmail()
        {
            var user = GetCurrentUser();
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
            var user = GetCurrentUser();
            if(code == user!.ValidationCode)
            {
                user.IsValidated = true;
                user.ValidationCode = string.Empty;
                _context.SaveChanges();
                response.Message = "Your email has been validated";
                response.Data = _mapper.Map<GetUserDto>(user);
                return response;
            }
            response.Message = "This is the wrong code";
            response.Success = false;
            return response;
        }
        // suporte boios
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

        public bool EmailIsValid(string email)
        {
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";    
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);    
            return regex.IsMatch(email);
        }

        public VoidServiceResponse PasswordIsValid(string password)
        {
            var response = new VoidServiceResponse();
            response.Message = "Invalid password. Your password must have letters, have numbers, be at least 8 characters long and not have any special characters. ";
            if(password.Length < 8)
            {
                response.Success = false;
                response.Message += "Your password is too short";
                return response;
            }
            if(Regex.IsMatch(password , @"^[a-zA-Z]+$")) // só letra
            {
                response.Success = false;
                response.Message += "It seems that your password only contains letters";
                return response;
            }
            if(Regex.IsMatch(password, @"^[0-9]+$")) // so numero
            {
                response.Success = false;
                response.Message += "It seems that your password only contains numbers";
                return response;
            }
            if(!Regex.IsMatch(password, @"^[a-zA-Z0-9]+$")) // algo alem de letra e numero
            {
                response.Success = false;
                response.Message += "It seems that you have used special characters";
                return response;
            }
            response.Message = string.Empty;
            return response;
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

    }
}
