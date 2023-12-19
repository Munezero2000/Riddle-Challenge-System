namespace RiddlesApplication.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullNames { get; set; }

        public string email { get; set; }

        public string password { get; set; }

        public int RoleId { get; set; }

        public int Score { get; set; }
        public User()
        {
        }
    }
}
