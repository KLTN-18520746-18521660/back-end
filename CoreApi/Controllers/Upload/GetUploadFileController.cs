// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Common;
// using CoreApi.Common;
// using CoreApi.Services;
// using DatabaseAccess.Context.Models;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json.Linq;

// namespace CoreApi.Controllers.Upload
// {
//     [ApiController]
//     [Route("/api/upload")]
//     public class UploadFileController : BaseController
//     {
//         #region Config Values
//         private int MAX_LENGTH_OF_SINGLE_FILE;
//         private int MAX_LENGTH_OF_MULTIPLE_FILE;
//         #endregion

//         public UploadFileController(BaseConfig _BaseConfig)
//             : base(_BaseConfig)
//         {
//             __ControllerName = "SocialUserLogin";
//             LoadConfig();
//         }

//         [NonAction]
//         public override void LoadConfig()
//         {
//             string Error = string.Empty;
//             try {
//                 (MAX_LENGTH_OF_SINGLE_FILE, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE);
//                 (MAX_LENGTH_OF_MULTIPLE_FILE, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_MULTIPLE_FILE);
//                 __LoadConfigSuccess = true;
//             } catch (Exception e) {
//                 __LoadConfigSuccess = false;
//                 StringBuilder msg = new StringBuilder(e.ToString());
//                 if (Error != e.Message && Error != string.Empty) {
//                     msg.Append($" && Error: { Error }");
//                 }
//                 LogError($"Load config value failed, message: { msg }");
//             }
//         }

//         [HttpPost("files")]
//         public async Task<IActionResult> UploadFiles(List<IFormFile> files)
//         {
//             if (!LoadConfigSuccess) {
//                 return Problem(500, "Internal Server error.");
//             }
//             #region Set TraceId for services
//             #endregion
//             try {
//                 long size = files.Sum(f => f.Length);
//                 if (size > MAX_LENGTH_OF_MULTIPLE_FILE) {
//                     return Problem(400, "Exceed max size of files.");
//                 }

//                 List<string> filesPath = new List<string>();
//                 foreach (var formFile in files) {
//                     if (formFile.Length > 0) {
//                         var filePath = Path.Combine(Program.ServerConfiguration.UploadFilePath, Path.GetRandomFileName());
//                         using (var stream = System.IO.File.Create(filePath))
//                         {
//                             await formFile.CopyToAsync(stream);
//                         }
//                         filesPath.Add(filePath);
//                     }
//                 }

//                 return Ok(200, "OK", new JObject(){
//                     { "files", Utils.ObjectToJsonToken(filesPath) }
//                 });
//             } catch (Exception e) {
//                 LogError($"Unexpected exception, message: { e.ToString() }");
//                 return Problem(500, "Internal Server error.");
//             }
//         }

//         [HttpPost("file")]
//         public async Task<IActionResult> UploadFile(IFormFile formFile)
//         {
//             if (!LoadConfigSuccess) {
//                 return Problem(500, "Internal Server error.");
//             }
//             #region Set TraceId for services
//             #endregion
//             try {
//                 if (formFile.Length > MAX_LENGTH_OF_SINGLE_FILE) {
//                     return Problem(400, "Exceed max size of file.");
//                 }

//                 var filePath = Path.Combine(Program.ServerConfiguration.UploadFilePath, Path.GetRandomFileName());
//                 using (var stream = System.IO.File.Create(filePath))
//                 {
//                     await formFile.CopyToAsync(stream);
//                 }

//                 return Ok(200, "OK", new JObject(){
//                     { "file", filePath }
//                 });
//             } catch (Exception e) {
//                 LogError($"Unexpected exception, message: { e.ToString() }");
//                 return Problem(500, "Internal Server error.");
//             }
//         }
//     }
// }