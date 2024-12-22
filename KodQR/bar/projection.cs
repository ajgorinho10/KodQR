using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace KodQR.bar
{
    public class projection
    {
        Image<Gray, Byte> barImages;
        public int[] barInTab;
        public Image<Gray, Byte> imBar;
        public int y_f;
        public projection(Image<Gray, Byte> p) { this.barImages = p;}

        public void Image_projection()
        {
            int m = this.barImages.Height;
            int n = this.barImages.Width;

            int[,] macierz = imgToMacierz(m,n);
            int[] vertical = vertical_sum(macierz);
        }

        public int[,] imgToMacierz(int m,int n)
        {
            int[,] tmp = new int[m,n];

            for(int i=0;i< m; i++)
            {
                for(int j=0;j< n; j++)
                {
                    int color = this.barImages.Data[i, j, 0] == 255 ? 0 : 1;
                    tmp[i, j] = color;
                }
            }

            return tmp;
        }

        public int[] vertical_sum(int[,] macierz)
        {
            Image<Gray, Byte> im = new Image<Gray, Byte>(this.barImages.Width, this.barImages.Height);
            int[] sum = new int[macierz.GetLength(1)];

            for(int i = 0; i < macierz.GetLength(1); i++)
            {
                int s = 0;
                for( int j= 0; j < macierz.GetLength(0); j++)
                {
                    if (macierz[j, i] == 0)
                    {
                        s++;
                    }
                }
                sum[i] = s;
                //Console.WriteLine($"i:{i} w:{s}");
            }

            for( int i = 0; i < sum.Length; i+= 1)
            {

               CvInvoke.Line(im,new System.Drawing.Point(i, sum[i]) ,new System.Drawing.Point(i, 0),new MCvScalar(255,255,255));
            }
            //CvInvoke.Imshow("img", im);
            //CvInvoke.WaitKey(0);

            findLine(im);

            return sum;
        }

        public int findLine(Image<Gray,Byte> ima)
        {


            List<int> y = new List<int>();
            for(int i = 0; i < ima.Height; i++)
            {
                int color1 = ima.Data[i, 0, 0];
                int j = 0;
                for(int x_first =0;x_first < ima.Width; x_first++)
                {
                    if (ima.Data[i, x_first, 0] == 0)
                    {
                        j = x_first;
                        break;
                    }
                }

                int changes = 0;
                for(j=j; j< ima.Width-1; j++)
                {
                    if (changes > 60)
                    {
                        break;
                    }
                        if (color1 != ima.Data[i, j, 0])
                    {
                        changes++;
                        color1 = ima.Data[i, j, 0];
                    }

                    if(j +1 == ima.Width && color1 == 1)
                    {
                        changes -= 1;
                    }
                    
                }

                
                //Console.WriteLine($"changes:{changes}");
                if(changes == 60)
                {
                    y.Add(i);
                }
            }

            Image<Bgr, Byte> im = ima.Convert<Bgr, Byte>();
            /*
            foreach (int i in y) {
                Console.WriteLine($"linia: {i}");
                
            }
            */
            if (y.Count > 0)
            {
                CvInvoke.Line(im, new System.Drawing.Point(0, y[0]), new System.Drawing.Point(im.Width, y[0]), new MCvScalar(255, 0, 0));
                CvInvoke.Line(im, new System.Drawing.Point(0, y[y.Count-1]), new System.Drawing.Point(im.Width, y[y.Count -1]), new MCvScalar(255, 0, 0));

                int y_final = (y[0] + y[y.Count - 1]) / 2;
                CvInvoke.Line(im, new System.Drawing.Point(0, y_final), new System.Drawing.Point(im.Width, y_final), new MCvScalar(255, 0, 0));

                int[] barTab = new int[ima.Width];

                for (int i = 0; i < barTab.Length; i++) {
                    barTab[i] = ima.Data[y_final,i,0];
                }

                /*
                int lastColor = barTab[0];
                foreach (int i in barTab)
                {
                    if(i == lastColor)
                    {
                        Console.Write(i + " ");
                    }
                    else
                    {
                        lastColor = i;
                        Console.WriteLine();
                        Console.Write(i + " ");
                    }

                }
                */
                this.barInTab = barTab;
                this.imBar = ima;
                y_f = y_final;

                //CvInvoke.Imshow("xd1", this.barImages);
                //CvInvoke.Imshow("xd2", ima);
                //CvInvoke.Imshow("xd", im);
                //CvInvoke.WaitKey(0);
            }
            return 0;
        }
    }
}
