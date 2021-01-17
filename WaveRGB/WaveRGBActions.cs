using LedCSharp;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WaveRGB
{
    class WaveRGBActions
    {
        // Attributes
        readonly string[] KeyboardBackgroundImageNames = { "Black", "Midnight", "Sunrise", "Gray", "Dark Gray" };
        readonly string[] presetNames = { "Basic Rings", "Bolder Basic", "Firmware", "Blue Paint", "Big Color Rings", "Lava Eruption",
            "Fireworks", "Onion Rings", "Key Pulse", "Rainbow Follower", "Ghost In The Machine" };
        private int selectedBackgroundIndex;
        private int selectedPresetIndex;
        ImageBrush KeyboardBackgroundPaint;
        private System.Timers.Timer animationTimer;
        private AnimatedCircleGroup theCircles;
        private Canvas drawCanvas;
        private Canvas keysCanvas;

        private int renderWidth;    // these next five values set by LoadCanvasBackgroundImage
        private int renderHeight;   //    allowed to be variable in case UI changes in the future
        private double renderScaleHoriz;
        private double renderScaleVert;
        private int renderBytesPerPixel;

        WaveRGB.SettingsWindow settingsWindow;

        private readonly int[] keyToZoneMap = new int[] {
            81,  // Q sets zone 1; on G213 zone 0 is whole keyboard; on other devices zone 0 is a specific light
            89,  // Y sets zone 2
            219, // [ sets zone 3
            35,  // END sets zone 4
            104  // KP_8 sets zone 5
            };

        private const double mult8bitToPercent = 100.0 / 255.0;

        // Methods
        public string StartUp(Canvas artCanvas)
        {
            string LGSresult = "";
            if (LogitechGSDK.LogiLedInitWithName("Wave RGB"))
            {   //if no error...
                System.Threading.Thread.Sleep(250); //wait for LGS to wake up
                int[] version = GetLGSversion();
                if (version[0] == -1)
                {
                    LGSresult = "Successfully connected to LogiLED but version unknown.";
                }
                else
                {
                    LGSresult = string.Format($"Successfully connected to LogiLED version {version[0]}.{version[1]}.{version[2]}.");
                }
            }
            else
            {
                LGSresult = "Failed to connect to LogiLED.";
            }
            settingsWindow = new SettingsWindow(this);
            settingsWindow.SetupBackgroundsMenu(KeyboardBackgroundImageNames);
            settingsWindow.SetupPresetsMenu(presetNames);
            LoadPreferences();
            KeyListener.StartKeyListener(this);
            return LGSresult;
        }

        internal void windowClosing()
        {
            settingsWindow.Close();
        }

        internal void StopLGS()
        {
            LogitechGSDK.LogiLedShutdown();
        }

        internal int[] GetLGSversion()
        {
            int major = 0;
            int minor = 0;
            int build = 0;
            bool error = LogitechGSDK.LogiLedGetSdkVersion(ref major, ref minor, ref build);
            //in testing, this call always returned false.  Therefore ignore the result
            int[] result = { major, minor, build };
            return result;
        }

        internal void LoadCanvasBackgroundImage()
        {
            String pathname = "Images\\" + KeyboardBackgroundImageNames[selectedBackgroundIndex] + ".png";
            Uri uriSource = new Uri(pathname, UriKind.Relative);
            BitmapImage anImage = new BitmapImage(uriSource);
            KeyboardBackgroundPaint = new ImageBrush(anImage);  //sets attribute
            KeyboardBackgroundPaint.TileMode = TileMode.None;
            renderWidth = (int)anImage.Width;       // the image must be the same dimentions as artCanvas in the UI
            renderHeight = (int)anImage.Height;
            renderScaleHoriz = (double)renderWidth / KeyboardInfo.designWidth;
            renderScaleVert = (double)renderHeight / KeyboardInfo.designHeight;
            renderBytesPerPixel = anImage.Format.BitsPerPixel / 8;
        }


        public void LoadPreferences()
        {
            RegistryKey userPrefs = Registry.CurrentUser.OpenSubKey("Software\\JD-Softcode\\WaveRGB", true);
            if (userPrefs == null)
            {
                // Let the default values in the constructor persist.
                selectedBackgroundIndex = 1;
                selectedPresetIndex = 0;
            }
            RingPrefs.renderFPS = JsonSerializer.Deserialize<int>(userPrefs.GetValue(RingPrefs.renderName).ToString());
            RingPrefs.active = JsonSerializer.Deserialize<bool[]>(userPrefs.GetValue(RingPrefs.activeName).ToString());
            RingPrefs.life = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.lifeName).ToString());
            RingPrefs.delay = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.delayName).ToString());
            RingPrefs.sizeStart = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.sizeStartName).ToString());
            RingPrefs.sizeEnd = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.sizeEndName).ToString());
            RingPrefs.color = JsonSerializer.Deserialize<Color[]>(userPrefs.GetValue(RingPrefs.colorName).ToString());
            RingPrefs.thickness = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.thicknessName).ToString());
            RingPrefs.opacityStart = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.opacityStartName).ToString());
            RingPrefs.opacityEnd = JsonSerializer.Deserialize<double[]>(userPrefs.GetValue(RingPrefs.opacityEndName).ToString());
            RingPrefs.showOnDevice = JsonSerializer.Deserialize<bool[]>(userPrefs.GetValue(RingPrefs.showOnSection).ToString());
            selectedBackgroundIndex = int.Parse(userPrefs.GetValue("backgroundIndex").ToString());
            selectedPresetIndex = int.Parse(userPrefs.GetValue("presetIndex").ToString());
            SetAnimationInterval((int)RingPrefs.renderFPS);
            LoadCanvasBackgroundImage();
            if (drawCanvas != null)
            {
                drawCanvas.Background = this.GetBackgroundImageBrush();     //needed to reset in user Cancels
            }
        }

        public void UpdatePreferences()
        {   //This is called when user clicks the main window button
            settingsWindow.framerateTextbox.Text = RingPrefs.renderFPS.ToString();
            for (int i = 0; i < RingPrefs.maxRings; i++)
            {
                int j = i + 1;
                (settingsWindow.FindName("ringActiveCheckbox" + j) as CheckBox).IsChecked = RingPrefs.active[i];
                (settingsWindow.FindName("lifetimeTextbox" + j)    as TextBox).Text = RingPrefs.life[i].ToString();
                (settingsWindow.FindName("renderDelayTextbox" + j) as TextBox).Text = RingPrefs.delay[i].ToString();
                (settingsWindow.FindName("startRadiusTextbox" + j) as TextBox).Text = RingPrefs.sizeStart[i].ToString();
                (settingsWindow.FindName("endRadiusTextbox" + j)   as TextBox).Text = RingPrefs.sizeEnd[i].ToString();
                (settingsWindow.FindName("redColorTextbox" + j)    as TextBox).Text = RingPrefs.color[i].R.ToString();
                (settingsWindow.FindName("greenColorTextbox" + j)  as TextBox).Text = RingPrefs.color[i].G.ToString();
                (settingsWindow.FindName("blueColorTextbox" + j)   as TextBox).Text = RingPrefs.color[i].B.ToString();
                (settingsWindow.FindName("thicknessTextbox" + j)   as TextBox).Text = RingPrefs.thickness[i].ToString();
                (settingsWindow.FindName("startVisibilityTextbox" + j) as TextBox).Text = RingPrefs.opacityStart[i].ToString();
                (settingsWindow.FindName("endVisibilityTextbox" + j)   as TextBox).Text = RingPrefs.opacityEnd[i].ToString();
                settingsWindow.refreshColors();
            }
            settingsWindow.mouseLightCheckbox.IsChecked   = RingPrefs.showOnDevice[RingPrefs.mouseDevice];
            settingsWindow.headsetLightCheckbox.IsChecked = RingPrefs.showOnDevice[RingPrefs.headsetDevice];
            settingsWindow.speakerLightCheckbox.IsChecked = RingPrefs.showOnDevice[RingPrefs.speakerDevice];
            settingsWindow.backgroundMenu.SelectedIndex   = selectedBackgroundIndex;
            settingsWindow.ringPresetsMenu.SelectedIndex  = selectedPresetIndex;
            settingsWindow.Show();
        }

        internal void reloadPreferences()
        {
            LoadPreferences();
        }

        internal void SavePreferences()
        {
            RegistryKey userPrefs = Registry.CurrentUser.OpenSubKey("Software\\JD-Softcode\\WaveRGB", true);
            if (userPrefs == null)
            {
                RegistryKey newKey = Registry.CurrentUser.CreateSubKey("Software\\JD-Softcode", true);
                userPrefs = newKey.CreateSubKey("WaveRGB");
            }
            userPrefs.SetValue(RingPrefs.renderName,        JsonSerializer.Serialize(RingPrefs.renderFPS));
            userPrefs.SetValue(RingPrefs.activeName,        JsonSerializer.Serialize(RingPrefs.active));
            userPrefs.SetValue(RingPrefs.lifeName,          JsonSerializer.Serialize(RingPrefs.life));
            userPrefs.SetValue(RingPrefs.delayName,         JsonSerializer.Serialize(RingPrefs.delay));
            userPrefs.SetValue(RingPrefs.sizeStartName,     JsonSerializer.Serialize(RingPrefs.sizeStart));
            userPrefs.SetValue(RingPrefs.sizeEndName,       JsonSerializer.Serialize(RingPrefs.sizeEnd));
            userPrefs.SetValue(RingPrefs.colorName,         JsonSerializer.Serialize(RingPrefs.color));
            userPrefs.SetValue(RingPrefs.thicknessName,     JsonSerializer.Serialize(RingPrefs.thickness));
            userPrefs.SetValue(RingPrefs.opacityStartName,  JsonSerializer.Serialize(RingPrefs.opacityStart));
            userPrefs.SetValue(RingPrefs.opacityEndName,    JsonSerializer.Serialize(RingPrefs.opacityEnd));
            userPrefs.SetValue(RingPrefs.showOnSection,     JsonSerializer.Serialize(RingPrefs.showOnDevice));
            userPrefs.SetValue("backgroundIndex", selectedBackgroundIndex);
            userPrefs.SetValue("presetIndex", selectedPresetIndex);
        }

        public void ActiveCheckSettingChanged(int ringNum, bool data)
        {
            RingPrefs.active[ringNum-1] = data;
            settingsWindow.ringPresetsMenu.SelectedIndex = selectedPresetIndex = 0;  // "Custom" preset 
        }

        public void PeripheralCheckSettingChanged(int arrayIndex, bool data)
        {
            RingPrefs.showOnDevice[arrayIndex] = data;
        }

        public void ChangeBackground(int newIndex)
        {
            selectedBackgroundIndex = newIndex;
            LoadCanvasBackgroundImage();
            //if (mainWindowCanvas != null)       // get called at startup
            if (drawCanvas != null)       // get called at startup
            {
                //mainWindowCanvas.Background = this.GetBackgroundImageBrush();
                drawCanvas.Background = this.GetBackgroundImageBrush();
            }
        }

        public void ChangeRingsPreset(int newIndex) {
            selectedPresetIndex = newIndex;
            string searchString = presetNames[newIndex-1];     // index 0 is "Custom" on menu; 1 is first preset name
            string[] lines = File.ReadAllLines(path: "Resources\\presets.txt", encoding: Encoding.UTF8);
            for (int i=0; i<lines.Length; i++)
            {
                if (lines[i] == searchString) {
                    RingPrefs.active       = JsonSerializer.Deserialize<bool[]>(lines[i+1]);
                    RingPrefs.life         = JsonSerializer.Deserialize<double[]>(lines[i+2]);
                    RingPrefs.delay        = JsonSerializer.Deserialize<double[]>(lines[i+3]);
                    RingPrefs.sizeStart    = JsonSerializer.Deserialize<double[]>(lines[i+4]);
                    RingPrefs.sizeEnd      = JsonSerializer.Deserialize<double[]>(lines[i+5]);
                    RingPrefs.color        = JsonSerializer.Deserialize<Color[]>(lines[i+6]);
                    RingPrefs.thickness    = JsonSerializer.Deserialize<double[]>(lines[i+7]);
                    RingPrefs.opacityStart = JsonSerializer.Deserialize<double[]>(lines[i+8]);
                    RingPrefs.opacityEnd   = JsonSerializer.Deserialize<double[]>(lines[i+9]);
                    break;
                }
            }
            UpdatePreferences();
        }

        public void TextSettingChanged(int ringNum, String setting, int value, bool changePresetMenuToCustom)
        {
            ringNum--;  // change ring number to array index
            switch (setting)
            {
                case "framerate":
                    RingPrefs.renderFPS = value;
                    SetAnimationInterval(value);
                    break;
                case "lifetimeTextbox":
                    RingPrefs.life[ringNum] = value;
                    break;
                case "renderDelayTextbox":
                    RingPrefs.delay[ringNum] = value;
                    break;
                case "startRadiusTextbox":
                    RingPrefs.sizeStart[ringNum] = value;
                    break;
                case "endRadiusTextbox":
                    RingPrefs.sizeEnd[ringNum] = value;
                    break;
                case "redColorTextbox":
                    RingPrefs.color[ringNum].R = (byte)value;
                    break;
                case "greenColorTextbox":
                    RingPrefs.color[ringNum].G = (byte)value;
                    break;
                case "blueColorTextbox":
                    RingPrefs.color[ringNum].B = (byte)value;
                    break;
                case "thicknessTextbox":
                    RingPrefs.thickness[ringNum] = value;
                    break;
                case "startVisibilityTextbox":
                    RingPrefs.opacityStart[ringNum] = value;
                    break;
                case "endVisibilityTextbox":
                    RingPrefs.opacityEnd[ringNum] = value;
                    break;
            }
            if (setting != "framerate" && changePresetMenuToCustom) {
                settingsWindow.ringPresetsMenu.SelectedIndex = selectedPresetIndex = 0;
            }
        }

        public ImageBrush GetBackgroundImageBrush()
        {
            return KeyboardBackgroundPaint;
        }

        public void KeyPressed(int keyCode)
        {
            Point origin = new Point(0, 0);
            for (int i = 0; i < KeyboardInfo.keysCount; i++)
            {
                if (KeyboardInfo.keyLocations[i, KeyboardInfo.key_WinCode] == keyCode)
                {
                    origin.X = KeyboardInfo.keyLocations[i, KeyboardInfo.key_X] * renderScaleHoriz;
                    origin.Y = KeyboardInfo.keyLocations[i, KeyboardInfo.key_Y] * renderScaleVert;
                    break;
                }
            }
            AddNewAnimatedCirle(origin);
            //System.Diagnostics.Debug.WriteLine(keyCode);
        }

        public void DoCanvasClick(Canvas theCanvas, Point clickLoc)
        {
            AddNewAnimatedCirle(clickLoc);
        }

        private void AddNewAnimatedCirle(Point where)
        {
            for (int i = 0; i < RingPrefs.maxRings; i++)
            {
                if (RingPrefs.active[i])
                {
                    AnimatedCircle circ = new AnimatedCircle(i, where);
                    theCircles.Add(circ);
                }
            }
        }

        public void InitializeAnimation(Canvas whereToDraw)
        {
            animationTimer = new System.Timers.Timer(1000 / RingPrefs.renderFPS);
            animationTimer.Elapsed += AnimationStep;    // the method to be called
            animationTimer.AutoReset = true;
            animationTimer.Enabled = true;

            theCircles = new AnimatedCircleGroup();
            drawCanvas = whereToDraw;   // artCanvas from the UI
            keysCanvas = new Canvas();
        }

        public void SetAnimationInterval(int newValue)
        {
            if (animationTimer != null) {
                animationTimer.Interval = 1000 / newValue;
            }
        }

        private void AnimationStep(object source, System.Timers.ElapsedEventArgs e)
        {
            theCircles.Animate();

            drawCanvas.Dispatcher.InvokeAsync(() =>      // must use deferred rendering because this code is not running on the UI thread
            {
                drawCanvas.Children.Clear();
                drawCanvas.Children.Add(theCircles.AllCirclesCanvas());
            });

            keysCanvas.Dispatcher.Invoke(() =>          // this is a work-around for the multiple threads of the UI and timer.
            {
                UpdateKeyLights();
            });
        }

        public void HaltAnimation()
        {
            animationTimer.Enabled = false;
            //allow queued canvas updates to play out to avoid app crash on exit (only in debugger?)
            System.Threading.Thread.Sleep(2 * (int)RingPrefs.renderFPS);
        }


        public void UpdateKeyLights()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(renderWidth, renderHeight, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(drawCanvas);
                ctx.DrawRectangle(vb, null, new Rect(0, 0, renderWidth, renderHeight));
            }
            rtb.Render(dv);
            // the code up to here creates a bitmap copy of what's shown in the UI artCanvas; the next 2 lines create a pixel map of RGB values
            int[] pixels = new int[renderWidth * renderHeight * renderBytesPerPixel];
            rtb.CopyPixels(pixels, renderBytesPerPixel * renderWidth, 0);
            // code above derived with thanks from https://blogs.msdn.microsoft.com/jaimer/2009/07/03/rendertargetbitmap-tips/

            // now loop through entire List of keys and set the color of each to match the pixel map at that location
            for (int i = 0; i < KeyboardInfo.keysCount; i++)
            {
                int keylocationX = (int)(KeyboardInfo.keyLocations[i, KeyboardInfo.key_X] * renderScaleHoriz);
                int keyLocationY = (int)(KeyboardInfo.keyLocations[i, KeyboardInfo.key_Y] * renderScaleVert);
                int arrayOffset = keyLocationY * renderWidth + keylocationX;
                int theColor = pixels[arrayOffset];
                int red = (int)(((theColor >> 16) & 0xFF) * mult8bitToPercent);
                int green = (int)(((theColor >> 8) & 0xFF) * mult8bitToPercent);
                int blue = (int)((theColor & 0xFF) * mult8bitToPercent);

                switch (KeyboardInfo.keyLocations[i, KeyboardInfo.key_WinCode])
                {
                    case KeyboardInfo.specialLogiKey:   // logo, badge, and G-keys
                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName((keyboardNames)KeyboardInfo.keyLocations[i, KeyboardInfo.key_HIDcode], red, green, blue);
                        break;
                    case KeyboardInfo.mediaKey:
                        break;                          // media keys have no controllable lights on G910
                    default:
                        LogitechGSDK.LogiLedSetLightingForKeyWithHidCode(KeyboardInfo.keyLocations[i, KeyboardInfo.key_HIDcode], red, green, blue);
                    // above three cases handle all per-key RGB device lighting. Next section is for zone devices
                        for (int j = 0; j < 5; j++) { 
                            if (keyToZoneMap[j] == KeyboardInfo.keyLocations[i, KeyboardInfo.key_WinCode]) {
                                //if the color is being set on one of my 5 trigger keys for zone lighting, set the zone lighting
                                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);
                                LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Keyboard, j+1, red, green, blue);  //zone 0 is entire G213; zone 1-5 set light colors
                                //assignment of trigger keys to set non-keyboard device colors is arbitrary; just did easy thing
                                if (RingPrefs.showOnDevice[RingPrefs.speakerDevice]) {
                                    LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Speaker, j, red, green, blue); //zone 4 doesn't exist but slower to if/then around it
                                }
                                if (j < 2)  // these devices have only 2 zones (0 and 1)
                                {
                                    if (RingPrefs.showOnDevice[RingPrefs.headsetDevice])
                                    {
                                        LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Headset, j, red, green, blue);
                                    }
                                    if (RingPrefs.showOnDevice[RingPrefs.mouseDevice])
                                    {
                                        LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Mouse, j, red, green, blue);
                                    }
                                }
                                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
                            }
                        }
                        break;
                }
            }
        }
    }
}
