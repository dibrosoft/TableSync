using System;
using System.Collections.ObjectModel;

namespace TableSync
{
    public class Connections : KeyedCollection<string, Connection>
    {
        public Connections() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected override string GetKeyForItem(Connection item)
        {
            return item.Name;
        }

        public string GetProofedConnectionStringName(string connectionStringOrName)
        {
            var isEmpty = string.IsNullOrEmpty(connectionStringOrName);

            if (!isEmpty && Contains(connectionStringOrName))
                return connectionStringOrName;

            return null;
        }

        public ConnectionInfo GetConnectionInfo(string connectionStringOrName)
        {
            if (this == null)
                return CreateInfoFromConnectionString(connectionStringOrName);

            var connectionStringName = GetProofedConnectionStringName(connectionStringOrName);
            if (connectionStringName != null)
                return new ConnectionInfo()
                {
                    ConnectionString = this[connectionStringName].ConnectionString,
                    HiddenTableNames = this[connectionStringName].HiddenTableNames
                };

            return CreateInfoFromConnectionString(connectionStringOrName);
        }

        private ConnectionInfo CreateInfoFromConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new MissingConnectionStringException();

            return new ConnectionInfo()
            {
                ConnectionString = connectionString,
                HiddenTableNames = new HiddenTableNames()
            };
        }

        public HiddenTableNames GetHiddenTableNames(string connectionStringOrName)
        {
            var connectionStringName = GetProofedConnectionStringName(connectionStringOrName);
            if (connectionStringName != null)
                return this[connectionStringName].HiddenTableNames;

            return new HiddenTableNames();
        }
    }
}
