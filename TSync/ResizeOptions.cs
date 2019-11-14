using CommandLine;
using System.Collections.Generic;

namespace TSync
{
    [Verb("resize", HelpText = HelpText.Resize)]
    class ResizeOptions
    {
        [Option('w', "WorkbookFileName", Required = true, HelpText = HelpText.WorkbookFileName)]
        public string WorkbookFileName { get; set; }

        [Option('n', "TableNames", SetName = "simple", Separator = ',', HelpText = HelpText.TableNames)]
        public IEnumerable<string> TableNames { get; set; }

        [Option('d', "SyncDefinitionFileName", SetName = "extended", HelpText = HelpText.SyncDefinitionFileName)]
        public string SyncDefinitionFileName { get; set; }

        [Option('c', "ConnectionStringOrName", HelpText = HelpText.ConnectionStringOrName)]
        public string ConnectionStringOrName { get; set; }

        [Option('o', "WorkbookOutputFileName", HelpText = HelpText.WorkbookOutputFileName)]
        public string WorkbookOutputFileName { get; set; }
    }
}
