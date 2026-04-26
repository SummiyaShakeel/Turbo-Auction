using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TurboAuctionWPF.Models;

namespace TurboAuctionWPF
{
    public partial class MainWindow : Window
    {
        private int currentCarIndex = 0;
        private decimal currentPrice;
        private List<Car>? cars;
        private DispatcherTimer? _timer;
        private int _timeLeft = 30;
        private string userName = "Areeba";

        public MainWindow()
        {
            InitializeComponent();

            UserStatusText.Text = $"User: {userName}";

            LoadCars();
            ShowFirstCar();
            StartCountdown();
        }

        private void LoadCars()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "Data", "cars.json");

                if (!File.Exists(path))
                {
                    MessageBox.Show("cars.json file not found!");
                    return;
                }

                string json = File.ReadAllText(path);

                cars = JsonSerializer.Deserialize<List<Car>>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cars: " + ex.Message);
            }
        }

        private void ShowFirstCar()
        {
            if (cars != null && cars.Count > 0)
            {
                var car = cars[currentCarIndex];
                currentPrice = car.CurrentBid;

                CarTitleText.Text = $"{car.Brand} {car.Model} ({car.Year})";
                CategoryText.Text = $"Category: {car.Category}";
                CurrentPriceText.Text = $"Current Bid: ${car.CurrentBid}";

                try
                {
                    string fullImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, car.ImagePath);
                    CarImage.Source = new BitmapImage(new Uri(fullImagePath, UriKind.Absolute));

                    LiveBadge.Visibility = car.IsLive ? Visibility.Visible : Visibility.Hidden;
                }
                catch
                {
                    MessageBox.Show("Image not found: " + car.ImagePath);
                }
            }
        }

        private void StartCountdown()
        {
            _timeLeft = 30;

            TimerBar.Maximum = 30;
            TimerBar.Value = 30;

            UpdateTimerUI();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timeLeft--;

            if (_timeLeft <= 0)
            {
                _timer?.Stop();

                MessageBox.Show($"Auction Ended!\nWinning Price: ${currentPrice}");

                NextCar();

                StartCountdown();
                return;
            }

            UpdateTimerUI();
        }

        private void UpdateTimerUI()
        {
            int mm = _timeLeft / 60;
            int ss = _timeLeft % 60;

            TimerText.Text = $"Time Left: {mm:D2}:{ss:D2}";
            TimerBar.Value = _timeLeft;

            if (_timeLeft <= 10)
            {
                TimerText.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                TimerText.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                userName = UsernameBox.Text.Trim();
                UserStatusText.Text = $"User: {userName}";

                NotificationList.Items.Insert(0, $"✅ {userName} logged in");
            }
        }

        private void NextCar_Click(object sender, RoutedEventArgs e)
        {
            if (cars == null || cars.Count == 0)
                return;

            currentCarIndex++;

            if (currentCarIndex >= cars.Count)
                currentCarIndex = 0;

            ShowFirstCar();
        }

        private void PreviousCar_Click(object sender, RoutedEventArgs e)
        {
            if (cars == null || cars.Count == 0)
                return;

            currentCarIndex--;

            if (currentCarIndex < 0)
                currentCarIndex = cars.Count - 1;

            ShowFirstCar();
        }

        private void PlaceBid_Click(object sender, RoutedEventArgs e)
        {
            if (cars == null || cars.Count == 0)
                return;

            var car = cars[currentCarIndex];

            currentPrice += 1000;
            car.CurrentBid = currentPrice;

            CurrentPriceText.Text = $"Current Bid: ${currentPrice}";

            BidHistoryList.Items.Insert(0,
                $"{userName} bid ${currentPrice} on {car.Brand} {car.Model}");

            NotificationList.Items.Insert(0,
                $"🔥 New bid placed by {userName}: ${currentPrice}");

            _timeLeft = 30;
            TimerBar.Value = 30;
            UpdateTimerUI();
        }

        private void NextCar()
        {
            if (cars == null || cars.Count == 0)
                return;

            currentCarIndex++;

            if (currentCarIndex >= cars.Count)
                currentCarIndex = 0;

            ShowFirstCar();
        }
    }
}
