using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiddlesApplication.Models;
using System.Data.SqlClient;
using BCrypt.Net;
using System.Data;

namespace RiddlesApplication.Pages
{
    public class LoginModel : PageModel
    {
        public User user = new User();
        public string errorMessage = "";
        public string successMessage = "";

        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");

            user.email = Request.Form["email"];
            user.password = Request.Form["password"];

            if (user.email.Length == 0 || user.password.Length == 0)
            {
                errorMessage = "ALL FIELDS ARE REQUIRED";
            }
            else
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        conn.Open();
                        string query = "SELECT * FROM [User] WHERE email=@email";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@email", user.email);

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    if (reader.Read())
                                    { 
                                        string hashedPasswordFromDatabase = reader.GetString(3); 
                                        if (BCrypt.Net.BCrypt.EnhancedVerify(user.password, hashedPasswordFromDatabase))
                                        {
                                            user.UserId = reader.GetInt32(0);
                                            Console.WriteLine(user.UserId);
                                            user.FullNames = reader.GetString(1);
                                            Console.WriteLine(user.FullNames);
                                            user.email = reader.GetString(2);
                                            Console.WriteLine(user.email);
                                            user.RoleId = reader.GetInt32(4);
                                            Console.WriteLine(user.RoleId);
                                            user.Score = reader.GetInt32(5);
                                            Console.WriteLine(user.Score);

                                            HttpContext.Session.SetInt32("_id", (int)user.UserId);
                                            HttpContext.Session.SetString("_name", user.FullNames);
                                            HttpContext.Session.SetString("_email", user.email);
                                            HttpContext.Session.SetInt32("_role", (int)user.RoleId);
                                            HttpContext.Session.SetInt32("_score", (int)user.Score);

                                            successMessage = "User found. Redirecting...";
                                            if(user.RoleId == 2)
                                            {
                                                Response.Redirect("/dashboard");
                                            }
                                            else if(user.RoleId==1)
                                            {
                                                Response.Redirect("/admin");
                                            }
                                           
                                        }
                                        else
                                        {
                                            errorMessage = "Incorrect password";
                                        }

                                    }
                                    else
                                    {
                                        errorMessage = "user not found";
                                    }
                                }
                                else
                                {
                                    errorMessage = "user not found";
                                }
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
}
