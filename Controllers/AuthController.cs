using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

        public AuthController(IConfiguration configuration)
        {
            _auth =
                new AuthHelpers(configuration);
            _dapper = new DataContextDapper(configuration);
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
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }


            byte[] passwordHash = _auth.GetPasswordHash(userForRegistration.Password, passwordSalt);

            string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth ([Email],[PasswordHash],[PasswordSalt]) VALUES('" +
                                userForRegistration.Email + "',@PasswordHash,@PasswordSalt)";
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
            passwordSaltParameter.Value = passwordSalt;
            SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
            passwordHashParameter.Value = passwordHash;
            sqlParams.Add(passwordHashParameter);
            sqlParams.Add(passwordSaltParameter);
            if (_dapper.ExecuteSqlWithParams(sqlAddAuth, sqlParams))
            {
                string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users (
                                  [FirstName],
                                  [LastName],
                                  [Email],
                                  [Gender],
                                  [Active]) VALUES('" + userForRegistration.FirstName + "','" +
                                    userForRegistration.LastName + "','" + userForRegistration.Email +
                                    "','" +
                                    userForRegistration.Gender + "','1')";

                if (!_dapper.ExecuteSql(sqlAddUser)) throw new InvalidOperationException("failed to add user");
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
            string sqlFprHashAndSalt = @"SELECT [Email],
                                        [PasswordHash],
                                        [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email='" +
                                       userForLogin.Email + "'";
            IEnumerable<UserForLogInConfirmationDto> userToConfirm =
                _dapper.LoadData<UserForLogInConfirmationDto>(sqlFprHashAndSalt);
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
    }
}