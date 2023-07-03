using System.Collections.Generic;
using EST.MIT.Payment.Models;
using Newtonsoft.Json;

namespace EST.MIT.Payment.Models;
public class PaymentRequest
{
    [JsonProperty("paymentRequestId")]
    public string PaymentRequestId { get; init; } = default!;
    [JsonProperty("frn")]
    public int FRN { get; init; }
    [JsonProperty("sourceSystem")]
    public string SourceSystem { get; init; } = default!;
    [JsonProperty("marketingYear")]
    public int MarketingYear { get; init; }
    [JsonProperty("deliveryBody")]
    public string DeliveryBody { get; init; } = default!;
    [JsonProperty("paymentRequestNumber")]
    public int PaymentRequestNumber { get; init; }
    [JsonProperty("agreementNumber")]
    public string AgreementNumber { get; init; } = default!;
    [JsonProperty("contractNumber")]
    public string ContractNumber { get; init; } = default!;
    [JsonProperty("value")]
    public decimal Value { get; init; }
    [JsonProperty("dueDate")]
    public string DueDate { get; init; } = default!;
    [JsonProperty("invoiceLines")]
    public List<InvoiceLine> InvoiceLines { get; init; } = default!;
    [JsonProperty("appendixReferences")]
    public AppendixReferences AppendixReferences { get; init; } = default!;
}