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

        public string GetConnectionString(string connectionStringOrName)
        {
            if (!string.IsNullOrEmpty(connectionStringOrName))
            {
                var valueIsName = this != null && this.Contains(connectionStringOrName);
                if (!valueIsName)
                    return connectionStringOrName;

                return this[connectionStringOrName].ConnectionString;
            }

            var connectionsContainDefault = this != null && this.Contains("Default");
            if (connectionsContainDefault)
                return this["Default"].ConnectionString;

            throw new MissingConnectionStringException();
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
