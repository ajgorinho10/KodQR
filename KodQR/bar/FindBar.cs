using System;
using System.Drawing;
using System.Linq;
using Accord.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace KodQR.bar
{
    public class FindBar
    {
        public Image<Gray, Byte> img;

        public FindBar(Image<Gray, Byte> im) { this.img = im; }

        public void find()
        {
            Image<Bgr, Byte> im = this.img.Convert<Bgr, Byte>();
            Mat gray = this.img.Mat;
            Mat gradX = new Mat();
            Mat gradY = new Mat();
            CvInvoke.Sobel(gray, gradX, DepthType.Cv32F, 1, 0, -1);
            CvInvoke.Sobel(gray, gradY, DepthType.Cv32F, 0, 1, -1);

            Mat gradient = new Mat();
            CvInvoke.Subtract(gradX, gradY, gradient);

            Mat absGradient = new Mat();
            CvInvoke.ConvertScaleAbs(gradient, absGradient,1.0,1.0);

            Mat blurred = new Mat();
            CvInvoke.Blur(absGradient, blurred, new Size(9, 9), new Point(-1, -1));

            Mat thresh = new Mat();
            CvInvoke.Threshold(blurred, thresh, 128, 255, ThresholdType.Binary|ThresholdType.Otsu);

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(14, 2),new Point(-1,-1));

            Mat closed = new Mat();
            CvInvoke.MorphologyEx(thresh, closed, MorphOp.Close, kernel,new Point(-1,-1),2,BorderType.Default,new MCvScalar(255));

            CvInvoke.Erode(closed,closed,null,new Point(-1,-1),15,BorderType.Default,new MCvScalar(255));
            CvInvoke.Dilate(closed, closed, null, new Point(-1, -1), 15, BorderType.Default, new MCvScalar(255));

            //Image<Bgr, byte> xd = closed.ToImage<Bgr, Byte>();
            Image<Bgr, byte> xd = this.img.Convert<Bgr, Byte>();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(closed, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxTc89Kcos);

                var xd33 = hierarchy.GetData();
                for (int i = 0; i < contours.Size; i++)
                {
                    if (xd33.GetValue(0, i, 1).ToString() == "-1")
                    {
                        continue;
                    }
                    RotatedRect rect = CvInvoke.MinAreaRect(contours[i]);
                    Console.WriteLine($"rect:{rect.Size}");
                    if (rect.Size.Width < 80 || rect.Size.Height < 80 || rect.Size.Width >= this.img.Width-10 || rect.Size.Height >= this.img.Height-10)
                    {
                       continue;
                    }
                    if(rect.Size.Height/rect.Size.Width < 0.5 || rect.Size.Height / rect.Size.Width > 3)
                    {
                        continue;
                    }
                    if(Math.Abs(rect.Size.Width-rect.Size.Height) < 20)
                    {
                        continue;
                    }
                    PointF[] boxF = CvInvoke.BoxPoints(rect);
                    CvInvoke.Circle(xd,new Point((int)rect.Center.X, (int)rect.Center.Y), 5,new MCvScalar(0, 255, 0),-1);

                    Point[] box = boxF.Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

                    // Rysowanie prostokąta na obrazie
                    using (VectorOfVectorOfPoint contoursToDraw = new VectorOfVectorOfPoint())
                    {
                        contoursToDraw.Push(new VectorOfPoint(box));  // Dodajemy kontur prostokąta
                        CvInvoke.DrawContours(xd, contoursToDraw, -1, new MCvScalar(0, 255, 0), 3);
                    }
                }
            }

            CvInvoke.Resize(xd, xd, new Size(500, 500));
            CvInvoke.Imshow("xddd", xd);
            CvInvoke.WaitKey(0);
            CvInvoke.DestroyAllWindows();
        }


        public void find3()
        {
            Image<Bgr, Byte> im = this.img.Convert<Bgr,Byte>();
            Mat image = this.img.Mat;
            CvInvoke.GaussianBlur(this.img, image, new Size(9, 9), 3.0);

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(15, 15), new System.Drawing.Point(-1, -1));
            CvInvoke.MorphologyEx(image, image, MorphOp.Blackhat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Threshold(image, image, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);

            CvInvoke.Imshow("z", image);
            CvInvoke.Imshow("bez", im);
            CvInvoke.WaitKey(0);
        }

        public void find4()
        {
            

        }

        public void ProposedAlgorithm(Mat image, double maxFrq)
        {
            CvInvoke.GaussianBlur(this.img, image, new Size(9, 9), 3.0);
            CvInvoke.Laplacian(image, image, DepthType.Default);

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(21, 7), new System.Drawing.Point(-1, -1));
            CvInvoke.MorphologyEx(image, image, MorphOp.Blackhat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            float[] histogramData = new float[256];
            Mat histogramMat = new Mat();
            VectorOfUMat vou = new VectorOfUMat();
            vou.Push(image.GetUMat(AccessType.ReadWrite));
            // Oblicz histogram
            CvInvoke.CalcHist(
                vou,        // Tablica obrazów
                new int[] { 0 },                // Kanał (0 = skala szarości)
                null,                           // Maska (null = cały obraz)
                histogramMat,                   // Wynikowy histogram
                new int[] { 256 },              // Liczba binów
                new float[] { 0, 256 },         // Zakres intensywności pikseli
                false                           // Nie normalizuj histogramu
            );

            // Skopiuj dane histogramu do tablicy
            histogramData = histogramMat.GetData() as float[];

            // Sprawdzenie, czy dane zostały poprawnie skopiowane
            if (histogramData == null)
                throw new InvalidOperationException("Nie można pobrać danych histogramu.");

            // Znajdź maksymalną wartość i jej indeks
            int maxIndex = Array.IndexOf(histogramData, histogramData.Max());
            float maxValue = histogramData[maxIndex];

            // Wyświetl wyniki
            Console.WriteLine($"Najczęstsza wartość pikseli: {maxIndex}");
            Console.WriteLine($"Częstotliwość: {maxValue}");
            /*
            CvInvoke.Threshold(image, image, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);

            Mat labels = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();
            int numComponents = CvInvoke.ConnectedComponentsWithStats(image, labels, stats, centroids, LineType.EightConnected);

            var statsData = stats.GetData();
            int totalArea = image.Rows * image.Cols;

            */
            Mat output = new Mat();
            image.ConvertTo(output, DepthType.Cv8U);
            CvInvoke.Normalize(output, output, 0, 255, NormType.MinMax);
            CvInvoke.Imshow("XDD1", output);
            CvInvoke.WaitKey(0);

        }

        
    }
}
