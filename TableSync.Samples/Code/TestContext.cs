using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TableSync.Samples
{

    public class TestContext
    {
        public ITestOutputHelper Output { get; set; }
        public TestConfiguration Config { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
    }
}
