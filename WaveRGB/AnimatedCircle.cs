using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WaveRGB
{
    class AnimatedCircle
    {
        // every animated circle has these attributes:
        private double radius;
        private readonly double radiusStep;
        private Point center;
        private double opacity;
        private readonly double opacityStep;
        private readonly double thickness;
        private Color color;
        private readonly double endLife;
        private double currentLife;
        private readonly double birthDelay;

        public enum AnimationResult
        {
            nothing,
            animationRunning,
            animationDone,
            animationHidden
        }

        public AnimatedCircle(int source, Point where)  // constructor method
        {
            radius = RingPrefs.sizeStart[source];
            radiusStep = (RingPrefs.sizeEnd[source] - RingPrefs.sizeStart[source]) / RingPrefs.life[source];
            center = where;
            opacity = RingPrefs.opacityStart[source];
            opacityStep = (RingPrefs.opacityEnd[source] - RingPrefs.opacityStart[source]) / RingPrefs.life[source];
            thickness = RingPrefs.thickness[source];
            color = RingPrefs.color[source];
            endLife = RingPrefs.life[source];
            currentLife = 0;
            birthDelay = RingPrefs.delay[source];
        }

        public Canvas GetCircCanvas()
        {
            Canvas circCanvas = new Canvas();
            if (currentLife > birthDelay)
            {
                Ellipse myCircle = new Ellipse
                {
                    Height = radius * 2,
                    Width = radius * 2,
                    StrokeThickness = thickness,
                    Stroke = new SolidColorBrush(color),   //Brushes.White;
                    Opacity = opacity / 100,
                };
                circCanvas.Children.Add(myCircle);
                Canvas.SetLeft(circCanvas, center.X - radius);
                Canvas.SetTop(circCanvas, center.Y - radius);
            }
            return circCanvas;
        }

        public AnimationResult AnimateCircle()
        {
            AnimationResult result = AnimationResult.animationRunning;
            if (currentLife > (endLife + birthDelay))
            {
                result = AnimationResult.animationDone;
            } else if (currentLife > birthDelay)
            {
                radius += radiusStep;
                opacity += opacityStep;
            //} else
            //{
            //    result =  AnimationResult.animationHidden;   Not currently used
            }
            currentLife++;
            return result;
        }
    }
}

