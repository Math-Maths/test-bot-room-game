using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Microsoft.Extensions.DependencyInjection;

namespace TestRoomCloud
{
    public class ModuleConfig : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
            config.Dependencies.AddSingleton(GameApiClient.Create());
            config.Dependencies.AddSingleton<PlayerDataService>();
            config.Dependencies.AddSingleton<PlayerEconomyService>();
        }
    }
}
