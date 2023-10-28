using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public interface IUserRepository
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entity);
        public void RemoveEntity<T>(T entity);

        //user functionality
        public IEnumerable<User> GetUsers();

        public User GetSingleUser(int userId);

        //user salary functionality
        public IEnumerable<UserSalary> GetUserSalaries();
        public UserSalary GetSalary(int id);

        //user job info functionality
        public IEnumerable<UserJobInfo> GetUserJobInfos();
        public UserJobInfo GetJobInfo(int id);
    }
}