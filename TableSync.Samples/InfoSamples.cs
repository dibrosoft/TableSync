using Xunit;
using Xunit.Abstractions;

namespace TableSync.Samples
{
    public class InfoSamples
    {
        private TestContext context;

        public InfoSamples(ITestOutputHelper output)
        {
            context = TestContextFactory.Create(output);
        }

        [Fact]
        public void InfoConnectionString()
        {
            var application = new Application();
            var text = application.Info(context.Config.ConnectionStringOrName, default, default, default);
            context.Output.WriteLine(text);
        }

        [Fact]
        public void InfoConnectionStringAsJson()
        {
            var application = new Application();
            var jsonText = application.Info(context.Config.ConnectionStringOrName, default, default, true);
            context.Output.WriteLine(jsonText);
        }

        [Fact]
        public void InfoConnectionStringAsJsonDeserialized()
        {
            var application = new Application();
            var jsonText = application.Info(context.Config.ConnectionStringOrName, default, default, true);
            var tableInfos = MyJsonConvert.DeserializeObject<TableInfos>(jsonText);
            context.Output.WriteLine(tableInfos.ToString());
        }

        [Fact]
        public void Info()
        {
            var application = new Application();
            var info = application.Info();
            context.Output.WriteLine(info);
        }
    }
}
