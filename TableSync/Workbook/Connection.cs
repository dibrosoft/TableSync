using Newtonsoft.Json;

namespace TableSync
{
    public class Connection
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        
        public ReservedTableNames ReservedTableNames {get; set;}

        public override string ToString()
        {
            return Name;
        }
    }
}
