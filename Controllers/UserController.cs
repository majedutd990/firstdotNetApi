using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotnetAPI.Controllers
{
    [ApiController]
    // it is gonna be looking fot the name of the controller or every thing that comes before the word controller in this case User 
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private DataContextDapper _dapper;

        public UserController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("TestConnection")]
        public DateTime TestConnection()
        {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }

        // GET: api/<UserController>
        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            string sql = @"SELECT [UserId],
                                  [FirstName],
                                  [LastName],
                                  [Email],
                                  [Gender],
                                  [Active] FROM TutorialAppSchema.Users";

            IEnumerable<User> users = _dapper.LoadData<User>(sql);
            return users;
        }

        // GET: api/<UserController>
        [HttpGet("GetSingleUser/{userId}")]
        public User GetSingleUser(int userId)
        {
            string sql = @"SELECT [UserId],
                                  [FirstName],
                                  [LastName],
                                  [Email],
                                  [Gender],
                                  [Active] FROM TutorialAppSchema.Users WHERE [UserId] = " + userId.ToString();

            User user = _dapper.LoadDataSingle<User>(sql);
            return user;
        }

        // PUT api/<UserController>/5
        [HttpPut("EditUser")]
        public IActionResult EditUser(User user)
        {
            string sql = @"Update TutorialAppSchema.Users SET
                                  [FirstName]='" + user.FirstName +
                         "', [LastName]='" + user.LastName +
                         "', [Email]='" + user.Email +
                         "', [Gender]='" + user.Gender +
                         "', [Active]='" + user.Active + "' WHERE UserId='" + user.UserId + "'";

            bool done = _dapper.ExecuteSql(sql);
            if (done)
            {
                return Ok();
            }

            throw new Exception("failed to update user");
        }


        // POST api/<UserController>
        [HttpPost("AddUser")]
        public IActionResult AddUser(UserToAddDto user)
        {
            string sql = @"INSERT INTO TutorialAppSchema.Users (
                                  [FirstName],
                                  [LastName],
                                  [Email],
                                  [Gender],
                                  [Active]) VALUES('" + user.FirstName + "','" + user.LastName + "','" + user.Email +
                         "','" +
                         user.Gender + "','" + user.Active + "')";

            bool done = _dapper.ExecuteSql(sql);
            if (done)
            {
                return Ok();
            }

            throw new Exception("failed to add user");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("DeleteUser/{id}")]
        public IActionResult Delete(int id)
        {
            string sql = @"DELETE FROM TutorialAppSchema.Users WHERE UserId='" + id.ToString() + "'";

            bool done = _dapper.ExecuteSql(sql);
            if (done)
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }
    }
}