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
        public Image<Bgr, Byte> img;

        public barDetection(Image<Bgr, byte> img)
        {
            this.img = img;
        }

        public void detectBAR()
        {
            FindBar fBar = new FindBar(this.img.Convert<Gray,Byte>(),this.img);
            List<Image<Gray, Byte>> barImages = fBar.find();

            //CvInvoke.Imshow("orginalBar", this.img);
            
            int i = 1;
            foreach(var img in barImages)
            {
                //CvInvoke.Imshow($"img: {i}", img);
                projection p = new projection(img);
                p.Image_projection();

                if (p.barInTab!=null)
                {
                    CvInvoke.Imshow($"img1: {i}", img);
                    CvInvoke.Imshow($"img2: {i}", p.imBar);
                    Decoding dec = new Decoding(p.barInTab,p.imBar,p.y_f);
                    dec.decode();
                    CvInvoke.WaitKey(0);
                }
                i++;
               
            }
            
        }
    }
}
