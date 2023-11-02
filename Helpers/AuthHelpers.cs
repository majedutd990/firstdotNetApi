using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelpers
    {
        private IConfiguration _config;
        private readonly DataContextDapper _dapper;
        private readonly AuthHelpers _auth;


        public AuthHelpers(IConfiguration configuration)
        {
            _config = configuration;
            _dapper = new DataContextDapper(configuration);
        }

        public string CreateToken(int userId, string email)
        {
            string? tK = _config.GetSection("AppSettings:TokenKey").Value;
            Console.WriteLine(tK);

            Claim[] claims = new Claim[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("email", email)
            };
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tK));
            SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            SecurityToken token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
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

        public bool SetPassword(UserForLoginDto user)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }


            byte[] passwordHash = GetPasswordHash(user.Password, passwordSalt);

            string sqlAddAuth = @"EXEC TutorialAppSchema.spRegisteratioin_Upsert 
                                @Email = @EmailParam,
                                @PasswordHash = @PasswordHashParam,
                                @PasswordSalt = @PasswordSaltParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@EmailParam", user.Email, DbType.String);
            sqlParams.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParams.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);
            return (_dapper.ExecuteSqlWithParams(sqlAddAuth, sqlParams));
        }
    }
}