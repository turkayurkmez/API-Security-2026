using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Injections.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandInjection : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> GenerateReport(string reportType)
        {
            // Simulate command execution based on user input
            string command = $"python3 /scripts/generate_report.py --type {reportType}";

            var process =Process.Start("bash",$"-c \"{reportType}\"");


            //Güvenli kod:

            string command2 = $"python3 /scripts/generate_report.py --type {reportType}";
            var process2 = Process.Start(new ProcessStartInfo
            {
                ArgumentList =
                {
                    "python3",
                    "/scripts/generate_report.py",
                    "--type",
                    reportType
                },
                FileName = "python3",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            });


            await process.WaitForExitAsync();
            // In a real application, you would execute the command here
            // For demonstration purposes, we will just return the constructed command
            return Ok(new { command });
        }

        //api/CommandInjection?reportType=monthly;
        //saldırgan ne gönderecek?

        //api/CommandInjection?reportType=monthly;rm -rf /important_data



    }
}
