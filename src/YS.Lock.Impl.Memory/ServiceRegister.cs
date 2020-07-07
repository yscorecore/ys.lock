using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife;
namespace YS.Lock.Impl.Memory
{
    public class ServiceRegister : IServiceRegister
    {

        public void RegisteServices(IServiceCollection services, IRegisteContext context)
        {
            services.AddMemoryCache();
        }
    }
}
