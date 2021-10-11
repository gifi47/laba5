using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2021_10_02_Лаба_номер_5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap map;
        Vector3[] vertices;
        int[] indeces;
        Vector3 Center;
        Vector3 Translation;
        Mat3 Bias;
        int planeWidth;
        int planeHeight;
        float planeZ;
        float scale;
        float ang;

        float angle_tick = 0.1f;
        float selected_angle = 0.1f;

        Model monke;
        DateTime lastCheck;
        long frameCount;

        private void Render()
        {
            Vector3[] pVertices = new Vector3[vertices.Length];
            int[,] zBuffer = new int[planeWidth, planeHeight];
            for (int i = 0; i < vertices.Length; i++)
            {
                pVertices[i] = Bias * (vertices[i] - Center) + Center + Translation;
                float vZ = pVertices[i].z;
                pVertices[i] = new Vector3(pVertices[i].x * planeZ / vZ * scale, pVertices[i].y * planeZ / vZ * scale, vZ);
                pVertices[i].x += planeWidth / 2;
                pVertices[i].y = (-pVertices[i].y) + planeHeight / 2;
            }
            
            unsafe
            {
                BitmapData bData = map.LockBits(new Rectangle(0, 0, planeWidth, planeHeight), ImageLockMode.ReadWrite, map.PixelFormat);
                byte* scan0 = (byte*)bData.Scan0.ToPointer();
                int bitsPerPixel = Image.GetPixelFormatSize(map.PixelFormat) / 8;
                for (int i = 0; i < indeces.Length; i += 3)
                {
                    FillTriangle(scan0, bData.Stride, bitsPerPixel, pVertices[indeces[i]], pVertices[indeces[i + 1]], pVertices[indeces[i + 2]]);
                }
                for (int i = 0; i < indeces.Length; i += 3)
                {
                    DrawLine(scan0, bData.Stride, bitsPerPixel, pVertices[indeces[i]], pVertices[indeces[i + 1]]);
                    DrawLine(scan0, bData.Stride, bitsPerPixel, pVertices[indeces[i + 1]], pVertices[indeces[i + 2]]);
                    DrawLine(scan0, bData.Stride, bitsPerPixel, pVertices[indeces[i + 2]], pVertices[indeces[i]]);
                }
                map.UnlockBits(bData);
            }
            /*for (int i = 0; i < indeces.Length; i += 3)
            {
                try
                {
                    FillTriangle(pVertices[indeces[i]], pVertices[indeces[i + 1]], pVertices[indeces[i + 2]], Color.Gold);
                }
                catch (Exception)
                {
                    map.Save(@"D:\Visual Studio Projects\Мтуси программирование\2021_10_02 Лаба номер 5\2021_10_02 Лаба номер 5\errors\img" + DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".bmp");
                }
            }
            for (int i = 0; i < indeces.Length; i+=3)
            {
                try
                {
                    DrawLine(pVertices[indeces[i]], pVertices[indeces[i + 1]], Color.Red);
                    DrawLine(pVertices[indeces[i + 1]], pVertices[indeces[i + 2]], Color.Red);
                    DrawLine(pVertices[indeces[i + 2]], pVertices[indeces[i]], Color.Red);
                }
                catch (Exception)
                {
                    map.Save(@"D:\Visual Studio Projects\Мтуси программирование\2021_10_02 Лаба номер 5\2021_10_02 Лаба номер 5\errors\img" + System.DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".bmp");
                }
            }*/
            /*for (int i = 0; i < indeces.Length - 1; i++)
            {
                DrawLine(pVertices[indeces[i]].toPointF(), pVertices[indeces[i + 1]].toPointF(), Color.Red);
            }*/
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            monke = new Model(@"D:\Visual Studio Projects\Мтуси программирование\2021_10_02 Лаба номер 5\2021_10_02 Лаба номер 5\makaka.obj", true);
            //monke = new Model(@"D:\Visual Studio Projects\Мтуси программирование\2021_10_02 Лаба номер 5\monke.obj", true);
            //monke = new Model(@"D:\Visual Studio Projects\Мтуси программирование\2021_10_02 Лаба номер 5\2021_10_02 Лаба номер 5\physic_final_ver.obj", true);
            //monke = new Model(@"D:\Visual Studio Projects\Мтуси программирование\2021_10_02 Лаба номер 5\2021_10_02 Лаба номер 5\pistol.obj", true);
            planeZ = 1;
            //Translation = new Vector3(0, -0.5f, 4);
            Translation = new Vector3(0, 0, 2);
            angle_tick = 0.01f;
            ang = (float)Math.PI / 2;
            planeHeight = pictureBox1.Height;
            planeWidth = pictureBox1.Width;
            scale = planeWidth / (2 * (float)Math.Tan(ang / 2) * planeZ);
            Console.WriteLine($"scale:{ scale }");
            Bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            Center = new Vector3(0, 0, 15);
            vertices = new Vector3[]{ 
                new Vector3(10, 10, 5), new Vector3(10, -10, 5), new Vector3(-10, -10, 5), new Vector3(-10, 10, 5),
                new Vector3(10, 10, 25), new Vector3(10, -10, 25), new Vector3(-10, -10, 25), new Vector3(-10, 10, 25)
            };
            Center = new Vector3(0, 0, 2);
            vertices = new Vector3[]{
                new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, 1),
                new Vector3(1, 1, 3), new Vector3(1, -1, 3), new Vector3(-1, -1, 3), new Vector3(-1, 1, 3)
            };
            Center = new Vector3(0, 0, 1.5f);
            vertices = new Vector3[]{
                new Vector3(0.5f, 0.5f, 1), new Vector3(0.5f, -0.5f, 1f), new Vector3(-0.5f, -0.5f, 1f), new Vector3(-0.5f, 0.5f, 1f),
                new Vector3(0.5f, 0.5f, 2), new Vector3(0.5f, -0.5f, 2), new Vector3(-0.5f, -0.5f, 2), new Vector3(-0.5f, 0.5f, 2)
            };

            //vertices = new Vector3[]{ new Vector3(0, 0, 5), new Vector3(10, 0, 5), new Vector3(10, 10, 5), new Vector3(0, 10, 5) };
            indeces = new int[] { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4, 7, 3, 2, 2, 6, 7, 0, 4, 5, 5, 1, 0, 7, 4, 0, 0, 3, 7, 2, 1, 5, 5, 6, 2 };
            //indeces = new int[] { 7, 3, 2 };
            //indeces = new int[] { 7, 3, 2, 2, 6, 7, 0, 4, 5, 5, 1, 0, 7, 4, 0, 0, 3, 7, 2, 1, 5, 5, 6, 2 };
            //indeces = new int[] { 7, 4, 0, 0, 3, 7, 2, 1, 5, 5, 6, 2 };

            Center = new Vector3(monke.right - (monke.right - monke.left) / 2, monke.top - (monke.top - monke.bottom) / 2, monke.front - (monke.front - monke.back) / 2);
            Console.WriteLine($"Center: ({ Center.x }, { Center.y }, { Center.z })");
            vertices = monke.vertices;
            indeces = monke.indeces;
            map = new Bitmap(planeWidth, planeWidth);
            pictureBox1.Image = map;
            lastCheck = DateTime.Now;
            frameCount = 0;
            Cycle();
        }

        double GetFps()
        {
            double secondsElapsed = (DateTime.Now - lastCheck).TotalSeconds;
            long count = System.Threading.Interlocked.Exchange(ref frameCount, 0);
            double fps = count / secondsElapsed;
            lastCheck = DateTime.Now;
            return fps;
        }

        private unsafe void DrawLine(byte* scan0, int stride, int bitsPerPixel, PointF p1, PointF p2)
        {
            int start, end;
            if (Math.Abs(p1.X - p2.X) > Math.Abs(p1.Y - p2.Y))
            {
                start = (int)Math.Floor(p1.X); end = (int)Math.Floor(p2.X);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                    PointF tem = p1;
                    p1 = p2;
                    p2 = tem;
                }
                float d = (p2.Y - p1.Y) / (p2.X - p1.X);
                float y = p1.Y;
                end = Math.Min(planeWidth - 1, Math.Max(1, end));
                start = Math.Max(1, Math.Min(planeWidth - 1, start));
                for (int x = start; x <= end; x++)
                {
                    if (0 <= y && y < planeHeight)
                    {
                        byte* data = scan0 + ((int)Math.Floor(y)) * stride + x * bitsPerPixel;
                        data[0] = 20;
                        data[1] = 200;
                        data[2] = 20;
                        data[3] = 255;
                    }
                    y += d;
                }
            }
            else
            {
                start = (int)Math.Floor(p1.Y); end = (int)Math.Floor(p2.Y);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                    PointF tem = p1;
                    p1 = p2;
                    p2 = tem;
                }
                float d = (p2.X - p1.X) / (p2.Y - p1.Y);
                float x = p1.X;
                end = Math.Min(planeHeight - 1, Math.Max(1, end));
                start = Math.Max(1, Math.Min(planeHeight - 1, start));
                for (int y = start; y <= end; y++)
                {
                    if (x >= 0 && x < planeWidth)
                    {
                        byte* data = scan0 + y * stride + ((int)Math.Floor(x)) * bitsPerPixel;
                        data[0] = 20;
                        data[1] = 200;
                        data[2] = 20;
                        data[3] = 255;
                    }
                    x += d;
                }
            }
        }

        private void DrawLine(PointF p1, PointF p2, Color c)
        {
            int start, end;
            if (Math.Abs(p1.X - p2.X) > Math.Abs(p1.Y - p2.Y))
            {
                start = (int)Math.Floor(p1.X); end = (int)Math.Floor(p2.X);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                    PointF tem = p1;
                    p1 = p2;
                    p2 = tem;
                }
                float d = (p2.Y - p1.Y) / (p2.X - p1.X);
                float y = p1.Y;
                end = Math.Min(planeWidth - 1, Math.Max(1, end));
                start = Math.Max(1, Math.Min(planeWidth - 1, start));
                for (int x = start; x <= end; x++)
                {
                    if (0 < y && y < planeHeight)
                    {
                        map.SetPixel(x, (int)Math.Floor(y), c);
                    }
                    y += d;
                }
            } 
            else
            {
                start = (int)Math.Floor(p1.Y); end = (int)Math.Floor(p2.Y);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                    PointF tem = p1;
                    p1 = p2;
                    p2 = tem;
                }
                float d = (p2.X - p1.X) / (p2.Y - p1.Y);
                float x = p1.X;
                end = Math.Min(planeHeight - 1, Math.Max(1, end));
                start = Math.Max(1, Math.Min(planeHeight - 1, start));
                for (int y = start; y <= end; y++)
                {
                    if (x > 0 && x < planeWidth)
                    {
                        map.SetPixel((int)Math.Floor(x), y, c);
                    }
                    x += d;
                }
            }
        }

        private PointF getMax(PointF[] points)
        {
            PointF max = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].X > max.X || (points[i].X == max.X && points[i].Y < max.Y))
                {
                    max = points[i];
                }
            }
            return max;
        }
        private PointF getMin(PointF[] points)
        {
            PointF min = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].X < min.X || (points[i].X == min.X && points[i].Y > min.Y))
                {
                    min = points[i];
                }
            }
            return min;
        }
        private void SortPoints(ref PointF left, ref PointF mid, ref PointF right)
        {
            PointF temp;
            if (left.X > mid.X)
            {
                temp = left;
                left = mid;
                mid = temp;
            }
            if (right.X < mid.X)
            {
                temp = right;
                right = mid;
                mid = temp;
            }
            if (left.X > mid.X)
            {
                temp = left;
                left = mid;
                mid = temp;
            }
        }

        private unsafe void FillTriangle(byte* scan0, int stride, int bitsPerPixel, PointF p1, PointF p2, PointF p3)
        {
            PointF left = p1;
            PointF mid = p2;
            PointF right = p3;
            SortPoints(ref left, ref mid, ref right);

            int leftXI = (int)Math.Round(left.X);
            int midXI = (int)Math.Round(mid.X);
            int rightXI = (int)Math.Round(right.X);
            float dUpLtY = (Math.Abs(mid.X - left.X) >= 1 ? (mid.Y - left.Y) / (mid.X - left.X) : 0);
            float dUpRtY = (Math.Abs(mid.X - right.X) >= 1 ? (mid.Y - right.Y) / (mid.X - right.X) : 0);
            float dDownY = (Math.Abs(right.X - left.X) >= 1 ? (right.Y - left.Y) / (right.X - left.X) : 0);

            float upY = left.Y;
            float downY = left.Y;
            float downMdY = left.Y + dDownY * (mid.X - left.X);
            bool negative = downMdY > mid.Y;

            if (negative)
            {
                float temp = dDownY;
                dDownY = dUpLtY;
                dUpLtY = temp;
            }

            for (int x = leftXI; x < midXI; x++)
            {
                int mxY = (int)Math.Round(upY);
                for (int y = (int)Math.Round(downY); y <= mxY; y++)
                {
                    if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                    {
                        byte* data = scan0 + y * stride + x * bitsPerPixel;
                        data[0] = 200;
                        data[1] = 20;
                        data[2] = 20;
                        data[3] = 255;
                    }
                }
                downY += dDownY;
                upY += dUpLtY;
            }
            upY = mid.Y;
            downY = downMdY;
            if (negative)
            {
                dDownY = dUpRtY;
                dUpRtY = dUpLtY;
                upY = downMdY;
                downY = mid.Y;
            }
            for (int x = midXI; x < rightXI; x++)
            {
                int mxY = (int)Math.Round(upY);
                for (int y = (int)Math.Round(downY); y <= mxY; y++)
                {
                    if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                    {
                        byte* data = scan0 + y * stride + x * bitsPerPixel;
                        data[0] = 200;
                        data[1] = 20;
                        data[2] = 20;
                        data[3] = 255;
                    }
                }
                downY += dDownY;
                upY += dUpRtY;
            }
        }

        private void FillTriangle(PointF p1, PointF p2, PointF p3, Color c)
        {
            //FillPolygon(new SolidBrush(Color.Blue), new PointF[]{ p1, p2, p3 });
            PointF left = p1;//getMin(new PointF[]{ p1, p2, p3 });
            PointF right = p3;//getMax(new PointF[]{ p1, p2, p3 });
            PointF mid = p2;//(p1 == left || p1 == right ? (p2 == left || p2 == right ? p3 : p2) : p1);
            SortPoints(ref left, ref mid, ref right);

            int leftXI = (int)Math.Round(left.X);
            int midXI = (int)Math.Round(mid.X);
            int rightXI = (int)Math.Round(right.X);
            float dUpLtY = (Math.Abs(mid.X - left.X) >= 1 ? (mid.Y - left.Y) / (mid.X - left.X) : 0); 
            //float dUpLtY = (Math.Abs(midXI - leftXI) >= 1 ? (mid.Y - left.Y) / (midXI - leftXI) : 0);
            float dUpRtY = (Math.Abs(mid.X - right.X) >= 1 ? (mid.Y - right.Y) / (mid.X - right.X) : 0);
            //float dUpRtY = (Math.Abs(midXI - rightXI) >= 1 ? (mid.Y - right.Y) / (midXI - rightXI) : 0);
            float dDownY = (Math.Abs(right.X - left.X) >= 1 ? (right.Y - left.Y) / (right.X - left.X) : 0);
            //float dDownY = (Math.Abs(rightXI - leftXI) >= 1 ? (right.Y - left.Y) / (rightXI - leftXI) : 0);

            float upY = left.Y;
            float downY = left.Y;
            float downMdY = left.Y + dDownY * (mid.X - left.X);
            bool negative = downMdY > mid.Y;

            if (negative)
            {
                float temp = dDownY;
                dDownY = dUpLtY;
                dUpLtY = temp;
            }

            for (int x = leftXI; x < midXI; x++)
            {
                int mxY = (int)Math.Round(upY);
                for (int y = (int)Math.Round(downY); y <= mxY; y++)
                {
                    if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                    {
                        map.SetPixel(x, y, c);
                    }
                }
                downY += dDownY;
                upY += dUpLtY;
            }
            upY = mid.Y;
            downY = downMdY;
            if (negative)
            {
                dDownY = dUpRtY;
                dUpRtY = dUpLtY;
                upY = downMdY;
                downY = mid.Y;
            }
            for (int x = midXI; x < rightXI; x++)
            {
                int mxY = (int)Math.Round(upY);
                for (int y = (int)Math.Round(downY); y <= mxY; y++)
                {
                    if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                    {
                        map.SetPixel(x, y, c);
                    }
                }
                downY += dDownY;
                upY += dUpRtY;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //DrawLine(new PointF(0, 0), new PointF(100, 20), Color.Red);
            //e.Graphics.FillRectangle(new SolidBrush(Color.Cyan), new Rectangle(0, 0, pictureBox1.ClientRectangle.Width, pictureBox1.ClientRectangle.Height));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //angle_tick += 0.01f;
            RotateBias(angle_tick, true, true, true);
            Cycle();
        }

        private void Cycle()
        {
            System.Threading.Interlocked.Increment(ref frameCount);
            map = new Bitmap(planeWidth, planeHeight);
            Render();
            pictureBox1.Image = map;
            label1.Text = GetFps().ToString() + " fps";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //map = new Bitmap(planeWidth, planeHeight);
            //Render();
            //DrawLine(new PointF(20, 20), new PointF(300, 300), Color.Red);
            //DrawLine(new PointF(300, 300), new PointF(30, 30), Color.Red);
            //DrawLine(new PointF(200, 200), new PointF(30, 200), Color.Red);
            //pictureBox1.Image = map;
            timer1.Enabled = !timer1.Enabled;
        }

        void RotateBias(float angle, bool x, bool y, bool z)
        {
            Bias.x.Rotate(angle, x, y, z);
            Bias.y.Rotate(angle, x, y, z);
            Bias.z.Rotate(angle, x, y, z);
            /*Console.WriteLine(Bias.x.x.ToString() + " " + Bias.x.y.ToString() + " " + Bias.x.z.ToString());
            Console.WriteLine(Bias.y.x.ToString() + " " + Bias.y.y.ToString() + " " + Bias.y.z.ToString());
            Console.WriteLine(Bias.z.x.ToString() + " " + Bias.z.y.ToString() + " " + Bias.z.z.ToString());*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //FillTriangle(new PointF(10, 10), new PointF(200, 10), new PointF(95, 140));
            //RotateBias((float)Math.PI / 8, false, true, false);
            map = new Bitmap(planeWidth, planeHeight);
            /*FillTriangle(new PointF(30, 200), new PointF(100, 150), new PointF(170, 200));
            FillTriangle(new PointF(30, 30), new PointF(100, 150), new PointF(170, 30));
            FillTriangle(new PointF(1, 100), new PointF(1, 200), new PointF(100, 150));
            FillTriangle(new PointF(101, 150), new PointF(200, 200), new PointF(200, 100));*/

            FillTriangle(new PointF(74.25013f, 224.219818f), new PointF(112.684692f, 113.438416f), new PointF(75.7498856f, 74.27984f), Color.DarkMagenta);
            //Render();
            pictureBox1.Image = map;
        }

        private void buttonQ_Click(object sender, EventArgs e)
        {
            RotateBias(-selected_angle, false, false, true);
            Cycle();
        }

        private void buttonE_Click(object sender, EventArgs e)
        {
            RotateBias(selected_angle, false, false, true);
            Cycle();
        }

        private void buttonW_Click(object sender, EventArgs e)
        {
            RotateBias(-selected_angle, true, false, false);
            Cycle();
        }

        private void buttonS_Click(object sender, EventArgs e)
        {
            RotateBias(selected_angle, true, false, false);
            Cycle();
        }

        private void buttonA_Click(object sender, EventArgs e)
        {
            RotateBias(selected_angle, false, true, false);
            Cycle();
        }

        private void buttonD_Click(object sender, EventArgs e)
        {
            RotateBias(-selected_angle, false, true, false);
            Cycle();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                selected_angle = float.Parse(comboBox1.Text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {

            } 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                angle_tick = float.Parse(comboBox2.Text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {

            }
        }
    }

    struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void Rotate(float angle, bool x, bool y, bool z)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            if (x)
            {
                float new_y = this.y * cos - this.z * sin;
                float new_z = this.y * sin + this.z * cos;
                this.y = new_y;
                this.z = new_z;
            }
            if (y)
            {
                float new_x = this.x * cos - this.z * sin;
                float new_z = this.x * sin + this.z * cos;
                this.x = new_x;
                this.z = new_z;
            }
            if (z)
            {
                float new_x = this.x * cos - this.y * sin;
                float new_y = this.x * sin + this.y * cos;
                this.x = new_x;
                this.y = new_y;
            }
        }
        public float Value()
        {
            return x + y + z;
        }
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static Vector3 operator *(Vector3 v1, float value)
        {
            return new Vector3(v1.x * value, v1.y * value, v1.z * value);
        }
        public static implicit operator PointF(Vector3 v1)
        {
            return new PointF(v1.x, v1.y);
        }
    }

    struct Mat3
    {
        public Vector3 x, y, z;
        public Mat3(Vector3 x, Vector3 y, Vector3 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void Transpose()
        {
            Vector3 x = new Vector3(this.x.x, this.y.x, this.z.x);
            Vector3 y = new Vector3(this.x.y, this.y.y, this.z.y);
            Vector3 z = new Vector3(this.x.z, this.y.z, this.z.z);
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static Vector3 operator *(Mat3 m1, Vector3 v1)
        {
            return new Vector3(m1.x.x * v1.x + m1.x.y * v1.y + m1.x.z * v1.z, m1.y.x * v1.x + m1.y.y * v1.y + m1.y.z * v1.z, m1.z.x * v1.x + m1.z.y * v1.y + m1.z.z * v1.z);
        }
        public static Vector3 operator *(Vector3 v1, Mat3 m1)
        {
            return new Vector3(v1.x * m1.x.x + v1.y * m1.y.x + v1.z * m1.z.x, v1.x * m1.x.y + v1.y * m1.y.y + v1.z * m1.z.z, v1.x * m1.x.z + v1.y * m1.y.z + v1.z * m1.z.z);
        }
        public static implicit operator Mat3(Vector3[] v1)
        {
            return new Mat3(v1[0], v1[1], v1[2]);
        }
    }

    class Model
    {
        public Vector3[] vertices;
        public int[] indeces;
        public float top;
        public float bottom;
        public float right;
        public float left;
        public float front;
        public float back;
        public Model(string filename, bool findBounds = false)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indeces = new List<int>();
            foreach (string line in System.IO.File.ReadLines(filename))
            {
                string[] words = line.Split(new char[] { ' ' });
                switch (words[0])
                {
                    case "v":
                        vertices.Add(new Vector3(
                            float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture), 
                            float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture)));
                        break;
                    case "f":
                        for (int i = 1; i < 4; i++)
                        {
                            string[] inds = words[i].Split(new char[] { '/' });
                            indeces.Add(int.Parse(inds[0]) - 1);
                            //indeces.Add(int.Parse(inds[1]) - 1);
                            //indeces.Add(int.Parse(inds[2]) - 1);
                        }
                        break;
                            
                    default:
                        continue;                   
                }
            }
            this.vertices = vertices.ToArray();
            this.indeces = indeces.ToArray();
            Console.WriteLine($"vertices loaded: {this.vertices.Length}");
            Console.WriteLine($"tris: {this.indeces.Length / 3}");
            if (findBounds)
            {
                foreach(Vector3 vertex in this.vertices)
                {
                    if (vertex.x > this.right) this.right = vertex.x;
                    if (vertex.x < this.left) this.left = vertex.x;
                    if (vertex.y > this.top) this.top = vertex.y;
                    if (vertex.y < this.bottom) this.bottom = vertex.y;
                    if (vertex.z > this.front) this.front = vertex.z;
                    if (vertex.z < this.back) this.back = vertex.z;
                }
                Console.WriteLine($"right =  {this.right}");
                Console.WriteLine($"left =  {this.left}");
                Console.WriteLine($"top =  {this.top}");
                Console.WriteLine($"bottom =  {this.bottom}");
                Console.WriteLine($"front =  {this.front}");
                Console.WriteLine($"back =  {this.back}");

            }
        }
    }
}
