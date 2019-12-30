using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TableSync.Samples
{
    public static class TestContextFactory
    {
        public static TestContext Create(ITestOutputHelper output)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<TestContext>()
                .Build();

            var configurationName = configBuilder.GetValue<string>("TestConfigurationName");
            var config = new TestConfiguration();
            configBuilder.GetSection(configurationName).Bind(config);

            var filterOptions = new LoggerFilterOptions();
            configBuilder.GetSection("LoggingFilterOptions").Bind(filterOptions);
            var loggerFactory = new LoggerFactory(new[] { new XunitLoggerProvider(output) }, filterOptions);

            return new TestContext()
            {
                Output = output,
                Config = config,
                LoggerFactory = loggerFactory
            };
        }
    }
}
