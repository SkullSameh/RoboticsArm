using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection.Emit;

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;


        private Image<Gray, Byte> R_frame;
        private Image<Gray, Byte> G_frame;
        private Image<Gray, Byte> B_frame;
        private Image<Gray, Byte> GrayImg;


        private Image<Gray, Byte> R_IMG_Seg;
        private Image<Gray, Byte> B_IMG_Seg;
        private Image<Gray, Byte> B_IMG_Co;
        private Image<Gray, Byte> R_IMG_Co;


        static SerialPort _serialPort;
        public byte[] Buff = new byte[2];



        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)


        public Form1()
        {
            InitializeComponent();
            
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM9";
            _serialPort.BaudRate = 9600;
            _serialPort.Open();
            
        }

        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)
            {
                try
                {
                    capture = new Capture();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();


            R_frame = IMG[2].Copy();
            G_frame = IMG[1].Copy();
            B_frame = IMG[0].Copy();
            GrayImg = IMG.Convert<Gray, Byte>();

            R_IMG_Seg = IMG.Convert<Gray, Byte>();
            R_IMG_Co = IMG.Convert<Gray, Byte>();
            B_IMG_Seg = IMG.Convert<Gray, Byte>();
            B_IMG_Co = IMG.Convert<Gray, Byte>();


            int R_th, B_th, R_co, B_co;
            int.TryParse(textBox1.Text, out R_th);
            int.TryParse(textBox2.Text, out B_th);
            int.TryParse(textBox3.Text, out R_co);
            int.TryParse(textBox4.Text, out B_co);

            for (int i = 0; i < GrayImg.Width; i++)
            {
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    if ((R_frame[j, i].Intensity >= R_th) && (B_frame[j, i].Intensity + G_frame[j, i].Intensity) < R_th)
                    {
                        R_IMG_Seg.Data[j, i, 0] = 255;

                    }
                    else
                    {
                        R_IMG_Seg.Data[j, i, 0] = 0;
                    }
                }
            }

            for (int i = 0; i < GrayImg.Width; i++)
            {
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    if ((R_frame[j, i].Intensity < B_th) && (G_frame[j, i].Intensity < B_th) && (B_frame[j, i].Intensity < B_th))
                    {
                        B_IMG_Seg.Data[j, i, 0] = 255;

                    }
                    else
                    {
                        B_IMG_Seg.Data[j, i, 0] = 0;
                    }
                }
            }





            R_IMG_Co = R_IMG_Seg;
            for (int count = 0; count < R_co; count++)
            {
                for (int i = 1; i < GrayImg.Width - 1; i++)
                {
                    for (int j = 1; j < GrayImg.Height - 1; j++)
                    {
                        if (R_IMG_Seg[j, i].Intensity != 0)
                        {
                            if (R_IMG_Seg[j, i - 1].Intensity == 0 || R_IMG_Seg[j, i + 1].Intensity == 0 ||
                                R_IMG_Seg[j - 1, i - 1].Intensity == 0 || R_IMG_Seg[j - 1, i].Intensity == 0 || R_IMG_Seg[j - 1, i + 1].Intensity == 0 ||
                                R_IMG_Seg[j + 1, i - 1].Intensity == 0 || R_IMG_Seg[j + 1, i + 1].Intensity == 0 || R_IMG_Seg[j + 1, i].Intensity == 0)
                            {
                                R_IMG_Co.Data[j, i, 0] = 0;
                            }
                            else
                            {
                                R_IMG_Co.Data[j, i, 0] = 255;
                            }
                        }
                        else
                        {
                            R_IMG_Co.Data[j, i, 0] = 0;
                        }
                    }
                }

                R_IMG_Co.CopyTo(R_IMG_Seg);
            }


            
            B_IMG_Co = B_IMG_Seg;
            for (int count = 0; count < B_co; count++)
            {
                for (int i = 1; i < GrayImg.Width - 1; i++)
                {
                    for (int j = 1; j < GrayImg.Height - 1; j++)
                    {
                        if (B_IMG_Seg[j, i].Intensity != 0)
                        {
                            if (B_IMG_Seg[j, i - 1].Intensity == 0 || B_IMG_Seg[j, i + 1].Intensity == 0 ||
                                B_IMG_Seg[j - 1, i - 1].Intensity == 0 || B_IMG_Seg[j - 1, i].Intensity == 0 || B_IMG_Seg[j - 1, i + 1].Intensity == 0 ||
                                B_IMG_Seg[j + 1, i - 1].Intensity == 0 || B_IMG_Seg[j + 1, i + 1].Intensity == 0 || B_IMG_Seg[j + 1, i].Intensity == 0)
                            {
                                B_IMG_Co.Data[j, i, 0] = 0;
                            }
                            else
                            {
                                B_IMG_Co.Data[j, i, 0] = 255;
                            }
                        }
                        else
                        {
                            B_IMG_Co.Data[j, i, 0] = 0;
                        }
                    }
                }

                B_IMG_Co.CopyTo(B_IMG_Seg);
            }

            try
            {

                imageBox1.Image = IMG;
                imageBox2.Image = R_IMG_Co; 
                imageBox3.Image = B_IMG_Co;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }





        }

        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

        private void button1_Click(object sender, EventArgs e)
        {
            // Application.Idle += processFrame;
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
        }
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Idle -= processFrame;
            button1.Enabled = true;
            button2.Enabled = false;

        }
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" + ".jpg");
        }


        private void button6_Click(object sender, EventArgs e)
        {
          // Shot();
           CalThetaBLACK();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            processFrame(sender, e);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Buff[0] = 45; //Th1 
            Buff[1] = 45; //Th2
            _serialPort.Write(Buff, 0, 2);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Buff[0] = 99;//Th1
            Buff[1] = 90;//Th2
            _serialPort.Write(Buff, 0, 2);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)


        private int N, Xpx, Ypx;
        private double Xcm, Ycm;

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {

        }

        private double Scale;


        private double Th1, Th2;

        private void button7_Click(object sender, EventArgs e)
        {
            Proje();
        }

        void CalThetaBLACK()
        {
            if(radioButton1.Checked == true)
            {
                 Scale = 70.0 / imageBox3.Width;
                Xpx = 0;
                Ypx = 0;
                N = 0;

                for (int i = 0; i < B_IMG_Co.Width; i++)
                    for (int j = 0; j < B_IMG_Co.Height; j++)
                    {
                        if (B_IMG_Co[j, i].Intensity > 128)
                        {
                            N++;
                            Xpx += i;
                            Ypx += j;

                        }
                    }

                if (N > 0)
                {
                    Xpx = Xpx / N;
                    Ypx = Ypx / N;


                    textBox10.Text = Xpx.ToString();
                    textBox11.Text = Ypx.ToString();

                    if (radioButton4.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 3.6) - Ypx) * Scale;
                    }
                    else if (radioButton3.Checked == true)
                    {
                        Xcm = ((Xpx - B_IMG_Co.Width / 1.0) * Scale);
                        Ycm = (((B_IMG_Co.Height / 2.0) - Ypx) * Scale);
                    }
                    else if (radioButton6.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 2.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 3.6) - Ypx) * Scale;
                    }
                    else if (radioButton5.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 2.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.0) - Ypx) * Scale;
                    }

                    else if (radioButton7.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.5) * Scale;
                        Ycm = ((B_IMG_Co.Height / 3.6) - Ypx) * Scale;
                    }
                    else if (radioButton8.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.5) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.0) - Ypx) * Scale;
                    }
                    else if (radioButton9.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.5) - Ypx) * Scale;
                    }
                    else if (radioButton10.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 2.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.5) - Ypx) * Scale;
                    }







                    int d1 = 20;
                    Ycm = Ycm * -1.4;

                    double Px = 100 * -1;
                    double Py = Xcm;
                    double Pz = -1 * (Ycm - 45);

                    textBox8.Text = Px.ToString();
                    textBox9.Text = Py.ToString();
                    textBox12.Text = Pz.ToString();

                    double Th1 = Math.Atan(Py / Px);
                    double Th2 = Math.Atan(((Pz - d1) / Py) * Math.Sin(Th1));

                    Th1 = Th1 * (180 / Math.PI);
                    Th2 = Th2 * (180 / Math.PI);

                    if (radioButton10.Checked == true || radioButton5.Checked == true || radioButton6.Checked == true)
                    {
                        if (Py < 0)
                        {
                            Th1 = ((int)(Th1) + 103);
                            Th2 = ((int)Th2 + 99);
                        }
                        else
                        {
                            Th1 = ((int)(Th1) + 99);
                            Th2 = ((int)Th2 + 98);
                        }

                    }
                    else
                    {
                        Th1 = ((int)(Th1) + 90);
                        Th2 = ((int)Th2 + 90);
                    }

                    textBox14.Text = Th1.ToString();
                    textBox13.Text = Th2.ToString();

                    Buff[0] = (byte)Th1; //Th1 
                    Buff[1] = (byte)Th2; //Th2

                    _serialPort.Write(Buff, 0, 2);

                }
            
        }
            else if(radioButton2.Checked == true)
            {
               // Scale = 70.0 / imageBox3.Width;
                Xpx = 0;
                Ypx = 0;
                N = 0;

                for (int i = 0; i < B_IMG_Co.Width; i++)
                    for (int j = 0; j < B_IMG_Co.Height; j++)
                    {
                        if (B_IMG_Co[j, i].Intensity > 128)
                        {
                            N++;
                            Xpx += i;
                            Ypx += j;

                        }
                    }

                if (N > 0)
                {
                    Xpx = Xpx / N;
                    Ypx = Ypx / N;


                    textBox10.Text = Xpx.ToString();
                    textBox11.Text = Ypx.ToString();

                    if (radioButton4.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 3.6) - Ypx) * Scale;
                    }
                    else if (radioButton3.Checked == true)
                    {
                        Xcm = ((Xpx - B_IMG_Co.Width / 1.0) * Scale);
                        Ycm = (((B_IMG_Co.Height / 2.0) - Ypx) * Scale);
                    }
                    else if (radioButton6.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 2.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 3.6) - Ypx) * Scale;
                    }
                    else if (radioButton5.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 2.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.0) - Ypx) * Scale;
                    }
                    
                    else if (radioButton7.Checked == true) 
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.5) * Scale;
                        Ycm = ((B_IMG_Co.Height / 3.6) - Ypx) * Scale;
                    }
                    else if(radioButton8.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.5) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.0) - Ypx) * Scale;
                    }
                    else if(radioButton9.Checked == true )
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 1.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.5) - Ypx) * Scale;
                    }
                    else if (radioButton10.Checked == true)
                    {
                        Xcm = (Xpx - B_IMG_Co.Width / 2.0) * Scale;
                        Ycm = ((B_IMG_Co.Height / 2.5) - Ypx) * Scale;
                    }
                    






                    int d1 = 20;
                    Ycm = Ycm * -1.4;

                    double Px = 100 * -1;
                    double Py = Xcm;
                    double Pz = -1 * (Ycm - 45);

                    textBox8.Text = Px.ToString();
                    textBox9.Text = Py.ToString();
                    textBox12.Text = Pz.ToString();

                    double Th1 = Math.Atan(Py / Px);
                    double Th2 = Math.Atan(((Pz - d1) / Py) * Math.Sin(Th1));

                    Th1 = Th1 * (180 / Math.PI);
                    Th2 = Th2 * (180 / Math.PI);

                    if(radioButton10.Checked == true || radioButton5.Checked == true || radioButton6.Checked == true )
                    {
                        if (Py < 0)
                        {
                            Th1 = ((int)(Th1) + 103);
                            Th2 = ((int)Th2 + 99);
                        }
                        else
                        {
                            Th1 = ((int)(Th1) + 99);
                            Th2 = ((int)Th2 + 98);
                        }

                    }
                    else
                    {
                        Th1 = ((int)(Th1) + 90);
                        Th2 = ((int)Th2 + 90);
                    }

                    textBox14.Text = Th1.ToString();
                    textBox13.Text = Th2.ToString();

                    Buff[0] = (byte)Th1; //Th1 
                    Buff[1] = (byte)Th2; //Th2

                    _serialPort.Write(Buff, 0, 2);

                }
            }
            
        }



        void Proje()
        {
            double[] proj = new double[GrayImg.Width];
            for (int i = 0; i < GrayImg.Width; i++)
            {
                double column = 0;
                for (int j = 0; j < GrayImg.Height; j++)
                {
                    proj[i] = column = column + ((R_IMG_Co[j, i].Intensity) / 255);
                }

            }


            int k = 0;
            double sum = 0;

            while (k < proj.Length && proj[k] == 0) { k++; }
            k += 5;
            int start = k;
            for (int i = 0; i < 2; i++)
            {
                while (k < proj.Length && proj[k] != 0) k++;
                k += 5;

                while (k < (GrayImg.Width - 5) && proj[k] == 0) k++;
                k += 5;
                int end = k;
                sum = sum + (end - start);
                start = end;
            }

            double Avg = sum / 2.0;
            Scale = 20.0 / Avg;

            textBox5.Text = IMG.Width.ToString();
            textBox6.Text = IMG.Height.ToString();
            textBox7.Text = Avg.ToString();
        }


        double Zcm;
        void Shot()
        {
            Xpx = 0;
            Ypx = 0;
            N = 0;

            for (int i = 0; i < R_IMG_Co.Width; i++)
                for (int j = 0; j < R_IMG_Co.Height; j++)
                {
                    if (R_IMG_Co[j, i].Intensity > 128)
                    {
                        N++;
                        Xpx += i;
                        Ypx += j;

                    }
                }

            if (N > 0)
            {
                Xpx = Xpx / N;
                Ypx = Ypx / N;

                textBox10.Text = Xpx.ToString();
                textBox11.Text = Ypx.ToString();

                Xcm = 100;
                 Ycm = (((B_IMG_Co.Height / 2.0) - Ypx) * Scale);
                 Zcm = (-1 * ((Xpx - B_IMG_Co.Width / 2.0) * Scale)) +20;


                textBox8.Text = Xcm.ToString();
                textBox9.Text = Ycm.ToString();
                textBox12.Text = Zcm.ToString();

                //invesrev
                /*
                double Th1 = Math.Atan(Ycm / Xcm);
                double Th2 = Math.Atan(((Zcm) / Ycm) * Math.Sin(Th1));

                Th1 = Th1 * (180 / Math.PI);
                Th2 = Th2 * (180 / Math.PI);


                Th1 = (90 - (int)Th1);
                Th2 = (90 - (int)Th2);

                textBox14.Text = Th1.ToString();
                textBox13.Text = Th2.ToString();


                Buff[0] = (byte)Th1; //Th1 
                Buff[1] = (byte)Th2; //Th2

                _serialPort.Write(Buff, 0, 2);
                */

                //invesrev

                int d1 = 20;
                Ycm = Ycm * -1.4;
                // Xcm = Xcm * 1.4;
                //  textBox5.Text = Xcm.ToString();
                // textBox6.Text = Ycm.ToString();
                // textBox7.Text = N.ToString();

                double rPx = 100 * -1;
                double rPy = (Xcm);
                double rPz = -1 * (Ycm - 45);

                // textBox10.Text = rPy.ToString();
                // textBox11.Text = rPz.ToString();

                double Th1 = Math.Atan(rPy / rPx);
                double Th2 = Math.Atan(((rPz - d1) / rPy) * Math.Sin(Th1));

                Th1 = Th1 * (180 / Math.PI);
                Th2 = Th2 * (180 / Math.PI);


                Th1 = (65 - (int)Th1);
                Th2 = (70 - (int)Th2);


                textBox14.Text = Th1.ToString();
                textBox13.Text = Th2.ToString();


                //  double ErrorTH2 = rPz - Th2;
                //  textBox10.Text = ErrorTH2.ToString();


                Buff[0] = (byte)Th1; //Th1 
                Buff[1] = (byte)Th2; //Th2




                _serialPort.Write(Buff, 0, 2);
            }
                
        }
    }
}
    

