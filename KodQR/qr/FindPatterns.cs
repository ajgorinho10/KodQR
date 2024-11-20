using System;
using System.Collections.Concurrent;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using static FindPatterns;

public class FindPatterns
 {
    private Image<Gray, Byte> img;
    private int image_width;
    private int image_height;

    public FindPatterns(Image<Gray, Byte> img)
    {
        this.img = img;
        this.image_width = img.Cols;
        this.image_height = img.Rows;
    }

    public class Punkt
    {
        public Punkt()
        {
            this.X = -1;
            this.Y = -1;
            this.w = -1.0;
        }

        public Punkt(int x, int y, double w)
        {
            X = x;
            Y = y;
            this.w = w;
        }

        public Punkt(int x, int y)
        {
            X = x;
            Y = y;
            this.w = 0.0;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public double w { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Punkt other)
            {
                return Math.Abs(this.X - other.X) <=1 && Math.Abs(this.Y - other.Y) <= 1 && Math.Abs(this.w - other.w) < 2;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, w);
        }
    }

    public struct RegionDescriptors
    {
        public int Area;
        public System.Drawing.Point Centroid;
        public Rectangle BoundingBox;
    }

    public struct Obszar
    {
        public int x1, y1;
        public int x2, y2;
        public int x3, y3;
        public int x4, y4;

        public Obszar(Punkt p)
        {
            this.x1 = p.X-1;
            this.y1 = p.Y-1;

            this.x2 = p.X+1;
            this.y2 = p.Y+1;

            this.x3 = p.X-1;
            this.y3 = p.Y+1;

            this.x4 = p.X+1;
            this.y4 = p.Y-1;
        }
    }

    public RegionDescriptors FloodFill(Punkt punkt, bool black)
    {
        int Width = this.image_width;
        int Height = this.image_height;

        Stack<System.Drawing.Point> pixels = new Stack<System.Drawing.Point>();
        RegionDescriptors descriptors = new RegionDescriptors();

        pixels.Push(new System.Drawing.Point(punkt.X, punkt.Y));
        descriptors.BoundingBox = new Rectangle(Width, Height, 1, 1);
        bool[,] visited = new bool[Height, Width];
        int expexted_color = black ? 0 : 255;

        System.Drawing.Point p1, p2, p3, p4;


        double maxArea = punkt.w * punkt.w;

        while (pixels.Count > 0)
        {
            System.Drawing.Point p = pixels.Pop();

            visited[p.Y, p.X] = true;

            byte color = this.img.Data[p.Y, p.X, 0];

            if (color == expexted_color)
            {
                descriptors.Area += 1;
                if(descriptors.Area <= 0 || descriptors.Area > maxArea) { descriptors.Area = 0; break; }

                descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X + p.X, descriptors.Centroid.Y + p.Y);
                if (descriptors.BoundingBox.X > p.X) descriptors.BoundingBox.X = p.X;
                if (descriptors.BoundingBox.Y > p.Y) descriptors.BoundingBox.Y = p.Y;
                if (descriptors.BoundingBox.Width < p.X) descriptors.BoundingBox.Width = p.X;
                if (descriptors.BoundingBox.Height < p.Y) descriptors.BoundingBox.Height = p.Y;

                p1 = new System.Drawing.Point(p.X + 1, p.Y);
                p2 = new System.Drawing.Point(p.X - 1, p.Y);
                p3 = new System.Drawing.Point(p.X, p.Y + 1);
                p4 = new System.Drawing.Point(p.X, p.Y - 1);

                if(!checkConditions(p1, visited)) pixels.Push(p1);
                if(!checkConditions(p2, visited)) pixels.Push(p2);
                if(!checkConditions(p3, visited)) pixels.Push(p3);
                if(!checkConditions(p4, visited)) pixels.Push(p4);
            }

        }

        if (descriptors.Area > 0)
        {
            descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X / descriptors.Area, descriptors.Centroid.Y / descriptors.Area);
        }
        pixels.Clear();
        return descriptors;
    }

    public bool checkConditions(System.Drawing.Point p, bool[,] visited)
    {
        if (p.X < 0 || p.X >= this.image_width || p.Y < 0 || p.Y >= this.image_height || visited[p.Y, p.X]) { return true; }
        return false;
    }

    public List<Punkt> FinderPatterns()
    {
        ConcurrentBag<Punkt> patterns_horizontal = new ConcurrentBag<Punkt>();
        ConcurrentBag<Punkt> patterns_vertical = new ConcurrentBag<Punkt>();

        Parallel.For(0, this.image_height, y =>
        {
            for(int x = 0; x<this.image_width; x+=2){

                isPattern(x, y, true, patterns_horizontal);
            }
            y += 2;
        });

        //patterns_horizontal = PogrupujPunkty(patterns_horizontal);
        Parallel.ForEach(patterns_horizontal, p => { 
            for(int x = p.X - (int)(p.w / 7.0) ; x <= p.X + (int)(p.w / 7.0) ; x+=4)
            {
                for (int y = p.Y - (int)(4.5 * p.w / 7.0) ; y <= p.Y + (int)(4.5 * p.w / 7.0); y+=4)
                {
                    isPattern(x, y, false, patterns_vertical);
                }
            }
        });

        patterns_vertical = PogrupujPunkty(patterns_vertical);

        ConcurrentBag<Punkt> patterns_final = new ConcurrentBag<Punkt>();

        Parallel.ForEach(patterns_vertical, p => {
         VerifyFinderPatternRegions(p, patterns_final);
        });

        //foreach (Punkt pattern in patterns_vertical)
        //{
          //  VerifyFinderPatternRegions(pattern, patterns_final);
        //}

        patterns_final = PogrupujPunkty(patterns_final);

        return new List<Punkt>(UsunPowturzenia(patterns_final));
    }

    public void VerifyFinderPatternRegions(Punkt p ,ConcurrentBag<Punkt> patterns_final)
    {
        RegionDescriptors area = FloodFill(p, true);
        double tmp = (p.w * 2.5 / 7.0) * (p.w * 2.5 / 7.0);
        if (area.Area == 0 || area.Area < tmp) { return ; }

        int pixel_value = this.img.Data[p.Y, p.X, 0];
        int y_start = p.Y;
        double distance = (p.w / 7.0) * 3.0;
        while ((p.Y > 0) && (pixel_value == 0) && (Math.Abs(p.Y-y_start)) < distance)
        {
            p.Y = p.Y - 1;
            pixel_value = this.img.Data[p.Y, p.X, 0];
        }

        if(Math.Abs(p.Y - y_start) > distance){ 
            return; 
        }

        RegionDescriptors area2 = FloodFill(p, false);
        if ((area2.Area == 0) || (area2.Area < tmp)) { return; }

        if(Math.Abs(area.Centroid.Y - area2.Centroid.Y) >= 3) { return; }
        if(Math.Abs(area.Centroid.X - area2.Centroid.X) >= 3) { return; }
   
        Punkt Pf = new Punkt(area.Centroid.X, area.Centroid.Y, ((area.BoundingBox.Height - area.BoundingBox.Y)/3.0)*7.0);
        patterns_final.Add(Pf);

    }

    public void isPattern(int Start_X,int Start_y,bool horizontal,ConcurrentBag<Punkt> patterns)
    {
        if (Start_X < 0 || Start_X >= this.image_width || Start_y < 0 || Start_y >= this.image_height) return;

        int initial_color = this.img.Data[Start_y, Start_X, 0];
        if (initial_color == 255) { return; }

        int[] pattern = new int[5];
        int length = 0;
        int x = Start_X;
        int y = Start_y;

        for (int i = 0; (i<5) && (x <this.image_width) && (y < this.image_height); i++)
        {
            length = 0;
            while (x < this.image_width && y < this.image_height)
            {
                int currentColor = this.img.Data[y, x, 0];
                if (initial_color !=currentColor) break;
                length++;
                if (i == 1 && Math.Abs(pattern[i - 1] - length) > pattern[i - 1]*1.5) { return; }
                if (horizontal) { 
                    x++; 
                }else { 
                    y++; 
                }
            }
            pattern[i] = length;
            initial_color = initial_color == 0 ? 255 : 0;
        }

        double w = (pattern[0] + pattern[1] + pattern[2] + pattern[3] + pattern[4]) / 7.0;

        double tolerance_small1 = w - 3.7;
        double tolerance_small2 = w + 3.7;

        if(pattern[2] <= (3 * w - 2.5) || pattern[2] >= (3 * w + 2.5)) { return; }

        if(pattern[0] < tolerance_small1 || pattern[0] > tolerance_small2) { return; }
        if(pattern[1] < tolerance_small1 || pattern[1] > tolerance_small2) { return; }
        if(pattern[3] < tolerance_small1 || pattern[3] > tolerance_small2) { return; }
        if(pattern[4] < tolerance_small1 || pattern[4] > tolerance_small2) { return; }

        if (pattern[2] >= (pattern[0] + pattern[1]) && pattern[2] >= (pattern[3] + pattern[4]))
        {
            int centroid_X = horizontal ? (x + Start_X - 1) / 2 : x;
            int centroid_Y = horizontal ? y : (y + Start_y - 1) / 2;

            if(this.img.Data[centroid_Y, centroid_X, 0] == 255) { return; }

            double w2 = pattern[0] + pattern[1] + pattern[2] + pattern[3] + pattern[4];

            patterns.Add(new Punkt(centroid_X, centroid_Y, w2));
        }

        return;
    }

    static ConcurrentBag<Punkt> PogrupujPunkty(ConcurrentBag<Punkt> Vpunkty)
    {
        List<Punkt> punkty = new List<Punkt>(Vpunkty);
        int punktyCount = punkty.Count;

        ConcurrentBag<List<Punkt>> grupy = new ConcurrentBag<List<Punkt>>();

        ConcurrentDictionary<int, bool> odwiedzone = new ConcurrentDictionary<int, bool>();

        Parallel.For(0, punktyCount, i =>
        {
            if (odwiedzone.TryAdd(i, true))
            {
                List<Punkt> grupa = new List<Punkt> { punkty[i] };

                for (int j = i + 1; j < punktyCount; j++)
                {
                    if (!odwiedzone.ContainsKey(j))
                    {
                        int deltaX = Math.Abs(punkty[i].X - punkty[j].X);
                        double deltaY = Math.Abs(punkty[i].Y - punkty[j].Y);
                        double thresholdY = (3 * punkty[i].w) / 7.0;

                        if (deltaX < 3 && deltaY <= thresholdY)
                        {
                            grupa.Add(punkty[j]);
                            odwiedzone.TryAdd(j, true);
                        }
                    }
                }

                grupy.Add(grupa);
            }
        });

        ConcurrentBag<Punkt> nowePunkty = new ConcurrentBag<Punkt>();

        Parallel.ForEach(grupy, grupa =>
        {
            int count = grupa.Count;

            int sumaX = 0, sumaY = 0; 
            double sumaW = 0;
            foreach (var punkt in grupa)
            {
                sumaX += punkt.X;
                sumaY += punkt.Y;
                sumaW += punkt.w;
            }

            int sredniaX = (int)Math.Round((double)sumaX / count);
            int sredniaY = (int)Math.Round((double)sumaY / count);
            int sredniaW = (int)Math.Round((double)sumaW / count);

            nowePunkty.Add(new Punkt(sredniaX, sredniaY, sredniaW));
        });

        return nowePunkty;
    }

    public ConcurrentBag<Punkt> UsunPowturzenia(ConcurrentBag<Punkt> Vpunkty)
    {
        ConcurrentBag<Punkt> punkty = new ConcurrentBag<Punkt>();

        foreach (Punkt t in Vpunkty) { 
            if(!punkty.Contains(t)) {
                punkty.Add(t); 
            }
        }

        return punkty;
    }


}