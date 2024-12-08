using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Accord.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;


namespace KodQR.bar
{
    public class FindBar
    {
        public Image<Gray, Byte> img;
        public Image<Bgr, Byte> img_color;

        public FindBar(Image<Gray, Byte> im, Image<Bgr, Byte> im2) { this.img = im;this.img_color = im2; }

        static Mat CropRotatedRect(Mat inputImage, RotatedRect rotatedRect)
        {
            float sideLength = 200;
            PointF[] sours = rotatedRect.GetVertices();
            PointF[] destinationPoints = new PointF[] {
                new PointF(0,0),
                new PointF(sideLength,0),
                new PointF(sideLength,sideLength),
                new PointF(0,sideLength),
            };

            Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(sours, destinationPoints);

            // Przekształć obraz
            Mat outputImage = new Mat();
            CvInvoke.WarpPerspective(inputImage, outputImage, perspectiveMatrix, new Size((int)sideLength, (int)sideLength),Inter.Cubic);


            return outputImage;
        }

        public List<Image<Gray, Byte>> find()
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
            CvInvoke.ConvertScaleAbs(gradient, absGradient, 1.0, 1.0);

            Mat blurred = new Mat();
            CvInvoke.Blur(absGradient, blurred, new Size(10, 10), new Point(-1, -1));

            Mat thresh = new Mat();
            CvInvoke.Threshold(blurred, thresh, 128, 255, ThresholdType.Binary | ThresholdType.Otsu);

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Cross, new System.Drawing.Size(21,7), new Point(-1, -1));

            Mat closed = new Mat();
            CvInvoke.MorphologyEx(thresh, closed, MorphOp.Close, kernel, new Point(-1, -1), 1 ,BorderType.Default, new MCvScalar(0));

            CvInvoke.Erode(closed, closed, null, new Point(-1, -1), 4, BorderType.Default, new MCvScalar(0));
            CvInvoke.Dilate(closed, closed, null, new Point(-1, -1),4, BorderType.Default, new MCvScalar(0));

            //Image<Bgr, byte> xd = closed.ToImage<Bgr, Byte>();
            Image<Bgr, byte> xd = this.img.Convert<Bgr, Byte>();
            List<RotatedRect> conturs = new List<RotatedRect>();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(closed, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxTc89Kcos);

