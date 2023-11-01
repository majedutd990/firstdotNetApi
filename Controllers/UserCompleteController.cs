using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserCompleteController : ControllerBase
    {
        private DataContextDapper _dapper;

        public UserCompleteController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
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

            string parameters = "";


            if (userId != 0)
            {
                parameters += ", @UserId=" + userId.ToString();
            }

            if (isActive)
            {
                parameters += ", @Active=" + isActive;
            }

            if (parameters.Length > 1)
            {
                Console.WriteLine(parameters);
                sql += parameters.Substring(1);
            }

            IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
            return users;
        }


        // PUT api/<UserController>/5
        [HttpPut("UpsertUser")]
        public IActionResult UpsertUser(UserComplete user)
        {
            string sql = @"EXEC TutorialAppSchema.spUser_Upsert
                             @FirstName='" + user.FirstName +
                         "', @LastName='" + user.LastName +
                         "', @Email='" + user.Email +
                         "', @Gender='" + user.Gender +
                         "', @JobTitle='" + user.JobTitle +
                         "', @Department='" + user.Department +
                         "', @Salary='" + user.Salary +
                         "', @Active='" + user.Active +
                         "', @UserId='" + user.UserId + "'";

            bool done = _dapper.ExecuteSql(sql);
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
            string sql = @"EXEC TutorialAppSchema.spUsers_Delete @UserId='" + id.ToString() + "'";

            bool done = _dapper.ExecuteSql(sql);
            if (done)
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }
    }
}