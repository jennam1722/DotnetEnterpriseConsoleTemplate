using EnterpriseTemplate.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.AutoMock;
using Moq;

namespace EnterpriseTemplate.Tests;

[TestClass]
public class JobTests
{
    [TestMethod]
    public async Task Job_StartAsync_ShouldWork()
    {
        var options = Options.Create(new EnterpriseTemplateContext() { Name = "Bob" });
        var mocker = new AutoMocker();
        mocker.Use(options);
        var mockLogger = mocker.GetMock<ILogger<Job>>();
        mockLogger.Setup(a => a.IsEnabled(LogLevel.Critical)).Returns(true);
        //mockLogger.Setup(a => a.Log(LogLevel.Critical, "Hello Bob!",));
        var job = mocker.CreateInstance<Job>();
        var source = new CancellationTokenSource();
        await job.StartAsync(source.Token);
        //mockLogger.Verify(mockLogger => mockLogger.Log(LogLevel.Critical, It.IsAny<EventId>, It.IsAny<It.IsAnyType>, It.IsAny<Exception?>, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),Times.Exactly(1));
        mocker.VerifyAll();
    }
}