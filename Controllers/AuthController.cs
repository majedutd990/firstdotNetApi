using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthController(IConfiguration configuration)
        {
            _config = configuration;
            _dapper = new DataContextDapper(configuration);
        }

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


            byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

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
                return Ok();
            }

            throw new InvalidOperationException("failed to register user");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlFprHashAndSalt = @"SELECT [Email],
                                        [PasswordHash],
                                        [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email='" +
                                       userForLogin.Email + "'";
            UserForLogInConfirmationDto userToConfirm =
                _dapper.LoadDataSingle<UserForLogInConfirmationDto>(sqlFprHashAndSalt);
            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userToConfirm.PasswordSalt);

            // passwordHash==userToConfirm.PasswordHash wont work compares pointers

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userToConfirm.PasswordHash[index])
                {
                    return StatusCode(401, "incorrect password");
                }
            }

            return Ok();
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                                            Convert.ToBase64String(passwordSalt);
            byte[] passwordHash = KeyDerivation.Pbkdf2(password,
                Encoding.ASCII.GetBytes(passwordSaltPlusString),
                KeyDerivationPrf.HMACSHA256,
                1000,
                256 / 8);
            return passwordHash;
        }
    }
}