namespace TextCheckIn.Functions.Models.Requests
{
    public class CheckInRequest
    {
        public required string PhoneNumber { get; set; }
        public required string StoreId { get; set; }
        public required string OTP { get; set; }
    }
}
