using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageProcessor.Imaging.Helpers;


namespace KodQR.bar
{
    public class FindBar
    {
        public Image<Gray, Byte> img;

        public FindBar(Image<Gray, Byte> im) { this.img = im; }

        public void find2()
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
            CvInvoke.Threshold(blurred, thresh, 100, 255, ThresholdType.BinaryInv);

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

        public void find()
        {
            Mat image = this.img.Mat;

            // Set maxFrq value
            double maxFrq = 700; // Replace with your MaxFrq value

            // Run the proposed algorithm
            ProposedAlgorithm(image, maxFrq);

        }

        public void ProposedAlgorithm(Mat image, double maxFrq)
        {
            CvInvoke.GaussianBlur(this.img, image, new Size(9, 9), 3.0);

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(21, 7), new System.Drawing.Point(-1, -1));
            CvInvoke.MorphologyEx(image, image, MorphOp.Blackhat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Threshold(image, image, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);

            Mat labels = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();
            int numComponents = CvInvoke.ConnectedComponentsWithStats(image, labels, stats, centroids, LineType.EightConnected);

            var statsData = stats.GetData();
            int totalArea = image.Rows * image.Cols;

            Mat output = new Mat();
            labels.ConvertTo(output, DepthType.Cv8U);
            CvInvoke.Normalize(output, output, 0, 255, NormType.MinMax);
            CvInvoke.Imshow("XDD1", output);
            CvInvoke.WaitKey(0);

        }

        
    }
}
