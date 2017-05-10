using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace K8055Test
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private bool _digitalOutputTest;
        private bool _isConnected;

        public MainWindow()
        {
            InitializeComponent();

            _timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _timer.Tick += TimerTick;
        }

        /// <summary>
        /// Handles click events of UI elements.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void K8055ButtonClick(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "K8055ConnectButton":
                {
                    bool sk5 = K8055SK5Checkbox.IsChecked ?? false;
                    bool sk6 = K8055SK6Checkbox.IsChecked ?? false;

                    //Resolves correct deviceaddress from jumper placing.
                    int deviceAdress = (Convert.ToInt32(!sk6) << 1 | Convert.ToInt32(!sk5));

                    //Falls die Verbindug fehlgeschlagen hat, soll aus der Methode gebrochen werden.
                    if (K8055.OpenDevice(deviceAdress) != deviceAdress) return;

                    _isConnected = true;
                    K8055ConnectLabel.Content = $"Connected to {deviceAdress}";
                    _timer.Start();
                    break;
                }

                case "K8055DisconnectButton":
                {
                    if (_isConnected)
                    {
                        K8055.CloseDevice();
                        _isConnected = false;
                        K8055ConnectLabel.Content = "Disconnected";
                        _timer.Stop();
                    }
                    break;
                }

                case "K8055SetAllDigitalButton":
                {
                    foreach (CheckBox checkBox in K8055DigitalOutputCanvas.Children.OfType<CheckBox>())
                    {
                        checkBox.IsChecked = true;
                    }
                    break;
                }

                case "K8055ClearAllDigitalButton":
                {
                    foreach (CheckBox checkBox in K8055DigitalOutputCanvas.Children.OfType<CheckBox>())
                    {
                        checkBox.IsChecked = false;
                    }
                    break;
                }

                case "K8055SetAllAnalogButton":
                {
                    K8055.SetAllAnalog();
                    K8055AnalogOutputSlider1.Value = 255;
                    K8055AnalogOutputSlider2.Value = 255;
                    break;
                }

                case "K8055ClearAllAnalogButton":
                {
                    K8055.ClearAllAnalog();
                    K8055AnalogOutputSlider1.Value = 0;
                    K8055AnalogOutputSlider2.Value = 0;
                    break;
                }

                case "K8055OutputTestButton":
                {
                    _digitalOutputTest = !_digitalOutputTest;
                    if (_digitalOutputTest)
                    {
                        Thread thread = new Thread(K8055DigitalOutputTest);
                        thread.Start();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Updates UI elements with the values of the K8055 periodically.
        /// </summary>
        private void TimerTick(object sender, EventArgs e)
        {
            if(!_isConnected) { return; }

            foreach (CheckBox checkBox in K8055DigitalInputCanvas.Children.OfType<CheckBox>())
            {
                checkBox.IsChecked = K8055.ReadDigitalChannel(Convert.ToInt32(checkBox.Content));
            }

            foreach (ProgressBar progressBar in K8055AnalogInputCanvas.Children.OfType<ProgressBar>())
            {
                progressBar.Value = K8055.ReadAnalogChannel(int.Parse(progressBar.Name[progressBar.Name.Length - 1].ToString()));
            }

            K8055Counter1TextBox.Text = K8055.ReadCounter(1).ToString();
            K8055Counter2TextBox.Text = K8055.ReadCounter(2).ToString();
        }

        /// <summary>
        /// Updates the analog output of the K8055 with the new output slider values.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void K8055AnalogOutputSliderValueChanged(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;
            K8055.OutputAnalogChannel(int.Parse(slider.Name[slider.Name.Length - 1].ToString()), Convert.ToInt32(slider.Value));
        }

        /// <summary>
        /// Once a digital output checkbox state changes to "checked", the K8055 digital output is set. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void K8055OutputCheckboxChecked(object sender, RoutedEventArgs e)
        {
            K8055.SetDigitalChannel(Convert.ToInt32(((CheckBox)sender).Content));
        }

        /// <summary>
        /// Once a digital output checkbox state changes to "unchecked", the K8055 digital output is cleared. 
        /// </summary>
        private void K8055OutputCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            K8055.ClearDigitalChannel(Convert.ToInt32(((CheckBox)sender).Content));
        }

        /// <summary>
        /// The digital outputs are sequentially switched on and off until the digital output test checkbox is unchecked.
        /// </summary>
        private void K8055DigitalOutputTest()
        {
            while (_digitalOutputTest)
            {
                for (int i = 1; i < 9; i++)
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        K8055ButtonClick(new Button { Name = "K8055ClearAllDigitalButton" }, null);
                        ((CheckBox)K8055DigitalOutputCanvas.Children[i - 1]).IsChecked = true;
                    }));
                    Thread.Sleep(100);
                }

                K8055.ClearDigitalChannel(8);
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    K8055DigitalOutputCheckbox8.IsChecked = false;
                }));
            }
        }

        /// <summary>
        /// Once the MainWindow closes _digitalOutputTest is set to "false" in order to terminate the "output test" thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            _digitalOutputTest = false;
            Application.Current.Shutdown();
        }

        /// <summary>
        /// The selected counter is reset.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void K8055CounterResetButtonClick(object sender, RoutedEventArgs e)
        {
            K8055.ResetCounter(int.Parse(((Button)sender).Name[12].ToString()));
        }

        /// <summary>
        /// Sets the debounce time of the selected counter to the entered value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void K8055SetDebounceButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int counter = int.Parse(((Button)sender).Name[16].ToString());
                int milliseconds = counter == 1 ? int.Parse(K8055SetDebounce1TextBox.Text)
                                                : int.Parse(K8055SetDebounce2TextBox.Text);

                K8055.SetCounterDebounceTime(counter, milliseconds);
            }
            catch
            {
                //ignored
            }
        }
    }
}
