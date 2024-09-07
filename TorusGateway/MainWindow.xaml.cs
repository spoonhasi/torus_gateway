using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TorusGateway.Torus;
using TorusGateway.WebServer;

namespace TorusGateway
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string torusAppGuid = "0C1016A3-45BF-4DE9-B0B8-1ECBB948177D";
        public const string torusAppName = "TorusGateway";

        private WebServer.WebServer? _webServer;
        public MainWindow()
        {
            TorusInit(torusAppName, torusAppGuid);
            InitializeComponent();
            ButtonWebServerOpen.IsEnabled = false;
            ButtonWebServerClose.IsEnabled = false;
        }

        public static void TorusInit(string appName, string appGuid)
        {
            Torus.Torus.Initialize(appName, appGuid);
        }

        public static int TorusConnect()
        {
            return Torus.Torus.Instance.Connect();
        }

        public void WebServerOpen(string host, int port)
        {
            if (_webServer == null)
            {
                _webServer = new WebServer.WebServer(host, port);
                _webServer.Start();
            }
        }

        public async Task WebServerCloseAsync()
        {
            if (_webServer != null)
            {
                await _webServer.StopAsync();  // 비동기적으로 웹 서버를 종료
                _webServer = null;
                ButtonWebServerOpen.IsEnabled = true;
                ButtonWebServerClose.IsEnabled = false;
                TextBoxWebServerHost.IsReadOnly = false;
                TextBoxWebServerPort.IsReadOnly = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button == ButtonTorusConnect)
                {
                    if (TorusConnect() == 0)
                    {
                        ButtonTorusConnect.IsEnabled = false;
                        ButtonWebServerOpen.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Torus connection failed", "Error");
                    }
                }
                else if (button == ButtonWebServerOpen)
                {
                    string host = TextBoxWebServerHost.Text;
                    if (host.Trim() == "")
                    {
                        MessageBox.Show("Invalid Host", "Error");
                        return;
                    }
                    if (!int.TryParse(TextBoxWebServerPort.Text, out int port))
                    {
                        MessageBox.Show("Invalid Port", "Error");
                        return;
                    }
                    WebServerOpen(host, port);
                    ButtonWebServerOpen.IsEnabled = false;
                    ButtonWebServerClose.IsEnabled = true;
                    TextBoxWebServerHost.IsReadOnly = true;
                    TextBoxWebServerPort.IsReadOnly = true;
                }
                else if (button == ButtonWebServerClose)
                {
                    _ = WebServerCloseAsync();
                }
            }
        }
    }
}