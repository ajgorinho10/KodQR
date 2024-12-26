using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodQR.bar
{
    public class Decoding
    {
        public int[] barTab;
        public Image<Gray, Byte> img;
        public int y_f;
        public Decoding(int[] tab, Image<Gray, Byte> im,int y) { this.barTab = tab; this.img = im; this.y_f = y; }

        public double distance(Point p1,Point p2)
        {
            int x = p1.X - p2.X;
            int y = p1.Y - p2.Y;
            x *= x;
            y *= y;

            return Math.Sqrt(x + y);
        }

        public String decode()
        {
            //CvInvoke.Imshow("praca", this.img);
            Image<Bgr, Byte> ima = this.img.Convert<Bgr, Byte>();
            //CvInvoke.Imshow("test", ima);
            //CvInvoke.WaitKey(0);
            Image<Bgr, Byte> ima2 = this.img.Convert<Bgr, Byte>();
            //CvInvoke.Line(ima, new System.Drawing.Point(0, y_f), new System.Drawing.Point(ima.Width, y_f), new MCvScalar(255, 0, 0));


            int w1 = 0;
            List<Point> m_points = new List<Point>();
            int d1 = 0, d3=0,d2 = 0;
            for(int i = 0; i < this.img.Width-1; i++)
            {
                if (this.img.Data[y_f,i,0] == 0)
                {
                    if (w1 == 0) { 
                        w1 = i;
                        if (d1 == 0)
                        {
                            d1 = i;
                        }
                    }
                    else if(this.img.Data[y_f, i + 1, 0] != 0)
                    {
                        m_points.Add(new Point(w1, y_f));
                        m_points.Add(new Point(i, y_f));
                        w1 = 0;

                        if (d3 == 0) { d3 = i; }
                        d2 = i;
                    }
                }
            }

            //d = Math.Round(d);
            double dend = m_points[m_points.Count - 1].X - m_points[m_points.Count - 2].X;
            //Console.WriteLine($"d1:{d1} d2:{m_points[m_points.Count - 2].X} d3:{m_points[m_points.Count - 1].X}");
            //Console.WriteLine($"modul_size:{d}");
            //Console.WriteLine($"start:{z0}");

            string binarry = "";
            double d = (m_points[3].X - m_points[0].X)/3.0;
            double dz = (m_points[m_points.Count-1].X - m_points[m_points.Count-4].X)/3.0;
            double dz2 = (m_points[31].X - m_points[28].X)/3.0;

            d = (d + dz +dz2) / 3.0;
            //double d = (d2-d1)/95.0;

            List<double> distance_list = new List<double>();
            
            for (int i = 0; i < m_points.Count - 1; i++)
            {
                double dis = distance(m_points[i], m_points[i + 1]);
                int x = (int)(m_points[i].X + m_points[i + 1].X) / 2;
                int color = this.img.Data[y_f, x, 0] == 255 ? 0 : 1;
                double ilosc = dis / d;
                //ilosc = Math.Round(ilosc);
                if(ilosc %((int)ilosc) > 0.63)
                {
                    ilosc++;
                }

                if ((int)ilosc == 0) { ilosc++; }

                //Console.WriteLine($"ilosc:{ilosc} kolor:{color}");
                if (binarry == "" && color == 0) { continue; }
                char c = color == 1 ? '1' : '0';
                binarry += new string(c,((int)ilosc));

                //CvInvoke.Circle(ima, new System.Drawing.Point(x, y_f), 3, new MCvScalar(0, 255, 0), -1);
                distance_list.Add(dis);
            }


            if(binarry.Count() < 43)
            {
                return "Brak";
            }
            //Console.WriteLine(binarry.Count()+" "+binarry);
            String msg = TabToString(binarry);
            if (msg == "Brak")
            {
                string xd = new string(binarry.Reverse().ToArray());
                msg = TabToString(xd);
                return msg;
            }
            else
            {
                return msg;
            }

            //CvInvoke.Imshow("ima", ima);
        }

        public String TabToString(string binarry)
        {
            string binaryCode = binarry;


            string trimmedCode = binaryCode.Substring(3, binaryCode.Length - 3 - 3);
            string leftPart = trimmedCode.Substring(0, 42);
            string rightPart = trimmedCode.Substring(47);

            //Console.WriteLine(" T:   " + trimmedCode);
            //Console.WriteLine("LR:   " + leftPart + "     " + rightPart);

            Dictionary<string, int> encodingA = new Dictionary<string, int>
        {
            {"0001101", 0}, {"0011001", 1}, {"0010011", 2}, {"0111101", 3},
            {"0100011", 4}, {"0110001", 5}, {"0101111", 6}, {"0111011", 7},
            {"0110111", 8}, {"0001011", 9}
        };

            Dictionary<string, int> encodingB = new Dictionary<string, int>
        {
            {"0100111", 0}, {"0110011", 1}, {"0011011", 2}, {"0100001", 3},
            {"0011101", 4}, {"0111001", 5}, {"0000101", 6}, {"0010001", 7},
            {"0001001", 8}, {"0010111", 9}
        };

            Dictionary<string, int> encodingC = new Dictionary<string, int>
        {
            {"1110010", 0}, {"1100110", 1}, {"1101100", 2}, {"1000010", 3},
            {"1011100", 4}, {"1001110", 5}, {"1010000", 6}, {"1000100", 7},
            {"1001000", 8}, {"1110100", 9}
        };


            string[] leftCodes = SplitIntoGroups(leftPart, 7);

            List<int> leftDigits = new List<int>();
            string leftEncodingPattern = "";

            foreach (string code in leftCodes)
            {
                if (encodingA.ContainsKey(code))
                {
                    leftDigits.Add(encodingA[code]);
                    leftEncodingPattern += "A";
                }
                else if (encodingB.ContainsKey(code))
                {
                    leftDigits.Add(encodingB[code]);
                    leftEncodingPattern += "B";
                }
                else
                {
                    //Console.WriteLine("Błąd: Nieznany wzorzec binarny w lewej stronie!");
                    return "Brak";
                }

            }

            string[] prefixPatterns = { "AAAAAA", "AABABB", "AABBAB", "AABBBA", "ABAABB", "ABBAAB", "ABBBAA", "ABABAB", "ABABBA", "ABBABA" };
            int prefix = Array.IndexOf(prefixPatterns, leftEncodingPattern);

            if (prefix == -1)
            {
                //Console.WriteLine("Błąd: Nie można ustalić prefiksu!");
                return "Brak";
            }

            string[] rightCodes = SplitIntoGroups(rightPart, 7);
            List<int> rightDigits = new List<int>();

            foreach (string code in rightCodes)
            {
                if (encodingC.ContainsKey(code))
                {
                    rightDigits.Add(encodingC[code]);
                }
                else
                {
                    //Console.WriteLine("Błąd: Nieznany wzorzec binarny w prawej stronie!");
                    return "Brak";
                }
            }

            List<int> fullCode = new List<int> { prefix };
            fullCode.AddRange(leftDigits);
            fullCode.AddRange(rightDigits);

            //Console.WriteLine("Dekodowany kod EAN-13: " + string.Join("", fullCode));
            return string.Join("", fullCode);
        }

        static string[] SplitIntoGroups(string input, int groupSize)
        {
            int numGroups = input.Length / groupSize;
            string[] groups = new string[numGroups];

            for (int i = 0; i < numGroups; i++)
            {
                groups[i] = input.Substring(i * groupSize, groupSize);
            }

            return groups;
        }
    }
}
