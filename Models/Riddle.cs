namespace RiddlesApplication.Models
{
    public class Riddle
    {
        public int Id { get; set; }
        public string Question { get; set; }

        public string Answer { get; set; }

        public int rank { get; set; }

        public Riddle()
        {
        }
    }
}
