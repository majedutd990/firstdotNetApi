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
        private IMapper _mapper;
        IUserRepository _userRepository;

        public UserEfController(IConfiguration configuration, IUserRepository userRepository)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(
                cfg =>
                    cfg.CreateMap<UserToAddDto, User>()
            ));
        }

        // GET: api/<UserController>
        [HttpGet("Users")]
        public IEnumerable<User> GetUsers()
        {
            return _userRepository.GetUsers();
        }

        // GET: api/<UserController>
        [HttpGet("Users/{userId}")]
        public User GetSingleUser(int userId)
        {
            return _userRepository.GetSingleUser(userId);
        }

        // PUT api/<UserController>/5
        [HttpPut("Users")]
        public IActionResult EditUser(User user)
        {
            User userDb = _userRepository.GetSingleUser(user.UserId);
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
            User userDb = _userRepository.GetSingleUser(id);
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
            return _userRepository.GetUserSalaries();
        }

        [HttpGet("UserSalary/{id}")]
        public UserSalary GetSalary(int id)
        {
            return _userRepository.GetSalary(id);
        }

        [HttpPost("UserSalary")]
        public IActionResult AddSalary(UserSalary userSalary)
        {
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
            User user = _userRepository.GetSingleUser(userSalary.UserId);

            UserSalary salaryToBe = _userRepository.GetSalary(userSalary.UserId);
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
            UserSalary userSalary = _userRepository.GetSalary(id);
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
            return _userRepository.GetUserJobInfos();
        }

        [HttpGet("UserJobInfo/{id}")]
        public UserJobInfo GetJobInfo(int id)
        {
            return _userRepository.GetJobInfo(id);
        }

        [HttpPost("UserJobInfo")]
        public IActionResult AddUserJoInfo(UserJobInfo jobInfo)
        {
            User user = _userRepository.GetSingleUser(jobInfo.UserId);
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
            User user = _userRepository.GetSingleUser(jobInfo.UserId);
            UserJobInfo infoToBe = _userRepository.GetJobInfo(jobInfo.UserId);
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
            UserJobInfo userJi = _userRepository.GetJobInfo(id);
            _userRepository.RemoveEntity<UserJobInfo>(userJi);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("failed to delete user");
        }
    }
}