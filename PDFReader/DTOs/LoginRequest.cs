namespace PDFReader.DTOs
{
    public class LoginRequest
    {
        public required string Password { get; set; }
        public required string DeviceMac { get; set; }
    }
}
