using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;

namespace KodQR.qr
{
    public static class Binarization
    {
        public static Image<Gray, byte> Binarize(string filepath)
        {
            Mat image = CvInvoke.Imread(filepath, ImreadModes.Color | ImreadModes.AnyDepth);
            if (image.IsEmpty)
            {
                Console.WriteLine("Nie udało się wczytać obrazu.");
                return null;
            }

            Mat grayImg = new Mat();
            CvInvoke.CvtColor(image, grayImg, ColorConversion.Bgr2Gray);
            Mat binary = new Mat();
            int tmp = image.Width / 10;
            if (tmp % 2 == 0) { tmp++; }

            double tmp2 = 20.0;
            if (image.Width < 700 && image.Height < 700)
            {
                CvInvoke.AdaptiveThreshold(grayImg, binary, 255.0, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 121, 12);
            }
            else
            {
                //Console.WriteLine($"Siema");
                CvInvoke.AdaptiveThreshold(grayImg, binary, 255.0, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, tmp, 19.0);
            }



            return binary.ToImage<Gray, byte>();
        }
    }
}
