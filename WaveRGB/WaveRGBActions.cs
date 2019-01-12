using LedCSharp;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WaveRGB
{
    class WaveRGBActions
    {
        // Attributes
        readonly string KeyboardBackgroundImage = "Images/keysBackground.png";
        ImageBrush KeyboardBackgroundPaint;
        private System.Timers.Timer animationTimer;
        private int animationInterval = 1000 / 30;  //30 FPS
        private AnimatedCircleGroup theCircles;
        private Canvas drawCanvas;
        private Canvas keysCanvas;

        private int renderWidth;    // these next five values set by LoadCanvasBackgroundImage
        private int renderHeight;   //    allowed to be variable in case UI changes in the future
        private double renderScaleHoriz;
        private double renderScaleVert;
        private int renderBytesPerPixel;

        private const double mult8bitToPercent = 100.0 / 255.0;

        // Methods
        public string StartUp()
        {
            if (LogitechGSDK.LogiLedInitWithName("Wave RGB"))
            {   //if no error...
                System.Threading.Thread.Sleep(250); //wait for LGS to wake up
                LoadPreferences();
                KeyListener.StartKeyListener(this);
                int[] version = LGSversion();
                if (version[0] == -1)
                {
                    return "Successfully connected to LGS but version unknown.";
                }
                else
                {
                    return string.Format($"Successfully connected to LGS version {version[0]}.{version[1]}.{version[2]}.");
                }
            }
            else
            {
                return "Failed to connect to LGS.";
            }

        }


        internal void StopLGS()
        {
            LogitechGSDK.LogiLedShutdown();
        }

        internal int[] LGSversion()
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
            Uri uriSource = new Uri(KeyboardBackgroundImage, UriKind.Relative);
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
            LogitechGSDK.LogiLedGetConfigOptionNumber("Wave RGB/" + RingPrefs.renderName, ref RingPrefs.renderFPS);
            SetAnimationInterval((int)(1000 / RingPrefs.renderFPS));
            for (int i = 0; i < 5; i++)
            {
                LoadSpecificRingPref(i);
            }
        }

        internal void LoadSpecificRingPref(int arrayIndex)
        {
            string prompt = RingPrefs.ringName + " " + (arrayIndex + 1) + "/";
            LogitechGSDK.LogiLedGetConfigOptionBool(prompt + RingPrefs.activeName, ref RingPrefs.active[arrayIndex]);
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.lifeName, ref RingPrefs.life[arrayIndex]);
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.delayName, ref RingPrefs.delay[arrayIndex]);
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.sizeStartName, ref RingPrefs.sizeStart[arrayIndex]);
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.sizeEndName, ref RingPrefs.sizeEnd[arrayIndex]);
            int red = RingPrefs.color[arrayIndex].R;
            int green = RingPrefs.color[arrayIndex].G;
            int blue = RingPrefs.color[arrayIndex].B;
            LogitechGSDK.LogiLedGetConfigOptionColor(prompt + RingPrefs.colorName, ref red, ref green, ref blue);
            RingPrefs.color[arrayIndex].R = (byte)red;
            RingPrefs.color[arrayIndex].G = (byte)green;
            RingPrefs.color[arrayIndex].B = (byte)blue;
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.thicknessName, ref RingPrefs.thickness[arrayIndex]);
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.opacityStartName, ref RingPrefs.opacityStart[arrayIndex]);
            LogitechGSDK.LogiLedGetConfigOptionNumber(prompt + RingPrefs.opacityEndName, ref RingPrefs.opacityEnd[arrayIndex]);
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
            animationTimer = new System.Timers.Timer(animationInterval);
            animationTimer.Elapsed += AnimationStep;    // the method to be called
            animationTimer.AutoReset = true;
            animationTimer.Enabled = true;

            theCircles = new AnimatedCircleGroup();
            drawCanvas = whereToDraw;   // artCanvas from the UI
            keysCanvas = new Canvas();
        }

        public void SetAnimationInterval(int newValue)
        {
            animationInterval = newValue;
            if (animationTimer != null) {
                animationTimer.Interval = animationInterval;
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
            System.Threading.Thread.Sleep(2 * animationInterval);
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
                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName((keyboardNames) KeyboardInfo.keyLocations[i, KeyboardInfo.key_HIDcode], red, green, blue);
                        break;
                    case KeyboardInfo.mediaKey:
                        break;                          // media keys have no controllable lights on G910
                    default:
                        LogitechGSDK.LogiLedSetLightingForKeyWithHidCode(KeyboardInfo.keyLocations[i, KeyboardInfo.key_HIDcode], red, green, blue);
                        break;
                }
            }
        }
    }
}
