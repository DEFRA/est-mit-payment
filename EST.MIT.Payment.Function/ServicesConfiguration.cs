using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using EST.MIT.Payment.Interfaces;
using EST.MIT.Payment.Services;

namespace EST.MIT.Payment.Function.Services
{
    /// <summary>
    /// Register service-tier services.
    /// </summary>
    public static class ServicesConfiguration
    {
        /// <summary>
        /// Method to register service-tier services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddQueueAndServiceBusServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEventQueueService>(_ =>
            {
                var storageAccountCredential = configuration.GetSection("QueueConnectionString:Credential").Value;
                var queueName = configuration.GetSection("EventQueueName").Value;
                if (IsManagedIdentity(storageAccountCredential))
                {
                    var serviceBusNamespace = configuration.GetSection("QueueConnectionString:FullyQualifiedNamespace").Value;
                    Console.WriteLine($"Startup.ServiceBusClient using Managed Identity with namespace {serviceBusNamespace}");
                    return new EventQueueService(new ServiceBus(queueName, new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential())));
                }
                else
                {
                    return new EventQueueService(new ServiceBus(queueName, new ServiceBusClient(configuration.GetSection("QueueConnectionString").Value)));
                }
            });

            services.AddSingleton<IServiceBus>(_ =>
            {
                var storageAccountCredential = configuration.GetSection("QueueConnectionString:Credential").Value;
                var paymentHubQueueName = configuration.GetSection("PaymentHubQueueName").Value;
                if (IsManagedIdentity(storageAccountCredential))
                {
                    var serviceBusNamespace = configuration.GetSection("QueueConnectionString:FullyQualifiedNamespace").Value;
                    Console.WriteLine($"Startup.ServiceBusClient using Managed Identity with namespace {serviceBusNamespace}");
                    var serviceBusClient = new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential());
                    return new ServiceBus(paymentHubQueueName, serviceBusClient);
                }
                else
                {
                    var serviceBusConnString = configuration.GetSection("QueueConnectionString").Value;
                    var serviceBusClient = new ServiceBusClient(serviceBusConnString);
                    return new ServiceBus(paymentHubQueueName, serviceBusClient);
                }
            });
        }
        private static bool IsManagedIdentity(string credentialName)
        {
            return credentialName != null && credentialName.ToLower() == "managedidentity";
        }
    }
}
    