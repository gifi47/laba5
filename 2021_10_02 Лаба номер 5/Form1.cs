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
        public static string filePath = @"C:\Visual Studio Projects 2019-2022\Мтуси программирование\2021_10_02 Лаба номер 5\";

        public Form1()
        {
            InitializeComponent();
        }

        Bitmap map;
        Vertex[] vertices;
        Vector3 Center;
        Vector3 Translation;
        Mat3 Bias;
        int planeWidth;
        int planeHeight;
        float planeZ;
        float scale;
        float ang;
        Bitmap texture;

        float angle_tick = 0.1f;
        float selected_angle = 0.1f;

        Model model;
        DateTime lastCheck;
        long frameCount;

        float[,] zBuffer;

        private void Render(bool a = false)
        {
            Vertex[] pVertices = new Vertex[vertices.Length];
            Array.Copy(vertices, pVertices, vertices.Length);
            zBuffer = new float[planeWidth, planeHeight];
            for (int i = 0; i < vertices.Length; i++)
            {
                pVertices[i].position = Bias * (vertices[i].position - Center) + Center + Translation;     
                float vZ = pVertices[i].position.z;
                pVertices[i].position = new Vector3(pVertices[i].position.x * planeZ / vZ * scale, pVertices[i].position.y * planeZ / vZ * scale, vZ);
                pVertices[i].position.x += planeWidth / 2;
                pVertices[i].position.y = (-pVertices[i].position.y) + planeHeight / 2;
                pVertices[i].texCoord.x *= texture.Width;
                pVertices[i].texCoord.y *= texture.Height;
            }
            unsafe
            {
                BitmapData mapData = map.LockBits(new Rectangle(0, 0, planeWidth, planeHeight), ImageLockMode.ReadWrite, map.PixelFormat);
                BitmapData textureData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Width), ImageLockMode.ReadOnly, texture.PixelFormat);
                byte* scan0 = (byte*)mapData.Scan0.ToPointer();
                int bitsPerPixel = Image.GetPixelFormatSize(map.PixelFormat) / 8;

                byte* textureScan0 = (byte*)textureData.Scan0.ToPointer();
                int textureBitsPerPixel = Image.GetPixelFormatSize(texture.PixelFormat) / 8;
                if (a)
                {
                    for (int i = 0; i < vertices.Length; i += 3)
                    {
                        DrawLine(scan0, mapData.Stride, bitsPerPixel, ref zBuffer, pVertices[i].position, pVertices[i + 1].position);
                        DrawLine(scan0, mapData.Stride, bitsPerPixel, ref zBuffer, pVertices[i + 1].position, pVertices[i + 2].position);
                        DrawLine(scan0, mapData.Stride, bitsPerPixel, ref zBuffer, pVertices[i + 2].position, pVertices[i].position);
                    }
                }
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    FillTriangle(scan0, mapData.Stride, bitsPerPixel, textureScan0, textureData.Stride, textureBitsPerPixel, ref zBuffer, ref pVertices[i], ref pVertices[i + 1], ref pVertices[i + 2]);
                }
                map.UnlockBits(mapData);
                texture.UnlockBits(textureData);
            }
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            //texture = new Bitmap(filePath + @"Cube_t_uv_2.png");
            texture = new Bitmap(filePath + @"msm_texture_2.bmp");
            model = new Model(filePath + @"cube_t_uv.obj", true);
            planeZ = 1;
            Translation = new Vector3(0, 0, 3);
            angle_tick = 0.01f;
            ang = (float)Math.PI / 2;
            planeHeight = pictureBox1.Height;
            planeWidth = pictureBox1.Width;
            scale = planeWidth / (2 * (float)Math.Tan(ang / 2) * planeZ);
            Bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };

            Center = new Vector3(model.right - (model.right - model.left) / 2, model.top - (model.top - model.bottom) / 2, model.front - (model.front - model.back) / 2);
            vertices = model.vertices;
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

        private unsafe void DrawLine(byte* scan0, int stride, int bitsPerPixel, ref float[,] zBuffer, Vector3 p1, Vector3 p2)
        {
            int start, end;
            if (Math.Abs(p1.x - p2.x) > Math.Abs(p1.y - p2.y))
            {
                start = (int)Math.Floor(p1.x); end = (int)Math.Floor(p2.x);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                    Vector3 tem = p1;
                    p1 = p2;
                    p2 = tem;
                }
                Vector3 d = (p2 - p1) / (p2.x - p1.x);
                float y = p1.y;
                float z = p1.z;
                end = Math.Min(planeWidth - 1, Math.Max(1, end));
                start = Math.Max(1, Math.Min(planeWidth - 1, start));
                for (int x = start; x <= end; x++)
                {
                    int yI = (int)Math.Floor(y);
                    if (0 <= y && y < planeHeight)
                    {
                        if (zBuffer[x, yI] == 0 || zBuffer[x, yI] > z)
                        {
                            byte* data = scan0 + yI * stride + x * bitsPerPixel;
                            data[0] = 20;
                            data[1] = 200;
                            data[2] = 20;
                            data[3] = 255;
                            zBuffer[x, yI] = z;
                        }
                    }
                    y += d.y;
                    z += d.z;
                }
            }
            else
            {
                start = (int)Math.Floor(p1.y); end = (int)Math.Floor(p2.y);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                    Vector3 tem = p1;
                    p1 = p2;
                    p2 = tem;
                }
                Vector3 d = (p2 - p1) / (p2.y - p1.y);
                float x = p1.x;
                float z = p1.z;
                end = Math.Min(planeHeight - 1, Math.Max(1, end));
                start = Math.Max(1, Math.Min(planeHeight - 1, start));
                for (int y = start; y <= end; y++)
                {
                    int xI = (int)Math.Floor(x);
                    if (x >= 0 && x < planeWidth)
                    {
                        if (zBuffer[xI, y] == 0 || zBuffer[xI, y] > z)
                        {
                            byte* data = scan0 + y * stride + xI * bitsPerPixel;
                            data[0] = 20;
                            data[1] = 200;
                            data[2] = 20;
                            data[3] = 255;
                            zBuffer[xI, y] = z;
                        }
                    }
                    x += d.x;
                    z += d.z;
                }
            }
        }

        private void SortVertices(ref Vertex left, ref Vertex mid, ref Vertex right)
        {
            Vertex temp;
            if (left.position.x > mid.position.x)
            {
                temp = left;
                left = mid;
                mid = temp;
            }
            if (right.position.x < mid.position.x)
            {
                temp = right;
                right = mid;
                mid = temp;
            }
            if (left.position.x > mid.position.x)
            {
                temp = left;
                left = mid;
                mid = temp;
            }
        }

        private unsafe void FillTriangle(byte* scan0, int stride, int bitsPerPixel, byte* textureScan0, int textureStride, int textureBitsPerPixel, ref float[,] zBuffer, ref Vertex p1, ref Vertex p2, ref Vertex p3)
        {
            Vertex left = p1;
            Vertex mid = p2;
            Vertex right = p3;
            SortVertices(ref left, ref mid, ref right);

            Vector3 left_to_right = new Vector3(0, (right.position.y - left.position.y) / (right.position.x - left.position.x), (right.position.z - left.position.z) / (right.position.x - left.position.x));
            Vector3 left_to_mid = new Vector3(0, (mid.position.y - left.position.y) / (mid.position.x - left.position.x), (mid.position.z - left.position.z) / (mid.position.x - left.position.x));
            Vector3 mid_to_right = new Vector3(0, (right.position.y - mid.position.y) / (right.position.x - mid.position.x), (right.position.z - mid.position.z) / (right.position.x - mid.position.x));

            Vector2 texture_left_to_right = (right.texCoord - left.texCoord) / (right.position.x - left.position.x);
            Vector2 texture_left_to_mid = (mid.texCoord - left.texCoord) / (mid.position.x - left.position.x);
            Vector2 texture_mid_to_right = (right.texCoord - mid.texCoord) / (right.position.x - mid.position.x);
            
            float mid_y = left.position.y + left_to_right.y * (mid.position.x - left.position.x);
            float mid_z = left.position.z + left_to_right.z * (mid.position.x - left.position.x);

            float texture_mid_y = left.texCoord.y + texture_left_to_right.y * (mid.position.x - left.position.x);
            float texture_mid_x = left.texCoord.x + texture_left_to_right.x * (mid.position.x - left.position.x);

            if (mid_y < mid.position.y)
            {
                Vector3 start = new Vector3(0, left.position.y, left.position.z);
                Vector3 end = new Vector3(0, left.position.y, left.position.z);

                Vector2 texture_start = left.texCoord;
                Vector2 texture_end = left.texCoord;
                for (int x = (int)left.position.x; x < (int)mid.position.x; x++)
                {
                    float start_to_end = (end.z - start.z) / (end.y - start.y);
                    float z = start.z;

                    Vector2 texture_start_to_end = (texture_end - texture_start) / (end.y - start.y);
                    Vector2 texture_coord = texture_start;
                    for (int y = (int)start.y; y <= (int)end.y; y++)
                    {
                        if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                        {
                            if (zBuffer[x, y] == 0 || zBuffer[x, y] > z)
                            {
                                int texture_x = Math.Min(Math.Max((int)texture_coord.x, 0), texture.Width - 1);
                                int texture_y = Math.Min(Math.Max((int)texture_coord.y, 0), texture.Height - 1);
                                byte* textureData = textureScan0 + texture_y * textureStride + texture_x * textureBitsPerPixel;
                                byte* data = scan0 + y * stride + x * bitsPerPixel;
                                data[0] = textureData[0];
                                data[1] = textureData[1];
                                data[2] = textureData[2];
                                data[3] = 255;
                                zBuffer[x, y] = z;

                            }
                        }
                        z += start_to_end;
                        texture_coord += texture_start_to_end;
                    }
                    start += left_to_right;
                    end += left_to_mid;

                    texture_start += texture_left_to_right;
                    texture_end += texture_left_to_mid;
                }

                start = new Vector3(0, mid_y, mid_z);
                end = new Vector3(0, mid.position.y, mid.position.z);

                texture_start = new Vector2(texture_mid_x, texture_mid_y);
                texture_end = mid.texCoord;
                for (int x = (int)mid.position.x; x < (int)right.position.x; x++)
                {
                    float start_to_end = (end.z - start.z) / (end.y - start.y);
                    float z = start.z;

                    Vector2 texture_start_to_end = (texture_end - texture_start) / (end.y - start.y);
                    Vector2 texture_coord = texture_start;
                    for (int y = (int)start.y; y <= (int)end.y; y++)
                    {
                        if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                        {
                            if (zBuffer[x, y] == 0 || zBuffer[x, y] > z)
                            {
                                int texture_x = Math.Min(Math.Max((int)texture_coord.x, 0), texture.Width - 1);
                                int texture_y = Math.Min(Math.Max((int)texture_coord.y, 0), texture.Height - 1);
                                byte* textureData = textureScan0 + texture_y * textureStride + texture_x * textureBitsPerPixel;
                                byte* data = scan0 + y * stride + x * bitsPerPixel;
                                data[0] = textureData[0];
                                data[1] = textureData[1];
                                data[2] = textureData[2];
                                data[3] = 255;
                                zBuffer[x, y] = z;
                            }
                        }
                        z += start_to_end;
                        texture_coord += texture_start_to_end;
                    }
                    start += left_to_right;
                    end += mid_to_right;

                    texture_start += texture_left_to_right;
                    texture_end += texture_mid_to_right;
                }
            }
            else
            {
                Vector3 start = new Vector3(0, left.position.y, left.position.z);
                Vector3 end = new Vector3(0, left.position.y, left.position.z);

                Vector2 texture_start = left.texCoord;
                Vector2 texture_end = left.texCoord;
                for (int x = (int)left.position.x; x < (int)mid.position.x; x++)
                {
                    float start_to_end = (end.z - start.z) / (end.y - start.y);
                    float z = start.z;

                    Vector2 texture_start_to_end = (texture_end - texture_start) / (end.y - start.y);
                    Vector2 texture_coord = texture_start;
                    for (int y = (int)start.y; y <= (int)end.y; y++)
                    {
                        if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                        {
                            if (zBuffer[x, y] == 0 || zBuffer[x, y] > z)
                            {
                                int texture_x = Math.Min(Math.Max((int)texture_coord.x, 0), texture.Width - 1);
                                int texture_y = Math.Min(Math.Max((int)texture_coord.y, 0), texture.Height - 1);
                                byte* textureData = textureScan0 + texture_y * textureStride + texture_x * textureBitsPerPixel;
                                byte* data = scan0 + y * stride + x * bitsPerPixel;
                                data[0] = textureData[0];
                                data[1] = textureData[1];
                                data[2] = textureData[2];
                                data[3] = 255;
                                zBuffer[x, y] = z;
                            }
                        }
                        z += start_to_end;
                        texture_coord += texture_start_to_end;
                    }
                    start += left_to_mid;
                    end += left_to_right;

                    texture_start += texture_left_to_mid;
                    texture_end += texture_left_to_right;
                }

                start = new Vector3(0, mid.position.y, mid.position.z);
                end = new Vector3(0, mid_y, mid_z);

                texture_start = mid.texCoord; 
                texture_end = new Vector2(texture_mid_x, texture_mid_y);
                for (int x = (int)mid.position.x; x < (int)right.position.x; x++)
                {
                    float start_to_end = (end.z - start.z) / (end.y - start.y);
                    float z = start.z;

                    Vector2 texture_start_to_end = (texture_end - texture_start) / (end.y - start.y);
                    Vector2 texture_coord = texture_start;
                    for (int y = (int)start.y; y <= (int)end.y; y++)
                    {
                        if (x >= 0 && x < planeWidth && y >= 0 && y < planeHeight)
                        {
                            if (zBuffer[x, y] == 0 || zBuffer[x, y] > z)
                            {
                                int texture_x = Math.Min(Math.Max((int)texture_coord.x, 0), texture.Width - 1);
                                int texture_y = Math.Min(Math.Max((int)texture_coord.y, 0), texture.Height - 1);
                                byte* textureData = textureScan0 + texture_y * textureStride + texture_x * textureBitsPerPixel;
                                byte* data = scan0 + y * stride + x * bitsPerPixel;
                                data[0] = textureData[0];
                                data[1] = textureData[1];
                                data[2] = textureData[2];
                                data[3] = 255;
                                zBuffer[x, y] = z;
                            }
                        }
                        z += start_to_end;
                        texture_coord += texture_start_to_end;
                    }
                    start += mid_to_right;
                    end += left_to_right;

                    texture_start += texture_mid_to_right;
                    texture_end += texture_left_to_right;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RotateBias(angle_tick, true, true, true);
            Cycle();
        }

        private void Cycle()
        {
            System.Threading.Interlocked.Increment(ref frameCount);
            planeHeight = pictureBox1.Height;
            planeWidth = pictureBox1.Width;
            scale = planeWidth / (2 * (float)Math.Tan(ang / 2) * planeZ);
            map = new Bitmap(planeWidth, planeHeight);
            Render();
            pictureBox1.Image = map;
            label1.Text = GetFps().ToString() + " fps";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        void RotateBias(float angle, bool x, bool y, bool z)
        {
            Bias.x.Rotate(angle, x, y, z);
            Bias.y.Rotate(angle, x, y, z);
            Bias.z.Rotate(angle, x, y, z);
        }

        void RotateBias(ref Mat3 bias, float angle, bool x, bool y, bool z)
        {
            bias.x.Rotate(angle, x, y, z);
            bias.y.Rotate(angle, x, y, z);
            bias.z.Rotate(angle, x, y, z);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            Cycle();
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

        const float ANGLE = (float)Math.PI / 6;

        private void button5_Click(object sender, EventArgs e)
        {
            Mat3 bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            RotateBias(ref bias, -ANGLE, false, true, false);
            Bias.x = bias * Bias.x;
            Bias.y = bias * Bias.y;
            Bias.z = bias * Bias.z;
            //RotateBias(-ANGLE, false, true, false);
            Cycle();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Mat3 bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            RotateBias(ref bias, ANGLE, false, true, false);
            Bias.x = bias * Bias.x;
            Bias.y = bias * Bias.y;
            Bias.z = bias * Bias.z;
            //RotateBias(ANGLE, false, true, false);
            Cycle();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Mat3 bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            RotateBias(ref bias, -ANGLE, true, false, false);
            Bias.x = bias * Bias.x;
            Bias.y = bias * Bias.y;
            Bias.z = bias * Bias.z;
            //RotateBias(-ANGLE, true, false, false);
            Cycle();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Mat3 bias = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            RotateBias(ref bias, ANGLE, true, false, false);
            Bias.x = bias * Bias.x;
            Bias.y = bias * Bias.y;
            Bias.z = bias * Bias.z;
            //RotateBias(ANGLE, true, false, false);
            Cycle();
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
        public static Vector3 operator /(Vector3 v1, float value)
        {
            return new Vector3(v1.x / value, v1.y / value, v1.z / value);
        }
        public static implicit operator PointF(Vector3 v1)
        {
            return new PointF(v1.x, v1.y);
        }
    }

    struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }
        public static Vector2 operator *(Vector2 v1, float value)
        {
            return new Vector2(v1.x * value, v1.y * value);
        }
        public static Vector2 operator /(Vector2 v1, float value)
        {
            return new Vector2(v1.x / value, v1.y / value);
        }
    }

    struct Vertex
    {
        public Vector3 position;
        public RGBA color;
        public Vector2 texCoord;
        public Vector3 normal;
        public Vertex(Vector3 position, RGBA color, Vector2 texCoord, Vector3 normal)
        {
            this.position = position;
            this.color = color;
            this.texCoord = texCoord;
            this.normal = normal;
        }
        public Vertex(Vector3 position, Vector2 texCoord, Vector3 normal)
        {
            this.position = position;
            this.color = new RGBA(255, 255, 255);
            this.texCoord = texCoord;
            this.normal = normal;
        }
    }

    struct RGBA
    {
        public byte r, g, b, a;
        public RGBA(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public RGBA(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 255;
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
        public Vertex[] vertices;
        public float top;
        public float bottom;
        public float right;
        public float left;
        public float front;
        public float back;
        public Model(string filename, bool findBounds = false)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector2> textureCoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int[]> indeces = new List<int[]>();
            foreach (string line in System.IO.File.ReadLines(filename))
            {
                string[] words = line.Split(new char[] { ' ' });
                switch (words[0])
                {
                    case "v":
                        positions.Add(new Vector3(
                            float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture), 
                            float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture)));
                        break;
                    case "vt":
                        textureCoords.Add(new Vector2(
                            float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture)));
                        break;
                    case "vn":
                        normals.Add(new Vector3(
                            float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture)));
                        break;
                    case "f":
                        for (int i = 1; i < 4; i++)
                        {
                            string[] inds = words[i].Split(new char[] { '/' });
                            indeces.Add(new int[] { GetIndex(inds[0]), GetIndex(inds[1]), GetIndex(inds[2]) });
                        }
                        break;
                            
                    default:
                        continue;                   
                }
            }
            this.vertices = new Vertex[indeces.Count];
            for (int i = 0; i < this.vertices.Length; i++)
            {
                this.vertices[i] = new Vertex(positions[indeces[i][0]], textureCoords[indeces[i][1]], normals[indeces[i][2]]);
            }
            Console.WriteLine($"vertices loaded: {this.vertices.Length}");
            if (findBounds)
            {
                for (int i = 0; i < this.vertices.Length; i++)
                {
                    if (vertices[i].position.x > this.right) this.right = vertices[i].position.x;
                    if (vertices[i].position.x < this.left) this.left = vertices[i].position.x;
                    if (vertices[i].position.y > this.top) this.top = vertices[i].position.y;
                    if (vertices[i].position.y < this.bottom) this.bottom = vertices[i].position.y;
                    if (vertices[i].position.z > this.front) this.front = vertices[i].position.z;
                    if (vertices[i].position.z < this.back) this.back = vertices[i].position.z;
                }
                Console.WriteLine($"right =  {this.right}");
                Console.WriteLine($"left =  {this.left}");
                Console.WriteLine($"top =  {this.top}");
                Console.WriteLine($"bottom =  {this.bottom}");
                Console.WriteLine($"front =  {this.front}");
                Console.WriteLine($"back =  {this.back}");
            }
        }

        private int GetIndex(string value)
        {
            if (value == "") return 0;
            return int.Parse(value) - 1;
        }
    }
}