                var xd33 = hierarchy.GetData();
                //Console.WriteLine($"img.width:{this.img.Width} img.height:{this.img.Height}");
                for (int i = 0; i < contours.Size; i++)
                {
                    RotatedRect rect = CvInvoke.MinAreaRect(contours[i]);
                    //Console.WriteLine($"rect:{rect.Size.ToPointF().ToString()}, rodzina:{xd33.GetValue(0, i, 1).ToString()}");
                    if (xd33.GetValue(0, i, 1).ToString() != "-1")
                    {
                        //continue;
                    }
                    int max = closed.Height < closed.Width ? closed.Width : closed.Height;
                    if (rect.Size.Width < 80 || rect.Size.Height < 80 || rect.Size.Width >= max-10 || rect.Size.Height >= max-10)
                    {
                        continue;
                    }
                    if (rect.Size.Height / rect.Size.Width < 0.5 || rect.Size.Height / rect.Size.Width > 3)
                    {
                        continue;
                    }
                    if (Math.Abs(rect.Size.Width - rect.Size.Height) < 20)
                    {
                        continue;
                    }
                    PointF[] boxF = CvInvoke.BoxPoints(rect);
                    CvInvoke.Circle(xd, new Point((int)rect.Center.X, (int)rect.Center.Y), 5, new MCvScalar(0, 255, 0), -1);

                    Point[] box = boxF.Select(p => new Point((int)p.X, (int)p.Y)).ToArray();

                    // Rysowanie prostokąta na obrazie
                    using (VectorOfVectorOfPoint contoursToDraw1 = new VectorOfVectorOfPoint())
                    {
                        contoursToDraw1.Push(new VectorOfPoint(box));
                        //Console.WriteLine($"vec:{contoursToDraw1[0].Size}");
                        conturs.Add(rect);
                        CvInvoke.DrawContours(xd, contoursToDraw1, -1, new MCvScalar(0, 255, 0), 2);
                    }
                }
            }

            //Console.WriteLine($"Narysowane kontury");
            /*
            foreach (var contur in conturs)
            {
                PointF[] punkty = contur.GetVertices();

                int i = 1;
                foreach (var punkt in punkty)
                {
                    Console.WriteLine($"i:{i} x:{punkt.X} y:{punkt.Y}");
                    i++;
                }
                Console.WriteLine();
            }
            */
           // CvInvoke.Resize(xd, xd, new Size(500, 500));
            //CvInvoke.Imshow("xddd", xd);
            //CvInvoke.WaitKey(0);
            //CvInvoke.DestroyAllWindows();

            List<Image<Gray,Byte>> image_list = new List<Image<Gray,Byte>>();

            foreach (var contur in conturs)
            {

                Mat dstImage = CropRotatedRect(this.img_color.Mat, contur);

                //CvInvoke.Imshow("ehh", dstImage);
                //CvInvoke.WaitKey(0);

                barBinarization binrize = new barBinarization(dstImage.ToImage<Bgr,Byte>());
                binrize.barBinarize();

                image_list.Add(findBetter(binrize.img_binarry));
            }

            return image_list;
        }

       

        public Image<Gray,Byte> findBetter(Image<Gray,Byte> img)
        {
            int width = img.Width;
            int height = img.Height;
            int[,] networkTable = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    networkTable[y, x] = (img.Data[y,x,0] == 255) ? 0 : 1;
                }
            }

            int sum_0 = GetSumOfOnes(networkTable,img);
            //Console.WriteLine($"SUM(0°): {sum_0}");

            float angle = 1;
            int maxSum = sum_0;
            int bestAngle = 0;
            // Obracanie sieci o różne kąty i obliczanie SUM(Φ)
            for (int i = 1; i <= 360; i++)
            {
                // Obrót sieci o kąt (i * angle)
                Image<Gray, byte> rotatedImage = RotateImage(img, (i * angle));
                //CvInvoke.Imshow("xd", rotatedImage);
                //CvInvoke.WaitKey(0);
                // Zaktualizowanie tablicy sieciowej
                int[,] rotatedNetworkTable = GetNetworkTableFromImage(rotatedImage);

                // Obliczanie SUM(i * angle)
                int sum = GetSumOfOnes(rotatedNetworkTable,img);
                //Console.WriteLine($"SUM({i * angle}°): {sum}");

                // Sprawdzanie, czy uzyskano większą wartość SUM
                if (sum >= maxSum)
                {
                    maxSum = sum;
                    bestAngle = (int)(i * angle);
                }
            }

            // Wyświetlenie najlepszej wartości i kąta obrotu
            //Console.WriteLine($"Najlepszy kąt obrotu: {bestAngle}° z wartością SUM(MAX): {maxSum}");
            Image<Gray, Byte> rotateed = RotateImage(img, bestAngle);

            //CvInvoke.Imshow("Binarized Image", rotateed);
            //CvInvoke.WaitKey(0);

            return rotateed;
        }

        int GetSumOfOnes(int[,] networkTable,Image<Gray,Byte> img)
        {

            int lenght_height = networkTable.GetLength(0);
            int lenght_width = networkTable.GetLength(1);

            
            int y = lenght_height / 2;
            List<int> x_final = new List<int>();

            for (int j = lenght_width/4; j < lenght_width*3/4; j++)
            {
                if (networkTable[y,j] == 1)
                {
                   x_final.Add(j); 
                }
            }

            int sum = 0;
            foreach (int x in x_final) {

                for (int i = lenght_height / 4; i <= lenght_height*3/4; i++) {
                    if (networkTable[i,x] == 1) {
                        sum++;
                    }
                    else
                    {
                        break;
                    }
                }

            }
            

            return sum;
        }

        static Image<Gray, byte> RotateImage(Image<Gray, byte> image, float angle)
        {
            // Wykorzystanie funkcji do obrotu obrazu
            PointF center = new PointF(image.Width / 2, image.Height / 2);
            Mat rotationMatrix = new Mat();
            CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);
            Image<Gray, byte> rotatedImage = new Image<Gray, byte>(image.Size);
            rotatedImage.SetValue(new MCvScalar(255));
            CvInvoke.WarpAffine(image, rotatedImage, rotationMatrix, rotatedImage.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255));
            return rotatedImage;
        }

        static int[,] GetNetworkTableFromImage(Image<Gray, byte> image)
        {
            int width = image.Width;
            int height = image.Height;
            int[,] networkTable = new int[height, width];

            // Zapełnienie tablicy sieciowej 0 i 1
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    networkTable[y, x] = (image.Data[y, x, 0] == 255) ? 1 : 0;
                }
            }
            return networkTable;
        }
    }
}
