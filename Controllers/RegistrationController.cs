using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationApp.Models;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace AuthenticationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class Response
    {
        public int Status; //by default false = 0
        public string Message;
    }

    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("get")]
        public IEnumerable<Registration> Get()
        {
            List<Registration> users = new List<Registration>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ApiDbCon")))
            {
                con.Open();
                string sqlQuery = "select Username, Email, Age, Gender from Users";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Registration getUsers = new Registration
                            {
                                userName = sdr.GetString(sdr.GetOrdinal("Username")),
                                email = sdr.GetString(sdr.GetOrdinal("Email")),
                                age = sdr.GetInt32(sdr.GetOrdinal("Age")),
                                gender = sdr.GetString(sdr.GetOrdinal("Gender"))
                            };
                            users.Add(getUsers);
                        }
                    }
                }
            }
            return users;
        }

        // api/registration/signup
        [HttpPost]
        [Route("signup")]
        public string Signup(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ApiDbCon").ToString());

            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO Users (Username, Email, Age, Gender, Password, IsActive) VALUES (@Username, @Email, @Age, @Gender, @Password, @IsActive); SELECT SCOPE_IDENTITY();", con);
                cmd.Parameters.AddWithValue("@Username", registration.userName);
                cmd.Parameters.AddWithValue("@Email", registration.email);
                cmd.Parameters.AddWithValue("@Age", registration.age);
                cmd.Parameters.AddWithValue("@Gender", registration.gender);
                cmd.Parameters.AddWithValue("@Password", registration.password);
                cmd.Parameters.AddWithValue("@IsActive", registration.isActive);

                int userId = Convert.ToInt32(cmd.ExecuteScalar());

                if (userId > 0)
                {
                    Response httpResponse = new Response();
                    httpResponse.Status = 1;
                    httpResponse.Message = "Registration successful with User ID : " + userId;
                    string jsonResponse = JsonConvert.SerializeObject(httpResponse);
                    return jsonResponse;
                }
                else
                {
                    Response httpResponse = new Response();
                    httpResponse.Status = 0;
                    httpResponse.Message = "Error occured while registration process, please try again later";
                    string jsonResponse = JsonConvert.SerializeObject(httpResponse);
                    return jsonResponse;
                }
            }
            catch (Exception ex)
            {
                return "An error occurred: " + ex.Message;
            }
            finally
            {
                con.Close();
            }
        }

        // api/registration/login
        [HttpPost]
        [Route("login")]
        public string Login(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("ApiDbCon").ToString());

            try
            {
                con.Open();
                //verifying secret key
                SqlCommand cmdKey = new SqlCommand("select count(*) from SecretKey where secretNum = @SecretNum", con);
                cmdKey.Parameters.AddWithValue("@SecretNum", registration.secretKey);
                int keyCount = (int)cmdKey.ExecuteScalar();

                if (keyCount == 0)
                {
                    Response httpResponse = new Response();
                    httpResponse.Status = 0;
                    httpResponse.Message = "Invalid secret key";
                    string jsonResponse = JsonConvert.SerializeObject(httpResponse);
                    return jsonResponse;
                }

                SqlCommand cmdUser = new SqlCommand("select UserId from Users where Email = @Email and Password = @Password and IsActive = 1", con);
                cmdUser.Parameters.AddWithValue("@Email", registration.email);
                cmdUser.Parameters.AddWithValue("@Password", registration.password);
                cmdUser.Parameters.AddWithValue("@IsActive", 1);

                object userIdObj = cmdUser.ExecuteScalar();
                int userID = userIdObj != null ? Convert.ToInt32(userIdObj) : 0;

                if (userID != 0)
                {
                    Response httpResponse = new Response();
                    httpResponse.Status = 1;
                    httpResponse.Message = "Login successful. User ID :" + userID;
                    string jsonResponse = JsonConvert.SerializeObject(httpResponse);
                    return jsonResponse;
                }

                else
                {
                    Response defaultResponse = new Response();
                    defaultResponse.Status = 0;
                    defaultResponse.Message = "Invalid user or account is inactive";
                    string defaultJsonResponse = JsonConvert.SerializeObject(defaultResponse);
                    return defaultJsonResponse;
                }
            }
            catch (Exception ex)
            {
                Response httpResponse = new Response();
                httpResponse.Status = 0;
                httpResponse.Message = "An error occurred : " + ex.Message;
                string jsonResponse = JsonConvert.SerializeObject(httpResponse);
                return jsonResponse;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
