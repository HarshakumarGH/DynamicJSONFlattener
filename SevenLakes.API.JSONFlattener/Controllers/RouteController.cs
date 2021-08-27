using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SevenLakes.BusinessLayer;
using SevenLakes.Infrastrcture;
using System;
using System.IO;
using System.Text;

namespace SevenLakes.API.JSONFlattener.Controllers
{
    [ApiController]
    [Route("route")]
    public class RouteController : ControllerBase
    {
        private readonly IJsonService _jsonService;
        private readonly ILoggerService _logger;

        /// <summary>
        /// Contructor to inject the dependencies.
        /// </summary>
        /// <param name="jsonService"></param>
        /// <param name="logger"></param>
        public RouteController(IJsonService jsonService, ILoggerService logger)
        {
            _jsonService = jsonService;
            _logger = logger;
        }

        /// <summary>
        /// Method to convert nested JSON input into flatten JSON
        /// </summary>
        /// <param name="fileUpload">Expects to upload the valid JSON file</param>
        /// <param name="downloadResultFile">Optional boolean value used to download the result in a JSON file</param>
        /// <returns>Returns either result text or downloadable file</returns>
        [HttpPost]
        public IActionResult Post([FromForm] IFormFile fileUpload, bool downloadResultFile = false)
        {
            _logger.LogDebug("Here is debug message from the controller.");
            _logger.LogInfo("Post Method Execution Starts");

            var result = new StringBuilder();
            //Reading the data from the uploaded file
            using (var reader = new StreamReader(fileUpload.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }
            string strJSON = Convert.ToString(result);

            //validating the uploaded file content to make sure it is valid JSON or not
            if (Validator.ValidateJSON(strJSON))
            {
                //call the flatten service, which does the job of flattenig the input
                var data = _jsonService.Flatten(strJSON);

                //Check whether result should be text/file
                if (downloadResultFile)
                {
                    //validate the directory to download file
                    string uploadDir = Path.Combine(AppContext.BaseDirectory, "download");
                    if (!System.IO.Directory.Exists(uploadDir))
                    {
                        _logger.LogWarn("Download folder not exist in the directory, Creating it");
                        System.IO.Directory.CreateDirectory(uploadDir);
                    }

                    //create the file in the above directory
                    var fileName = "RestAPI_JSON_Output.json";
                    string filePath = Path.Combine(uploadDir, fileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        _logger.LogWarn("JSON file not exist in the folder, Creating it");
                        System.IO.File.Create(filePath);
                    }

                    //write the result content to the file
                    System.IO.File.WriteAllText(filePath, data.ToString());

                    //read the content to download as file.
                    var bytes = System.IO.File.ReadAllBytes(filePath);

                    _logger.LogInfo("Post Method Execution Ends");
                    //return file path
                    return File(bytes, "text/plain", Path.GetFileName(filePath));
                }
                else
                {
                    _logger.LogInfo("Post Method Execution Ends");
                    //return the result as text
                    return Ok(data);
                }
            }
            else
            {
                _logger.LogError("Given JSON is Invalid");
                return BadRequest("Invalid JSON");
            }
        }
    }
}
