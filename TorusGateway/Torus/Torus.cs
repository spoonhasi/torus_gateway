using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using IntelligentApiCS;

namespace TorusGateway.Torus
{
    public class Torus
    {
        public class ItemObject
        {
            [JsonPropertyName("value")]
            public List<JsonElement>? Value { get; set; }

            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("datatype")]
            public string? DataType { get; set; }

            [JsonPropertyName("time")]
            public string? Timestamp { get; set; }

            [JsonPropertyName("address")]
            public string? Address { get; set; }

            [JsonPropertyName("filter")]
            public string? Filter { get; set; }

            /*
JsonValueKind 값 목록
JsonValueKind.Undefined
정의되지 않은 JSON 값. 일반적으로 잘못된 상태를 나타냅니다.

JsonValueKind.Object
JSON 객체를 나타냅니다. { "name": "value" }와 같은 형식을 가집니다.

JsonValueKind.Array
JSON 배열을 나타냅니다. [1, 2, 3]과 같은 형식을 가집니다.

JsonValueKind.String
JSON 문자열을 나타냅니다. "text"와 같은 형식을 가집니다.

JsonValueKind.Number
JSON 숫자를 나타냅니다. 42 또는 3.14와 같은 형식을 가집니다.

JsonValueKind.True
JSON 불리언 값 true를 나타냅니다.

JsonValueKind.False
JSON 불리언 값 false를 나타냅니다.

JsonValueKind.Null
JSON null 값을 나타냅니다.
*/
        }

        private static readonly object _lockInit = new();
        private static Torus? _instance;

        public string AppName { get; private set; }
        public Guid AppGuid { get; private set; }
        private int _connectStatus = -1;
        public string LastErrorMessage { get; private set; } = "";
        public static void Initialize(string appName, string appGuid)
        {
            if (_instance == null)
            {
                lock (_lockInit)
                {
                    _instance ??= new Torus(appName, appGuid);
                }
            }
        }
        private Torus(string appName, string appGuid)
        {
            AppName = appName;
            AppGuid = new Guid(appGuid);
        }
        public static Torus Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("Torus is not initialized. Call Initialize first.");
                }
                return _instance;
            }
        }
        public int Connect()
        {
            try
            {
                Interlocked.CompareExchange(ref _connectStatus, Api.Initialize(AppGuid, AppName), -1);
            }
            catch (DllNotFoundException)
            {
                LastErrorMessage = "Failed to connect to Torus: DLL not found";
            }
            return _connectStatus;
        }

        public string GetData(string address, string filter)
        {
            int result = Api.getData(address, filter, out Item item, true);
            if (item == null)
            {
                return "{\"status\":" + result + "}";
            }
            else
            {
                return item.ToString();
            }
        }

        public string UpdateData(string address, string filter, string value)
        {
            Item InItem = Item.Parse(value);
            int result = Api.updateData(address, filter, InItem, out Item _);
            return "{\"status\":" + result + "}";
        }

        public string UploadFile(string localPath, string ncPath, int machineID)
        {
            int result = Api.UploadFile(localPath, ncPath, machineID);
            return "{\"status\":" + result + "}";
        }

        public string DownloadFile(string ncPath, string localPath, int machineID)
        {
            int result = Api.DownloadFile(ncPath, localPath, machineID);
            return "{\"status\":" + result + "}";
        }

        public string DeleteFile(string ncPath, int machineID)
        {
            int result = Api.CNCFileDelete(ncPath, out Item _, machineID);
            return "{\"status\":" + result + "}";
        }
    }
}
