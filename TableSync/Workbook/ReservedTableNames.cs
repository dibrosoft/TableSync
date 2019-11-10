using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TableSync
{
    public class ReservedTableNames : HashSet<string> 
    {
        public ReservedTableNames() : base(StringComparer.InvariantCultureIgnoreCase) { }
    }
}
