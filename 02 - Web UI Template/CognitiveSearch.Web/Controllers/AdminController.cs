using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CognitiveSearch.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace CognitiveSearch.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IFileProvider _fileProvider;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly AppConfig _appConfig;

        public AdminController(IFileProvider fileProvider, IHostingEnvironment hostingEnvironment, AppConfig appConfig)
        {
            _fileProvider = fileProvider;
            _hostingEnvironment = hostingEnvironment;
            _appConfig = appConfig;
        }

        [HttpGet]
        public IActionResult Customize()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCustomImage()
        {
            foreach (var file in Request.Form.Files)
            {
                if (file.Length > 0)
                {
                    var fi = new FileInfo(file.FileName);

                    var webPath = _hostingEnvironment.WebRootPath;
                    var path = Path.Combine("", webPath + @"\images\" + file.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            ViewBag.Message = $"Custom app logo uploaded as to the web server successfully";

            return View("Customize");
        }

        public async Task<IActionResult> DownloadCustomCss()
        {
            var webPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine("", webPath + @"\css\custom.css");

            var memory = new MemoryStream();
            using(var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "text/css", "custom.css");
        }

        [HttpPost]
        public async Task<IActionResult> UploadCustomCss()
        {
            foreach (var file in Request.Form.Files)
            {
                if (file.Length > 0)
                {
                    var webPath = _hostingEnvironment.WebRootPath;
                    var path = Path.Combine("", webPath + @"\css\custom.css");

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            ViewBag.Message = $"Custom CSS file uploaded as to the web server successfully";

            return View("Customize");
        }

        [HttpGet]
        public IActionResult UploadData()
        {
            return View(_appConfig);
        }
    }
}