using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dai;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FindPatterns;
using ZXing.Windows.Compatibility;

namespace KodQR
{
    public class Decode
    {
        public Image<Gray, Byte> img;
        public Decode() { }
        public Decode(Image<Gray,Byte> img) {
            this.img = img;       
        }



        public void fromImgToArray()
        {
            Console.WriteLine($"w:{img.Width} h:{img.Height}");
            MCvScalar color = new MCvScalar(255, 0, 255);
            double modulsize = cleanImg();
            Image<Bgr,Byte> ima = img.Convert<Bgr,Byte>();

            FindPatterns f = new FindPatterns(img);
            List<Punkt> po = f.FinderPatterns();

            if (po.Count <= 0)
            {
                Console.WriteLine($"error");
                return;
            }

            foreach (Punkt pa in po)
            {
                Console.WriteLine($"{pa.w} ");
            }

            //double modulsize = cleanImg();
            //modulsize = Math.Round(modulsize);
            double qrSize1 = img.Width /modulsize;
            qrSize1 = Math.Round(qrSize1);
            int qrSize = (int)qrSize1;

            Console.WriteLine($"ModulSize:{modulsize}");
            Console.WriteLine($"Qrsize:{qrSize}");


            int[,] binaryMatrix = new int[qrSize, qrSize];
            Bitmap map = new Bitmap(qrSize, qrSize);

            for (int y = 0; y < qrSize; y++)
            {
                for (int x = 0; x < qrSize; x++)
                {
                    // Znajdź środek każdego modułu
                    int sampleX = (int)((x + 0.50) * modulsize);
                    int sampleY = (int)((y + 0.50) * modulsize);
                    if(sampleX >= ima.Width)
                    {
                        sampleX = ima.Width-1;
                    }
                    if(sampleY >= ima.Height)
                    {
                        sampleY = ima.Height-1;
                    }

                    int pixelColor = img.Data[sampleY, sampleX, 0];

                    if (Math.Abs(sampleY-img.Width) <= modulsize)
                    {
                        pixelColor = img.Data[img.Width-1, sampleX-1, 0];
                        binaryMatrix[y, x] = pixelColor <= 128 ? 1 : 0;
                        map.SetPixel(x, y, pixelColor < 128 ? Color.Black : Color.White);
                        CvInvoke.Circle(ima, new Point(sampleX, sampleY), 1, color);
                        continue;
                    }

                    int C_r = img.Data[sampleY, sampleX + 1, 0];
                    int C_l = img.Data[sampleY, sampleX - 1, 0];
                    int C_u = img.Data[sampleY + 1, sampleX, 0];
                    int C_d = img.Data[sampleY - 1, sampleX, 0];

                    int C_r_u = img.Data[sampleY + 1, sampleX + 1, 0];
                    int C_l_u = img.Data[sampleY + 1, sampleX - 1, 0];
                    int C_r_d = img.Data[sampleY - 1, sampleX + 1, 0];
                    int C_l_d = img.Data[sampleY - 1, sampleX - 1, 0];

                    int sum = pixelColor + C_u + C_d + C_r + C_l + C_r_u + C_l_u + C_r_d + C_l_d;
                    sum /= 9;

                    binaryMatrix[y, x] = pixelColor <= 128 ? 1 : 0;
                    map.SetPixel(x, y, pixelColor < 128 ? Color.Black : Color.White);
                    CvInvoke.Circle(ima, new Point(sampleX, sampleY), 1, color);
                }
            }

            for (int x = 0; x < qrSize; x++)
            {
                for (int y = 0; y < qrSize; y++)
                {
                    Console.Write($"{(binaryMatrix[x, y] == 1 ? "@" : "-")} ");
                }
                Console.WriteLine();
            }

            var decoder = new BarcodeReader();
            var result = decoder.Decode(map);

            if (result != null)
            {
                Console.WriteLine($"Decoded:");
                Console.WriteLine($"{result.Text}");
            }

            CvInvoke.Imshow("ima-color", ima);
            //CvInvoke.Imshow("img-black_white", img);
            CvInvoke.WaitKey(0);
        }


        public Double cleanImg()
        {
            img = img.SmoothGaussian(3, 3, 54.0, 54.0);
            CvInvoke.AdaptiveThreshold(img, img, 255.0, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 121, 2.0);

            int leght = 0;
            for (int x = 0; x < img.Width; x++) {
                if (img.Data[3,x,0] == 0)
                {
                    leght++;
                }
                else
                {
                    break;
                }

            }
            //Console.WriteLine($"inny sposob:{leght/7.0}");
            return (leght+0.0) / 7.0;
            //CvInvoke.Imshow("siema",img);
            //CvInvoke.WaitKey(0);
        }


    }
}
