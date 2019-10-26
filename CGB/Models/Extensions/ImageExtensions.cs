using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace CGB.Models.Extensions
{
    public static class ImageExtensions
    {
        public static Bitmap AdjustCurves(this Bitmap image, byte threshold = 127)
        {
            Bitmap imageClone = image.Clone(new Rectangle(0, 0, image.Width - 1, image.Height - 1), image.PixelFormat);

            Channel channel = Channel.All;

            byte[] Levels = new byte[256];
            for (int i = 76; i < Levels.Length; i++)
            {
                Levels[i] = 255;
            }

            Rectangle selectRect = new Rectangle(0, 0, imageClone.Width - 1, imageClone.Height - 1);

            Rectangle rect = new Rectangle(0, 0, imageClone.Width - 1, imageClone.Height - 1);
            BitmapData bmpData =
                imageClone.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                imageClone.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * (imageClone.Height);
            byte[] rgbValues = new byte[bytes];
            int bytesStart = 3 * selectRect.Left - 1;
            //int bytesEnd = 3 * selectRect.Right + 1;
            int scanStart = selectRect.Top * bmpData.Stride;
            int scanEnd = selectRect.Bottom * bmpData.Stride;

            if (channel == Channel.All)
            {
                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                // I try use for... for... two loops, but it is much slower 
                // than one loop

                for (int i = scanStart; i < scanEnd + 1; i++)//only one loop 
                {
                    int w = i % bmpData.Stride;
                    if (w > bytesStart)//&& w < bytesEnd)
                    {
                        rgbValues[i] = Levels[rgbValues[i]];
                    }
                }
            }

            // Copy the RGB values back to the bitmap
            Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            imageClone.UnlockBits(bmpData);

            return imageClone;
        }

        public static Bitmap AdjustCurves2(this Bitmap image)
        {
            Bitmap imageClone = image.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);
            image.Dispose();

            Channel channel = Channel.All;

            byte[] Levels = new byte[256];
            for (int i = 95; i < Levels.Length; i++)
            {
                Levels[i] = 255;
            }

            Rectangle selectRect = new Rectangle(0, 0, imageClone.Width - 1, imageClone.Height - 1);

            Rectangle rect = new Rectangle(0, 0, imageClone.Width - 1, imageClone.Height - 1);
            BitmapData bmpData =
                imageClone.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                imageClone.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride * (imageClone.Height));
            byte[] rgbValues = new byte[bytes];
            int bytesStart = 3 * selectRect.Left - 1;
            int scanStart = selectRect.Top * bmpData.Stride;
            int scanEnd = selectRect.Bottom * bmpData.Stride;

            if (channel == Channel.All)
            {
                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                // I try use for... for... two loops, but it is much slower 
                // than one loop

                for (int i = scanStart; i < scanEnd + 1; i++)//only one loop 
                {
                    int w = i % bmpData.Stride;
                    if (w > bytesStart)//&& w < bytesEnd)
                    {
                        rgbValues[i] = Levels[rgbValues[i]];
                    }
                }
            }

            // Copy the RGB values back to the bitmap
            Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            imageClone.UnlockBits(bmpData);

            return imageClone;
        }

        public static Bitmap GetCaptchaInfo(this Bitmap image, ref CaptchaInfo info)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixel = image.GetPixel(i, j);

                    if (!info.Colors.ContainsKey(pixel.Name))
                        info.Colors[pixel.Name] = 1;
                    else
                        info.Colors[pixel.Name] = info.Colors[pixel.Name] + 1;
                }
            }

            var list = info.Colors.Values.ToList();
            list.Sort();
            list.Reverse();

            info.BackgroundColor = "";
            info.CaptchaColor = "";

            foreach (var item in info.Colors)
            {
                if (item.Value == list[0] && string.IsNullOrEmpty(info.BackgroundColor))
                {
                    info.BackgroundColor = item.Key;
                    continue;
                }

                if (item.Value == list[1] && string.IsNullOrEmpty(info.CaptchaColor))
                {
                    info.CaptchaColor = item.Key;
                    continue;
                }

                info.IrrelevantColor.Add(item.Key);
            }

            return image;
        }

        private static bool IsPixelWithinRadius(int x, int y, Bitmap img, CaptchaInfo info, int connections = 3)
        {
            try
            {
                // Check left, right, bottomLeft, bottom, bottomRight, topLeft, top, topRight
                Color color = img.GetPixel(x, y);

                int count = 0;

                //left
                if (x - 1 >= 0)
                {
                    color = img.GetPixel(x - 1, y);

                    if (info.CaptchaColor == color.Name)
                        count++;
                }

                //right
                if (x + 1 < img.Width)
                {
                    color = img.GetPixel(x + 1, y);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }

                //top
                if (y - 1 >= 0)
                {
                    color = img.GetPixel(x, y - 1);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }

                //top left
                if (y - 1 >= 0 && x - 1 >= 0)
                {
                    color = img.GetPixel(x - 1, y - 1);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }

                //top right
                if (y - 1 >= 0 && x + 1 >= 0)
                {
                    color = img.GetPixel(x + 1, y - 1);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }

                //bottomLeft
                if (y + 1 < img.Height && x - 1 >= 0)
                {
                    color = img.GetPixel(x - 1, y + 1);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }


                //bottomRight
                if (y + 1 < img.Height && x + 1 < img.Width)
                {
                    color = img.GetPixel(x + 1, y + 1);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }


                //bottom
                if (y + 1 < img.Height)
                {
                    color = img.GetPixel(x, y + 1);
                    if (info.CaptchaColor == color.Name)
                        count++;
                }

                return count >= connections;
            }
            catch { }

            return true;
        }
        
        public static Bitmap CleanUnecessaryPixel(this Bitmap image, CaptchaInfo info)
        {
            var colorList = info.Colors.Keys.ToList().Where(x => x != info.BackgroundColor).ToList();

            int connections = 2;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);

                    // Skip background pixel
                    if (pixel.Name == info.BackgroundColor)
                        continue;

                    if (colorList.Contains(pixel.Name))
                    {
                        bool connected = IsPixelWithinRadius(x, y, image, info, connections);

                        if (!connected)
                            image.SetPixel(x, y, ColorTranslator.FromHtml("#" + info.BackgroundColor));
                    }
                }
            }

            return image;
        }

        public static Bitmap BlackenColor(this Bitmap image)
        {
            Bitmap imageClone = image.Clone(new Rectangle(0, 0, image.Width, image.Height - 1), image.PixelFormat);

            for (int i = 0; i < imageClone.Width; i++)
            {
                for (int j = 0; j < imageClone.Height; j++)
                {
                    Color color = imageClone.GetPixel(i, j);
                    if (color.Name != "ffffffff")
                    {
                        imageClone.SetPixel(i, j, Color.Black);
                    }
                }
            }

            return imageClone;
        }

        public static Bitmap Sharpen(this Bitmap image)
        {
            Bitmap sharpenImage = new Bitmap(image.Width, image.Height);

            int filterWidth = 3;
            int filterHeight = 3;
            int w = image.Width;
            int h = image.Height;

            double[,] filter = new double[filterWidth, filterHeight];

            filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            double factor = 1.0;
            double bias = 0.0;

            Color[,] result = new Color[image.Width, image.Height];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    ////=====[REMOVE LINES]========================================================
                    //// Color must be read per filter entry, not per image pixel.
                    //Color imageColor = image.GetPixel(x, y);
                    ////===========================================================================

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + w) % w;
                            int imageY = (y - filterHeight / 2 + filterY + h) % h;

                            //=====[INSERT LINES]========================================================
                            // Get the color here - once per fiter entry and image pixel.
                            Color imageColor = image.GetPixel(imageX, imageY);
                            //===========================================================================

                            red += imageColor.R * filter[filterX, filterY];
                            green += imageColor.G * filter[filterX, filterY];
                            blue += imageColor.B * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    sharpenImage.SetPixel(i, j, result[i, j]);
                }
            }
            return sharpenImage;
        }

        public static Bitmap RemoveNoise(this Bitmap image, params string[] colorToRemove)
        {
            List<string> listColorToRemove = new List<string>();

            listColorToRemove.AddRange(colorToRemove.ToList());

            if (listColorToRemove.Count > 0)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        Color pixel = image.GetPixel(i, j);

                        if (listColorToRemove.Contains(pixel.Name))
                        {
                            image.SetPixel(i, j, Color.White);
                        }
                    }
                }
            }

            return image;
        }

        public static Bitmap GetLongColoredText(this Bitmap image)
        {
            Dictionary<string, int> color = new Dictionary<string, int>();

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixel = image.GetPixel(i, j);

                    if (!color.ContainsKey(pixel.Name))
                        color[pixel.Name] = 1;
                    else
                        color[pixel.Name] = color[pixel.Name] + 1;
                }
            }

            var list = color.Values.ToList();
            list.Sort();
            list.Reverse();

            string backgroundKey = "";
            string captchaKey = "";
            foreach (var item in color)
            {
                if (item.Value == list[0] && string.IsNullOrEmpty(backgroundKey))
                {
                    backgroundKey = item.Key;
                }

                if (item.Value == list[1] && string.IsNullOrEmpty(captchaKey))
                {
                    captchaKey = item.Key;
                }
            }

            List<Tuple<Point, Point>> yPoint = new List<Tuple<Point, Point>>();

            int[] yAxisHeight = new int[image.Height];

            for (int y = 0; y < image.Height; y++)
            {
                int pixWidthStart = image.Width - 1;
                int pixWidthEnd = 0;
                int lastY = 0;

                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);

                    if (captchaKey == pixel.Name)
                    {
                        if (pixWidthStart > x)
                        {
                            pixWidthStart = x;
                        }

                        if (pixWidthEnd < x)
                        {
                            pixWidthEnd = x;
                            lastY = y;
                        }
                    }
                }

                if (pixWidthStart < pixWidthEnd)
                {
                    yAxisHeight[y] = pixWidthEnd - pixWidthStart;
                    yPoint.Add(Tuple.Create(new Point(pixWidthStart, lastY), new Point(pixWidthEnd, lastY)));
                }
                else
                    yPoint.Add(Tuple.Create(new Point(0, 0), new Point(0, 0)));
            }

            ///
            bool newSet = true;
            List<List<int>> groupIndex = new List<List<int>>();
            for (int i = 0; i < yAxisHeight.Length; i++)
            {
                if (yAxisHeight[i] == 0)
                {
                    newSet = true;
                    continue;
                }

                if (newSet)
                {
                    groupIndex.Add(new List<int>() { i });
                    newSet = false;
                }
                else
                    groupIndex.Last().Add(i);
            }

            double highestAve = 0;
            int widthIndex = 0;
            int maxWidth = 0;
            int newLeft = image.Width;
            int newRight = 0;



            for (int i = 0; i < groupIndex.Count; i++)
            {
                int sum = 0;

                for (int j = 0; j < groupIndex[i].Count; j++)
                {
                    sum += yAxisHeight[groupIndex[i][j]];
                }

                double ave = sum / groupIndex[i].Count;

                if (highestAve < ave)
                {
                    highestAve = ave;
                    widthIndex = i;

                    for (int x = 0; x < groupIndex[i].Count; x++)
                    {
                        if (maxWidth < yAxisHeight[groupIndex[i][x]])
                            maxWidth = yAxisHeight[groupIndex[i][x]];

                        if (newLeft > yPoint[groupIndex[i][x]].Item1.X)
                            newLeft = yPoint[groupIndex[i][x]].Item1.X;

                        if (newRight < yPoint[groupIndex[i][x]].Item2.X)
                            newRight = yPoint[groupIndex[i][x]].Item2.X;
                    }
                }
            }


            int newYStart = groupIndex[widthIndex].First();
            int newYEnd = groupIndex[widthIndex].Last();
            int newHeight = newYEnd - newYStart;
            int newWidth = image.Width;

            Rectangle cropRect = new Rectangle(0, newYStart - 2, newWidth, newHeight + 6);
            Bitmap imageClone = new Bitmap(newWidth, newHeight + 6);

            using (Graphics g = Graphics.FromImage(imageClone))
            {
                g.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight + 6), cropRect, GraphicsUnit.Pixel);
            }

            image.Dispose();
            return imageClone;
        }

        public static float BrightnessLevel(this Color c)
        {
            return ((float)Math.Sqrt(
               c.R * c.R * .241 +
               c.G * c.G * .691 +
               c.B * c.B * .068)) / 256;
        }
    }

    public enum Channel { Red = 2, Green = 1, Blue = 0, All = 3 };

    public class CaptchaInfo
    {
        public Dictionary<string, int> Colors = new Dictionary<string, int>();
        public String BackgroundColor { get; set; }
        public String CaptchaColor { get; set; }
        public List<string> IrrelevantColor = new List<string>();
    }
}
