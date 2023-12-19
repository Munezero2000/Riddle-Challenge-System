using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiddlesApplication.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;
namespace RiddlesApplication.Pages
{
    public class IndexModel : PageModel
    {
        public List<Riddle> riddles = new List<Riddle>();
        public Riddle riddleObj = new Riddle();
        public string errorMessage = "";
        public string successMessage = "";

        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        { 

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
                                riddles.Add(riddleObj);
                              
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
    }
}