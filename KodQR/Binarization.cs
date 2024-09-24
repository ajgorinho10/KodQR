using System;
using System.Drawing.Imaging;
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

            // Konwersja do skali szarości
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Gray2Bgr);

            // Binaryzacja obrazu za pomocą Adaptive Threshold
            Mat binary = new Mat();
            CvInvoke.AdaptiveThreshold(image, binary, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 91, 20.0);
            //CvInvoke.Threshold(image, binary, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);

            // Zwracamy wynikowy obraz w odcieniach szarości
            //CvInvoke.Imshow("Obraz z punktem", binary);
            //CvInvoke.WaitKey(0); // Czekanie na naciśnięcie klawisza

            //if (binary.Width > 2000 || binary.Height > 2000) CvInvoke.Resize(binary, binary, new System.Drawing.Size(), 0.9, 0.9, Inter.Linear);
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