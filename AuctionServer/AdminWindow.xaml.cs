using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TurboAuctionWPF
{
    public partial class AdminWindow : Window
    {
        private TcpListener? _listener;
        private List<TcpClient> _clients = new List<TcpClient>();
        private string _selectedImagePath = "";

        // Tracking variables for the auction
        private string _lastBidder = "No one";
        private double _highestBid = 0;
        private System.Windows.Threading.DispatcherTimer _auctionTimer = new();
        private int _timeLeft = 30;

        public AdminWindow()
        {
            InitializeComponent();
            _ = Task.Run(async () => await StartServer());
        }

        private async Task StartServer()
        {
            try
            {
                int port = 8080;
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();

                string localIp = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";

                Dispatcher.Invoke(() => {
                    AdminLog.Items.Insert(0, "🚀 MASTER NODE ONLINE");
                    AdminLog.Items.Insert(0, $"📍 IP: {localIp} | PORT: {port}");
                });

                while (true)
                {
                    var client = await _listener!.AcceptTcpClientAsync();
                    _ = Task.Run(() => RequestAccess(client));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => AdminLog.Items.Insert(0, $"❌ SERVER ERROR: {ex.Message}"));
            }
        }

        private async Task RequestAccess(TcpClient client)
        {
            var reader = new StreamReader(client.GetStream());
            var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            string? authMsg = await reader.ReadLineAsync();
            if (authMsg == null) return;

            string username = authMsg.Replace("AUTH:", "");
            bool isAllowed = false;

            await Dispatcher.InvokeAsync(() => {
                var result = MessageBox.Show($"DASHBOARD ACCESS REQUEST:\nUser: {username}\n\nGrant access?",
                    "SECURITY GATEKEEPER", MessageBoxButton.YesNo, MessageBoxImage.Question);
                isAllowed = (result == MessageBoxResult.Yes);
            });

            if (isAllowed)
            {
                writer.WriteLine("ACCESS:GRANTED");
                lock (_clients) { _clients.Add(client); }
                Dispatcher.Invoke(() => {
                    UserCountText.Text = $"NODES CONNECTED: {_clients.Count}";
                    AdminLog.Items.Insert(0, $"✅ ADMITTED: {username}");
                });
                _ = Task.Run(() => HandleClientCommunication(client));
            }
            else
            {
                writer.WriteLine("ACCESS:DENIED");
                client.Close();
            }
        }

        // Logic for handling bids and tie-breaking
        private async Task HandleClientCommunication(TcpClient client)
        {
            var reader = new StreamReader(client.GetStream());
            while (client.Connected)
            {
                try
                {
                    string? msg = await reader.ReadLineAsync();
                    if (msg != null && msg.StartsWith("BID:"))
                    {
                        var parts = msg.Split(':');
                        string bidder = parts[1];
                        double amount = double.Parse(parts[2]);

                        // 1. TIE-BREAK & VALIDATION: Only accept if the NEW bid is HIGHER
                        if (amount > _highestBid)
                        {
                            _highestBid = amount;
                            _lastBidder = bidder;
                            _timeLeft = 30; // Reset timer

                            Dispatcher.Invoke(() => AdminLog.Items.Insert(0, $"🔨 {bidder.ToUpper()} BID ${_highestBid:N0}!"));

                            // 2. CRITICAL: Tell EVERYONE the price has changed
                            SendToAll($"NEW_HIGH_BID:{_highestBid}");
                        }
                    }
                }
                catch { break; }
            }
        }
        // Admin Button Actions
        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            _timeLeft = 30;
            SendToAll("TIME:30");
            AdminLog.Items.Insert(0, "⏱️ Global Clock Reset to 30s");
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            AdminLog.Items.Clear();
            AdminLog.Items.Insert(0, "🧹 Logs Cleared");
        }

        private void StartAuction_Click(object sender, RoutedEventArgs e)
        {
            _timeLeft = 30;
            _lastBidder = "No one";
            _highestBid = 0;

            _auctionTimer.Interval = TimeSpan.FromSeconds(1);
            _auctionTimer.Tick += (s, ev) => {
                _timeLeft--;
                SendToAll($"TIME:{_timeLeft}");
                if (_timeLeft <= 0)
                {
                    _auctionTimer.Stop();
                    // BROADCAST WINNER
                    SendToAll($"WINNER:{_lastBidder}:{_highestBid}");
                    AdminLog.Items.Insert(0, $"🏆 AUCTION CLOSED. Winner: {_lastBidder}");
                }
            };
            _auctionTimer.Start();
            SendToAll("START_AUCTION:GO");
        }

        private void SendToAll(string msg)
        {
            lock (_clients)
            {
                foreach (var c in _clients)
                {
                    try
                    {
                        var sw = new StreamWriter(c.GetStream()) { AutoFlush = true };
                        sw.WriteLine(msg);
                    }
                    catch { }
                }
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Graphics|*.jpg;*.jpeg;*.png" };
            if (op.ShowDialog() == true)
            {
                _selectedImagePath = op.FileName;
                AdminPreviewImage.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private void DeployAsset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedImagePath)) return;

                byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // Map UI textboxes to the Car class properties
                Car car = new Car
                {
                    Brand = BrandIn.Text,
                    Model = ModelIn.Text,
                    StartingPrice = double.Parse(PriceIn.Text),
                    Engine = EngineIn.Text,
                    Horsepower = int.Parse(HPIn.Text),
                    TopSpeed = "350 km/h", // Add a textbox for this if needed
                    Acceleration = AccelIn.Text,
                    Transmission = TransIn.Text,
                    FuelType = FuelIn.Text,
                    Description = DescIn.Text,
                    ImagePath = base64Image
                };

                // Send the data
                string payload = "PREPARE_BID:" + JsonSerializer.Serialize(car);
                SendToAll(payload);

                AdminLog.Items.Insert(0, $"✅ Asset Staged: {car.Brand} {car.Model}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Please check if all fields (Price, HP) are numbers.");
            }
        }

        
        private void Shutdown_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
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