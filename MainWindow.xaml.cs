using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TurboAuctionWPF
{
    public partial class MainWindow : Window
    {
        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        public MainWindow() { InitializeComponent(); }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string ip = IPBox.Text;
            if (int.TryParse(PortBox.Text, out int port) && !string.IsNullOrWhiteSpace(username))
            {
                await Task.Run(async () => await ConnectToServer(ip, port));
            }
        }

        private async Task ConnectToServer(string ip, int port)
        {
            try
            {
                _client = new TcpClient(ip, port);
                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };

                string name = Dispatcher.Invoke(() => UsernameBox.Text);
                _writer.WriteLine($"AUTH:{name}");

                string? response = await _reader.ReadLineAsync();
                if (response == "ACCESS:GRANTED")
                {
                    Dispatcher.Invoke(() =>
                    {
                        MainViewSwitcher.SelectedIndex = 1;
                        ActiveUserLabel.Text = UsernameBox.Text.ToUpper();
                        NotificationList.Items.Insert(0, "✅ SESSION ACTIVE.");
                    });

                    // CRITICAL FIX: You must start listening for updates here!
                    _ = Task.Run(async () => await ListenForServerUpdates());
                }
                else
                {
                    MessageBox.Show("ACCESS DENIED.");
                }
            }
            catch { MessageBox.Show("Server Offline."); }
        }

        private async Task ListenForServerUpdates()
        {
            while (_client != null && _client.Connected)
            {
                try
                {
                    string? msg = await _reader!.ReadLineAsync();
                    if (msg != null) HandleIncomingMessage(msg);
                }
                catch { break; }
            }
        }

        private void HandleIncomingMessage(string msg)
        {
            if (msg.StartsWith("PREPARE_BID:"))
            {
                string json = msg.Substring(12);
                var car = JsonSerializer.Deserialize<Car>(json);
                if (car != null) Dispatcher.Invoke(() => {
                    UpdateUI(car);
                    // CRITICAL FIX: Make the buttons appear again for the new auction
                    BiddingPanel.Visibility = Visibility.Visible;
                    PriceText.Foreground = System.Windows.Media.Brushes.DarkGreen;
                });
            }
            else if (msg.StartsWith("TIME:"))
            {
                string time = msg.Split(':')[1];
                Dispatcher.Invoke(() => {
                    TimerText.Text = $"Time Left: 00:{time.PadLeft(2, '0')}";
                    TimerBar.Value = int.Parse(time);
                });
            }
            // NEW: Listen for price updates from the Admin
            else if (msg.StartsWith("NEW_HIGH_BID:"))
            {
                string newPrice = msg.Split(':')[1];
                Dispatcher.Invoke(() => {
                    // Update the big number on the screen
                    PriceText.Text = $"${double.Parse(newPrice):N0}";

                    // Visual Feedback: Flash the price green to show it increased
                    PriceText.Foreground = System.Windows.Media.Brushes.LimeGreen;

                    // Log it in the feed
                    NotificationList.Items.Insert(0, $"⬆️ NEW HIGH BID: ${double.Parse(newPrice):N0}");
                });
            }
            else if (msg.StartsWith("WINNER:"))
            {
                var parts = msg.Split(':');
                string winnerName = parts[1];
                string finalPrice = parts[2];
                string currentUser = Dispatcher.Invoke(() => UsernameBox.Text);

                Dispatcher.Invoke(() => {
                    // LOCKDOWN UI
                    BiddingPanel.Visibility = Visibility.Collapsed;

                    if (winnerName.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
                    {
                        PriceText.Text = "🏆 CONGRATULATIONS! YOU WON!";
                        PriceText.Foreground = System.Windows.Media.Brushes.Gold;
                    }
                    else
                    {
                        PriceText.Text = $"BID CLOSED - WON BY {winnerName.ToUpper()}";
                        PriceText.Foreground = System.Windows.Media.Brushes.Crimson;
                    }
                });
            }
        }

        private void Bid_Click(object sender, RoutedEventArgs e)
        {
            if (_writer != null)
                _writer.WriteLine($"BID:{UsernameBox.Text}:{BidAmountBox.Text}");
        }

        private void IncreaseBid_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(BidAmountBox.Text, out int current))
                BidAmountBox.Text = (current + 1000).ToString();
        }

        private void DecreaseBid_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(BidAmountBox.Text, out int current) && current > 1000)
                BidAmountBox.Text = (current - 1000).ToString();
        }
        private void UpdateUI(Car car)
        {
            SpecBrand.Text = $"Brand: {car.Brand}";
            SpecModel.Text = $"Model: {car.Model}";
            SpecEngine.Text = $"Engine: {car.Engine}";
            SpecHP.Text = $"Horsepower: {car.Horsepower}";
            SpecAccel.Text = $"0-100 km/h: {car.Acceleration}";
            SpecFuel.Text = $"Fuel: {car.FuelType}";
            SpecTrans.Text = $"Transmission: {car.Transmission}";
            SpecDesc.Text = car.Description;
            PriceText.Text = $"${car.StartingPrice:N0}";

            if (!string.IsNullOrEmpty(car.ImagePath))
            {
                try
                {
                    byte[] binaryData = Convert.FromBase64String(car.ImagePath);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.EndInit();
                    CarDisplay.Source = bi;
                }
                catch { /* Handle image error */ }
            }
        }
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
            public string ImagePath { get; set; } = "";
        }
    }
}