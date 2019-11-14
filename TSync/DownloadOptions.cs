using CommandLine;
using System.Collections.Generic;

namespace TSync
{
    [Verb("download", HelpText = HelpText.Download)]
    class DownloadOptions
    {
        [Option('c', "ConnectionStringOrName", Required = true, HelpText = HelpText.ConnectionStringOrName)]
        public string ConnectionStringOrName { get; set; }

        [Option('w', "WorkbookFileName", Required = true, HelpText = HelpText.WorkbookFileName)]
        public string WorkbookFileName { get; set; }

        [Option('n', "TableNames", SetName = "simple", Separator = ',', HelpText = HelpText.TableNames)]
        public IEnumerable<string> TableNames { get; set; }

        [Option('d', "SyncDefinitionFileName", SetName = "extended", HelpText = HelpText.SyncDefinitionFileName)]
        public string SyncDefinitionFileName { get; set; }

        [Option('s', "SettingsFileName", SetName = "extended", HelpText = HelpText.SettingsFileName)]
        public string SettingsFileName { get; set; }

        [Option('o', "WorkbookOutputFileName", HelpText = HelpText.WorkbookOutputFileName)]
        public string WorkbookOutputFileName { get; set; }

        [Option('k', "KeepFormula", Default = false, HelpText = HelpText.KeepFormula)]
        public bool KeepFormula { get; set; }
    }
}
