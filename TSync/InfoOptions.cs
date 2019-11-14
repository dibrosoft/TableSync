using CommandLine;
using System.Collections.Generic;

namespace TSync
{
    [Verb("info", HelpText = HelpText.Info)]
    class InfoOptions
    {
        [Option('w', "WorkbookFileName", SetName = "wb", HelpText = HelpText.WorkbookFileName)]
        public string WorkbookFileName { get; set; }

        [Option('c', "ConnectionStringOrName", SetName = "data", HelpText = HelpText.ConnectionStringOrName)]
        public string ConnectionStringOrName { get; set; }

        [Option('n', "TableNames", SetName = "data", Separator = ',', HelpText = HelpText.TableNames)]
        public IEnumerable<string> TableNames { get; set; }

        [Option('j', "Json", HelpText = HelpText.Json)]
        public bool Json { get; set; }
    }
}
