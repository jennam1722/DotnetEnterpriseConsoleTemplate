using EnterpriseTemplate.Models;
using Microsoft.Extensions.Hosting;
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

        public Job(IHostApplicationLifetime lifetime,
            IOptions<EnterpriseTemplateContext> context)
        {
            this.lifetime = lifetime;
            this.context = context;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Hello {context.Value.Name}!");
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
