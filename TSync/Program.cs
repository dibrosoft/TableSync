using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using TableSync;

namespace TSync
{
    class Program
    {
        private static ServiceProvider serviceProvider;

        private static void Init()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<Program>()
                .Build();

            var connections = ConnectionsProvider.GetDefaultInstance();
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

                workbook.Upload(opts.ConnectionStringOrName, syncDefinition, settings, opts.RemoveMissingRows);
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
            var output = application.Info(opts.ConnectionStringOrName, opts.TableNames, opts.WorkbookFileName, opts.Json);
            Console.Out.Write(output);

            return ExitCode.Success;
        }
    }
}
