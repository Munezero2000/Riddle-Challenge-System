using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiddlesApplication.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Configuration;

namespace RiddlesApplication.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<Riddle> riddlesList = new List<Riddle>();
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
            if (Request.Query["successMessage"].ToString().Length > 0)
            {
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
                                riddleObj.rank = reader.GetInt32(3);
                                riddlesList.Add(riddleObj);
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
        }

        public void OnPost()
        {
            if (HttpContext.Session.GetInt32("_id") == null)
            {
                Response.Redirect("/Login");
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");

            
            riddleObj.Question = Request.Form["question"];
            riddleObj.Answer= Request.Form["answer"];
            riddleObj.rank= Convert.ToInt32(Request.Form["rank"]);
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string insertQuery = "INSERT INTO [ibisakuzo] (Question, Answer, rank) VALUES (@question, @answer, @rank)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@question", riddleObj.Question);
                        cmd.Parameters.AddWithValue("@answer", riddleObj.Answer);
                        cmd.Parameters.AddWithValue("@rank", riddleObj.rank);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected >= 1)
                        {
                            successMessage = "Riddle added successfully!";
                            Response.Redirect("/Admin");
                        }
                        else
                        {
                            errorMessage = "Failed to add the riddle.";
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

    }
}