using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
        private DataContextEf _ef;

        public UserRepository(IConfiguration configuration)
        {
            _ef = new DataContextEf(configuration);
        }

        public bool SaveChanges()
        {
            return _ef.SaveChanges() > 0;
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null) _ef.Add(entityToAdd);
        }

        public void RemoveEntity<T>(T entityToBeRemoved)
        {
            if (entityToBeRemoved != null) _ef.Remove(entityToBeRemoved);
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _ef.Users.ToList<User>();
            return users;
        }

        public IEnumerable<UserSalary> GetUserSalaries()
        {
            return _ef.UserSalary.ToList<UserSalary>();
        }

        public IEnumerable<UserJobInfo> GetUserJobInfos()
        {
            return _ef.UserJobInfo.ToList<UserJobInfo>();
        }

        public User GetSingleUser(int userId)
        {
            User user = _ef.Users.FirstOrDefault(u => u.UserId == userId) ??
                        throw new InvalidOperationException("failed to get user");
            return user;
        }

        public UserSalary GetSalary(int id)
        {
            UserSalary salary = _ef.UserSalary.FirstOrDefault(us => us.UserId == id) ??
                                throw new InvalidOperationException("no such record");
            return salary;
        }

        public UserJobInfo GetJobInfo(int id)
        {
            UserJobInfo jobInfo = _ef.UserJobInfo.FirstOrDefault(us => us.UserId == id) ??
                                  throw new InvalidOperationException("no such record");
            return jobInfo;
        }
    }
}