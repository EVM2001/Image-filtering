using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Diplom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            GaussFilterCreation(GKernel);
        }
        double[,] GKernel = new double[5, 5];
        string FilePath;
        string Filename;
        int width, height;//ширина и высота        
        Bitmap bitmap;//элемент класса для работы с пикселями
        Bitmap grayNoise;

        //double[,] mask = { { 0.1, 0.1, 0.1 }, { 0.1, 0.2, 0.1 }, { 0.1, 0.1, 0.1 } };//сглаживающий фильтр

        //int[,] mask = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };//контрастоповышающий фильтр
        //int[,] mask = { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };//контрастоповышающий фильтр

        int[,] mask = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };//фильтр Лапласа
        //int[,] mask = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };

        //int[,] mask = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };//Фильтр Превитта


        //реализация кнопки "загрузить"
        private void GetAndShowFile(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //pictureBox1.Image = null;//убрать старое изображение ???

                    FilePath = openFileDialog1.FileName;//задание пути к файлу
                    Filename = openFileDialog1.SafeFileName;//задание имени файла
                    Image NewImage = Image.FromFile(FilePath);//создание объекта типа "изображение"
                    width = NewImage.Width;//получение ширины
                    height = NewImage.Height;//и высоты


                    bitmap = new Bitmap(NewImage);//заполнение элемента класса для работы с пикселями(оригинальное изображение)

                    //Bitmap gray = CreateGray(bitmap);
                    //grayNoise = CreateNoise(CreateGray(bitmap));//перевод оригинального изображения в серый цвет и добавление шума
                    grayNoise = CreateNoise(bitmap);

                    pictureBox1.Image = grayNoise;//визуализация изображения
                    pictureBox2.Image = bitmap;

                    //pictureBox1.Image = bitmap;

                    txtFileName.Text = Filename;//вывод имени файла
                    txtImageSize.Text = width.ToString() + " X " + height.ToString();//вывод размера файла

                    //textBox1.Text = mask[0,1].ToString();

                    //GaussFormula(-2, -2);
                    
                    //button2.Enabled = true;//разрешение на использование фильтра
                }
                catch
                {
                    MessageBox.Show("Выберите файл-изображение");
                    //button2.Enabled = false;
                }
            }
        }
        private void Filter1(object sender, EventArgs e)
        {
            Bitmap FilteredImage = new Bitmap(width, height);

            /*for (int w = 0; w < width; w++)
            {
                FilteredImage.SetPixel(w, 0, bitmap.GetPixel(w, 0));
                FilteredImage.SetPixel(w, height - 1, bitmap.GetPixel(w, height - 1));
            }            
            for (int h = 0; h < height; h++)
            {
                FilteredImage.SetPixel(0, h, bitmap.GetPixel(0, h));
                FilteredImage.SetPixel(width - 1, h, bitmap.GetPixel(width - 1, h));
            }*/



            for (int h = 1; h < height - 1; h++)
            {
                for (int w = 1; w < width - 1; w++)
                {
                    int color = (int)ForFilter1(w, h);
                    //FilteredImage.SetPixel(w, h, Color.FromArgb(ForFilter1(w, h)));
                    FilteredImage.SetPixel(w, h, Color.FromArgb(color, color, color));
                }
            }
            grayNoise = FilteredImage;
            //bitmap = FilteredImage;
            pictureBox1.Image = FilteredImage;
        }
        private double ForFilter1(int w, int h)
        {
            double newcolor = 0;
            

            for (int i = h - 1; i < h + 2; i++)
            {
                for (int j = w - 1; j < w + 2; j++)
                {
                    //newcolor += mask[i - (h - 1), j - (w - 1)] * grayNoise.GetPixel(j, i).ToArgb();
                    int test = grayNoise.GetPixel(j, i).R;
                    double test2 = mask[i - (h - 1), j - (w - 1)];
                    newcolor += mask[i - (h - 1), j - (w - 1)] * grayNoise.GetPixel(j, i).R;
                }
            }

            /*newcolor = mask[0, 0] * bitmap.GetPixel(w - 1, h - 1).ToArgb() + mask[0, 1] * bitmap.GetPixel(w, h - 1).ToArgb() + mask[0, 2] * bitmap.GetPixel(w + 1, h - 1).ToArgb()
                + mask[1, 0] * bitmap.GetPixel(w - 1, h).ToArgb() + mask[1, 1] * bitmap.GetPixel(w, h).ToArgb() + mask[1, 2] * bitmap.GetPixel(w + 1, h).ToArgb()
                + mask[2, 0] * bitmap.GetPixel(w - 1, h + 1).ToArgb() + mask[2, 1] * bitmap.GetPixel(w, h + 1).ToArgb() + mask[2, 2] * bitmap.GetPixel(w + 1, h + 1).ToArgb();*/


            return newcolor;
        }

        private void Median(object sender, EventArgs e)
        {
            grayNoise = MedianFilter(grayNoise, 3);
            pictureBox1.Image = grayNoise;
        }

        private Bitmap MedianFilter(Bitmap sourceBitmap, int matrixSize, bool grayscale = false)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            /*if (grayscale == true)
            {
                float rgb = 0;
                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }*/
            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;
            int byteOffset = 0;
            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;

            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;
                    neighbourPixels.Clear();
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            neighbourPixels.Add(BitConverter.ToInt32(pixelBuffer, calcOffset));
                        }
                    }
                    neighbourPixels.Sort();
                    middlePixel = BitConverter.GetBytes(neighbourPixels[filterOffset]);
                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        private Bitmap CreateGray(Bitmap bitmap)
        {
            // создаём Bitmap для черно-белого изображения
            Bitmap output = new Bitmap(bitmap.Width, bitmap.Height);
            // перебираем в циклах все пиксели исходного изображения
            for (int j = 0; j < bitmap.Height; j++)
                for (int i = 0; i < bitmap.Width; i++)
                {
                    // получаем (i, j) пиксель
                    UInt32 pixel = (UInt32)(bitmap.GetPixel(i, j).ToArgb());
                    // получаем компоненты цветов пикселя
                    float R = (float)((pixel & 0x00FF0000) >> 16); // красный
                    float G = (float)((pixel & 0x0000FF00) >> 8); // зеленый
                    float B = (float)(pixel & 0x000000FF); // синий
                                                           // делаем цвет черно-белым (оттенки серого) - находим среднее арифметическое
                    R = G = B = (R + G + B) / 3.0f;
                    // собираем новый пиксель по частям (по каналам)
                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);
                    // добавляем его в Bitmap нового изображения
                    output.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            // выводим черно-белый Bitmap в pictureBox2
            return output;

        }      

        private Bitmap CreateNoise(Bitmap bitmap)
        {
            Random rnd = new Random();
            Bitmap output = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);

            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    int q = rnd.Next(100);
                    if (q <= 40) output.SetPixel(x, y, Color.White);
                }
            }
            return output;
        }

        private void SaveImage(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("E:\\мой ИНСТИТУТ\\4 курс\\диплом\\изображения\\TestNoiseImage2.jpg");
        }

        private void GaussFilter(object sender, EventArgs e)
        {
            Bitmap GaussFilteredImage = new Bitmap(width, height);

            for (int h = 2; h < height - 2; h++)
            {
                for (int w = 2; w < width - 2; w++)
                {
                    int test = (int)ForGaussFilter(w, h);
                    //Color test2 = Color.FromArgb((int)test, (int)test, (int)test);
                    GaussFilteredImage.SetPixel(w, h, Color.FromArgb(test, test, test));                   
                }
            }
            grayNoise = GaussFilteredImage;
            //bitmap = GaussFilteredImage;
            pictureBox1.Image = GaussFilteredImage;
        }

        private double ForGaussFilter(int w, int h)
        {
            double newcolor = 0;


            /*for (int i = h - 3; i < h + 4; i++)
            {
                for (int j = w - 3; j < w + 4; j++)
                {
                    newcolor += (int)(GaussFormula(j - (w - 3) - 3, i - (h - 3) - 3) * grayNoise.GetPixel(j, i).ToArgb());
                }
            }*/

            for (int i = h - 2; i < h + 3; i++)
            {
                for (int j = w - 2; j < w + 3; j++)
                {
                    //newcolor += GaussFormula(j - (w - 2) - 2, i - (h - 2) - 2) * grayNoise.GetPixel(j, i).R;

                    //Color test = grayNoise.GetPixel(j, i);
                    //int test2 = grayNoise.GetPixel(j, i).ToArgb();
                    //double pixcolor = 0.21 * grayNoise.GetPixel(j, i).R + 0.72 * grayNoise.GetPixel(j, i).G + 0.07 * grayNoise.GetPixel(j, i).B;


                    //newcolor += GKernel[i - (h - 2), j - (w - 2)] * pixcolor;


                    newcolor += GKernel[i - (h - 2), j - (w - 2)] * grayNoise.GetPixel(j, i).R;
                }
            }

            return newcolor;
        }

        private double GaussFormula(int x,int y)
        {
            double sig = 0.84089642;
            //double sig = 1;
            double formula = 1 / (2 * Math.PI * sig * sig) * Math.Exp(-(x * x + y * y) / (2 * sig * sig));
            //textBox1.Text = formula.ToString();
            return formula;
        }

        void GaussFilterCreation(double [,]GKernel)
        {
            // initialising standard deviation to 1.0
            double sigma = 1.0;
            double r, s = 2.0 * sigma * sigma;

            // sum is for normalization
            double sum = 0.0;

            // generating 5x5 kernel
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    r = Math.Sqrt(x * x + y * y);
                    GKernel[x + 2,y + 2] = (Math.Exp(-(r * r) / s)) / (Math.PI * s);
                    sum += GKernel[x + 2,y + 2];
                }
            }

            // normalising the Kernel
            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j)
                    GKernel[i,j] /= sum;
        }


    }
}
