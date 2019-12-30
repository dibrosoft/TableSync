using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace TableSync.Samples
{
    public class DownloadSamples
    {
        private TestContext context;

        public DownloadSamples(ITestOutputHelper output)
        {
            context = TestContextFactory.Create(output);
        }

        [Fact]
        public void DirectDownload()
        {
            var application = new Application();
            var workbookFilename = Path.Combine(context.Config.DownloadPath, "DirectDownload.xlsx");
            using (var workbook = application.CreateOrOpen(workbookFilename))
            {
                workbook.Download(context.Config.ConnectionStringOrName, context.Config.TableNames);
                workbook.Save();
            }
        }

        [Fact]
        public void DownloadWithEmbeddedDefinition()
        {
            var application = new Application();
            var workbookFilename = Path.Combine(context.Config.DownloadPath, "DownloadWithEmbeddedDefinition.xlsx");
            using (var workbook = application.CreateOrOpen(workbookFilename))
            {
                workbook.EmbedDefinition(context.Config.ConnectionStringOrName, context.Config.TableNames, true);
                workbook.Save();
            }

            using (var workbook = application.Open(workbookFilename))
            {
                workbook.Download(context.Config.ConnectionStringOrName);
                workbook.Save();
            }
        }
    }
}
