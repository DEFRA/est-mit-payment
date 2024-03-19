using Azure.Messaging.ServiceBus;
using EST.MIT.Payment.Interfaces;
using EST.MIT.Payment.Models;
using System.Text.Json;

namespace EST.MIT.Payment.Services
{
    public class EventQueueService : IEventQueueService
    {
        private readonly IServiceBus _serviceBus;

        public EventQueueService(IServiceBus serviceBus)
        {
            _serviceBus = serviceBus;
        }

        public async Task CreateMessage(string id, string status, string action, string message, string data)
        {
            var eventRequest = new Event()
            {
                Name = "Payments",
                Properties = new EventProperties()
                {
                    Id = id,
                    Status = status,
                    Checkpoint = "Payment",
                    Action = new EventAction()
                    {
                        Type = action,
                        Message = message,
                        Timestamp = DateTime.UtcNow,
                        Data = data
                    }
                }
            };

            try
            {
                await _serviceBus.SendServiceBus(JsonSerializer.Serialize(eventRequest).EncodeMessage());

            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                // Ignore any errors if the queue already exists
            }
        }
    }
}

