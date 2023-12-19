using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiddlesApplication.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System;

namespace RiddlesApplication.Pages.Dashboard
{
    public class ChallengeModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ChallengeModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Riddle riddleObj = new Riddle();
        public User user = new User();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("_id") == null)
            {
                Response.Redirect("/Login");
            }
            string id = Request.Query["id"];
            RetrieveRiddleData(id);
        }

        public void OnPost()
        {
            string Answer = Request.Form["answer"].ToString().Trim().ToLower();
           
            string QuestionId = Request.Query["id"];

            RetrieveRiddleData(QuestionId);

            if (riddleObj != null)
            {
                if (Answer == riddleObj.Answer.ToLower())
                {
                    
                    successMessage = "You made it!! Take another challenge";
                   
                    updateUserAnswer(QuestionId);

                    updateUserMarks();
                    Response.Redirect("/Dashboard/Index?successMessage=" + Uri.EscapeDataString(successMessage));

                }
                else
                {
                    errorMessage = "Try again! This is not the answer or try another challenge";
                }
            }
            else
            {
                errorMessage = "Riddle data not found. Try another challenge.";
            }

        }

        private void RetrieveRiddleData(string id)
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT * FROM [ibisakuzo] where id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                riddleObj.Id = reader.GetInt32(0);
                                riddleObj.Question = reader.GetString(1);
                                riddleObj.Answer = reader.GetString(2);
                               
                            }
                            else
                            {
                                riddleObj = null;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorMessage = $"Server Error:{e.Message}";
            }
        }


        private int getCurrentUserScore(int userId)
        {
            int score = 0;
            string connString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT score FROM [User] WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out score))
                        {
                            return score;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorMessage = $"Server Error: {e.Message}";
            }

            return score;
        }

        private void updateUserMarks()
        {
            string QuestionIid = Request.Query["id"];
            int userId = (int)HttpContext.Session.GetInt32("_id");
            string connString = _configuration.GetConnectionString("DefaultConnection");
            RetrieveRiddleData(QuestionIid);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    int currentUserScore = getCurrentUserScore(userId);

                    int newScore = currentUserScore + 1;

                    string query = "UPDATE [User] SET score = @score WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.Parameters.AddWithValue("@score", newScore);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected >= 1)
                        {
                            
                           
                        }
                        else
                        {
                            errorMessage = "No rows were updated.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorMessage = $"Server Error: {e.Message}";
            }
        }

        private void updateUserAnswer(string questionId)
        {
            int userId = (int)HttpContext.Session.GetInt32("_id");
            string connString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "INSERT INTO [user_answers] VALUES (@userid, @questionid)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userid", userId);
                        cmd.Parameters.AddWithValue("@questionid", questionId);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected >= 1)
                        {
                            successMessage = "You made it!! Take another challenge";
                        }
                        else
                        {
                            errorMessage = "No";
                        }
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                errorMessage = e.ToString();
            }
        }
    }
}
