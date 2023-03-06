using EnterpriseTemplate.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.AutoMock;
using Moq;
using EnterpriseTemplate.Tests.Extensions;

namespace EnterpriseTemplate.Tests;

[TestClass]
public class JobTests
{
    [DataRow("Bob")]
    [DataRow("")]
    [TestMethod]
    public async Task Job_StartAsync_ShouldWork(string name)
    {
        var options = Options.Create(new EnterpriseTemplateContext() { Name = name });
        var mocker = new AutoMocker();
        mocker.Use(options);
        var mockLogger = mocker.GetMock<ILogger<Job>>();
        mockLogger.Setup(a => a.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var job = mocker.CreateInstance<Job>();
        var source = new CancellationTokenSource();
        await job.StartAsync(source.Token);

        mockLogger.VerifyLogMessage(LogLevel.Information, $"Hello {name}!", 1);
        mockLogger.VerifyLogMessage(LogLevel.Critical, $"Hello {name}!", 1);
        mocker.VerifyAll();
    }
}