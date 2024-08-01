using Microsoft.Xna.Framework;
using System;

namespace Flyga.AdditionalAchievements
{
    public static class RectangleExtensions
    {
        public static Rectangle ScaleAroundCenter(this Rectangle rectangle, float scale)
        {
            Point center = rectangle.Center;

            float newWidth = (float)rectangle.Width * scale;
            float newHeight = (float)rectangle.Height * scale;

            float newX = (float)center.X - (newWidth / 2);
            float newY = (float)center.Y - (newHeight / 2);

            return new Rectangle((int)Math.Floor(newX), (int)Math.Floor(newY), (int)Math.Ceiling(newWidth), (int)Math.Ceiling(newHeight));
        }

        public static Rectangle CenterAround(this Rectangle rectangle, Point center)
        {
            Point currentCenter = rectangle.Center;

            Point delta = center - currentCenter;

            return new Rectangle(rectangle.X + delta.X, rectangle.Y + delta.Y, rectangle.Width, rectangle.Height);
        }
    }
}
