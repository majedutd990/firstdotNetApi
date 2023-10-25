using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [ApiController]
    // it is gonna be looking fot the name of the controller or every thing that comes before the word controller in this case User 
    [Route("[controller]")]
    public class UserEfController : ControllerBase
    {
        private DataContextEf _ef;
        private IMapper _mapper;
        IUserRepository _userRepository;

        public UserEfController(IConfiguration configuration, IUserRepository userRepository)
        {
            _ef = new DataContextEf(configuration);
            _userRepository = _userRepository;

            _mapper = new Mapper(new MapperConfiguration(
                cfg =>
                    cfg.CreateMap<UserToAddDto, User>()
            ));
        }

        // GET: api/<UserController>
        [HttpGet("Users")]
        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _ef.Users.ToList<User>();
            return users;
        }

        // GET: api/<UserController>
        [HttpGet("Users/{userId}")]
        public User GetSingleUser(int userId)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == userId) ??
                        throw new InvalidOperationException("failed to get user");
            return user;
        }

        // PUT api/<UserController>/5
        [HttpPut("Users")]
        public IActionResult EditUser(User user)
        {
            User userDb = _ef.Users.FirstOrDefault(u => u.UserId == user.UserId) ??
                          throw new InvalidOperationException("failed to get user");


            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to update user");
        }


        // POST api/<UserController>
        [HttpPost("Users")]
        public IActionResult AddUser(UserToAddDto user)
        {
            User userDb = _mapper.Map<User>(user);
            _userRepository.AddEntity<User>(userDb);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to add user");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("Users/{id}")]
        public IActionResult Delete(int id)
        {
            User userDb = _ef.Users.FirstOrDefault(u => u.UserId == id) ??
                          throw new InvalidOperationException("failed to get user");
            _userRepository.RemoveEntity<User>(userDb);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }

        [HttpGet("UserSalary/All")]
        public IEnumerable<UserSalary> GetUserSalaries()
        {
            return _ef.UserSalary.ToList<UserSalary>();
        }

        [HttpGet("UserSalary/{id}")]
        public UserSalary GetSalary(int id)
        {
            UserSalary salary = _ef.UserSalary.FirstOrDefault(us => us.UserId == id) ??
                                throw new InvalidOperationException("no such record");
            return salary;
        }

        [HttpPost("UserSalary")]
        public IActionResult AddSalary(UserSalary userSalary)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == userSalary.UserId) ??
                        throw new InvalidOperationException("failed to get user");

            _userRepository.AddEntity<UserSalary>(userSalary);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to add user");
        }

        [HttpPut("UserSalary/{id}")]
        public IActionResult UpdateSalary(UserSalary userSalary)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == userSalary.UserId) ??
                        throw new InvalidOperationException("failed to get user");

            UserSalary salaryToBe = _ef.UserSalary.FirstOrDefault(us => us.UserId == userSalary.UserId) ??
                                    throw new InvalidOperationException("no such record");
            salaryToBe.AvgSalary = userSalary.AvgSalary;
            salaryToBe.Salary = userSalary.Salary;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to update user");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("UserSalary/{id}")]
        public IActionResult DeleteUserSalary(int id)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == id) ??
                        throw new InvalidOperationException("failed to get user");
            UserSalary userSalary = _ef.UserSalary.FirstOrDefault(u => u.UserId == id) ??
                                    throw new InvalidOperationException("failed to get user");
            _userRepository.RemoveEntity<UserSalary>(userSalary);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }

        [HttpGet("UserJobInfo/All")]
        public IEnumerable<UserJobInfo> GetUserJobInfos()
        {
            return _ef.UserJobInfo.ToList<UserJobInfo>();
        }

        [HttpGet("UserJobInfo/{id}")]
        public UserJobInfo GetJobInfo(int id)
        {
            UserJobInfo jobInfo = _ef.UserJobInfo.FirstOrDefault(us => us.UserId == id) ??
                                  throw new InvalidOperationException("no such record");
            return jobInfo;
        }

        [HttpPost("UserJobInfo")]
        public IActionResult AddUserJoInfo(UserJobInfo jobInfo)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == jobInfo.UserId) ??
                        throw new InvalidOperationException("failed to get user");
            _userRepository.AddEntity<UserJobInfo>(jobInfo);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to add user");
        }

        [HttpPut("UserJobInfo")]
        public IActionResult UpdateUserJobInfo(UserJobInfo jobInfo)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == jobInfo.UserId) ??
                        throw new InvalidOperationException("failed to get user");
            UserJobInfo infoToBe = _ef.UserJobInfo.FirstOrDefault(us => us.UserId == jobInfo.UserId) ??
                                   throw new InvalidOperationException("no such record");
            infoToBe.Department = jobInfo.Department;
            infoToBe.JobTitle = jobInfo.JobTitle;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to update user");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("UserJobInfo/{id}")]
        public IActionResult UserJobInfoDelete(int id)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == id) ??
                        throw new InvalidOperationException("failed to get user");
            UserJobInfo userJi = _ef.UserJobInfo.FirstOrDefault(u => u.UserId == id) ??
                                 throw new InvalidOperationException("failed to get user");
            _userRepository.RemoveEntity<UserJobInfo>(userJi);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }
    }
}