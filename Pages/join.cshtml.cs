using Microsoft.AspNetCore.Mvc.RazorPages;
using RiddlesApplication.Helper;
using RiddlesApplication.Models;
using System.Data.SqlClient;
using BCrypt.Net;

namespace RiddlesApplication.Pages
{
    public class joinModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public joinModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public User user = new User();
        public string errorMessage = "";
        public string successMessage = "";
        public void OnGet()
        {
        }

        public void OnPost()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            user.FullNames = Request.Form["fullname"];
            user.email = Request.Form["email"];
            user.password = Request.Form["password"];
            Console.WriteLine(user.password);
            string cpassword = Request.Form["password"];
            Console.WriteLine(cpassword);

            int RoleId = 2;

            if(user.FullNames.Length == 0 || user.email.Length == 0 || user.password.Length == 0) 
            {
                errorMessage = "ALL FIELDS ARE REQUIRED";

            }
            else
            {
                if (user.password.Equals(cpassword))
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connString))
                        {
                            conn.Open();
                            string query = "INSERT INTO [USER] (fullnames, email, [password], role_id, score) VALUES (@fullname, @email, @password, @role, 0)";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@fullname", user.FullNames);
                                cmd.Parameters.AddWithValue("@email", user.email);
                                cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.EnhancedHashPassword(user.password));
                                cmd.Parameters.AddWithValue("@role", RoleId);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected >= 1)
                                {
                                    successMessage = "Account created successfully";
                                    Response.Redirect("/Login");
                                }
                                else
                                {
                                    errorMessage = "Account not created";
                                }
                            }

                        }

                    }catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        errorMessage = e.ToString();
                    }


                }
                else
                {
                    errorMessage = "password doesn't much";
                }
                

            }

          

        }
    }
}
