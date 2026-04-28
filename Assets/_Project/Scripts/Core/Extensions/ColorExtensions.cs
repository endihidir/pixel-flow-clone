using UnityEngine;

namespace Core.Extensions
{
	public static class ColorExtensions
	{
		public static Color SetTransparent(this Color c)
		{
			c.a = 0f;
			return c;
		}

		public static Color SetOpaque(this Color c)
		{
			c.a = 1f;
			return c;
		}

		public static Color SetRed(this Color c, float r)
		{
			c.r = r;
			return c;
		}

		public static Color SetGreen(this Color c, float g)
		{
			c.g = g;
			return c;
		}

		public static Color SetBlue(this Color c, float b)
		{
			c.b = b;
			return c;
		}

		public static Color SetAlpha(this Color c, float a)
		{
			c.a = a;
			return c;
		}
	}
}