using System;
using System.Drawing;
using System.Linq;
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
            CvInvoke.Threshold(blurred, thresh, 205, 255, ThresholdType.Binary);

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(14, 7),new Point(-1,-1));

            Mat closed = new Mat();
            CvInvoke.MorphologyEx(thresh, closed, MorphOp.Close, kernel,new Point(-1,-1),1,BorderType.Default,new MCvScalar(255));

            CvInvoke.Erode(closed,closed,null,new Point(-1,-1),4,BorderType.Default,new MCvScalar(0));
            CvInvoke.Dilate(closed, closed, null, new Point(-1, -1), 4, BorderType.Default, new MCvScalar(0));

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
                    if (rect.Size.Width < 80 || rect.Size.Height < 80)
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

            CvInvoke.Imshow("xddd", xd);
            CvInvoke.WaitKey(0);
            CvInvoke.DestroyAllWindows();
        }


        public void find2()
        {
            Image<Bgr, Byte> im = this.img.Convert<Bgr,Byte>();
            Mat grayImg = this.img.Mat;

            // 3. Wykryj krawędzie za pomocą algorytmu Canny
            Mat edges = new Mat();
            CvInvoke.Canny(grayImg, edges, 50, 200);

            // 4. Wykryj linie za pomocą transformacji Hougha
            using (Mat lines = new Mat())
            {
                CvInvoke.HoughLines(edges, lines, 1, Math.PI / 180, 100);

                // 5. Iteruj przez wykryte linie i rysuj je na obrazie
                for (int i = 0; i < lines.Rows; i++)
                {
                    float[] lineData = lines.GetData() // Pobiera dane jako tablicę
                        .Cast<float>() // Rzutuje każdy element na `float`
                        .Skip(i * 2) // Pomija pierwsze `i * 2` elementów
                        .Take(2) // Pobiera kolejne 2 elementy
                        .ToArray(); // Konwertuje wynik na tablicę
                    double rho = lineData[0];   // Odległość od środka (r)
                    double theta = lineData[1]; // Kąt w radianach

                    // Przekształć (rho, theta) na punkty na linii
                    double a = Math.Cos(theta);
                    double b = Math.Sin(theta);
                    double x0 = a * rho;
                    double y0 = b * rho;

                    Point pt1 = new Point((int)(x0 + 1000 * (-b)), (int)(y0 + 1000 * a));
                    Point pt2 = new Point((int)(x0 - 1000 * (-b)), (int)(y0 - 1000 * a));

                    // Rysuj linię na obrazie
                    CvInvoke.Line(im, pt1, pt2, new MCvScalar(0, 0, 255), 2);
                }
            }

            // 6. Wyświetl wynik
            CvInvoke.Imshow("Hough Lines", im);
            CvInvoke.WaitKey(0);
        }
    }
}
