using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;
using KodQR.qr;

namespace KodQR.bar
{
    public class barDetection
    {
        public Image<Gray, Byte> img;

        public barDetection(Image<Gray, byte> img)
        {
            this.img = img;
        }

        public void detectBAR()
        {
            FindBar fBar = new FindBar(this.img);
            fBar.find();
        }
    }
}
