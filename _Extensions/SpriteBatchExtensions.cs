using Blish_HUD.Controls;
using Blish_HUD;
using MonoGame.Extended.BitmapFonts;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements
{
    public static class SpriteBatchExtensions
    {
        public static void DrawStringOnCtrl(this SpriteBatch spriteBatch, Control ctrl, string text, BitmapFont font, Rectangle destinationRectangle, Color color, bool wrap, bool stroke, Color strokeColor, int strokeDistance = 1, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, VerticalAlignment verticalAlignment = VerticalAlignment.Middle)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!stroke || strokeColor == Color.Black)
            {
                spriteBatch.DrawStringOnCtrl(ctrl, text, font, destinationRectangle, color, wrap, stroke, strokeDistance, horizontalAlignment, verticalAlignment);
                return;
            }

            text = (wrap ? DrawUtil.WrapText(font, text, destinationRectangle.Width) : text);
            if (horizontalAlignment != 0 && (wrap || text.Contains("\n")))
            {
                using (StringReader stringReader = new StringReader(text))
                {
                    for (int i = 0; destinationRectangle.Height - i > 0; i += font.LineHeight)
                    {
                        string text2;
                        if ((text2 = stringReader.ReadLine()) == null)
                        {
                            break;
                        }

                        spriteBatch.DrawStringOnCtrl(ctrl, text2, font, destinationRectangle.Add(0, i, 0, 0), color, wrap, stroke, strokeColor, strokeDistance, horizontalAlignment, verticalAlignment);
                    }

                    return;
                }
            }

            Vector2 vector = font.MeasureString(text);
            destinationRectangle = destinationRectangle.ToBounds(ctrl.AbsoluteBounds);
            int num = destinationRectangle.X;
            int num2 = destinationRectangle.Y;
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    num += destinationRectangle.Width / 2 - (int)vector.X / 2;
                    break;
                case HorizontalAlignment.Right:
                    num += destinationRectangle.Width - (int)vector.X;
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Middle:
                    num2 += destinationRectangle.Height / 2 - (int)vector.Y / 2;
                    break;
                case VerticalAlignment.Bottom:
                    num2 += destinationRectangle.Height - (int)vector.Y;
                    break;
            }

            Vector2 vector2 = new Vector2(num, num2);
            float num3 = ctrl.AbsoluteOpacity();
            if (stroke)
            {
                spriteBatch.DrawString(font, text, vector2.OffsetBy(0f, -strokeDistance), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(strokeDistance, -strokeDistance), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(strokeDistance, 0f), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(strokeDistance, strokeDistance), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(0f, strokeDistance), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(-strokeDistance, strokeDistance), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(-strokeDistance, 0f), strokeColor * num3);
                spriteBatch.DrawString(font, text, vector2.OffsetBy(-strokeDistance, -strokeDistance), strokeColor * num3);
            }

            spriteBatch.DrawString(font, text, vector2, color * num3);
        }

        public static void DrawFrame(this SpriteBatch spriteBatch, Control ctrl, Color color, int stroke = 1)
        {
            // border top
            spriteBatch.DrawOnCtrl(ctrl,
                ContentService.Textures.Pixel,
                new Rectangle(0, 0, ctrl.Width, stroke),
                color
            );

            // border bottom
            spriteBatch.DrawOnCtrl(ctrl,
                ContentService.Textures.Pixel,
                new Rectangle(0, ctrl.Height - stroke, ctrl.Width, stroke),
                color
            );

            // border left
            spriteBatch.DrawOnCtrl(ctrl,
                ContentService.Textures.Pixel,
                new Rectangle(0, 0, stroke, ctrl.Height),
                color
            );

            // border right
            spriteBatch.DrawOnCtrl(ctrl,
                ContentService.Textures.Pixel,
                new Rectangle(ctrl.Width - stroke, 0, stroke, ctrl.Height),
                color
            );
        }

        public static void DrawFrame(this SpriteBatch spriteBatch, Control ctrl, int stroke = 1)
        {
            spriteBatch.DrawFrame(ctrl, Color.Black, stroke);
        }
    }
}
