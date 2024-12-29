using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CryptoTrader.Pilot
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer = new();
        private readonly Random _random = new();
        private const double DEFAULT_BALANCE = 10000.0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeTradePanel();
            InitializePriceUpdates();
            UpdateCurrentTimeUTC();
            StartDemoDataUpdates();
        }

        private void InitializeTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick!;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateCurrentTimeUTC();
            UpdateDemoData();
        }

        private void UpdateCurrentTimeUTC()
        {
            CurrentTimeUTC.Text = $"Current Date and Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        }

        private void UpdateDemoData()
        {
            double basePrice = 42513.20;
            double variation = _random.NextDouble() * 100 - 50;
            double newPrice = basePrice + variation;
            
            CurrentPrice.Text = newPrice.ToString("N2");
            MarkPrice.Text = (newPrice + _random.NextDouble() * 2).ToString("N2");
            IndexPrice.Text = (newPrice - _random.NextDouble() * 2).ToString("N2");

            double priceChange = (variation / basePrice) * 100;
            PriceChange.Text = $"{(priceChange >= 0 ? "+" : "")}{priceChange:N2}%";
            PriceChange.Foreground = new SolidColorBrush(
                priceChange >= 0 ? Color.FromRgb(0, 192, 135) : Color.FromRgb(246, 70, 93));

            UpdateFundingCountdown();
        }

        private void InitializeTradePanel()
        {
            leverageSlider.ValueChanged += (s, e) =>
            {
                int leverage = (int)e.NewValue;
                leverageButton.Content = $"{leverage}x";
                UpdateMarginCalculation();
            };

            priceTextBox.TextChanged += (s, e) => UpdateCostCalculation();
            sizeTextBox.TextChanged += (s, e) => UpdateCostCalculation();

            percent25Button.Click += (s, e) => SetSizePercentage(0.25);
            percent50Button.Click += (s, e) => SetSizePercentage(0.50);
            percent75Button.Click += (s, e) => SetSizePercentage(0.75);
            percent100Button.Click += (s, e) => SetSizePercentage(1.00);

            buyButton.Click += (s, e) => PlaceOrder(OrderSide.Buy);
            sellButton.Click += (s, e) => PlaceOrder(OrderSide.Sell);

            crossMarginRadio.Checked += (s, e) => UpdateMarginMode(true);
            isolatedMarginRadio.Checked += (s, e) => UpdateMarginMode(false);
        }

        private void UpdateCostCalculation()
        {
            if (double.TryParse(priceTextBox.Text, out double price) && 
                double.TryParse(sizeTextBox.Text, out double size))
            {
                double cost = price * size;
                costDisplay.Text = cost.ToString("F2");
                UpdateMarginCalculation();
            }
        }

        private void SetSizePercentage(double percentage)
        {
            try
            {
                double availableBalance = DEFAULT_BALANCE;
                double currentPrice = double.Parse(priceTextBox.Text);
                int leverage = (int)leverageSlider.Value;
                
                double maxSize = (availableBalance * leverage) / currentPrice;
                double size = maxSize * percentage;
                sizeTextBox.Text = size.ToString("F8");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating size: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMarginCalculation()
        {
            if (double.TryParse(costDisplay.Text, out double cost))
            {
                int leverage = (int)leverageSlider.Value;
                double requiredMargin = cost / leverage;
                marginDisplay.Text = requiredMargin.ToString("F2");
                
                marginDisplay.Foreground = new SolidColorBrush(
                    requiredMargin > DEFAULT_BALANCE 
                        ? Color.FromRgb(246, 70, 93) 
                        : Color.FromRgb(0, 192, 135));
            }
        }

        private void UpdateMarginMode(bool isCross)
        {
            UpdateMarginCalculation();
        }

        private