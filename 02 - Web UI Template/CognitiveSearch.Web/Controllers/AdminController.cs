using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CognitiveSearch.Web.Configuration;
using CognitiveSearch.Web.Models;
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
        public async Task<IActionResult> Customize()
        {
            var model = new CustomizeViewModel
            {
                NavBar = await ReadCssColorProperties("navbar"),
                Footer = await ReadCssColorProperties("footer")
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CustomizeNavBar()
        {
            var cssString = "";
            var hasLogo = Request.Form.Files.Any();

            if (hasLogo)
            {
                // Upload the new logo file
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length > 0)
                    {
                        var fi = new FileInfo(file.FileName);
                        cssString += $"#navbar-img{{content:url('../images/{file.FileName}');height:25px;}}";

                        var webPath = _hostingEnvironment.WebRootPath;
                        var path = Path.Combine("", webPath + @"\images\" + file.FileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
            }
            else
            {
                var currentLogoName = await ReadCssImageNameProperty("navbar");
                cssString += $"#navbar-img{{content:url('../images/{currentLogoName}');height:25px;}}";
            }

            var hasBgColor = Request.Form.Keys.Any(k => k == "navbar-bg");
            var bgColor = hasBgColor ? Request.Form["navbar-bg"].ToString() : "#ffffff";
            cssString += $".navbar-bg{{background-color:{bgColor}}}";

            var hasTextColor = Request.Form.Keys.Any(k => k == "navbar-text");
            var textColor = hasTextColor ? Request.Form["navbar-text"].ToString() : "#000000";
            cssString += $".navbar-text{{color:{textColor}}}";

            await WriteCss("navbar", cssString);

            ViewBag.Message = $"Successfully updated the navigation bar.";

            var model = new CustomizeViewModel
            {
                NavBar = await ReadCssColorProperties("navbar"),
                Footer = await ReadCssColorProperties("footer")
            };
            return View("Customize", model);
        }

        [HttpPost]
        public async Task<IActionResult> CustomizeFooter()
        {
            var cssString = "";
            var hasLogo = Request.Form.Files.Any();

            if (hasLogo)
            {
                // Upload the new logo file
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length > 0)
                    {
                        var fi = new FileInfo(file.FileName);
                        cssString += $"#footer-img{{content:url('../images/{file.FileName}');height:25px;}}";

                        var webPath = _hostingEnvironment.WebRootPath;
                        var path = Path.Combine("", webPath + @"\images\" + file.FileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
            }
            else
            {
                var currentLogoName = await ReadCssImageNameProperty("navbar");
                cssString += $"#footer-img{{content:url('../images/{currentLogoName}');height:25px;}}";
            }

            var hasBgColor = Request.Form.Keys.Any(k => k == "footer-bg");
            var bgColor = hasBgColor ? Request.Form["footer-bg"].ToString() : "#ffffff";
            cssString += $".footer-bg{{background-color:{bgColor}}}";

            var hasTextColor = Request.Form.Keys.Any(k => k == "footer-text");
            var textColor = hasTextColor ? Request.Form["footer-text"].ToString() : "#000000";
            cssString += $".footer-text{{color:{textColor}}}";

            await WriteCss("footer", cssString);

            ViewBag.Message = $"Successfully updated the footer.";

            var model = new CustomizeViewModel
            {
                NavBar = await ReadCssColorProperties("navbar"),
                Footer = await ReadCssColorProperties("footer")
            };
            return View("Customize", model);
        }

        private async Task WriteCss(string fileName, string fileContent)
        {
            var webPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine("", webPath + $"\\css\\{fileName}.css");

            await System.IO.File.WriteAllTextAsync(path, fileContent);
        }

        private async Task<string> ReadCssImageNameProperty(string fileName)
        {
            var webPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine("", webPath + $"\\css\\{fileName}.css");

            var fileText = await System.IO.File.ReadAllTextAsync(path);
            
            var nameStart = fileText.IndexOf("../images/") + "..images/".Length + 1;
            var nameEnd = fileText.IndexOf("');");
            var length = nameEnd - nameStart;

            var logoName = fileText.Substring(nameStart, length);

            return logoName;
        }

        private async Task<ColorSettings> ReadCssColorProperties(string section)
        {
            var colorSettings = new ColorSettings();
            var webPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine("", webPath + $"\\css\\{section}.css");

            var css = await System.IO.File.ReadAllTextAsync(path);
            var parts = css.Split($"{section}-", StringSplitOptions.RemoveEmptyEntries);
            var props = new Dictionary<string, string>();

            foreach (var part in parts)
            {
                var partClean = part.Substring(part.IndexOf("{") + 1).Replace("}.", "").Replace("}", "");
                var subParts = partClean.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (partClean.StartsWith("background-color:"))
                {
                    colorSettings.BackgroundColor = subParts[1];
                }
                if (partClean.StartsWith("color:"))
                {
                    colorSettings.TextColor = subParts[1];
                }
            }
            return colorSettings;
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

        [HttpGet]
        public IActionResult Deploy()
        {
            return View();
        }
    }
}