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

        public string GetConnectionString(string connectionStringOrName)
        {
            var connectionStringName = GetProofedConnectionStringName(connectionStringOrName);
            if (connectionStringName != null)
                return this[connectionStringName].ConnectionString;

            if (string.IsNullOrEmpty(connectionStringOrName))
                throw new MissingConnectionStringException();

            return connectionStringOrName;
        }
    }

    public class Connection
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
