using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Wimbley.AppleWatch.HeartRate.Site.Core;
using Wimbley.AppleWatch.HeartRate.Site.Core.Exceptions;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Wimbley.AppleWatch.HeartRate.Site.Controllers
{
    public class HeartRateController : Controller
    {
        private readonly IHostingEnvironment _environment = null;
        private readonly ILogger _log = ApplicationLogging.CreateLogger<HeartRateController>();

        public HeartRateController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public IActionResult UploadFile()
        {
            _log.LogDebug(new EventId(1), string.Format(@"Uploading {0} file(s)", Request.Form.Files.Count), Request.Form.Files);

            foreach (var file in Request.Form.Files)
            {
                var uploaddir = Path.Combine(_environment.WebRootPath, "uploads");
                var filename = Path.Combine(uploaddir, file.FileName);
                Directory.CreateDirectory(uploaddir);
                
                if (file.Length > 0)
                {
                    var hr = new HeartRateXml();

                    try
                    {
                        if (!hr.VerifyExtension(file.FileName))
                        {
                            throw new FileExtensionNotSupportedException(string.Format(@"File format {0} is not supported. Please upload XML data only at this time", Path.GetExtension(filename)));
                        }
                        
                        using (var fs = new FileStream(filename, FileMode.Create))
                        {
                            file.CopyTo(fs);  
                        }

                        var filesf = System.IO.File.ReadAllBytes(filename);
                        var xmldata = System.Text.Encoding.UTF8.GetString(filesf);
                        xmldata = string.IsNullOrEmpty(xmldata) ? "" : xmldata.Replace(((char)0xFEFF).ToString(), "");
                        
                        var html = hr.Parse(xmldata);

                        return Json(new {msg = "SUCCESS", data = html, errors = ""});
                    }
                    catch (Exception ex)
                    {
                        var errmsg = string.Empty;

                        if (ex is ApplicationException)
                        {
                            errmsg = ex.Message;
                        }
                        else
                        {
                            errmsg = "System erorr ocurred. Please see log file for more details";
                        }
                        
                        _log.LogCritical(new EventId(4), ex, errmsg);

                        return Json(new {msg = "ERROR", data = "", errors = errmsg});
                    }
                }
            }

            return Json(new { msg = "ERROR", data = "", errors = "No files were uploaded successfully" });
        }
    }
}
