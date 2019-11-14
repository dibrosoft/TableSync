using CommandLine;
using System.Collections.Generic;

namespace TSync
{
    [Verb("embed", HelpText = HelpText.Embed)]
    class EmbedOptions
    {
        [Option('c', "ConnectionStringOrName", Required = true, HelpText = HelpText.ConnectionStringOrName)]
        public string ConnectionStringOrName { get; set; }

        [Option('w', "WorkbookFileName", Required = true, HelpText = HelpText.WorkbookFileName)]
        public string WorkbookFileName { get; set; }

        [Option('n', "TableNames", SetName = "simple", Separator = ',', HelpText = HelpText.TableNames)]
        public IEnumerable<string> TableNames { get; set; }

        [Option('d', "SyncDefinitionFileName", SetName = "extended", HelpText = HelpText.SyncDefinitionFileName)]
        public string SyncDefinitionFileName { get; set; }

        [Option('o', "WorkbookOutputFileName", HelpText = HelpText.WorkbookOutputFileName)]
        public string WorkbookOutputFileName { get; set; }

        [Option('f', "FullDefinition", Default = false, HelpText = HelpText.FullDefinition)]
        public bool FullDefinition { get; set; }
    }
}
