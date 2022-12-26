namespace WpfGame
{
    public static class Mathf
    {
        public static double Lerp(double firstFloat, double secondFloat, double by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static double RemapToRange(double value, double start1, double stop1, double start2, double stop2)
        {
            return start2 + (value - start1) * ((stop2 - start2) / (stop1 - start1));
        }

        public static double TryParse(string s)
        {
            _ = double.TryParse(s, out double d);
            return d;
        }

        public static double LerpFloat(double start, double end, double progress)
        {
            return start + (end - start) * progress;
        }

        public static double Clamp(double value, double min, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static int[] ConvertToHighLow(long val)
        {
            int a1 = (int)(val & uint.MaxValue);
            int a2 = (int)(val >> 32);
            return new int[] { a1, a2 };
        }
        public static long ConvertFromHighLow(int a1, int a2)
        {
            long b = a2;
            b <<= 32;
            b |= (uint)a1;
            return b;
        }

        /// <summary>
        /// A tween-like function that takes a starting velocity and some other factors and returns an altered velocity.
        /// </summary>
        /// <param name="velocity">Any component of velocity (e.g. 20).</param>
        /// <param name="acceleration">Rate at which the velocity is changing.</param>
        /// <param name="drag">Really kind of a deceleration, this is how much the velocity changes if Acceleration is not set.</param>
        /// <param name="max">An absolute value cap for the velocity (0 for no cap).</param>
        /// <param name="elapsed">The amount of time passed in to the latest update cycle</param>
        /// <returns></returns>
        public static double ComputeVelocity(double velocity, double acceleration, double drag, double max, double elapsed)
        {
            if (acceleration != 0) velocity += acceleration * elapsed;
            else if (drag != 0)
            {
                var drag1 = drag * elapsed;
                if (velocity - drag1 > 0) velocity -= drag1;
                else if (velocity + drag < 0) velocity += drag1;
                else velocity = 0;
            }
            if (velocity != 0 && max != 0)
            {
                if (velocity > max) velocity = max;
                else if (velocity < -max) velocity = -max;
            }
            return velocity;
        }

    }
}
