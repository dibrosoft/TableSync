using System.Collections.Generic;
using System.Text;

namespace TableSync
{
    public class QueryBuilder
    {
        private StringBuilder sqlBuilder = new StringBuilder();

        public void Append(string value) => sqlBuilder.Append(value);

        public string Sql { get { return sqlBuilder.ToString(); } }
        public List<QueryParameter> Parameters { get; set; } = new List<QueryParameter>(); 
    }

    public class QueryParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
