using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace NetCore.Stateless.Service
{
    internal sealed class Service : StatelessService
    {
		public Service(StatelessServiceContext context)
            : base(context)
        { }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        private int fact(int acc, int num)
        {
            switch (num)
            {
                case 0:
                case 1:
                    return acc;
                default:
                    return fact(acc * num, num - 1);
            }
        }
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            int num = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                var fresult = fact(1, num);
                long range = Int32.MaxValue;
                if (fresult <= 0 || fresult >= range)
                {
                    num = 1;
                    fresult = fact(1, num);
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"resetting value of num");
                }
                ServiceEventSource.Current.ServiceMessage(this.Context, $"fact result at: {DateTime.Now.ToLongTimeString()} is: {fresult}");
                num += 1;
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
