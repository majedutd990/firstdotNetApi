using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using DotnetAPI.Helpers;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelpers _auth;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration configuration)
        {
            _auth =
                new AuthHelpers(configuration);
            _dapper = new DataContextDapper(configuration);
            _reusableSql = new ReusableSql(configuration);
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
            }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password != userForRegistration.PasswordConfirmation)
                throw new InvalidOperationException("passwords not match");
            string sqlCheckUserExist = "SELECT * FROM TutorialAppSchema.Auth WHERE Email = '" +
                                       userForRegistration.Email + "'";

            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExist);
            if (existingUsers.Count() != 0)
                throw new InvalidOperationException("email already exist");

            if (_auth.SetPassword(new UserForLoginDto()
                {
                    Email = userForRegistration.Email,
                    Password = userForRegistration.PasswordConfirmation,
                }))
            {
                UserComplete uComplete = _mapper.Map<UserComplete>(userForRegistration);
                bool done = _reusableSql.UpsertUser(uComplete);
                uComplete.Active = true;

                if (!done) throw new InvalidOperationException("failed to add user");
                return Ok();
            }

            throw new InvalidOperationException("failed to register user");
        }


        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string getUserIdSql = @"SELECT  [UserId],
                                            [FirstName],
                                            [LastName],
                                            [Email],
                                            [Gender],
                                            [Active] FROM TutorialAppSchema.Users WHERE UserId='" +
                                  User.FindFirst("userId")?.Value +
                                  "'";


            User user = _dapper.LoadDataSingle<User>(getUserIdSql);
            return _auth.CreateToken(user.UserId, user.Email);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlFprHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation @Email = @EmailParam";
            DynamicParameters sqlParams = new DynamicParameters();
            // SqlParameter email = new SqlParameter("@EmailParam", SqlDbType.NVarChar);
            // email.Value = userForLogin.Email;
            // sqlParams.Add(email);
            sqlParams.Add("@EmailParam", userForLogin.Email, DbType.String);
            IEnumerable<UserForLogInConfirmationDto> userToConfirm =
                _dapper.LoadDataWithParameters<UserForLogInConfirmationDto>(sqlFprHashAndSalt, sqlParams);
            if (!userToConfirm.Any())
            {
                return StatusCode(404, "user not found");
            }

            byte[] passwordHash = _auth.GetPasswordHash(userForLogin.Password, userToConfirm.First().PasswordSalt);

            // passwordHash==userToConfirm.PasswordHash wont work compares pointers

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userToConfirm.First().PasswordHash[index])
                {
                    return StatusCode(401, "incorrect password");
                }
            }

            string getUserIdSql = @"SELECT  [UserId],
                                            [FirstName],
                                            [LastName],
                                            [Email],
                                            [Gender],
                                            [Active] FROM TutorialAppSchema.Users WHERE Email='" + userForLogin.Email +
                                  "'";


            User user = _dapper.LoadDataSingle<User>(getUserIdSql);

            return Ok(new Dictionary<string, string>
            {
                {
                    "token", _auth.CreateToken(user.UserId, user.Email)
                },
            });
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPass)
        {
            if (_auth.SetPassword(userForSetPass))
            {
                return Ok();
            }

            return StatusCode(401, "bad request");
        }
    }
}