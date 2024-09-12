using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodQR
{
    public static class Binarization
    {
        static Bitmap ConvertToGrayscale(Bitmap original)
        {
            Bitmap grayscale;
            if (original.Width > 1200 || original.Height > 1200)
            {
                original = new Bitmap(original, original.Width / 2, original.Height / 2);
                grayscale = new Bitmap(original.Width, original.Height);
            }
            else
            {
                grayscale = new Bitmap(original.Width, original.Height);
            }

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);
                    int grayValue = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);
                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                    grayscale.SetPixel(x, y, grayColor);
                }
            }
            return grayscale;
        }

        static Bitmap AdaptiveThreshold(Bitmap gray)
        {
            double T = 128;
            double previousT = 0;

            while (Math.Abs(T - previousT) > 1e-2)
            {
                previousT = T;

                double sumBelowThreshold = 0;
                double sumAboveThreshold = 0;
                int countBelowThreshold = 0;
                int countAboveThreshold = 0;

                for (int y = 0; y < gray.Height; y++)
                {
                    for (int x = 0; x < gray.Width; x++)
                    {
                        Color pixelColor = gray.GetPixel(x, y);
                        if (pixelColor.R < T)
                        {
                            sumBelowThreshold += pixelColor.R;
                            countBelowThreshold++;
                        }
                        else
                        {
                            sumAboveThreshold += pixelColor.R;
                            countAboveThreshold++;
                        }
                    }
                }

                double V1 = countBelowThreshold == 0 ? 0 : sumBelowThreshold / countBelowThreshold;
                double V2 = countAboveThreshold == 0 ? 0 : sumAboveThreshold / countAboveThreshold;

                T = (V1 + V2) / 2.2;
            }

            Bitmap binary = new Bitmap(gray.Width, gray.Height);

            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    Color pixelColor = gray.GetPixel(x, y);
                    Color binaryColor = pixelColor.R < T ? Color.Black : Color.White;
                    binary.SetPixel(x, y, binaryColor);
                }
            }
            return binary;
        }

        public static Bitmap Binarize(String filepath)
        {
            Bitmap original = new Bitmap(filepath);
            Bitmap gray = ConvertToGrayscale(original);
            Bitmap binary = AdaptiveThreshold(gray);
            return binary;
        }
    }
}
