﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorusGateway.WebServer
{
    [ApiController]
    [Route("[controller]/machine/ncpath")]
    public class FileController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> GetTorusDownloadFile()
        {
            // 동적 쿼리 매개변수를 읽어옴
            var queryParameters = HttpContext.Request.Query;

            // 쿼리 매개변수 로직 추가 (예시)
            int machineID = -1;
            string ncPath = "";
            foreach (var param in queryParameters)
            {
                if (param.Key == "machine")
                {
                    _ = int.TryParse(param.Value, out machineID);
                }
                else if (param.Key == "ncpath")
                {
                    string? tmpNcPath = param.Value;
                    if (tmpNcPath == null)
                    {
                        ncPath = "";
                    }
                    else
                    {
                        ncPath = tmpNcPath;
                    }
                }
            }
            string currentDatetime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string directory = "ncTempFileForDownloading/" + currentDatetime;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string absolutePath = Path.GetFullPath(directory, Directory.GetCurrentDirectory());
            string downloadFileResult = Torus.Torus.Instance.DownloadFile(ncPath, absolutePath + "/", machineID);

            string fileName = Path.GetFileName(ncPath);
            string filePath = absolutePath + "/" + fileName;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "File not found" });
            }

            // 파일 읽기
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // 파일을 다운로드로 전송
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpPut]
        public async Task<ActionResult<string>> PutTorusUploadFile(IFormFile file)
        {
            // 동적 쿼리 매개변수를 읽어옴
            var queryParameters = HttpContext.Request.Query;

            // 쿼리 매개변수 로직 추가 (예시)
            int machineID = -1;
            string ncPath = "";
            foreach (var param in queryParameters)
            {
                if (param.Key == "machine")
                {
                    _ = int.TryParse(param.Value, out machineID);
                }
                else if (param.Key == "ncpath")
                {
                    string? tmpNcPath = param.Value;
                    if (tmpNcPath == null)
                    {
                        ncPath = "";
                    }
                    else
                    {
                        ncPath = tmpNcPath;
                    }
                }
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("File not provided or is empty");
            }

            string currentDatetime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string filePath = "ncTempFileForUploading/" + currentDatetime + "/" + file.FileName;
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "File upload failed", error = ex.Message });
            }

            string absolutePath = Path.GetFullPath(filePath, Directory.GetCurrentDirectory());

            string uploadFileResult = Torus.Torus.Instance.UploadFile(absolutePath, ncPath, machineID);
            return new ContentResult
            {
                Content = uploadFileResult,  // 이미 JSON 형식인 문자열
                ContentType = "application/json",  // JSON MIME 타입 설정
                StatusCode = 200  // 상태 코드 설정 (OK)
            };
        }

        [HttpDelete]
        public ActionResult<string> DeleteTorusUploadFile()
        {
            // 동적 쿼리 매개변수를 읽어옴
            var queryParameters = HttpContext.Request.Query;

            // 쿼리 매개변수 로직 추가 (예시)
            int machineID = -1;
            string ncPath = "";
            foreach (var param in queryParameters)
            {
                if (param.Key == "machine")
                {
                    _ = int.TryParse(param.Value, out machineID);
                }
                else if (param.Key == "ncpath")
                {
                    string? tmpNcPath = param.Value;
                    if (tmpNcPath == null)
                    {
                        ncPath = "";
                    }
                    else
                    {
                        ncPath = tmpNcPath;
                    }
                }
            }

            string deleteFileResult = Torus.Torus.Instance.DeleteFile(ncPath, machineID);
            return new ContentResult
            {
                Content = deleteFileResult,  // 이미 JSON 형식인 문자열
                ContentType = "application/json",  // JSON MIME 타입 설정
                StatusCode = 200  // 상태 코드 설정 (OK)
            };
        }
    }
}
