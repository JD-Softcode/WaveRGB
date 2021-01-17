using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WaveRGB
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private Object changeHandler;
         
        public SettingsWindow(Object theActionHandler)
        {
            changeHandler = theActionHandler;
            InitializeComponent();
        }

        private void activeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            int ringNum = (int)(sender as CheckBox).Name.Last() - 48;   // Last returns a char, which is toll-free to int
            (changeHandler as WaveRGBActions).ActiveCheckSettingChanged(ringNum, (sender as CheckBox).IsChecked ?? false);
        }

        private void peripheralCheckbox_click(object sender, RoutedEventArgs e)
        {
            switch ((sender as CheckBox).Name)
            {
                case "mouseLightCheckbox":
                    (changeHandler as WaveRGBActions).PeripheralCheckSettingChanged(RingPrefs.mouseDevice, (sender as CheckBox).IsChecked ?? false);
                    break;
                case "headsetLightCheckbox":
                    (changeHandler as WaveRGBActions).PeripheralCheckSettingChanged(RingPrefs.headsetDevice, (sender as CheckBox).IsChecked ?? false);
                    break;
                case "speakerLightCheckbox":
                    (changeHandler as WaveRGBActions).PeripheralCheckSettingChanged(RingPrefs.speakerDevice, (sender as CheckBox).IsChecked ?? false);
                    break;
            }
        }

        internal void SetupBackgroundsMenu(string[] keyboardBackgroundImageNames)
        {
            foreach (string menuTitle in keyboardBackgroundImageNames)
            {
                backgroundMenu.Items.Add(new ComboBoxItem { Content = menuTitle });
            }
        }

        public void SetupPresetsMenu(string[] presetTitles)
        {
            ringPresetsMenu.Items.Add(new ComboBoxItem { Content = "Custom" });
            foreach (string menuTitle in presetTitles)
            {
                ringPresetsMenu.Items.Add(new ComboBoxItem { Content = menuTitle });
            }
        }

        private void Textbox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox theTextbox = (sender as TextBox);
            //String pp = theTextbox.Parent.GetValue(NameProperty).ToString();
            Point limits = theTextbox.RenderTransformOrigin;   // borrowing this convenient Point for my purposes. What could go wrong??
            int lastKnownGoodVal;
            bool makePresetCustom = false;
            if (theTextbox.Tag != null)
            {
                lastKnownGoodVal = (int)theTextbox.Tag;
            } else
            {
                lastKnownGoodVal = (int)limits.X;
            }
            int value = 0;
            try
            {
                value = Int16.Parse(theTextbox.Text);
            } catch (FormatException)  // typing letters
            {
                value = lastKnownGoodVal;
                makePresetCustom = true;
            } catch (System.OverflowException)  // happens if auto-repeating too many digits 
            {
                value = lastKnownGoodVal;
                makePresetCustom = true;
            }
            if (value < limits.X ) {
                value = (int) limits.X;
                makePresetCustom = true;
            }
            if (value > limits.Y) {
                value = (int) limits.Y;
                makePresetCustom = true;
            }
            theTextbox.Text = "" + value;
            theTextbox.Tag = value;    // store good value for reuse

            String textboxName = theTextbox.Name;
            if (textboxName.Contains("Color") )
            {
                switch (textboxName.Last())
                {
                    case '1': colorSampleRect1.Fill = SetRectColor(redColorTextbox1, greenColorTextbox1, blueColorTextbox1); break;
                    case '2': colorSampleRect2.Fill = SetRectColor(redColorTextbox2, greenColorTextbox2, blueColorTextbox2); break;
                    case '3': colorSampleRect3.Fill = SetRectColor(redColorTextbox3, greenColorTextbox3, blueColorTextbox3); break;
                    case '4': colorSampleRect4.Fill = SetRectColor(redColorTextbox4, greenColorTextbox4, blueColorTextbox4); break;
                    case '5': colorSampleRect5.Fill = SetRectColor(redColorTextbox5, greenColorTextbox5, blueColorTextbox5); break;
                }
                //    this next line would be a slick way to update the right color Rect with no repetition, but C# disallows duplicate names. FindName appears to be global search.
                //((theTextbox.Parent as Grid).FindName("colorSampleRect") as Rectangle).Fill = new SolidColorBrush(Color.FromRgb(red, green, blue));
            }

            if (textboxName == "framerateTextbox")
            {
                (changeHandler as WaveRGBActions).TextSettingChanged(0, "framerate", value, false);
            }
            else
            {
                int ringNum = (int)textboxName.Last() - 48;   // Last returns a char, which is toll-free to int
                String setting = textboxName.Remove(textboxName.Length - 1, 1);
                if ( (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                     (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                      e.Key == Key.Delete )
                {
                    makePresetCustom = true;
                }
                (changeHandler as WaveRGBActions).TextSettingChanged(ringNum, setting, value, makePresetCustom);
            }
        }

        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();    // for tabbing in. When clicked, selection always collapses goes to point clicked.
        }

        private SolidColorBrush SetRectColor(TextBox redbox, TextBox greenbox, TextBox bluebox)
        {
            byte red = Byte.Parse(redbox.Text);
            byte green = Byte.Parse(greenbox.Text);
            byte blue = Byte.Parse(bluebox.Text);
            return new SolidColorBrush(Color.FromRgb(red, green, blue));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            (changeHandler as WaveRGBActions).SavePreferences();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (changeHandler as WaveRGBActions).reloadPreferences();
            Hide();
        }

        private void BackgroundMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int theIndex = (sender as ComboBox).SelectedIndex;
            if (changeHandler != null)      // gets here during init before this is set.
            {
                (changeHandler as WaveRGBActions).ChangeBackground(theIndex);
            }
        }

        internal void refreshColors()
        {
            colorSampleRect1.Fill = SetRectColor(redColorTextbox1, greenColorTextbox1, blueColorTextbox1);
            colorSampleRect2.Fill = SetRectColor(redColorTextbox2, greenColorTextbox2, blueColorTextbox2);
            colorSampleRect3.Fill = SetRectColor(redColorTextbox3, greenColorTextbox3, blueColorTextbox3);
            colorSampleRect4.Fill = SetRectColor(redColorTextbox4, greenColorTextbox4, blueColorTextbox4);
            colorSampleRect5.Fill = SetRectColor(redColorTextbox5, greenColorTextbox5, blueColorTextbox5);
        }

        private void RingPresetsMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int theIndex = (sender as ComboBox).SelectedIndex;
            if (changeHandler != null && theIndex > 0)      // gets here during init before this is set. Also index 0 is "Custom" not a preset
            {
                (changeHandler as WaveRGBActions).ChangeRingsPreset(theIndex);
            }

        }
    }
}
