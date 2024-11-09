namespace DTO.VNPay;

public class VnPayResponseDTO
{
    public bool Success { get; set; }
    public string PaymentMethod { get; set; }
    public string OrderDescription { get; set; }
    public string OrderId { get; set; }
    public string PaymentId { get; set; }
    public string TransactionId { get; set; }
    public string VnPayResponseCode { get; set; }
    public string PaymentUrl { get; set;}
}

public class VnPayRequestDTO
{
    public decimal Amount { get; set; }
}

