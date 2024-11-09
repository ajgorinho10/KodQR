using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;
using System.Threading.Channels;
using Accord.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XPhoto;
using static FindPatterns;

namespace KodQR
{
    public static class Binarization
    {
        public static Image<Gray, Byte> Binarize(String filepath)
        {
            //Wczytujemy obraz kolorowy
            Mat image = CvInvoke.Imread(filepath, ImreadModes.AnyDepth);

            if (image.IsEmpty)
            {
                Console.WriteLine("Nie udało się wczytać obrazu.");
                return null;
            }

            Image<Gray, Byte> grayImg = image.ToImage<Gray,Byte>();
            Image<Gray, byte> claheImg = new Image<Gray, byte>(image.Size);

            // Tworzymy obiekt CLAHE
            CvInvoke.CLAHE(grayImg,10.0,new Size(8,8), claheImg);
            CvInvoke.Normalize(claheImg, claheImg, 0, 255, NormType.MinMax, DepthType.Default);
            //CvInvoke.Imshow("Obraz z punktem", claheImg);
            //CvInvoke.Imshow("Obraz z punktem2", image);
            CvInvoke.WaitKey(0);

            // Konwersja do skali szarości
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Gray2Bgr);

            // Binaryzacja obrazu za pomocą Adaptive Threshold
            Mat binary = new Mat();
            int tmp = claheImg.Width / 5;
            if (tmp % 2 == 0) { tmp++; }
            CvInvoke.AdaptiveThreshold(image, binary, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, tmp, 20.0);
            //CvInvoke.Threshold(claheImg, binary, 0, 255, ThresholdType.Binary | ThresholdType.Triangle);

            // Zwracamy wynikowy obraz w odcieniach szarości
            if (binary.Width > 2000 || binary.Height > 2000) CvInvoke.Resize(binary, binary, new System.Drawing.Size(), 0.5, 0.5, Inter.NearestExact);
            CvInvoke.Imshow("Obraz z punktem", binary);
            CvInvoke.WaitKey(0); // Czekanie na naciśnięcie klawisza

            
            return binary.ToImage<Gray, Byte>();
        }
    }
}

//int row = 22;
//int col = 22;
//int Channel = 0;
//Image<Gray, Byte> ImageFormat = binary.ToImage<Emgu.CV.Structure.Gray,Byte>();
//int pixel_value = ImageFormat.Data[row, col, Channel];
//Console.WriteLine($"Wartość piksela w ({row}, {col}): {pixel_value}");

//CvInvoke.Imwrite("resized_binary_qr_code.png", binary);