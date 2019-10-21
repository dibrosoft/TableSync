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

        [Verb("upload", HelpText = "Upload data from workbook into sql database.")]
        class UploadOptions
        {
            [Option('c', "ConnectionStringOrName", Required = false, HelpText = "The database connection string or name. You can use a full connection string or a name from tsync's appsettings.json. If you leave this option empty tsync will asume the name 'default' from it's appsettings.json.")]
            public string ConnectionStringOrName { get; set; }

            [Option('w', "WorkbookFileName", Required = true, HelpText = "The workbook file name. Only the xlsx format is allowed.")]
            public string WorkbookFileName { get; set; }

            [Option('n', "TableNames", Required = false, Separator = ',', HelpText = "Simple synchronisation definition with comma separated table names. You can use the underscore character to prefix table names with a database schema.")]
            public IEnumerable<string> TableNames { get; set; }

            [Option('d', "SyncDefinitionFileName", Required = false, HelpText = "A synchronisation definiton as JSON file.")]
            public string SyncDefinitionFileName { get; set; }

            [Option('s', "SettingsFileName", Required = false, HelpText = "Additional settings as JSON file.")]
            public string SettingsFileName { get; set; }

            [Option('a', "AutoResize", Required = false, Default = false, HelpText = "Use this option if 'resize' should always run before upload.")]
            public bool AutoResize { get; set; }
        }

        [Verb("download", HelpText = "Download data from sql database into workbook.")]
        class DownloadOptions
        {
            [Option('c', "ConnectionStringOrName", Required = false, HelpText = "The database connection string or name. You can use a full connection string or a name from tsync's appsettings.json. If you leave this option empty tsync will asume the name 'default' from it's appsettings.json.")]
            public string ConnectionStringOrName { get; set; }

            [Option('w', "WorkbookFileName", Required = true, HelpText = "The workbook file name. Only the xlsx format is allowed.")]
            public string WorkbookFileName { get; set; }

            [Option('n', "TableNames", Required = false, Separator = ',', HelpText = "Simple synchronisation definition with comma separated table names. You can use the underscore character to prefix table names with a database schema.")]
            public IEnumerable<string> TableNames { get; set; }

            [Option('d', "SyncDefinitionFileName", Required = false, HelpText = "A synchronisation definiton as JSON file.")]
            public string SyncDefinitionFileName { get; set; }

            [Option('s', "SettingsFileName", Required = false, HelpText = "Additional settings as JSON file.")]
            public string SettingsFileName { get; set; }

            [Option('o', "WorkbookOutputFileName", Required = false, HelpText = "An optional output workbook file name. If used the download is saved to this file. Otherwise the original workbook will be overwritten.")]
            public string WorkbookOutputFileName { get; set; }

            [Option('k', "KeepFormula", Required = false, Default = false, HelpText = "Download overwrites formulas in the download area. Use this option if you wan't to keep them.")]
            public bool KeepFormula { get; set; }
        }

        [Verb("updatedef", HelpText = "Insert or update synchronisation definition in workbook.")]
        class UpdateDefinitionOptions
        {
            [Option('w', "WorkbookFileName", Required = true, HelpText = "The workbook file name. Only the xlsx format is allowed.")]
            public string WorkbookFileName { get; set; }

            [Option('n', "TableNames", Required = false, Separator = ',', HelpText = "Simple synchronisation definition with comma separated table names. You can use the underscore character to prefix table names with a database schema.")]
            public IEnumerable<string> TableNames { get; set; }

            [Option('d', "SyncDefinitionFileName", Required = false, HelpText = "A synchronisation definiton as JSON file.")]
            public string SyncDefinitionFileName { get; set; }

            [Option('o', "WorkbookOutputFileName", Required = false, HelpText = "An optional output workbook file name. If used the download is saved to this file. Otherwise the original workbook will be overwritten.")]
            public string WorkbookOutputFileName { get; set; }

            [Option('f', "InsertFullDefinition", Required = false, Default = false, HelpText = "Insert the full synchronisation definition. If true the parts of the definition currently not needed are also inserted into the workbook." )]
            public bool InsertFullDefinition { get; set; }
        }

        [Verb("getdef", HelpText = "Get synchronisation definition from workbook.")]
        class GetDefinitionOptions 
        {
            [Option('w', "WorkbookFileName", Required = true, HelpText = "The workbook file name. Only the xlsx format is allowed.")]
            public string WorkbookFileName { get; set; }
        }

        [Verb("resize", HelpText = "Resize ranges to fit to data.")]
        class ResizeOptions
        {
            [Option('w', "WorkbookFileName", Required = true, HelpText = "The workbook file name. Only the xlsx format is allowed.")]
            public string WorkbookFileName { get; set; }

            [Option('n', "TableNames", Required = false, Separator = ',', HelpText = "Simple synchronisation definition with comma separated table names. You can use the underscore character to prefix table names with a database schema.")]
            public IEnumerable<string> TableNames { get; set; }

            [Option('d', "SyncDefinitionFileName", Required = false, HelpText = "A synchronisation definiton as JSON file.")]
            public string SyncDefinitionFileName { get; set; }

            [Option('c', "ConnectionStringOrName", Required = false, HelpText = "The database connection string or name to get table information about. You can use a full connection string or a name from tsync's appsettings.json. If you leave this option empty tsync will list the available names from appsettings.json.")]
            public string ConnectionStringOrName { get; set; }

            [Option('o', "WorkbookOutputFileName", Required = false, HelpText = "An optional output workbook file name. If used the download is saved to this file. Otherwise the original workbook will be overwritten.")]
            public string WorkbookOutputFileName { get; set; }
        }

        [Verb("info", HelpText = "Get information about connections and tables.")]
        class InfoOptions
        {
            [Option('c', "ConnectionStringOrName", Required = false, HelpText = "The database connection string or name to get table information about. You can use a full connection string or a name from tsync's appsettings.json. If you leave this option empty tsync will list the available names from appsettings.json.")]
            public string ConnectionStringOrName { get; set; }

            [Option('t', "TableName", Required = false, HelpText = "The table name to get column information about. If you leave this option empty tsync will list the available tables.")]
            public string TableName { get; set; }
        }

        static int Main(string[] args)
        {
            Init();

            try
            {
                return CommandLine.Parser.Default.ParseArguments<DownloadOptions, UploadOptions, UpdateDefinitionOptions, GetDefinitionOptions, ResizeOptions, InfoOptions>(args)
                    .MapResult(
                        (DownloadOptions opts) => Download(opts),
                        (UploadOptions opts) => Upload(opts),
                        (UpdateDefinitionOptions opts) => UpdateDefinition(opts),
                        (GetDefinitionOptions opts) => GetDefinition(opts),
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
            using (var workbook = application.Open(opts.WorkbookFileName, false))
            {
                var syncDefinition = application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);
                var settings = application.GetSettingsOrDefault(opts.SettingsFileName);

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
                var syncDefinition = application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);
                var settings = application.GetSettingsOrDefault(opts.SettingsFileName);

                if (opts.AutoResize)
                    workbook.Resize(opts.ConnectionStringOrName, syncDefinition);

                workbook.Upload(opts.ConnectionStringOrName, syncDefinition, settings);
            }

            return ExitCode.Success;
        }

        private static int UpdateDefinition(UpdateDefinitionOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.Open(opts.WorkbookFileName, false))
            {
                var definition = application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);
                if (definition == null)
                    throw new MissingSyncDefinitionException();

                workbook.UpdateDefinition(definition, opts.InsertFullDefinition);

                if (string.IsNullOrEmpty(opts.WorkbookOutputFileName))
                    workbook.Save();
                else
                    workbook.SaveAs(opts.WorkbookOutputFileName);
            }

            return ExitCode.Success;
        }

        private static int GetDefinition(GetDefinitionOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.Open(opts.WorkbookFileName))
            {
                var definition = workbook.GetDefinition();
                var text = MyJsonConvert.SerializeObject(definition);
                Console.Out.Write(text);
            }

            return ExitCode.Success;
        }

        private static int Resize(ResizeOptions opts)
        {
            var application = serviceProvider.GetService<Application>();
            using (var workbook = application.Open(opts.WorkbookFileName))
            {
                var syncDefinition = application.GetDefinitionOrDefault(opts.TableNames, opts.SyncDefinitionFileName);

                workbook.Resize(opts.ConnectionStringOrName, syncDefinition);

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
            Console.Out.Write(application.Info(opts.ConnectionStringOrName, opts.TableName));

            return ExitCode.Success;
        }
    }

    class ExitCode
    {
        public const int Success = 0;
        public const int Failed = 1;
    }
}
