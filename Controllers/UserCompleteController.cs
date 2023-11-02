using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserCompleteController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly ReusableSql _reusableSql;

        public UserCompleteController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
            _reusableSql = new ReusableSql(configuration);
        }

        [HttpGet("TestConnection")]
        public DateTime TestConnection()
        {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }

        // GET: api/<UserController>
        [HttpGet("GetUsers/{userId}/{isActive}")]
        public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
        {
            string sql = @"EXEC TutorialAppSchema.spUsers_Get";

            DynamicParameters sqlParams = new DynamicParameters();
            string parameters = "";

            if (userId != 0)
            {
                sqlParams.Add("@UserIdParams", userId, DbType.Int32);
                parameters += ", @UserId= @UserIdParams";
            }

            if (isActive)
            {
                sqlParams.Add("@ActiveParams", isActive, DbType.Boolean);
                parameters += ", @Active= @ActiveParams";
            }

            if (parameters.Length > 1)
            {
                sql += parameters.Substring(1);
            }

            IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParams);
            return users;
        }


        // PUT api/<UserController>/5
        [HttpPut("UpsertUser")]
        public IActionResult UpsertUser(UserComplete user)
        {
            bool done = _reusableSql.UpsertUser(user);
            if (done)
            {
                return Ok();
            }

            throw new Exception("failed to update user");
        }


        // DELETE api/<UserController>/5
        [HttpDelete("DeleteUser/{id}")]
        public IActionResult Delete(int id)
        {
            string sql = @"EXEC TutorialAppSchema.spUsers_Delete @UserId= @UserIdParam ";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", id, DbType.Int32);
            bool done = _dapper.ExecuteSqlWithParams(sql, sqlParams);
            if (done)
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }
    }
}