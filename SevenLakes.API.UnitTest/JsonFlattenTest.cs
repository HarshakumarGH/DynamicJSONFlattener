using Microsoft.VisualStudio.TestTools.UnitTesting;
using SevenLakes.API.JSONFlattener.Controllers;
using Moq;
using SevenLakes.BusinessLayer;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SevenLakes.Infrastrcture;
using Microsoft.AspNetCore.Http;

namespace SevenLakes.API.UnitTest
{

    [TestClass]
    public class JsonFlattenTest
    {
        // write test for flatten method. 
        private readonly Mock<IJsonService> _jsonService;
        private readonly Mock<ILoggerService> _loggerService;
        private readonly string inputJson = string.Empty;
        private readonly string expectedJson = string.Empty;
        public JsonFlattenTest()
        {
            _jsonService = new Mock<IJsonService>();
            _loggerService = new Mock<ILoggerService>();

            string inputFilePath = Path.Combine(AppContext.BaseDirectory, "AppData", "RestAPI_JSON_Input.json");
            inputJson = System.IO.File.ReadAllText(inputFilePath);

            string outputFilePath = Path.Combine(AppContext.BaseDirectory, "AppData", "RestAPI_JSON_Output.json");
            expectedJson = System.IO.File.ReadAllText(outputFilePath);

            _jsonService.Setup(x => x.Flatten(inputJson)).Returns(expectedJson);
        }

        /// <summary>
        /// Test API with Correct Input
        /// </summary>
        [TestMethod]
        public void TestFlattenAPICorrectInput()
        {
            var content = inputJson;
            var fileName = "RestAPI_JSON_Input.json";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            //create FormFile with desired data
            var file = new FormFile(stream, 0, stream.Length, "fileUpload", fileName);

            var controllerInstance = new RouteController(_jsonService.Object, _loggerService.Object);
            var actualResult = controllerInstance.Post(file) as OkObjectResult;
            Assert.AreNotEqual(Convert.ToString(actualResult.Value), expectedJson);
        }

        /// <summary>
        /// Test API with Wrong input
        /// </summary>
        [TestMethod]
        public void TestFlattenAPIWrongInput()
        {

            string input = "dummy:'Some Value'}";
            string expectedOutput = "Invalid JSON";

            var content = input;
            var fileName = "RestAPI_JSON_Input.json";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            //create FormFile with desired data
           var file = new FormFile(stream, 0, stream.Length, "fileUpload", fileName);

            var controllerInstance = new RouteController(_jsonService.Object, _loggerService.Object);
            var expectedResult = controllerInstance.Post(file) as BadRequestObjectResult;
            Assert.AreEqual(Convert.ToString(expectedResult.Value), expectedOutput);

        }

        /// <summary>
        /// Test the JSON service
        /// </summary>
        [TestMethod]
        public void TestJsonService()
        {
            var actualResult = this._jsonService.Object.Flatten(inputJson);
            Assert.AreEqual(Convert.ToString(actualResult), expectedJson);
        }
    }

}
