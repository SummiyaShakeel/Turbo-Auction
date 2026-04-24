namespace TurboAuctionWPF.Models
{
    public class Car
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public decimal StartingPrice { get; set; }
        public string? ImagePath { get; set; }
    }
}