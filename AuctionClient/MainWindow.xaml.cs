using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TurboAuctionWPF
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Car> AuctionCars { get; set; } = new ObservableCollection<Car>();
        private int currentCarIndex = 0;
        private string userName = "Areeba";

        private TcpClient? _client;
        private StreamWriter? _writer;
        private StreamReader? _reader;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadCarsFromLocal();
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync("127.0.0.1", 5000);
                _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
                _reader = new StreamReader(_client.GetStream());

                ConnectionStatusDot.Fill = Brushes.LimeGreen;
                ConnectionStatusText.Text = "Connected";
                NotificationList.Items.Insert(0, "🌐 Connected to Server!");

                _ = Task.Run(() => ReceiveMessages());
            }
            catch
            {
                NotificationList.Items.Insert(0, "⚠️ Offline Mode: No Server Found.");
            }
        }

        private async Task ReceiveMessages()
        {
            while (_client != null && _client.Connected)
            {
                try
                {
                    string? msg = await _reader!.ReadLineAsync();
                    if (msg != null)
                    {
                        Dispatcher.Invoke(() => {
                            if (msg.StartsWith("TIME:")) UpdateTimerUI(msg.Split(':')[1]);
                            else if (msg.StartsWith("BID_UPDATE:")) HandleIncomingBid(msg.Substring(11));
                        });
                    }
                }
                catch { break; }
            }
        }

        private void LoadCarsFromLocal()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cars.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    // Nayi details ke liye options add kiye hain
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var list = JsonSerializer.Deserialize<List<Car>>(json, options);

                    if (list != null)
                    {
                        AuctionCars.Clear();
                        foreach (var car in list) AuctionCars.Add(car);
                        ShowCar();
                    }
                }
                else
                {
                    MessageBox.Show("Error: cars.json file nahi mili bin folder mein!");
                }
            }
            catch (Exception ex) { MessageBox.Show("JSON Error: " + ex.Message); }
        }

        private void ShowCar()
        {
            if (AuctionCars.Count == 0) return;
            var car = AuctionCars[currentCarIndex];

            // Areeba wala design update
            CarTitleText.Text = $"{car.Brand} {car.Model} ({car.Year})";
            CurrentPriceText.Text = $"Current Bid: ${car.StartingPrice}";

            // Nayi details jo file mein hain
            CategoryText.Text = $"Engine: {car.Engine} | HP: {car.Horsepower}";
            NotificationList.Items.Insert(0, $"ℹ️ Speed: {car.TopSpeed} | 0-100: {car.Acceleration}");

            try
            {
                string imgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, car.ImagePath);
                if (File.Exists(imgPath))
                    CarImage.Source = new BitmapImage(new Uri(imgPath));
            }
            catch { }
        }

        private async void PlaceBid_Click(object sender, RoutedEventArgs e)
        {
            if (AuctionCars.Count == 0) return;
            var currentCar = AuctionCars[currentCarIndex];
            currentCar.StartingPrice += 1000;
            currentCar.LastBidder = UsernameBox.Text;

            if (_writer != null)
            {
                string bidJson = "BID:" + JsonSerializer.Serialize(currentCar);
                await _writer.WriteLineAsync(bidJson);
            }

            BidHistoryList.Items.Insert(0, $"🔥 {userName} bid ${currentCar.StartingPrice} on {currentCar.Brand}");
            ShowCar();
        }

        // --- Other UI Helpers ---
        private void UpdateTimerUI(string seconds)
        {
            TimerText.Text = $"Time Left: 00:{seconds.PadLeft(2, '0')}";
            if (int.TryParse(seconds, out int s)) TimerBar.Value = s;
        }

        private void HandleIncomingBid(string json)
        {
            var updatedCar = JsonSerializer.Deserialize<Car>(json);
            if (updatedCar != null)
            {
                NotificationList.Items.Insert(0, $"📢 {updatedCar.LastBidder} bid ${updatedCar.StartingPrice}!");
                ShowCar();
            }
        }

        private void NextCar_Click(object sender, RoutedEventArgs e) { if (currentCarIndex < AuctionCars.Count - 1) { currentCarIndex++; ShowCar(); } }
        private void PreviousCar_Click(object sender, RoutedEventArgs e) { if (currentCarIndex > 0) { currentCarIndex--; ShowCar(); } }
        private void Login_Click(object sender, RoutedEventArgs e) { userName = UsernameBox.Text; UserStatusText.Text = "User: " + userName; }
    }

    //[cite: 1] Updated Car Class to match your new JSON file
    public class Car
    {
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
        public int Year { get; set; }
        public double StartingPrice { get; set; }
        public string Engine { get; set; } = "";
        public int Horsepower { get; set; }
        public string TopSpeed { get; set; } = "";
        public string Acceleration { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string Description { get; set; } = "";
        public string LastBidder { get; set; } = "No one";
        public string Category { get; set; } = "Luxury"; // Default
    }
}