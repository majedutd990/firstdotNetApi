namespace DotnetAPI.Models
{
    public class UserSalary
    {
        public int UserId { get; set; }
        public decimal Salary { get; set; }
        public decimal AvgSalary { get; set; }

        public UserSalary()
        {
        }
    }
}