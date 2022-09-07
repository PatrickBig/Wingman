namespace WarThunder.Wingman.Utilities
{
    public static class MathHelpers
    {
        public static double Approach(double from, double to, double dt, double viscosity)
        {
            if (viscosity < 1e-9)
            {
                return to;
            }
            else
            {
                return from + ((1.0 - Math.Exp(-dt / viscosity)) * (to - from));
            }
        }

        public static double Clamp(double x, double low, double high)
        {
            return Math.Max(low, Math.Min(x, high));
        }
    }
}
