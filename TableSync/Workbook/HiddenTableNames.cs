using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TableSync
{
    public class HiddenTableNames : HashSet<string> 
    {
        public HiddenTableNames() : base(StringComparer.InvariantCultureIgnoreCase) { }
    }
}
