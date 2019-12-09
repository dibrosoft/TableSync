using System;
using System.IO;
using System.Reflection;

namespace TableSync
{
    public static class ConnectionsProvider
    {
        public static string GetDefaultConnectionsPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tsync", "connections.json");
        }

        public static Connections GetInstance(string path)
        {
            return MyJsonConvert.DeserializeObjectFromFile<Connections>(path);
        }

        public static Connections GetDefaultInstance()
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tsync");
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            var connectionsPath = Path.Combine(configPath, "connections.json");
            if (!File.Exists(connectionsPath))
                WriteResourceToFile(Assembly.GetExecutingAssembly(), "TableSync.connections.json", connectionsPath);

            return GetInstance(connectionsPath);
        }

        private static void WriteResourceToFile(Assembly assembly, string resourceName, string destinationPath)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            using (var file = new FileStream(destinationPath, FileMode.Create))
            {
                var buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    file.Write(buffer, 0, bytesRead);
            }
        }
    }
}
