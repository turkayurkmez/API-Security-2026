using Injections.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Injections.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SqlInjectionProcess(ProductDbContext productDbContext) : ControllerBase
    {
        [HttpPost]
        public IActionResult Login(LoginRequest loginRequest)
        {

            SqlConnection connection = new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Northwind;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");


            // Simulate a SQL query vulnerable to injection
            string query = $"SELECT * FROM Employees WHERE FirstName = @name AND LastName = @password";



            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", loginRequest.name);
            command.Parameters.AddWithValue("@password",loginRequest.password);
            connection.Open();
            var reader = command.ExecuteReader();
            var result = reader.Read() ? "Login successful" : "Login failed";
            //reader.Close();
            connection.Close();


            // For demonstration purposes, we will just return the constructed query

            //Eğer EF Core kullanılsaydı:

            //var result = context.Employees.Where(e => e.FirstName == loginRequest.name && e.LastName == loginRequest.password).FirstOrDefault() != null ? "Login successful" : "Login failed";

            //BÜYÜK RİSK!:
            productDbContext.Products.FromSqlRaw($"Select * FROM tablo WHERE Hede='{loginRequest.name}' ");


            return Ok(new {result });

        }
    }

    public record LoginRequest(string name, string password);
}
