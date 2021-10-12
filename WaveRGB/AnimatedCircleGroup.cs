using System.Collections.Generic;
using System.Windows.Controls;

namespace WaveRGB
{
    class AnimatedCircleGroup
    {
        public List<AnimatedCircle> theCircles = new List<AnimatedCircle>();

        public bool drawLineOnly = false;

        public void Add(AnimatedCircle newCircle)
        {
            theCircles.Add(newCircle);
        }

        public void Animate()
        {
            if (theCircles.Count > 0)
            {
                // cannot mutate the List within the foreach loop, so create a new list of what's finished animating
                List<AnimatedCircle> toRemove = new List<AnimatedCircle>();

                foreach (AnimatedCircle circ in theCircles)
                {
                    // trigger animation step for each circle and assess the result
                    if (circ.AnimateCircle() == AnimatedCircle.AnimationResult.animationDone)
                    {
                        toRemove.Add(circ);
                    }
                }

                foreach (AnimatedCircle circ in toRemove)
                {
                        theCircles.Remove(circ);
                }
            }
        }

        public Canvas AllCirclesCanvas()        //returns a Canvas of all circle art combined
        {
            Canvas everything = new Canvas();
            foreach (AnimatedCircle circ in theCircles)
            {
                everything.Children.Add(circ.GetCircCanvas(drawLineOnly));
            }
            return everything;
        }
    }
}
