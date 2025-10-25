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
using System.Windows.Forms.DataVisualization.Charting;

namespace Image_filtering
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }
        double[,] GKernel;
        string FilePath;
        string Filename;
        int width, height;//ширина и высота        
        Bitmap bitmap;//элемент класса для работы с пикселями
        Bitmap grayNoise;
        Bitmap infocopy;
        int MaskSize;

        double[,] mask = { { 0.1, 0.1, 0.1 }, { 0.1, 0.2, 0.1 }, { 0.1, 0.1, 0.1 } };//сглаживающий фильтр
        double[,] ContrastMask = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };//контрастоповышающий фильтр
        double[,] LaplasMask = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };//фильтр Лапласа

        //реализация кнопки "загрузить"
        private void GetAndShowFile(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {                    
                    FilePath = openFileDialog1.FileName;//задание пути к файлу
                    Filename = openFileDialog1.SafeFileName;//задание имени файла
                    Image NewImage = Image.FromFile(FilePath);//создание объекта типа "изображение"
                    width = NewImage.Width;//получение ширины
                    height = NewImage.Height;//и высоты

                    bitmap = new Bitmap(NewImage);//заполнение элемента класса для работы с пикселями(оригинальное изображение)
                    Bitmap gray = CreateGray(bitmap);//полутоновое изображение
                    grayNoise = CreateNoise(gray);//зашумленное изображение

                    pictureBox2.Image = grayNoise;//визуализация изображений
                    pictureBox1.Image = gray;
                    
                    infocopy = new Bitmap(grayNoise);//копия зашумленного изображения
                    txtFileName.Text = Filename;//вывод имени файла
                    txtImageSize.Text = width.ToString() + " X " + height.ToString();//вывод размера файла
                    CreateHystograme(bitmap, pictureBox4);//создание гистограмм
                    CreateHystograme(grayNoise, pictureBox5);
                }
                catch
                {
                    MessageBox.Show("Выберите файл-изображение");                    
                }
            }
        }

        //нажатие на кнопку фильтр Лапласа
        private void Laplasbutton_click(object sender, EventArgs e)
        {
            grayNoise = LaplasFilter(grayNoise);
            pictureBox3.Image = grayNoise;
            CreateHystograme(grayNoise, pictureBox6);
        }

        //функция, реализующая фильтр Лапласа
        private Bitmap LaplasFilter(Bitmap Sourcebitmap)
        {
            Bitmap FilteredImage = new Bitmap(width, height);
            double max = 0;
            double min = 0;
            double newmax = 255;
            double newmin = 0;
            int[][] Colors;
            int NormalizedColor;

            Colors = new int[height][];
            for (int i = 0; i < height; i++)
            {
                Colors[i] = new int[width];
            }

            for (int h = 1; h < height - 1; h++)
            {
                for (int w = 1; w < width - 1; w++)
                {
                    int color = (int)ForFilter1(w, h, LaplasMask, 1, Sourcebitmap);
                    if (color < min)
                        min = color;
                    if (color > max)
                        max = color;

                    Colors[h][w] = color;
                }
            }

            for (int h = 1; h < height - 1; h++)
            {
                for (int w = 1; w < width - 1; w++)
                {
                    NormalizedColor = (int)(((Colors[h][w] - min) * ((newmax - newmin) / (max - min))) + newmin);//формула нормализации
                    FilteredImage.SetPixel(w, h, Color.FromArgb(NormalizedColor, NormalizedColor, NormalizedColor));
                }
            }
            return FilteredImage;
        }

        //нажатие на кнопку сглаживающий фильтр
        private void Filter1button_Click(object sender, EventArgs e)
        {
            grayNoise = Filter1(grayNoise, 3);
            pictureBox3.Image = grayNoise;
            CreateHystograme(grayNoise, pictureBox6);
        }

        //функция, реализующая сглаживающий фильтр
        private Bitmap Filter1(Bitmap Sourcebitmap, int MaskSize)
        {
            Bitmap FilteredImage = new Bitmap(width, height);
            int offset = (MaskSize - 1) / 2;

            for (int h = offset; h < height - offset; h++)
            {
                for (int w = offset; w < width - offset; w++)
                {
                    int color = (int)ForFilter1(w, h, mask, offset, Sourcebitmap);
                    FilteredImage.SetPixel(w, h, Color.FromArgb(color, color, color));
                }
            }
            return FilteredImage;
        }
        
        //функция для "подвижного окна"
        private double ForFilter1(int w, int h, double[,] mask, int offset, Bitmap Sourcebitmap)
        {
            double newcolor = 0;

            for (int i = h - offset; i <= h + offset; i++)
            {
                for (int j = w - offset; j <= w + offset; j++)
                {
                    
                    newcolor += mask[i - (h - offset), j - (w - offset)] * Sourcebitmap.GetPixel(j, i).R;
                }
            }
            return newcolor;
        }

        //нажатие на кнопку медианный фильтр
        private void Median(object sender, EventArgs e)
        {
            grayNoise = MedianFilter(grayNoise, MaskSize);
            pictureBox3.Image = grayNoise;
            CreateHystograme(grayNoise, pictureBox6);
        }

        //функция, реализующая медианный фильтр
        private Bitmap MedianFilter(Bitmap sourceBitmap, int matrixSize)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
           
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

        //функция перевода изображения в полутоновое
        private Bitmap CreateGray(Bitmap bitmap)
        {
            // создаём Bitmap для полутонового изображения
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
            // выводим полутоновый Bitmap
            return output;
        }      

        //функция зашумления изображения
        private Bitmap CreateNoise(Bitmap bitmap)
        {
            Random rnd = new Random();
            Bitmap output = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);

            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    int q = rnd.Next(100);
                    if (q <= 5) output.SetPixel(x, y, Color.White);
                }
            }
            return output;
        }

        //функция сохранения результата фильтрации
        private void SaveImage(object sender, EventArgs e)
        {            
            saveFileDialog1.Filter = "JPEG|*.jpg|PNG|*.png";
            saveFileDialog1.FileName = "Безымянный";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {  
                    String FilePath = saveFileDialog1.FileName;//задание пути к файлу
                    if (pictureBox3.Image != null)
                    {
                        pictureBox3.Image.Save(FilePath);
                    }
                    else MessageBox.Show("Ошибка");
                }
                catch
                {
                    MessageBox.Show("Ошибка");
                }
            }
        }

        //нажатие на кнопку фильтр Гаусса
        private void Gaussbutton_Click(object sender, EventArgs e)
        {
            grayNoise = GaussFilter(grayNoise,MaskSize);
            pictureBox3.Image = grayNoise;
            CreateHystograme(grayNoise, pictureBox6);
        }

        //функция, реализующая фильтр Гаусса
        private Bitmap GaussFilter(Bitmap sourceBitmap,int MaskSize)
        {
            Bitmap GaussFilteredImage = new Bitmap(width, height);
            GKernel = new double[MaskSize, MaskSize];
            GaussFilterCreation(GKernel, MaskSize);
            int offset = (MaskSize - 1) / 2;

            for (int h = offset; h < height - offset; h++)
            {
                for (int w = offset; w < width - offset; w++)
                {
                    int test = (int)ForGaussFilter(w, h, offset, sourceBitmap);
                    GaussFilteredImage.SetPixel(w, h, Color.FromArgb(test, test, test));
                }
            }
            return GaussFilteredImage;
        }

        //функция для "подвижного окна" фильтра Гаусса
        private double ForGaussFilter(int w, int h, int offset, Bitmap sourceBitmap)
        {
            double newcolor = 0;

            for (int i = h - offset; i <= h + offset; i++)
            {
                for (int j = w - offset; j <= w + offset; j++)
                {                    
                    newcolor += GKernel[i - (h - offset), j - (w - offset)] * sourceBitmap.GetPixel(j, i).R;
                }
            }
            return newcolor;
        }     

        //создание ядра свертки для фильтра Гаусса
        void GaussFilterCreation(double [,]GKernel, int MaskSize)
        {            
            //double sigma = 1.0;
            double sigma = 0.84089642;
            double r, s = 2.0 * sigma * sigma;
            int offset = (MaskSize - 1) / 2;           
            double sum = 0.0;
            
            for (int x = -offset; x <= offset; x++)
            {
                for (int y = -offset; y <= offset; y++)
                {
                    r = Math.Sqrt(x * x + y * y);
                    GKernel[x + offset, y + offset] = (Math.Exp(-(r * r) / s)) / (Math.PI * s);
                    sum += GKernel[x + offset, y + offset];
                }
            }
            
            for (int i = 0; i < MaskSize; ++i)
                for (int j = 0; j < MaskSize; ++j)
                    GKernel[i,j] /= sum;
        }  

        //функция для создания гистограмм
        void CreateHystograme(Bitmap bitmap, PictureBox picbox)
        {
            int max = 0;
            Bitmap mybitmap = new Bitmap(picbox.Width, picbox.Height);
            Chart Graphic = new Chart();
            Graphic.Width = picbox.Width;
            Graphic.Height = picbox.Height;

            Graphic.ChartAreas.Add(new ChartArea("Hystograme"));
            Graphic.ChartAreas[0].AxisX.Title = "Яркость пиксела";
            Graphic.ChartAreas[0].AxisY.Title = "Число пикселей";

            Graphic.ChartAreas[0].AxisX.Minimum = 0;
            Graphic.ChartAreas[0].AxisX.Maximum = 256;
            Graphic.ChartAreas[0].AxisX.Interval = 32;

            Series Hystograme = Graphic.Series.Add("Hystograme");
            Hystograme.ChartType = SeriesChartType.Column;

            int[] Lights = new int[256];
            int i, j;
            Color color;
            for (i = 0; i < bitmap.Width; i++)//проходим по всему изображению 
            {
                for (j = 0; j < bitmap.Height; j++)
                {
                    color = bitmap.GetPixel(i, j);
                    Lights[color.R]++;//заполняем массив, каждый элемент которого это количество пикселей с яркостью равной индексу массива
                }
            }

            for (i = 0; i < 255; i++)//прорисовка гистограммы
            {
                
                if (Lights[i] > max)
                    max = Lights[i];
            }
            Graphic.ChartAreas[0].AxisY.Maximum = max;
            for (i = 0; i < 256; i++)//прорисовка гистограммы
            {
                if (Lights[i] != 0) 
                    for (j = 0; j <= Lights[i]; j += Lights[i])// отрисовываем столбец за столбцом нашу гистограмму 
                    {
                        Hystograme.Points.AddXY(i, j);
                    }
            }
            Graphic.DrawToBitmap(mybitmap, new Rectangle(0,0, picbox.Width, picbox.Height));
            picbox.Image = mybitmap;
        }

        //выпадающее меню с выбором размеров маски фильтра
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    MaskSize = 3;
                    break;
                case 1:
                    MaskSize = 5;
                    break;
                case 2:
                    MaskSize = 7;
                    break;
            }
        }

        //нажатие на кнопку контрастоповышающего фильтра
        private void Contrastbutton_click(object sender, EventArgs e)
        {
            grayNoise = ContrastFilter(grayNoise);
            pictureBox3.Image = grayNoise;
            CreateHystograme(grayNoise, pictureBox6);
        }

        //функция, реализующая контрастоповышающий фильтр
        private Bitmap ContrastFilter(Bitmap Sourcebitmap)
        {
            Bitmap FilteredImage = new Bitmap(width, height);
            double max = 0;
            double min = 1000;
            double newmax = 255;
            double k;
            int[][] Colors;
            int NormalizedColor;

            Colors = new int[height][];
            for (int i = 0; i < height; i++)
            {
                Colors[i] = new int[width];
            }

            for (int h = 1; h < height - 1; h++)
            {
                for (int w = 1; w < width - 1; w++)
                {
                    int color = (int)ForFilter1(w, h, ContrastMask, 1, Sourcebitmap);
                    if (color < min)
                        min = color;
                    if (color > max)
                        max = color;

                    Colors[h][w] = color;
                }
            }
            k = newmax / (max - min);
            for (int h = 1; h < height - 1; h++)
            {
                for (int w = 1; w < width - 1; w++)
                {
                    NormalizedColor = (int)((Colors[h][w] - min) * k);//формула нормализации
                    FilteredImage.SetPixel(w, h, Color.FromArgb(NormalizedColor, NormalizedColor, NormalizedColor));
                }
            }
            return FilteredImage;
        }

        //нажатие на кнопку информативность
        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap smooth = Filter1(infocopy, 3);
            Bitmap median = MedianFilter(infocopy, MaskSize);
            Bitmap gauss = GaussFilter(infocopy, MaskSize);
            Bitmap laplas = LaplasFilter(infocopy);
            Bitmap contrast = ContrastFilter(infocopy);
            Form2 f = new Form2(pictureBox1.Image, pictureBox2.Image, pictureBox3.Image, smooth, median, gauss, laplas, contrast);
            f.Show();
        }

    }
}
