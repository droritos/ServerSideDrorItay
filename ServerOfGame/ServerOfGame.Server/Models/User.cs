namespace ServerOfGame.Server.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Id { get; set; } // Just hold the data

        // 👇 THIS IS THE CONSTRUCTOR
        public User(string username, string password)
        {
            Username = username;
            Password = password;

            // Generate the ID once, right here!
            Id = System.Guid.NewGuid().ToString();
        }
    }
}