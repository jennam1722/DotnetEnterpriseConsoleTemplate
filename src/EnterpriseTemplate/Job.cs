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
    public class Job : IHostedService
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


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation($"Hello {context.Value.Name}!");
                logger.LogCritical($"Hello {context.Value.Name}!");
            }
            finally
            {
                lifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            lifetime.StopApplication();
            return Task.CompletedTask;
        }
    }
}
