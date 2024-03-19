using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using EST.MIT.Payment.Function.Util;
using EST.MIT.Payment.Function.Validation;
using EST.MIT.Payment.Interfaces;
using EST.MIT.Payment.Models;
using Newtonsoft.Json;
using EST.MIT.Payment.Services;
using Newtonsoft.Json.Linq;

namespace EST.MIT.Payment.Function.Functions
{
	public class CreatePayment
	{
        private readonly IEventQueueService _eventQueueService;
        private readonly IServiceBus _serviceBus;
        private readonly ISchemeValidator _schemeValidator;
        private readonly ILogger _logger;

        public CreatePayment(IEventQueueService eventQueueService, IServiceBus serviceBus, ISchemeValidator schemeValidator, ILoggerFactory loggerFactory)
        {
            _eventQueueService = eventQueueService;
            _schemeValidator = schemeValidator;
            _serviceBus = serviceBus;
            _logger = loggerFactory.CreateLogger("CreatePayment");
        }

        [Function("CreatePayment")]
        public async Task Run([ServiceBusTrigger("%PaymentQueueName%", Connection = "QueueConnectionString")] ServiceBusReceivedMessage message)
        {
            string paymentRequestMsg = message.Body.ToString().DecodeMessage();
            _logger.LogInformation($"C# Queue trigger function processed: {paymentRequestMsg}");

            InvoiceScheme invoiceScheme;

            if (paymentRequestMsg == null)
            {
                _logger.LogError("Payment request is null");
                await _eventQueueService.CreateMessage("", "failed", "paymentrequest", "Payment request is null", paymentRequestMsg);
                return;
            }

            var isValid = ValidateRequest.IsValid(paymentRequestMsg);

            if (!isValid)
            {
                _logger.LogError("Payment request is not valid");
                var errors = ValidateRequest.GetValidationErrors(paymentRequestMsg);
                foreach (var error in errors)
                {
                    _logger.LogError($"JSON validation error: {error}");
                }
                return;
            }

            string id = JObject.Parse(paymentRequestMsg)["paymentRequestsBatches"][0]["id"].ToString();

            try
            {
                
                invoiceScheme = JsonConvert.DeserializeObject<InvoiceScheme>(paymentRequestMsg);

                if (invoiceScheme.SchemeType == null)
                {
                    await _eventQueueService.CreateMessage(id, "failed", "paymentrequest", "payment request is Transformation Layer", paymentRequestMsg);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing payment request");
                return;
            }

            _logger.LogInformation($"Payment request size: {MessageSize.GetMessageSize(invoiceScheme)}kb");

            try
            {
                await _eventQueueService.CreateMessage(id, "sending", "paymentrequest", "payment request about to send", paymentRequestMsg);

                var schemeType = invoiceScheme.SchemeType;
                var schemeExists = _schemeValidator.ValueExists(schemeType);

                _logger.LogInformation($"Executing Service Bus For Strategic Payments...schemeExists={schemeExists}");

                await _serviceBus.SendServiceBus(JsonConvert.SerializeObject(invoiceScheme).EncodeMessage());

                await _eventQueueService.CreateMessage(id, "sent", "paymentrequest", "payment request sent", paymentRequestMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment request to service bus");
                await _eventQueueService.CreateMessage(id, "failed", "paymentrequest", "payment request failed", paymentRequestMsg);
            }
        }
    }
}
