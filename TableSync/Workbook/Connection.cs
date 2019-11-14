using Newtonsoft.Json;

namespace TableSync
{
    public class Connection
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        
        public HiddenTableNames HiddenTableNames {get; set;}

        public override string ToString()
        {
            return Name;
        }
    }
}
