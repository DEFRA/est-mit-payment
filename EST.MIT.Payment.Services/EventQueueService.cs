﻿using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using EST.MIT.Payment.Interfaces;
using EST.MIT.Payment.Models;
using System.Text;
using System.Text.Json;

namespace EST.MIT.Payment.Services
{
    public class EventQueueService : IEventQueueService
    {
        private readonly QueueClient _queueClient;

        public EventQueueService(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public async Task CreateMessage(string status, string action, string message, string data)
        {
            var eventRequest = new Event()
            {
                Name = "Payments",
                Properties = new EventProperties()
                {
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
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventRequest));
                await _queueClient.SendMessageAsync(Convert.ToBase64String(bytes));

            }
            catch (RequestFailedException ex)
                    when (ex.ErrorCode == QueueErrorCode.QueueAlreadyExists)
            {
                // Ignore any errors if the queue already exists
            }
        }
    }
}

