using CommandLine;
using System.Collections.Generic;

namespace TSync
{
    [Verb("upload", HelpText = HelpText.Upload)]
    class UploadOptions
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

        [Option('a', "AutoResize", Default = false, HelpText = HelpText.AutoResize)]
        public bool AutoResize { get; set; }
    }
}
