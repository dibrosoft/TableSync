using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TableSync
{
    public static class MyJsonConvert
    {

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, mySerializerSettings);
        }

        public static void SerializeObjectToFile(object obj, string fileName)
        {
            File.WriteAllText(fileName, SerializeObject(obj));
        }

        public static T DeserializeObject<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text, mySerializerSettings);
        }

        public static T DeserializeObjectFromFile<T>(string fileName)
        {
            return DeserializeObject<T>(File.ReadAllText(fileName));
        }

        private static JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Error = HandleSerializationError
        };

        private static void HandleSerializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            //            var currentError = e.ErrorContext.Error.Message;
            e.ErrorContext.Handled = false;
        }
    }
}
