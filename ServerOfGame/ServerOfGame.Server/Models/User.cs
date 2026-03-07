namespace ServerOfGame.Server.Models
{
    public class User
    {
        public string Id           { get; set; }
        public string Username     { get; set; }
        public string PasswordHash { get; set; }   // bcrypt hash – never plain text
        public bool   IsBanned     { get; set; }
        public string BanReason    { get; set; }
        public int    Wins         { get; set; }
        public int    GamesPlayed  { get; set; }
        public int    Gold         { get; set; }

        public User() { }

        public User(string username, string passwordHash)
        {
            Id           = Guid.NewGuid().ToString();
            Username     = username;
            PasswordHash = passwordHash;
            IsBanned     = false;
            BanReason    = string.Empty;
            Wins         = 0;
            GamesPlayed  = 0;
            Gold         = 500;
        }
    }
}
