using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Diplom
{
    public partial class Form2 : Form
    {
        int[,] OriginalInformation;
        int[,] FilteredInformation;
        Bitmap smooth;
        Bitmap median;
        Bitmap gauss;
        Bitmap laplas;
        Bitmap contrast;

        Bitmap original;
        Bitmap noize;
        Bitmap result;
        
        bool res = true;

        bool noizestart = true;
        bool resultstart = true;

        Bitmap RecoveryErrorBmap;
        Bitmap NormalizedRecoveryErrorBmap;
        int RecoveryErrorMin;
        int RecoveryErrorMax;
        bool RecoveryErrorOn1 = false;
        bool RecoveryErrorOn2 = false;
        public Form2(Image pic1, Image pic2, Image pic3, Bitmap smooth, Bitmap median, Bitmap gauss, Bitmap laplas, Bitmap contrast)
        {
            InitializeComponent();
            pictureBox1.Image = pic1;
            pictureBox2.Image = pic2;
            pictureBox3.Image = pic3;

            original = new Bitmap(pic1);
            noize = new Bitmap(pic2);
            result = new Bitmap(pic3);
            this.smooth = smooth;
            this.median = median;
            this.gauss = gauss;
            this.laplas = laplas;
            this.contrast = contrast;

            comboBox1.SelectedIndex = 3;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            RecoveryError(original, result);           
        }

        //функция информативности
        private int[,] Informationfuncktion(Image img, PictureBox pic, DataGridView datagrid, int size, Panel panel)
        {
            int width = (img.Width - img.Width % size);
            int height = (img.Height - img.Height % size);
            int WidthSquare = width / size;
            int HeightSquare = height / size;
            Bitmap bitmap = new Bitmap(img);
            int InformationSquare = 0;
            int[,] InformationSquareArr = new int[HeightSquare, WidthSquare];
            int[,] NormalizedInformationSquareArr;
            Bitmap Picbitmap = new Bitmap(WidthSquare * 10, HeightSquare * 10);

            for (int h = 0; h < HeightSquare; h++)
            {
                for (int w = 0; w < WidthSquare; w++)
                {
                    for (int i = h * size; i < h * size + size - 1; i++)
                    {
                        for (int j = w * size; j < w * size + size - 1; j++)
                        {
                            InformationSquare += Math.Abs(bitmap.GetPixel(j, i + 1).R - bitmap.GetPixel(j, i).R) + Math.Abs(bitmap.GetPixel(j + 1, i).R - bitmap.GetPixel(j, i).R);
                        }
                    }
                    InformationSquareArr[h, w] = InformationSquare;
                    InformationSquare = 0;
                }
            }

            NormalizedInformationSquareArr = Normalization(InformationSquareArr, HeightSquare, WidthSquare);
            for (int h = 0; h < HeightSquare; h++)
            {
                for (int w = 0; w < WidthSquare; w++)
                {
                    for (int i = h * 10; i < h * 10 + 10; i++)
                    {
                        for (int j = w * 10; j < w * 10 + 10; j++)
                        {
                            Picbitmap.SetPixel(j, i, Color.FromArgb(NormalizedInformationSquareArr[h, w], NormalizedInformationSquareArr[h, w], NormalizedInformationSquareArr[h, w]));
                        }
                    }
                }
            }

            panel.Height = 207;
            panel.Width = 207;
            if (Picbitmap.Height < 200)
                panel.Height = Picbitmap.Height + 7;
            if (Picbitmap.Width < 200)
                panel.Width = Picbitmap.Width + 7;
            pic.Height = Picbitmap.Height;
            pic.Width = Picbitmap.Width;
            pic.Image = Picbitmap;

            datagrid.Rows.Clear();
            datagrid.Columns.Clear();
            for (int w = 0; w < WidthSquare; w++)
            {
                datagrid.Columns.Add("name", null);
            }
            for (int h = 0; h < HeightSquare; h++)
            {
                datagrid.Rows.Add();
            }
            for (int h = 0; h < HeightSquare; h++)
            {
                for (int w = 0; w < WidthSquare; w++)
                {                    
                    datagrid.Rows[h].Cells[w].Value = InformationSquareArr[h, w] / (size * size);
                }
            }

            return InformationSquareArr;
        }

        //функция нормализации
        private int[,] Normalization(int[,] Arr, int height, int width)
        {
            double max = 0;
            double min = 0;
            double newmax = 255;
            double newmin = 0;
            int[,] NormalizedArr = new int[height, width];

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    if (Arr[h, w] < min)
                        min = Arr[h, w];
                    if (Arr[h, w] > max)
                        max = Arr[h, w];
                }
            }
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    NormalizedArr[h, w] = (int)(((Arr[h, w] - min) * ((newmax - newmin) / (max - min))) + newmin);//формула нормализации
                }
            }
            return NormalizedArr;
        }

        //выпадающее меню с размером сектора вычисления информативности
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    OriginalInformation = Informationfuncktion(pictureBox1.Image, pictureBox4, dataGridView1, 50, panel1);
                    if (!RecoveryErrorOn1)
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, 50, panel2);
                    if (!RecoveryErrorOn2)
                        FilteredInformation = Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, 50, panel3);

                    if (res)
                        InformationComparison(dataGridView4, OriginalInformation, FilteredInformation);                  
                    break;
                case 1:
                    OriginalInformation = Informationfuncktion(pictureBox1.Image, pictureBox4, dataGridView1, 100, panel1);
                    if (!RecoveryErrorOn1)
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, 100, panel2);
                    if (!RecoveryErrorOn2)
                        FilteredInformation = Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, 100, panel3);

                    if (res)
                        InformationComparison(dataGridView4, OriginalInformation, FilteredInformation);
                    break;
                case 2:
                    OriginalInformation = Informationfuncktion(pictureBox1.Image, pictureBox4, dataGridView1, 150, panel1);
                    if (!RecoveryErrorOn1)
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, 150, panel2);
                    if (!RecoveryErrorOn2)
                        FilteredInformation = Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, 150, panel3);

                    if (res)                    
                        InformationComparison(dataGridView4, OriginalInformation, FilteredInformation);                    
                    break;
                case 3:
                    OriginalInformation = Informationfuncktion(pictureBox1.Image, pictureBox4, dataGridView1, 200, panel1);
                    if (!RecoveryErrorOn1)
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, 200, panel2);
                    if (!RecoveryErrorOn2)
                        FilteredInformation = Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, 200, panel3);

                    if (res)                    
                        InformationComparison(dataGridView4, OriginalInformation, FilteredInformation);                    
                    break;
            }
        }

        //функция для сравнения информативностей
        private void InformationComparison(DataGridView datagrid, int[,] Arr1, int[,] Arr2)
        {
            datagrid.Rows.Clear();
            datagrid.Columns.Clear();
            for (int w = 0; w < Arr1.GetLongLength(1); w++)
            {
                datagrid.Columns.Add("name", null);
            }
            for (int h = 0; h < Arr1.GetLongLength(0); h++)
            {
                datagrid.Rows.Add();
            }
            for (int h = 0; h < Arr1.GetLongLength(0); h++)
            {
                for (int w = 0; w < Arr1.GetLongLength(1); w++)
                {

                    datagrid.Rows[h].Cells[w].Value = Arr1[h, w] - Arr2[h, w];
                }
            }
        }

        //второе выпадающее меню
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (noizestart)
            {
                noizestart = false;
            }
            else
            {
                int SectorSize = -1;
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        SectorSize = 50;
                        break;
                    case 1:
                        SectorSize = 100;
                        break;
                    case 2:
                        SectorSize = 150;
                        break;
                    case 3:
                        SectorSize = 200;
                        break;
                }

                switch (comboBox2.SelectedIndex)
                {
                    case 0:
                        pictureBox2.Image = noize;
                        groupBox4.Text = "Зашумленное изображение (снег 5%)";
                        groupBox2.Text = "Информативность зашумленного изображения";
                        label2.Text = "Таблица средних значений" + Environment.NewLine +
                        "информативности пикселей по секторам" + Environment.NewLine +
                        "зашумленного изображения:";
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, SectorSize, panel2);

                        label2.Visible = true;
                        dataGridView2.Visible = true;
                        panel2.Visible = true;
                        checkBox1.Visible = false;
                        label6.Visible = false;
                        RecoveryErrorOn1 = false;
                        break;
                    case 1:
                        pictureBox2.Image = smooth;
                        groupBox4.Text = "Изображение после применения сглаживающего фильтра";
                        groupBox2.Text = "Информативность изображения после применения сглаживающего фильтра";
                        label2.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения сглаживающего фильтра:";
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, SectorSize, panel2);

                        label2.Visible = true;
                        dataGridView2.Visible = true;
                        panel2.Visible = true;
                        checkBox1.Visible = false;
                        label6.Visible = false;
                        RecoveryErrorOn1 = false;
                        break;
                    case 2:
                        pictureBox2.Image = median;
                        groupBox4.Text = "Изображение после применения медианного фильтра";
                        groupBox2.Text = "Информативность изображения после применения медианного фильтра";
                        label2.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения медианного фильтра:";
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, SectorSize, panel2);

                        label2.Visible = true;
                        dataGridView2.Visible = true;
                        panel2.Visible = true;
                        checkBox1.Visible = false;
                        label6.Visible = false;
                        RecoveryErrorOn1 = false;
                        break;
                    case 3:
                        pictureBox2.Image = gauss;
                        groupBox4.Text = "Изображение после применения фильтра Гаусса";
                        groupBox2.Text = "Информативность изображения после применения фильтра Гаусса";
                        label2.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения фильтра Гаусса:";
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, SectorSize, panel2);

                        label2.Visible = true;
                        dataGridView2.Visible = true;
                        panel2.Visible = true;
                        checkBox1.Visible = false;
                        label6.Visible = false;
                        RecoveryErrorOn1 = false;
                        break;
                    case 4:
                        pictureBox2.Image = laplas;
                        groupBox4.Text = "Изображение после применения фильтра Лапласа";
                        groupBox2.Text = "Информативность изображения после применения фильтра Лапласа";
                        label2.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения фильтра Лапласа:";
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, SectorSize, panel2);

                        label2.Visible = true;
                        dataGridView2.Visible = true;
                        panel2.Visible = true;
                        checkBox1.Visible = false;
                        label6.Visible = false;
                        RecoveryErrorOn1 = false;
                        break;
                    case 5:
                        pictureBox2.Image = contrast;
                        groupBox4.Text = "Изображение после применения контрастоповышающего фильтра";
                        groupBox2.Text = "Информативность изображения после применения контрастоповышающего фильтра";
                        label2.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения контрастоповышающего фильтра:";
                        Informationfuncktion(pictureBox2.Image, pictureBox5, dataGridView2, SectorSize, panel2);

                        label2.Visible = true;
                        dataGridView2.Visible = true;
                        panel2.Visible = true;
                        checkBox1.Visible = false;
                        label6.Visible = false;
                        RecoveryErrorOn1 = false;
                        break;
                    case 6:
                        if (checkBox1.Checked)                        
                            pictureBox2.Image = NormalizedRecoveryErrorBmap;                        
                        else
                            pictureBox2.Image = RecoveryErrorBmap;
                        groupBox4.Text = "Ошибка восстановления изображения";
                        groupBox2.Text = "Параметры ошибки восстановления изображения";
                        
                        label2.Visible = false;
                        dataGridView2.Visible = false;
                        panel2.Visible = false;
                        checkBox1.Visible = true;
                        label6.Visible = true;
                        RecoveryErrorOn1 = true;
                        label6.Text = "Минимальная величина ошибки восстановления = " + RecoveryErrorMin + Environment.NewLine +
                            "Максимальная величина ошибки восстановления = " + RecoveryErrorMax;
                        break;
                }
            }            
        }

        //третье выпадающее меню
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (resultstart)
            {
                resultstart = false;
            }
            else
            {
                int SectorSize = -1;
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        SectorSize = 50;
                        break;
                    case 1:
                        SectorSize = 100;
                        break;
                    case 2:
                        SectorSize = 150;
                        break;
                    case 3:
                        SectorSize = 200;
                        break;
                }

                switch (comboBox3.SelectedIndex)
                {
                    case 0:
                        pictureBox3.Image = result;
                        groupBox5.Text = "Результат фильтрации изображения";
                        groupBox6.Text = "Информативность результата фильтрации изображения";
                        label3.Text = "Таблица средних значений информативности " + Environment.NewLine +
                        "пикселей по секторам результата" + Environment.NewLine +
                        " фильтрации изображения:";
                        FilteredInformation = Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, SectorSize, panel3);
                        res = true;
                        InformationComparison(dataGridView4, OriginalInformation, FilteredInformation);
                        groupBox7.Visible = true;

                        label3.Visible = true;
                        dataGridView3.Visible = true;
                        panel3.Visible = true;
                        checkBox2.Visible = false;
                        label7.Visible = false;
                        RecoveryErrorOn2 = false;
                        break;
                    case 1:
                        pictureBox3.Image = smooth;
                        groupBox5.Text = "Изображение после применения сглаживающего фильтра";
                        groupBox6.Text = "Информативность изображения после применения сглаживающего фильтра";
                        label3.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения сглаживающего фильтра:";
                        Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, SectorSize, panel3);
                        res = false;
                        groupBox7.Visible = false;

                        label3.Visible = true;
                        dataGridView3.Visible = true;
                        panel3.Visible = true;
                        checkBox2.Visible = false;
                        label7.Visible = false;
                        RecoveryErrorOn2 = false;
                        break;
                    case 2:
                        pictureBox3.Image = median;
                        groupBox5.Text = "Изображение после применения медианного фильтра";
                        groupBox6.Text = "Информативность изображения после применения медианного фильтра";
                        label3.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения медианного фильтра:";
                        Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, SectorSize, panel3);
                        res = false;
                        groupBox7.Visible = false;

                        label3.Visible = true;
                        dataGridView3.Visible = true;
                        panel3.Visible = true;
                        checkBox2.Visible = false;
                        label7.Visible = false;
                        RecoveryErrorOn2 = false;
                        break;
                    case 3:
                        pictureBox3.Image = gauss;
                        groupBox5.Text = "Изображение после применения фильтра Гаусса";
                        groupBox6.Text = "Информативность изображения после применения фильтра Гаусса";
                        label3.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения фильтра Гаусса:";
                        Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, SectorSize, panel3);
                        res = false;
                        groupBox7.Visible = false;

                        label3.Visible = true;
                        dataGridView3.Visible = true;
                        panel3.Visible = true;
                        checkBox2.Visible = false;
                        label7.Visible = false;
                        RecoveryErrorOn2 = false;
                        break;
                    case 4:
                        pictureBox3.Image = laplas;
                        groupBox5.Text = "Изображение после применения фильтра Лапласа";
                        groupBox6.Text = "Информативность изображения после применения фильтра Лапласа";
                        label3.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения фильтра Лапласа:";
                        Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, SectorSize, panel3);
                        res = false;
                        groupBox7.Visible = false;

                        label3.Visible = true;
                        dataGridView3.Visible = true;
                        panel3.Visible = true;
                        checkBox2.Visible = false;
                        label7.Visible = false;
                        RecoveryErrorOn2 = false;
                        break;
                    case 5:
                        pictureBox3.Image = contrast;
                        groupBox5.Text = "Изображение после применения контрастоповышающего фильтра";
                        groupBox6.Text = "Информативность изображения после применения контрастоповышающего фильтра";
                        label3.Text = "Таблица средних значений информативности" + Environment.NewLine +
                        "пикселей по секторам изображения после" + Environment.NewLine +
                        "применения контрастоповышающего фильтра:";
                        Informationfuncktion(pictureBox3.Image, pictureBox6, dataGridView3, SectorSize, panel3);
                        res = false;
                        groupBox7.Visible = false;

                        label3.Visible = true;
                        dataGridView3.Visible = true;
                        panel3.Visible = true;
                        checkBox2.Visible = false;
                        label7.Visible = false;
                        RecoveryErrorOn2 = false;
                        break;
                    case 6:
                        if (checkBox2.Checked)
                            pictureBox3.Image = NormalizedRecoveryErrorBmap;
                        else
                            pictureBox3.Image = RecoveryErrorBmap;
                        groupBox5.Text = "Ошибка восстановления изображения";
                        groupBox6.Text = "Параметры ошибки восстановления изображения";
                        res = false;
                        groupBox7.Visible = false;

                        label3.Visible = false;
                        dataGridView3.Visible = false;
                        panel3.Visible = false;
                        checkBox2.Visible = true;
                        label7.Visible = true;
                        RecoveryErrorOn2 = true;
                        label7.Text = "Минимальная величина ошибки восстановления = " + RecoveryErrorMin + Environment.NewLine +
                            "Максимальная величина ошибки восстановления = " + RecoveryErrorMax;
                        break;
                }
            }
        }

        //функция для вычисления ошибки восстановления изображения
        private void RecoveryError(Bitmap original, Bitmap result)
        {
            RecoveryErrorBmap = new Bitmap(original.Width, original.Height);
            int[,] RecoveryErrorArr = new int[original.Height, original.Width];
            int[,] NormalizedRecoveryErrorArr = new int[original.Height, original.Width];
            NormalizedRecoveryErrorBmap = new Bitmap(original.Width, original.Height);
            int RecoveryError;
            for (int i = 0; i < original.Height; i++)
            {
                for (int j = 0; j < original.Width; j++)
                {
                    RecoveryError = original.GetPixel(j, i).R - result.GetPixel(j, i).R;
                    RecoveryErrorArr[i, j] = Math.Abs(RecoveryError);
                    RecoveryErrorBmap.SetPixel(j, i, Color.FromArgb(RecoveryErrorArr[i, j], RecoveryErrorArr[i, j], RecoveryErrorArr[i, j]));
                    if (RecoveryError < RecoveryErrorMin)
                        RecoveryErrorMin = RecoveryError;
                    if (RecoveryError > RecoveryErrorMax)
                        RecoveryErrorMax = RecoveryError;
                }
            }
            NormalizedRecoveryErrorArr = Normalization(RecoveryErrorArr, original.Height, original.Width);
            for (int i = 0; i < original.Height; i++)
            {
                for (int j = 0; j < original.Width; j++)
                {
                    NormalizedRecoveryErrorBmap.SetPixel(j, i, Color.FromArgb(NormalizedRecoveryErrorArr[i, j], NormalizedRecoveryErrorArr[i, j], NormalizedRecoveryErrorArr[i, j]));
                }
            }

        }

        //отображение нормализованной ошибки восстановления
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)            
                pictureBox2.Image = NormalizedRecoveryErrorBmap;            
            else
                pictureBox2.Image = RecoveryErrorBmap;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                pictureBox3.Image = NormalizedRecoveryErrorBmap;
            else
                pictureBox3.Image = RecoveryErrorBmap;
        }
    }
}
