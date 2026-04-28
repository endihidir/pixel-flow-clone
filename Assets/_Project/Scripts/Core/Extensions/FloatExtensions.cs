
namespace Core.Extensions
{
    public static class FloatExtensions
    {
        public static float Remap(this float value, float fromLow, float toLow, float fromHigh, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        public static float RemapBy(float value, float fromLow, float toLow, float fromHigh, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }
    }
}