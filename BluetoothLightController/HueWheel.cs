﻿using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using static ColorPicker.Shared.ColorHelper;

#if WINDOWS_PHONE
using System.Windows.Media;
using System.Windows.Media.Imaging;
#else
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace ColorPicker.Shared
{
    public static class HueWheel
    {
        public static Task CreateBitmap(WriteableBitmap bitmap, float lightness, Func<Vector4, Color> colorSpace)
        {
            return FillBitmap(bitmap, (x, y) =>
            {
                return GetWheelColor(x, y, lightness, colorSpace);
            });
        }

        public static async Task FillBitmap(WriteableBitmap bmp, Func<float, float, Color> fillPixel)
        {
#if WINDOWS_PHONE
#else
            var stream = bmp.PixelBuffer.AsStream();
#endif
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            await Task.Run(() =>
            {
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < height; x++)
                    {
                        var color = fillPixel((x + .5f) / width, (y + .5f) / height);
#if WINDOWS_PHONE
                        bmp.Pixels[x + y * width] = WriteableBitmapExtensions.ConvertColor(color);
#else
                        WriteBGRA(stream, color);
#endif
                    }
                }
            });
#if !WINDOWS_PHONE
            stream.Dispose();
#endif
            bmp.Invalidate();
        }

        private static void WriteBGRA(Stream stream, Color color)
        {
            stream.WriteByte(color.B);
            stream.WriteByte(color.G);
            stream.WriteByte(color.R);
            stream.WriteByte(color.A);
        }

        public static Color GetWheelColor(float x, float y, float lightValue, Func<Vector4, Color> colorSpace)
        {
            x = x - 0.5f;
            y = (1 - y) - 0.5f;
            float saturation = 2 * (float)Math.Sqrt(x * x + y * y);
            float hue = y < 0 ?
                (float)Math.Atan2(-y, -x) + (float)Math.PI :
                (float)Math.Atan2(y, x);
            if (saturation > 1)
                saturation = 1;
            return colorSpace(new Vector4(hue / ((float)Math.PI * 2), saturation, lightValue, 1));
        }
    }
}
