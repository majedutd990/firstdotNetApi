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
    }
}