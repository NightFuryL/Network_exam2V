namespace DatabaseLibrary.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Password { get; set; } = "";

        public int Rating { get; set; }

        public int Wins { get; set; }

        public int Loses { get; set; }

        public int Draws { get; set; }
    }
}
