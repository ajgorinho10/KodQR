using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Accord.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace KodQR.bar
{
    public class barBinarization
    {
        public Image<Bgr, Byte> img;
        public Image<Gray, Byte> img_binarry;

        public barBinarization(Image<Bgr, byte> img)
        {
            this.img = img;
        }

        public void barBinarize()
        {
            Image<Bgr, Byte> ColorImg = this.img.Clone();
            Image<Gray, Byte> GrayImg = new Image<Gray, Byte>(this.img.Width, this.img.Height);

            for (int y = 0; y < this.img.Height; y++)
            {
                for (int x = 0; x < this.img.Width; x++)
                {
                    var color_r = ColorImg.Data[y, x, 2];
                    var color_g = ColorImg.Data[y, x, 1];
                    var color_b = ColorImg.Data[y, x, 0];

                    byte gray = (byte)((color_b + color_g + color_r) / 3);
                    GrayImg.Data[y, x, 0] = gray;

                }
            }

            //CvInvoke.Imshow("GrayImg", GrayImg);
            //CvInvoke.WaitKey(0);

            float[] GrayHist;
            DenseHistogram Histo = new DenseHistogram(256, new RangeF(0, 256));
            Histo.Calculate(new Image<Gray, Byte>[] { GrayImg }, true, null);
            GrayHist = new float[256];
            Histo.CopyTo(GrayHist);

            float maxVal = 0;
            foreach (var val in GrayHist)
            {
                if (val > maxVal) maxVal = val;
            }

            int bins = 256;
            int histWidth = 512; // szerokość obrazu histogramu
            int histHeight = 400; // wysokość obrazu histogramu
            int binWidth = histWidth / bins;

            // Tworzenie obrazu wyjściowego
            Mat histImage = new Mat(histHeight, histWidth, DepthType.Cv8U, 1);
            histImage.SetTo(new MCvScalar(255)); // tło białe

            // Rysowanie histogramu
            for (int i = 0; i < bins; i++)
            {
                int intensity = (int)(GrayHist[i] / maxVal * histHeight);
                CvInvoke.Line(
                    histImage,
                    new System.Drawing.Point(i * binWidth, histHeight),
                    new System.Drawing.Point(i * binWidth, histHeight - intensity),
                    new MCvScalar(0), // kolor linii (czarny)
                    binWidth - 1 // szerokość linii
                );
            }

            // Znalezienie dwóch szczytów
            float peak1 = -1, peak2 = -1;
            int w1 = -1; int w2 = -1;
            int ilosc_0 = 0;
            for (int i = 1; i < bins - 1; i++)
            {
                //Console.WriteLine($"i:{i} w:{GrayHist[i]}");
                if(i < 128)
                {
                    if(peak1 < GrayHist[i])
                    {
                        peak1 = GrayHist[i];
                        w1 = i;
                    }
                }
                else if(i > 128)
                {
                    if (peak2 < GrayHist[i])
                    {
                        peak2 = GrayHist[i];
                        w2 = i;
                    }
                }
            }

            // Obliczenie wartości progowej
            int threshold = (w1 + w2) / 2;
            //Console.WriteLine($"Znaleziono szczyty w: {w1} i {w2}, próg: {threshold}");

            // Wyświetlenie obrazu histogramu
            //CvInvoke.Imshow("Histogram", histImage);
            //CvInvoke.WaitKey(0);

            Mat binaryImage = new Mat();
            //CvInvoke.Threshold(GrayImg, binaryImage, threshold, 255, ThresholdType.Binary|ThresholdType.Otsu);
            CvInvoke.AdaptiveThreshold(GrayImg, binaryImage, 255.0, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 81, 7.5);

            //CvInvoke.Resize(binaryImage, binaryImage, new Size(500, 500));
            //CvInvoke.Imshow("Binarized Image", binaryImage);
            //CvInvoke.WaitKey(0);

            this.img_binarry = binaryImage.ToImage<Gray,Byte>();
        }
    }
}
