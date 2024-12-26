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

        public List<String> detectBAR()
        {
            int x = img.Width;
            int y = img.Height;
            if(this.img.Width > 2000 || this.img.Height > 2000)
            {
                x /= 2;
                y /= 2;
                CvInvoke.Resize(this.img, this.img, new Size(x, y));
                //Console.WriteLine("ok");
            }

            
            //CvInvoke.GaussianBlur(this.img, this.img, new Size(1, 1), 2.0);

            FindBar fBar = new FindBar(this.img.Convert<Gray,Byte>(),this.img);
            List<Image<Gray, Byte>> barImages = fBar.find();

            CvInvoke.Resize(fBar.img_codes, fBar.img_codes, new Size(800, 600));
            //CvInvoke.Imshow("orginalBar", fBar.img_codes);
            //CvInvoke.WaitKey(0);
            int i = 1;
            List<String> msg = new List<String>();
            foreach(var img in barImages)
            {
                //CvInvoke.Imshow($"img: {i}", img);
                //CvInvoke.WaitKey(0);
                projection p = new projection(img);
                p.Image_projection();

                if (p.barInTab != null)
                {
                    //CvInvoke.Imshow($"img1: {i}", img);
                    //CvInvoke.Imshow($"img2: {i}", p.imBar);
                    Decoding dec = new Decoding(p.barInTab, p.imBar, p.y_f);
                    String odp = dec.decode();
                    if(odp != "Brak")
                    {
                        msg.Add(odp);
                    }
                    CvInvoke.WaitKey(0);
                }
                i++;
               
            }

            return msg;
        }
    }
}
