using EnterpriseTemplate.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseTemplate
{
    public partial class Job : IHostedService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly IOptions<EnterpriseTemplateContext> context;
        private readonly ILogger<Job> logger;

        public Job(IHostApplicationLifetime lifetime,
            IOptions<EnterpriseTemplateContext> context,
            ILogger<Job> logger)

        {
            this.lifetime = lifetime;
            this.context = context;
            this.logger = logger;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                LogInformationHello(context.Value.Name);
                LogCriticalHello(context.Value.Name);
            }
            finally
            {
                lifetime.StopApplication();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            lifetime.StopApplication();
            return Task.CompletedTask;
        }

        [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Hello {name}!")]
        public partial void LogInformationHello(string name);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Critical, Message = "Hello {name}!")]
        public partial void LogCriticalHello(string name);
    }
}
