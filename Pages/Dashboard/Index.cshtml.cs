using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiddlesApplication.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace RiddlesApplication.Pages.Dashboard
{
    public class DashboardModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public DashboardModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Riddle> riddles = new List<Riddle>();
        public List<UserAnswer> AnsweredList = new List<UserAnswer>();
        public Riddle riddleObj = new Riddle();
        public string errorMessage = "";
        public string successMessage = "";

       
        public void OnGet()
        {
            
            if (HttpContext.Session.GetInt32("_id") == null)
            {
                Response.Redirect("/Login");
            }
            if(Request.Query["successMessage"].ToString().Length >0 ){
                successMessage = Request.Query["successMessage"];
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");
            @ViewData["fullname"] = HttpContext.Session.GetString("_name");
            @ViewData["score"] = HttpContext.Session.GetInt32("_score");
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    
                    List<int> userAnsweredQuestionIds = GetUserAnsweredQuestionIds(conn);
                    AnsweredList = GetUserAnswers(conn);
                    string query = "SELECT * FROM [ibisakuzo]";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                riddleObj = new Riddle();
                                riddleObj.Id = reader.GetInt32(0);
                                riddleObj.Question = reader.GetString(1);
                                riddleObj.Answer = reader.GetString(2);

                                
                                if (!userAnsweredQuestionIds.Contains(riddleObj.Id))
                                {
                                    riddles.Add(riddleObj);
                                }
                            }
                        }
                        else
                        {
                            errorMessage = "RIDDLES NOT FOUND";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorMessage = $"Server Error:{e.Message}";
            }

            if (HttpContext.Request.Query.ContainsKey("id"))
            {
                var idValue = HttpContext.Request.Query["id"];
                Console.WriteLine($"Found id: {idValue}");
            }
        }

        private List<int> GetUserAnsweredQuestionIds(SqlConnection conn)
        {
            List<int> userAnsweredQuestionIds = new List<int>();

            try
            {
                int userId = (int)HttpContext.Session.GetInt32("_id");

                string query = "SELECT question_id FROM [user_answers] WHERE user_id = @userId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int questionId = reader.GetInt32(0);
                            userAnsweredQuestionIds.Add(questionId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorMessage = $"Server Error:{e.Message}";
            }

            return userAnsweredQuestionIds;
        }
        private List<UserAnswer> GetUserAnswers(SqlConnection conn)
        {
            List<UserAnswer> userAnswers = new List<UserAnswer>();

            try
            {
                int userId = (int)HttpContext.Session.GetInt32("_id");

                string query = @"
                    SELECT ua.user_id, ua.question_id, i.question, i.answer
                    FROM [user_answers] ua
                    JOIN [ibisakuzo] i ON ua.question_id = i.id
                    WHERE ua.user_id = @userId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserAnswer userAnswer = new UserAnswer
                            {
                                UserId = reader.GetInt32(0),
                                QuestionId = reader.GetInt32(1),
                                Question = reader.GetString(2),
                                Answer = reader.GetString(3)
                                // Add other properties as needed
                            };

                            userAnswers.Add(userAnswer);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorMessage = $"Server Error:{e.Message}";
            }

            return userAnswers;
        }
    }
}
