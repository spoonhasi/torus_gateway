using Microsoft.AspNetCore.Http;
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
    [Route("[controller]")]
    public class MachineController : ControllerBase
    {
        // GET machine/{*segments}
        [HttpGet("{*segments}")]
        public ActionResult<string> GetTorusGetData(string segments)
        {
            // 동적 경로 세그먼트 처리
            var segmentArray = segments.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // 동적 쿼리 매개변수를 읽어옴
            var queryParameters = HttpContext.Request.Query;

            // 쿼리 매개변수 로직 추가 (예시)
            string address = "data://machine/" + $"{string.Join("/", segmentArray)}";
            string filter = "";
            bool firstParam = true;
            foreach (var param in queryParameters)
            {
                if (firstParam)
                {
                    filter += $"{param.Key}={param.Value}";
                    firstParam = false;
                }
                else
                {
                    filter += $"&{param.Key}={param.Value}";
                }
            }
            string fullPath = address + "?" + filter;
            //http://localhost:5000/machine/channel/axis/machinePosition?machine=1&channel=1&axis=1

            string getDataResult = Torus.Torus.Instance.GetData(address, filter);
            return new ContentResult
            {
                Content = getDataResult,  // 이미 JSON 형식인 문자열
                ContentType = "application/json",  // JSON MIME 타입 설정
                StatusCode = 200  // 상태 코드 설정 (OK)
            };
        }

        // PUT machine/{*segments}
        [HttpPut("{*segments}")]
        public async Task<ActionResult<string>> PutTorusUpdateData(string segments)
        {
            /*
            curl -Method PUT "http://localhost:5001/machine/channel/variable/userVariable?machine=1&channel=1&variable=10" -Headers @{"Content-Type"="application/json"} -Body '{"value":[51.999]}'
            */

            // 동적 경로 세그먼트 처리
            var segmentArray = segments.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // 동적 쿼리 매개변수를 읽어옴
            var queryParameters = HttpContext.Request.Query;

            // 쿼리 매개변수 로직 추가 (예시)
            string address = "data://machine/" + $"{string.Join("/", segmentArray)}";
            string filter = "";
            bool firstParam = true;
            foreach (var param in queryParameters)
            {
                if (firstParam)
                {
                    filter += $"{param.Key}={param.Value}";
                    firstParam = false;
                }
                else
                {
                    filter += $"&{param.Key}={param.Value}";
                }
            }
            string fullPath = address + "?" + filter;

            // 비동기적으로 원시 데이터를 읽어옴
            using (var reader = new StreamReader(Request.Body))
            {
                string body = await reader.ReadToEndAsync();
                
                string updateDataResult = Torus.Torus.Instance.UpdateData(address, filter, body);
                return new ContentResult
                {
                    Content = updateDataResult,  // 이미 JSON 형식인 문자열
                    ContentType = "application/json",  // JSON MIME 타입 설정
                    StatusCode = 200  // 상태 코드 설정 (OK)
                };
            }
        }
    }
}
