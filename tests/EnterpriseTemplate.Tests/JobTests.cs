using EnterpriseTemplate.Models;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

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
        var job = mocker.CreateInstance<Job>();
        var source = new CancellationTokenSource();
        await job.StartAsync(source.Token);
    }
}