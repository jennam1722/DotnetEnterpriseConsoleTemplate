using Microsoft.Extensions.Hosting;
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

        public Job(IHostApplicationLifetime lifetime)
        {
            this.lifetime = lifetime;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Hello World!");
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
