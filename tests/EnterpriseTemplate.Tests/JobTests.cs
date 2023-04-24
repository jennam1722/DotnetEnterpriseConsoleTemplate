using EnterpriseTemplate.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.AutoMock;
using Moq;
using EnterpriseTemplate.Tests.Extensions;
using System.Xml.Linq;
using EnterpriseTemplate.Services;

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
        mockLogger.Setup(a => a.IsEnabled(LogLevel.Information)).Returns(true);
        var job = mocker.CreateInstance<Job>();
        var source = new CancellationTokenSource();
        await job.StartAsync(source.Token);
        mockLogger.VerifyLogMessage(LogLevel.Critical, "Print 10", 0);
        mockLogger.VerifyLogMessage(LogLevel.Information, $"Hello {name}!", 1);
        mockLogger.VerifyLogMessage(LogLevel.Critical, $"Hello {name}!", 0);
        mocker.VerifyAll();
    }


    [DataRow("2023-03-01", 0, false)]
    [DataRow("2023-01-01", 1, true)]
    [TestMethod]
    public async Task Job_Happy_NewYear_ShouldWork(string dateVal, int happyNewYearCallCount, bool newYearReturn)
    {
        var options = Options.Create(new EnterpriseTemplateContext() { Name = "Test" });
        var mocker = new AutoMocker();
        mocker.Use(options);
        var mockDateService = mocker.GetMock<IDateService>();
        mockDateService.Setup(a => a.Today).Returns(DateTime.Parse(dateVal));
        var mockFirstDateOfYearService = mocker.GetMock<IFirstDayOfYearService>();
        mockFirstDateOfYearService.Setup(a => a.IsFirstDayOfYear()).Returns(newYearReturn);

        var mockLogger = mocker.GetMock<ILogger<Job>>();
        mockLogger.Setup(a => a.IsEnabled(LogLevel.Critical)).Returns(true);
        var job = mocker.CreateInstance<Job>();
        var source = new CancellationTokenSource();
        await job.StartAsync(source.Token);

        mockLogger.VerifyLogMessage(LogLevel.Critical, "Happy New Year", happyNewYearCallCount);
    }

    [TestMethod]
    public async Task Job_StopAsync_ShouldWork()
    {
        var options = Options.Create(new EnterpriseTemplateContext() { Name = "Test" });
        var mocker = new AutoMocker();
        mocker.Use(options);
        var job = mocker.CreateInstance<Job>();
        var source = new CancellationTokenSource();
        var task = job.StopAsync(source.Token).ConfigureAwait(false);
        Assert.IsNotNull(task);

    }
}