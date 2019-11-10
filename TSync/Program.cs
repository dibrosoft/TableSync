using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TableSync;

namespace TSync
{

    class HelpText
    {
        public const string Download = "Download data from sql database into workbook.";
        public const string Upload = "Upload data from workbook into sql database.";
        public const string Embed = "Embed synchronisation definition into workbook.";
        public const string Resize = "Resize workbook ranges to fit to data.";
        public const string Info = "Get information about synchronisation or database objects.";

        public const string ConnectionStringOrName = "The database connection string or the name of a registered connection string from appsettings.json. You can query the registered connection string s with 'tsync list'.";
        public const string WorkbookFileName = "The file name of a new or existing workbook. Only the xlsx format is supported.";
        public const string TableNames = "Comma separated list of table names for a simple synchronisation definition. You can use the underscore character to prefix table names with a database scheme.";
        public const string TableName = "Table name to list columns.";
        public const string Json = "Output in Json format.";
        public const string SyncDefinitionFileName = "File name of a synchronisation definiton (JSON).";
        public const string SettingsFileName = "File name of additional settings (JSON).";
        public const string WorkbookOutputFileName = "Optional output workbook file name. If used, the result of the operation is saved to this file. Otherwise the original workbook will be overwritten.";
        public const string AutoResize = "Option to configure that 'resize' should always run before upload.";
        public const string KeepFormula = "Option to configure that download will not overwrite formulas in the download area.";
        public const string FullDefinition = "Option to embed a full instead of a simple synchronisation definition.";
    }

    class Program
    {
        private static ServiceProvider serviceProvider;
        private static Connections connections;

        private static void Init()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<Program>()
                .Build();

            connections = new Connections();
            configBuilder.GetSection("TSync:Connections").Bind(connections);
            var application = new Application(connections);

            var services = new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.AddConfiguration(configBuilder.GetSection("Logging"));
                    logging.AddConsole();
                })
                .AddSingleton(application);

            serviceProvider = services.BuildServiceProvider();
        }

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

            [Option('f', "FullDefinition", Default = false, HelpText = HelpText.FullDefinition )]
            public bool FullDefinition { get; set; }
        }

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

        [Verb("info", HelpText = HelpText.Info)]
        class InfoOptions
        {
            [Option('w', "WorkbookFileName", SetName = "wb", HelpText = HelpText.WorkbookFileName)]
            public string WorkbookFileName { get; set; }

            [Option('c', "ConnectionStringOrName", SetName = "data", HelpText = HelpText.ConnectionStringOrName)]
            public string ConnectionStringOrName { get; set; }

            [Option('t', "TableName", SetName = "data", HelpText = HelpText.TableName)]
            public string TableName { get; set; }

            [Option('j', "Json", HelpText = HelpText.Json)]
            public bool Json { get; set; }
        }

        static int Main(string[] args)
        {
            Init();

            try
            {
                return CommandLine.Parser.Default.ParseArguments<DownloadOptions, UploadOptions, EmbedOptions, ResizeOptions, InfoOptions>(args)
                    .MapResult(
                        (DownloadOptions opts) => Download(opts),
                        (UploadOptions opts) => Upload(opts),
                        (EmbedOptions opts) => Embed(opts),
                        (ResizeOptions opts) => Resize(opts),
                        (InfoOptions opts) => Info(opts),
                        errs => ExitCode.Failed);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                return ExitCode.Failed;
            }
        }

        private static int Download(DownloadOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.CreateOrOpen(opts.WorkbookFileName))
            {
                var syncDefinition = Application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);
                var settings = Application.GetSettingsOrDefault(opts.SettingsFileName);

                workbook.Download(opts.ConnectionStringOrName, opts.KeepFormula, syncDefinition, settings);

                if (string.IsNullOrEmpty(opts.WorkbookOutputFileName))
                    workbook.Save();
                else
                    workbook.SaveAs(opts.WorkbookOutputFileName);
            }

            return ExitCode.Success;
        }

        private static int Upload(UploadOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.Open(opts.WorkbookFileName))
            {
                var syncDefinition = Application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);
                var settings = Application.GetSettingsOrDefault(opts.SettingsFileName);

                if (opts.AutoResize)
                {
                    var hasChanged = workbook.Resize(opts.ConnectionStringOrName, syncDefinition);
                    if (hasChanged)
                        workbook.Save();
                }

                workbook.Upload(opts.ConnectionStringOrName, syncDefinition, settings);
            }

            return ExitCode.Success;
        }

        private static int Embed(EmbedOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.CreateOrOpen(opts.WorkbookFileName))
            {
                var definition = Application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);
                if (definition == null)
                    throw new MissingSyncDefinitionException();

                workbook.EmbedDefinition(opts.ConnectionStringOrName, definition, opts.FullDefinition);

                if (string.IsNullOrEmpty(opts.WorkbookOutputFileName))
                    workbook.Save();
                else
                    workbook.SaveAs(opts.WorkbookOutputFileName);
            }

            return ExitCode.Success;
        }

        private static int Resize(ResizeOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.Open(opts.WorkbookFileName))
            {
                var syncDefinition = Application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);

                var hasChanged = workbook.Resize(opts.ConnectionStringOrName, syncDefinition);
                if (hasChanged)
                    if (string.IsNullOrEmpty(opts.WorkbookOutputFileName))
                        workbook.Save();
                    else
                        workbook.SaveAs(opts.WorkbookOutputFileName);
            }

            return ExitCode.Success;
        }

        private static int Info(InfoOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            var output = application.Info(opts.ConnectionStringOrName, opts.TableName, opts.WorkbookFileName, opts.Json);
            Console.Out.Write(output);

            return ExitCode.Success;
        }
    }

    class ExitCode
    {
        public const int Success = 0;
        public const int Failed = 1;
    }
}
