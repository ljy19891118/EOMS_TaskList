using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace WebUtility
{
    /// <summary> 
    /// 解析JSON，仿Javascript风格 
    /// </summary> 
    static class JSON
    {

        public static T parse<T>(string jsonString)
        {
           

            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(jsonString)));

        }

     }
}
