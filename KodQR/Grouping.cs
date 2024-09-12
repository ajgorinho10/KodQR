using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FindPatterns;
using static QRCodeReader;

namespace KodQR
{
    public static class Grouping
    {
        public static List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>> FindQRCodeCandidates(Bitmap binary,List<RegionDescriptors> finderPatternCandidates, double minQRCodeSize, double maxQRCodeSize, double sizeTolerance, double hypotenuseTolerance)
        {
            var qrCodeCandidates = new List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>>();

            for (int i = 0; i < finderPatternCandidates.Count; i++)
            {
                for (int j = i+1; j < finderPatternCandidates.Count; j++)
                {
                    for (int k = j+1; k < finderPatternCandidates.Count; k++)
                    {
                        double bok1 = ObliczOdleglosc(finderPatternCandidates[i].Centroid, finderPatternCandidates[j].Centroid);
                        double bok2 = ObliczOdleglosc(finderPatternCandidates[i].Centroid, finderPatternCandidates[k].Centroid);
                        double bok3 = ObliczOdleglosc(finderPatternCandidates[j].Centroid, finderPatternCandidates[k].Centroid);
                        //Console.WriteLine($"i-j:{bok1} i-k:{bok2} j-k:{bok3}");
                        //Console.WriteLine($" i:{finderPatternCandidates[i].Centroid} j:{finderPatternCandidates[j].Centroid} k:{finderPatternCandidates[k].Centroid}");
                        
                            if(bok1 > bok2 && bok1 > bok3)
                            {   
                                double hypotenuse = Math.Sqrt(Math.Pow(bok3, 2) + Math.Pow(bok2, 2));
                                minQRCodeSize = ((finderPatternCandidates[k].BoundingBox.Width - finderPatternCandidates[k].BoundingBox.X) / 7.0) * 15.0;
                                bool isQRCode = (bok1 >= minQRCodeSize) && (bok1 <= maxQRCodeSize) && (Math.Abs(hypotenuse - bok1) <= hypotenuseTolerance) && (Math.Abs(bok3 - bok2) <= sizeTolerance);
                                if (isQRCode) qrCodeCandidates.Add(new Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>(finderPatternCandidates[j], finderPatternCandidates[k], finderPatternCandidates[i]));
                            }
                            else if(bok2 > bok1 && bok2 > bok3)
                            {
                                double hypotenuse = Math.Sqrt(Math.Pow(bok3, 2) + Math.Pow(bok1, 2));
                                minQRCodeSize = ((finderPatternCandidates[j].BoundingBox.Width - finderPatternCandidates[j].BoundingBox.X) / 7.0) * 15.0;
                                bool isQRCode = (bok2 >= minQRCodeSize) && (bok2 <= maxQRCodeSize) && (Math.Abs(hypotenuse - bok2) <= hypotenuseTolerance) && (Math.Abs(bok3 - bok1) <= sizeTolerance);
                                if (isQRCode) qrCodeCandidates.Add(new Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>(finderPatternCandidates[i], finderPatternCandidates[j], finderPatternCandidates[k]));
                            }
                            else if(bok3 > bok1 && bok3 > bok2)
                            {
                                double hypotenuse = Math.Sqrt(Math.Pow(bok2, 2) + Math.Pow(bok1, 2));
                                minQRCodeSize = ((finderPatternCandidates[i].BoundingBox.Width - finderPatternCandidates[i].BoundingBox.X) / 7.0) * 15.0;
                                bool isQRCode = (bok3 >= minQRCodeSize) && (bok3 <= maxQRCodeSize) && (Math.Abs(hypotenuse - bok3) <= hypotenuseTolerance) && (Math.Abs(bok2 - bok1) <= sizeTolerance);
                                if(isQRCode) qrCodeCandidates.Add(new Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>(finderPatternCandidates[j], finderPatternCandidates[i], finderPatternCandidates[k]));
                            }
                       

                    }
                }
            }

            return qrCodeCandidates;
        }

        public static double ObliczOdleglosc(Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

    }
}
