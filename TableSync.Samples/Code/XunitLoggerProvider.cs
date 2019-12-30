namespace TableSync.Samples
{
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly LogLevel _minLogLevel;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper, LogLevel minLogLevel = LogLevel.Information)
        {
            _testOutputHelper = testOutputHelper;
            _minLogLevel = minLogLevel;
        }

        public ILogger CreateLogger(string categoryName)
            => new XunitLogger(_testOutputHelper, categoryName, _minLogLevel);

        public void Dispose() { }
    }
}