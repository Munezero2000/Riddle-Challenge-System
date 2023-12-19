namespace RiddlesApplication.Models
{
   
        public class UserAnswer
        {
        public UserAnswer()
        {
        }

        public int UserId { get; set; }
            public int QuestionId { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
        
            
        }

    }

