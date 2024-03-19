using Azure.Messaging.ServiceBus;
using EST.MIT.Payment.Interfaces;
using System.Text;

namespace EST.MIT.Payment.Services
{
    public class ServiceBus : IServiceBus
    {
        private ServiceBusSender _sender = default!;

        private readonly string _queueName;

        private readonly ServiceBusClient _client;

        public ServiceBus(string queueName, ServiceBusClient client)
        {
            _queueName = queueName;

            _client = client;
        }

        public async Task SendServiceBus(string message)
        {
            try
            {
                _sender = _client.CreateSender(_queueName);

                var sbMessage = new ServiceBusMessage(message);

                await _sender.SendMessageAsync(sbMessage);
            }

            catch (ServiceBusException ex) when
            (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
            }

            catch (ServiceBusException ex) when
            (ex.Reason == ServiceBusFailureReason.ServiceTimeout)
            {
            }

            catch (ServiceBusException ex) when
            (ex.Reason == ServiceBusFailureReason.MessageSizeExceeded)
            {
            }
        }
    }
}
