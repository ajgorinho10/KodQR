using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Diagnostics;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

class QRCodeReader
{
    public struct Punkt
    {
        public Punkt(int x, int y, double w)
        {
            this.X = x;
            this.Y = y;
            this.w = w;
        }
        public int X;
        public int Y;
        public double w;
    }

    public struct RegionDescriptors
    {
        public int Area; // M00
        public System.Drawing.Point Centroid; // (M10/M00, M01/M00)
        public Rectangle BoundingBox; // (Top, Left, Right, Bottom)
    }

    public static RegionDescriptors FloodFill(byte[] ptr, System.Drawing.Point start,bool black, int stride, int bytesPerPixel,int Width,int Height)
    {
        Stack<System.Drawing.Point> pixels = new Stack<System.Drawing.Point>();
        pixels.Push(start);
        RegionDescriptors descriptors = new RegionDescriptors();
        descriptors.BoundingBox = new Rectangle(Width, Height, 1, 1);
        bool[,] visited = new bool[Width, Height]; // Tablica odwiedzonych pikseli

        while (pixels.Count > 0)
        {
            System.Drawing.Point p = pixels.Pop();
            if (p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height || visited[p.X, p.Y])
                continue;

            visited[p.X, p.Y] = true; // Oznacz piksel jako odwiedzony

            byte color = ptr[(p.Y * stride) + (p.X * bytesPerPixel)];

            // Sprawdzenie, czy piksel należy do regionu (na przykład jest ciemny)
            if (black) // Przykładowe kryterium
            {
                if (color == 0)
                {
                    //image.SetPixel(p.X, p.Y, Color.Red);
                    // Aktualizacja deskryptorów regionu
                    descriptors.Area++;
                    descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X + p.X, descriptors.Centroid.Y + p.Y);
                    if(descriptors.BoundingBox.X > p.X) descriptors.BoundingBox.X = p.X;
                    if(descriptors.BoundingBox.Y > p.Y) descriptors.BoundingBox.Y = p.Y;
                    if(descriptors.BoundingBox.Width < p.X) descriptors.BoundingBox.Width = p.X;
                    if(descriptors.BoundingBox.Height < p.Y) descriptors.BoundingBox.Height = p.Y;


                    // Dodajemy sąsiednie piksele do stosu
                    pixels.Push(new System.Drawing.Point(p.X + 1, p.Y));
                    pixels.Push(new System.Drawing.Point(p.X - 1, p.Y));
                    pixels.Push(new System.Drawing.Point(p.X, p.Y + 1));
                    pixels.Push(new System.Drawing.Point(p.X, p.Y - 1));
                }

            }
            else
            {
                if (color == 255)
                {
                    //image.SetPixel(p.X, p.Y, Color.Blue);
                    // Aktualizacja deskryptorów regionu
                    descriptors.Area++;
                    descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X + p.X, descriptors.Centroid.Y + p.Y);
                    if (descriptors.BoundingBox.X > p.X) descriptors.BoundingBox.X = p.X;
                    if (descriptors.BoundingBox.Y > p.Y) descriptors.BoundingBox.Y = p.Y;
                    if (descriptors.BoundingBox.Width < p.X) descriptors.BoundingBox.Width = p.X;
                    if (descriptors.BoundingBox.Height < p.Y) descriptors.BoundingBox.Height = p.Y;

                    // Dodajemy sąsiednie piksele do stosu
                    pixels.Push(new System.Drawing.Point(p.X + 1, p.Y));
                    pixels.Push(new System.Drawing.Point(p.X - 1, p.Y));
                    pixels.Push(new System.Drawing.Point(p.X, p.Y + 1));
                    pixels.Push(new System.Drawing.Point(p.X, p.Y - 1));
                }
            }
        }

        // Obliczenie centroidu
        if (descriptors.Area > 0)
        {
            descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X / descriptors.Area, descriptors.Centroid.Y / descriptors.Area);
        }
        pixels.Clear();
        return descriptors;
    }

    static void Main(string[] args)
    {
        //string filePath = "zyczenia-sensonauka.png";
        //string filePath = "qr-code-21x21.png";
        //string filePath = "qr7.png";
        //string filePath = "qr6.png";
        //string filePath = "rq3.png";
        //string filePath = "qr-1.png";
        //string filePath = "megaqr.png";
        //string filePath = "qrmax.png";
        string filePath = "qrkat.png";
        //string filePath = "qrmid.png";
        //string filePath = "qrtest1.png";
        //string filePath = "qrciekawy.png";
        //string filePath = "QR4b.jpg";
        //
        //string filePath = "qrostateczny.png";
        //string filePath = "qrxd2.png";
        //string filePath = "qrzaduzy.png";
        string outputFilePath = "output.png";

        Stopwatch stopwatch = Stopwatch.StartNew();
        Bitmap original = new Bitmap(filePath);

        Bitmap grayscale = ConvertToGrayscale(original);
        Bitmap binary = AdaptiveThreshold(grayscale);

        List<Punkt> finderPatterns = FindFinderPatterns(binary);
        stopwatch.Stop();
        Console.WriteLine($"Czas wykonania operacji: {stopwatch.ElapsedMilliseconds} ms");
        int i = 1;
        foreach (var point in finderPatterns)
        {
            Console.WriteLine($"{i} Finder Pattern at: {point.X}, {point.Y} w=={point.w}");
            i++;
        }

        using (Graphics g = Graphics.FromImage(binary))
        {
            foreach (var point in finderPatterns)
            {
                g.DrawRectangle(Pens.Red, (int)(point.X), (int)(point.Y), 1, 1);
            }
        }

        binary.Save(outputFilePath, ImageFormat.Png);

        Process.Start(new ProcessStartInfo(outputFilePath) { UseShellExecute = true });

    }

    static Bitmap ConvertToGrayscale(Bitmap original)
    {
        Bitmap grayscale;
        if (original.Width > 1200 || original.Height > 1200)
        {
            original = new Bitmap(original, original.Width / 2, original.Height / 2);
            grayscale = new Bitmap(original.Width, original.Height);
        }
        else
        {
            grayscale = new Bitmap(original.Width, original.Height);
        }

        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                Color originalColor = original.GetPixel(x, y);
                int grayValue = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);
                Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                grayscale.SetPixel(x, y, grayColor);
            }
        }
        return grayscale;
    }

    static Bitmap AdaptiveThreshold(Bitmap gray)
    {
        double T = 128;
        double previousT = 0;

        while (Math.Abs(T - previousT) > 1e-2)
        {
            previousT = T;

            double sumBelowThreshold = 0;
            double sumAboveThreshold = 0;
            int countBelowThreshold = 0;
            int countAboveThreshold = 0;

            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    Color pixelColor = gray.GetPixel(x, y);
                    if (pixelColor.R < T)
                    {
                        sumBelowThreshold += pixelColor.R;
                        countBelowThreshold++;
                    }
                    else
                    {
                        sumAboveThreshold += pixelColor.R;
                        countAboveThreshold++;
                    }
                }
            }

            double V1 = countBelowThreshold == 0 ? 0 : sumBelowThreshold / countBelowThreshold;
            double V2 = countAboveThreshold == 0 ? 0 : sumAboveThreshold / countAboveThreshold;

            T = (V1 + V2) / 2.2;
        }

        Bitmap binary = new Bitmap(gray.Width, gray.Height);

        for (int y = 0; y < gray.Height; y++)
        {
            for (int x = 0; x < gray.Width; x++)
            {
                Color pixelColor = gray.GetPixel(x, y);
                Color binaryColor = pixelColor.R < T ? Color.Black : Color.White;
                binary.SetPixel(x, y, binaryColor);
            }
        }
        return binary;
    }



    static List<Punkt> FindFinderPatterns(Bitmap binary)
    {
        ConcurrentBag<Punkt> patterns = new ConcurrentBag<Punkt>();
        ConcurrentBag<Punkt> patterns3 = new ConcurrentBag<Punkt>();

        // Użycie LockBits dla szybszego dostępu do pikseli
        BitmapData binaryData = binary.LockBits(new Rectangle(0, 0, binary.Width, binary.Height), ImageLockMode.ReadOnly, binary.PixelFormat);
        int bytesPerPixel = Image.GetPixelFormatSize(binary.PixelFormat) / 8;
        int stride = binaryData.Stride;
        IntPtr scan0 = binaryData.Scan0;
        // Przygotowanie tablicy do przechowywania danych pikseli
        byte[] pixels = new byte[binary.Width * binary.Height * bytesPerPixel];
        System.Runtime.InteropServices.Marshal.Copy(scan0, pixels, 0, pixels.Length);

        int imageWidth = binary.Width;
        int imageHeight = binary.Height;

        unsafe
        {
            Parallel.For(0, imageHeight, y =>
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    if (pixels[(y * stride) + (x * bytesPerPixel)] == 0) // Sprawdzenie tylko jednego kanału, ponieważ obraz jest binarny
                    {
                        Punkt c = isPattern(pixels, stride, bytesPerPixel, imageWidth, imageHeight, x, y, true);
                        if (c.X >= 0)
                        {
                            patterns.Add(c);
                        }
                    }
                }
            });

            patterns = PogrupujPunkty(patterns);

            Parallel.ForEach(patterns, p =>
            {
                Parallel.For(p.X - (int)(p.w / 7.0) - 1, p.X + (int)(p.w / 7.0) + 1, x =>
                {
                    for (int y = p.Y - (int)(4.5 * p.w / 7.0) + 1; y <= p.Y + (int)(4.5 * p.w / 7.0) + 1; y++)
                    {
                        Punkt c = isPattern(pixels, stride, bytesPerPixel, imageWidth, imageHeight, x, y, false);
                        if (c.X >= 0)
                        {
                            if (patterns3.Contains(p)) continue;
                            patterns3.Add(p);
                        }
                    }
                });
            });
        }
        binary.UnlockBits(binaryData);

        ConcurrentBag<Punkt> patterns4 = new ConcurrentBag<Punkt>();
        Parallel.ForEach(patterns3, p =>
        {
            Punkt c = VerifyFinderPatternRegions(pixels, p, stride, bytesPerPixel, imageWidth, imageHeight);
            if (c.X >= 0)
            {
                patterns4.Add(c);
            }
        });

        patterns4 = PogrupujPunkty(patterns4);

        return new List<Punkt>(patterns4);
    }




    public static Punkt VerifyFinderPatternRegions(byte[] ptr ,Punkt p, int stride, int bytesPerPixel,int width,int height)
    {
        int x = p.X;
        int y = p.Y;
        RegionDescriptors area2, area3;
        //Console.WriteLine("y0==" + y + " " + img.GetPixel(x, y).R+"-"+ img.GetPixel(x, y).G + "-" + img.GetPixel(x, y).B);
        RegionDescriptors area = FloodFill(ptr, new System.Drawing.Point(p.X, p.Y),true,stride,bytesPerPixel,width,height);

        byte color = ptr[(y * stride) + (x * bytesPerPixel)];
        while ((y > 0) && (color == 0))
        {
            y = y-1;
            color = ptr[(y * stride) + (x * bytesPerPixel)];
            //Console.WriteLine("dziala");
        }

        //Console.WriteLine("y1=="+y+" "+ img.GetPixel(x, y).R);
        area2 = FloodFill(ptr, new System.Drawing.Point(x, y),false, stride, bytesPerPixel, width, height);

        color = ptr[(y * stride) + (x * bytesPerPixel)];
        while ((y > 0) && (color == 255))
        {
            y--;
            color = ptr[(y * stride) + (x * bytesPerPixel)];
        }

        //Console.WriteLine("y2==" + y + " " + img.GetPixel(x, y).R);
        area3 = FloodFill(ptr, new System.Drawing.Point(x, y), true, stride, bytesPerPixel, width, height);


        double W_RA = area.BoundingBox.Width-area.BoundingBox.X;
        double H_RA = area.BoundingBox.Height - area.BoundingBox.Y;
        double Area_RA = area.Area;

        double W_RB = area2.BoundingBox.Width - area2.BoundingBox.X;
        double H_RB = area2.BoundingBox.Height - area2.BoundingBox.Y;
        double Area_RB = area2.Area;

        double W_RC = area3.BoundingBox.Width - area3.BoundingBox.X;
        double H_RC = area3.BoundingBox.Height - area3.BoundingBox.Y;
        double Area_RC = area3.Area;

        bool areaCondition = Area_RA < Area_RB && Area_RB < Area_RC &&
                             (double)Area_RB / Area_RA < 6 &&
                             (double)Area_RC / Area_RA < 7;

        // Weryfikacja proporcji
        bool proportionCondition = 0.1 < (double)W_RB / H_RB && (double)W_RB / H_RB < 3 &&
                                    0.1 < (double)W_RC / H_RC && (double)W_RC / H_RC < 3;

        double distanceA = Math.Sqrt(Math.Pow(area.Centroid.X - area2.Centroid.X, 2.0) + Math.Pow(area.Centroid.Y - area2.Centroid.Y, 2.0));
        double distanceB = Math.Sqrt(Math.Pow(area.Centroid.X - area3.Centroid.X, 2.0) + Math.Pow(area.Centroid.Y - area3.Centroid.Y, 2.0));

        bool distanceCondition = (distanceA <= 5) && (distanceB <= 5);

        /*
        Console.WriteLine();
        Console.WriteLine("RA == " + Area_RA + "|" + W_RA + "|" + H_RA + "| X == " + area.Centroid.X + "| Y == " + area.Centroid.Y);
        Console.WriteLine("RB == " + Area_RB + "|" + W_RB + "|" + H_RB + "| X == " + area2.Centroid.X + "| Y == " + area2.Centroid.Y);
        Console.WriteLine("RC == " + Area_RC + "|" + W_RC + "|" + H_RC + "| X == " + area3.Centroid.X + "| Y == " + area3.Centroid.Y);
        Console.WriteLine();
        */
        if (areaCondition && proportionCondition && distanceCondition)
        {
            return new Punkt(area.Centroid.X,area.Centroid.Y, W_RC);
        }

        return new Punkt(-1,-1,-1.0);
    }


    static unsafe Punkt isPattern(byte[] ptr, int stride, int bytesPerPixel, int width, int height, int startX, int startY, bool horizontal)
    {
        if (startY < 0 || startY >= height || startX < 0 || startX >= width) return new Punkt(-2, -2, -2);

        int[] pattern = new int[5];
        int length = 0;
        int x = startX;
        int y = startY;
        byte initialColor = ptr[(y * stride) + (x * bytesPerPixel)];

        for (int i = 0; i < 5 && x < width && y < height; i++)
        {
            length = 0;
            while (x < width && y < height)
            {
                byte currentColor = ptr[(y * stride) + (x * bytesPerPixel)];
                if (initialColor != currentColor) break;
                length++;
                if (horizontal) x++;
                else y++;
            }
            pattern[i] = length;
            initialColor ^= 0xFF; // Zmienia kolor na przeciwny (0 na 255 i odwrotnie)
        }

        double w = (pattern[0] + pattern[1] + pattern[2] + pattern[3] + pattern[4]) / 7.0;
        double w2 = pattern[0] + pattern[1] + pattern[2] + pattern[3] + pattern[4];

        for (int i = 0; i < pattern.Length; i++)
        {
            if (i != 2)
            {
                double tmp1 = w - 5;
                double tmp2 = w + 5;

                if (pattern[i] < tmp1 || pattern[i] > tmp2)
                {
                    return new Punkt(-2, -2, -2);
                }
            }
            else
            {
                double tmp1 = 3 * w - 6;
                double tmp2 = 3 * w + 6;
                if (pattern[i] < tmp1 || pattern[i] > tmp2)
                {
                    return new Punkt(-2, -2, -2);
                }
            }
        }

        double tmp3 = pattern[0] + pattern[1];
        double tmp4 = pattern[3] + pattern[4];
        if (pattern[2] >= tmp3 && pattern[2] >= tmp4)
        {
            int cx = horizontal ? (x + startX - 1) / 2 : x;
            int cy = horizontal ? y : (y + startY - 1) / 2;
            return new Punkt(cx, cy, w2);
        }

        return new Punkt(-2, -2, -2);
    }

        static ConcurrentBag<Punkt> PogrupujPunkty(ConcurrentBag<Punkt> Vpunkty)
    {
        List<Punkt> punkty = new List<Punkt>(Vpunkty);
        List<List<Punkt>> grupy = new List<List<Punkt>>();
        HashSet<int> odwiedzone = new HashSet<int>();

        for (int i = 0; i < punkty.Count; i++)
        {
            if (!odwiedzone.Contains(i))
            {
                List<Punkt> grupa = new List<Punkt> { punkty[i] };
                odwiedzone.Add(i);

                for (int j = i + 1; j < punkty.Count; j++)
                {
                    if (!odwiedzone.Contains(j))
                    {
                        if (Math.Abs(punkty[i].X - punkty[j].X) <= 3 && (Math.Abs(punkty[i].Y - punkty[j].Y) <= ((3 * punkty[i].w) / 7.0)))
                        {
                            grupa.Add(punkty[j]);
                            odwiedzone.Add(j);
                        }
                    }
                }

                grupy.Add(grupa);
            }
        }

        List<Punkt> nowePunkty = new List<Punkt>();

        foreach (var grupa in grupy)
        {
            int sredniaX = (int)Math.Round(grupa.Average(p => p.X));
            int sredniaY = (int)Math.Round(grupa.Average(p => p.Y));
            int sredniaW = (int)Math.Round(grupa.Average(p => p.w));

            nowePunkty.Add(new Punkt(sredniaX, sredniaY, sredniaW));
        }

        return new ConcurrentBag <Punkt>(nowePunkty);
    }

}