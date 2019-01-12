using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            StatusText.Content = theApp.StartUp();
            bottomText.Content = RingPrefs.appVersionString + "  " + bottomText.Content;
        }

        private void ArtCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            theApp.LoadCanvasBackgroundImage();
            canvas.Background = theApp.GetBackgroundImageBrush();
            theApp.InitializeAnimation(canvas);
        }

        private void ArtCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas parentCanvas = (Canvas)sender;
            Point startPoint = (e.GetPosition(parentCanvas));
            theApp.DoCanvasClick(parentCanvas, startPoint);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KeyListener.StopKeyListener();
            theApp.HaltAnimation();
            theApp.StopLGS();
        }

        private void SettingsUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            theApp.LoadPreferences();
        }

    }
}
