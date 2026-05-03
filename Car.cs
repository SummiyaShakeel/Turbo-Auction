namespace TurboAuctionWPF
{
    public class Car
    {
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
        public double StartingPrice { get; set; }
        public string Engine { get; set; } = "";
        public int Horsepower { get; set; }
        public string TopSpeed { get; set; } = "";
        public string Acceleration { get; set; } = "";
        public string Transmission { get; set; } = "";
        public string FuelType { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImagePath { get; set; } = ""; // This will hold the Base64 string
        public string LastBidder { get; set; } = "No one";
        public bool IsReady { get; set; } = false;
    }
}