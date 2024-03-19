using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using EST.MIT.Payment.Function.Functions;
using EST.MIT.Payment.Models;
using EST.MIT.Payment.Interfaces;
using Azure.Messaging.ServiceBus;

namespace EST.MIT.Payment.Function.Tests;
public class CreatePaymentTests
{
    private readonly CreatePayment _createPayment;
    private readonly Mock<ILogger<CreatePayment>> _mockLogger;
    private readonly Mock<IServiceBus> _mockServiceBus;
    private readonly Mock<ISchemeValidator> _mockSchemeValidator;
    private readonly Mock<IEventQueueService> _mockEventQueueService;

    public CreatePaymentTests()
    {
        _mockEventQueueService = new Mock<IEventQueueService>();
        _mockServiceBus = new Mock<IServiceBus>();
        _mockSchemeValidator = new Mock<ISchemeValidator>();
        _mockLogger = new Mock<ILogger<CreatePayment>>();
        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<object>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception, string>>()));
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(() => _mockLogger.Object);
        _createPayment = new CreatePayment(_mockEventQueueService.Object, _mockServiceBus.Object, _mockSchemeValidator.Object, mockLoggerFactory.Object);
    }

    [Fact]
    public void Given_Function_Receives_Valid_PaymentRequest_Message()
    {
        var paymentRequest = new InvoiceScheme()
        {
            PaymentRequestsBatches = new List<PaymentRequestsBatch>()
                {
                     new PaymentRequestsBatch()
                     {
                        Id = "1",
                        AccountType = "AD",
                        Created = new DateTime(2022,11,05),
                        CreatedBy = "henry",
                        Organisation = "FGH",
                        PaymentType = "AP",
                        PaymentRequests = new List<PaymentRequest>()
                        {
                            new PaymentRequest()
                            {
                                AgreementNumber = "12345",
                                DueDate = "string",
                                FRN = 56789043,
                                InvoiceLines = new List<InvoiceLine>()
                                {
                                    new InvoiceLine()
                                    {
                                        DeliveryBody = "DeliveryBody",
                                        MainAccount = "AccountA",
                                        MarketingYear = 2022,
                                        Description = "This is a description",
                                        FundCode = "2ADC",
                                        SchemeCode = "D4ERT",
                                        Value = 3
                                    }
                                },
                                MarketingYear = 2023,
                                PaymentRequestId = "2",
                                PaymentRequestNumber = 34567,
                                SourceSystem = "sourceSystem",
                                Value = 2,
                                Currency = "GBP",
                                Description = "Description",
                                InvoiceCorrectionReference = "ERQ567",
                                OriginalInvoiceNumber = "23ER56",
                                OriginalSettlementDate = DateTime.Now,
                                RecoveryDate = DateTime.Now,
                                SBI = 4567,
                                Vendor = "A"

                            }
                        },
                        Reference = "123",
                        SchemeType = "AD",
                        Status = "A",
                        Updated = DateTime.Now,
                        UpdatedBy = "henry"
                     }
                },
            SchemeType = "AD"
        };

        var messageBody = BinaryData.FromString(JsonConvert.SerializeObject(paymentRequest));
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

        var result = _createPayment?.Run(message);

        _mockLogger.Verify(
    x => x.Log(
        It.IsAny<LogLevel>(),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString() == $"C# Queue trigger function processed: {message}"),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Executing Service Bus For Strategic Payments...schemeExists=False"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));


        Assert.True(result?.IsCompletedSuccessfully);
    }

    [Fact]
    public void Given_Function_Receives_InValid_PaymentRequest_Message()
    {
        var InvalidPaymentRequest = new InvoiceScheme()
        {
            PaymentRequestsBatches = new List<PaymentRequestsBatch>()
            {
                new PaymentRequestsBatch()
                {
                    Id = "1",
                    AccountType = "AD",
                    Created = DateTime.Now,
                    CreatedBy = "henry",
                    Organisation = "FGH",
                    PaymentRequests = new List<PaymentRequest>()
                    {
                        new PaymentRequest()
                        {
                            AgreementNumber = "12345",
                            DueDate = "string",
                            FRN = 56789043,
                            InvoiceLines = new List<InvoiceLine>()
                            {
                                new InvoiceLine()
                                {
                                    DeliveryBody = "DeliveryBody",
                                    MainAccount = "AccountA",
                                    MarketingYear = 2022,
                                    Description = "This is a description",
                                    FundCode = "2ADC",
                                    SchemeCode = "D4ERT",
                                    Value = 3
                                }
                            },
                            MarketingYear = 2023,
                            PaymentRequestId = "2",
                            PaymentRequestNumber = 34567,
                            SourceSystem = "sourceSystem",
                            Value = 2
                        }
                    }

                }
            }
        };

        var messageBody = BinaryData.FromString(JsonConvert.SerializeObject(InvalidPaymentRequest));
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

        _createPayment?.Run(message);

        _mockLogger.Verify(
            x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == $"C# Queue trigger function processed: {message}"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _mockLogger.Verify(
            x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Payment request is not valid"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public void Given_Function_Receives_Null_PaymentRequest_Message()
    {
        var messageBody = BinaryData.FromString(JsonConvert.SerializeObject(null));
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

        _createPayment?.Run(message);

        _mockLogger.Verify(
            x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Payment request is null"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // TODO - check result
    }

    [Fact]
    public void Given_Function_Receives_Invalid_PaymentRequest_Message()
    {
        var messageBody = BinaryData.FromString(JsonConvert.SerializeObject("{'invalid' : 'json'}"));
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

        _createPayment?.Run(message);

        _mockLogger.Verify(
            x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Payment request is not valid"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // TODO - check result
    }
}
