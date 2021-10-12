using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WaveRGB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // OBJECT ATTRIBUTES
        WaveRGBActions theApp;
       
        // OBJECT METHODS
        public MainWindow()
        {
            InitializeComponent();
            theApp = new WaveRGBActions();
            StatusText.Content = theApp.StartUp(this);// artCanvas);
            bottomText.Content = RingPrefs.appVersionString + "  " + bottomText.Content;
         }

        private void ArtCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            theApp.StartupBackgroundImage();
            canvas.Background = theApp.GetBackgroundBrush();
            theApp.InitializeAnimation(canvas);
        }

        private void ArtCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas parentCanvas = (Canvas)sender;
            Point startPoint = (e.GetPosition(parentCanvas));
            theApp.DoCanvasClick(parentCanvas, startPoint);
        }

        // Closing is raised when Close is called, if a window's Close button is clicked, or if the user presses ALT+F4.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            ShowInTaskbar = false; // makes this not appear when alt-tabbing
            e.Cancel = true; // keeps this window open
        }

        public void TerminateApp() { 
            KeyListener.StopKeyListener();
            theApp.HaltAnimation();
            theApp.StopLGS();
            theApp.WindowClosing(); // closes settings window and other clean up
            Close();
            Application.Current.Shutdown();

        }

        private void SettingsUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            theApp.UpdatePreferences();
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            Show();
            ShowInTaskbar = true;
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            TerminateApp();
        }
    }
}
