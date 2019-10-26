using CGB.Models.Extensions;
using System.Linq;
using tessnet2;

namespace CGB.OCRLib
{
    public static class OCR
    {
        public static string GetTextByAccountBased(System.Drawing.Bitmap image, string destAccountNum)
        {
            string text = "";
            try
            {
                System.Drawing.Bitmap bitmap = OCR.ProcessImage(image);
                string val = new string(destAccountNum.ToCharArray().Distinct<char>().ToArray<char>());
                string tessdata = @"tessdata";
                Tesseract tesseract = new Tesseract();
                tesseract.SetVariable("tessedit_char_whitelist", val);
                tesseract.Init(tessdata, "eng", true);
                System.Collections.Generic.List<Word> list = tesseract.DoOCR(bitmap, System.Drawing.Rectangle.Empty);
                image.Dispose();
                foreach (Word current in list)
                {
                    text += current.Text;
                }
            }
            catch (System.Exception value)
            {
                System.Console.WriteLine(value);
            }
            return text.Replace(" ", "");
        }

        private static System.Drawing.Bitmap ProcessImage(System.Drawing.Bitmap image)
        {
            System.Drawing.Bitmap bitmap = null;
            try
            {
                bitmap = image.Clone(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);
                CaptchaInfo captchaInfo = new CaptchaInfo();
                bitmap = bitmap.AdjustCurves(127).RemoveNoise(new string[]
                {
                    "ff000000"
                }).GetCaptchaInfo(ref captchaInfo);
                System.Drawing.Color c = System.Drawing.ColorTranslator.FromHtml("#" + captchaInfo.CaptchaColor);
                float num = c.BrightnessLevel();
                byte threshold = (byte)(80f * num + 50f);
                if (c.B >= 250 && c.R == 0 && c.G == 0)
                {
                    threshold = 144;
                }
                else if (c.B >= 250 && (c.R == 0 || c.G == 0))
                {
                    threshold = 135;
                }
                else if (c.R == 255 && c.G == 255)
                {
                    threshold = 142;
                }
                bitmap = image.AdjustCurves(threshold)
                              .RemoveNoise(new string[] { "ff000000" })
                              .CleanUnecessaryPixel(captchaInfo).GetLongColoredText();

                //bitmap.Save("Test.jpg");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            return bitmap;
        }
    }
}
