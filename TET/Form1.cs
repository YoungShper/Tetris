using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TET
{
    /*Дописать присваивание нулевым фигурам отрицательного значения и не двигать их*/
    public partial class Form1 : Form
    {
        public static int cellSize = 20;
        Graphics g, g2;
        Brick figure;
        Brick nextFigure;
        public List<Brick> bricks = new List<Brick>();
        Random random = new Random();
        Random colorRandom = new Random();
        KeyEventArgs keyEvent;
        int nextBrick = 0;
        int nextBrickColor;
        int currentScore = 0;
        int level = 0;
        int lines = 0;
        public Brick selectBrick(int next) // выбор фигуры
        {
            switch (next)
            {
                
                case 0:
                    {
                        Line tmp = new Line();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                case 1:
                    {
                        Square tmp = new Square();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                case 2:
                    {
                        Zet tmp = new Zet();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                case 3:
                    {
                        Es tmp = new Es();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                case 4:
                    {
                        El tmp = new El();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                case 5:
                    {
                        Jei tmp = new Jei();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                case 6:
                    {
                        Tet tmp = new Tet();
                        tmp.Constructor(cellSize);
                        return tmp;
                    }
                default: return null;
            }
        }

        public void ReDraw()
        {
            g.Clear(Color.Black);
            foreach (Brick b in bricks)
            {
                b.Draw(g);
            }
        }

        public void DrawNextFigure(Graphics gr, Brick b)
        {
            gr.Clear(Color.Black);
            b.Draw(gr);
        }

        public Color PickColor(int a)
        {
            switch (a)
            {
                case 0: return Color.FromArgb(235, 102, 186); 
                case 1: return Color.FromArgb(253, 188, 85); 
                case 2: return Color.FromArgb(215, 246, 107);
                case 3: return Color.FromArgb(92, 155, 253);
                default: return Color.Black;
            }
        }

        public void animation(Brick a, Graphics g, int cellSize, List<Brick> bricks)
        {
            a.Deleter(g, cellSize, keyEvent);
            a.Move(cellSize, bricks);
            a.Draw(g);
            a.GameOver(bricks, timer1);
        }

        public Form1()
        {
            MessageBox.Show("A D - движение\n P - пауза\n S - ускорение\n Пробел - вращение фигуры");
            nextBrick = random.Next(0, 7); // рандом для первой фигуры
            nextBrickColor = colorRandom.Next(0, 4);
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            g2 = pictureBox2.CreateGraphics();
            label1.Text = "Score: 0";
            label2.Text = "Level: 0";
            label3.Text = "Lines: 0";
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            this.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (figure == null) 
            { 
                figure = selectBrick(nextBrick); 
                figure.brickColor = PickColor(nextBrickColor); 
                nextBrick = random.Next(0, 7); 
                nextBrickColor = colorRandom.Next(0, 4);
                nextFigure = selectBrick(nextBrick); 
                nextFigure.brickColor = PickColor(nextBrickColor);
                for(int i = 0; i < 4; i++) // двигаем фигуру в окне
                {
                    nextFigure.Shape[i].X -= 3 * cellSize;
                    nextFigure.Shape[i].Y += 3 * cellSize;
                }
                DrawNextFigure(g2, nextFigure);
            } // если фигура null, создаем новый объект, рандом для следующей фигуры

            if (figure.Stop == false) animation(figure, g, cellSize, bricks); // проигрываем анимацию, пока флаг остановки не активен
            else 
            {
                figure.LineDelete(bricks, cellSize); // если фигура остановлена проверяем, можно ли удалить линию по ее координатам

                switch (figure.removes) // счетчик очков
                {
                    case 0: break;
                    case 1: currentScore += 10; break;
                    case 2: currentScore += 30; break;
                    case 3: currentScore += 60; break;
                    case 4: currentScore += 100; break;
                    default: currentScore += 100; break;
                }
                label1.Text = "Score: " + currentScore; // вывод текущих очков 
                if (currentScore >= 200 && currentScore / 200 != level && timer1.Interval > 100) { timer1.Interval -= 30; level++; }
                lines += figure.removes; // прибавляем убраные линии
                label2.Text = "Level: " + level;
                label3.Text = "Lines: " + lines;
                var tmpBricks = bricks.Where(brick => brick.Shape[0].Height == 0 && brick.Shape[1].Height == 0 && brick.Shape[2].Height == 0 && brick.Shape[3].Height == 0).ToList();
                // если есть фигуры со всеми пустыми прямоугольниками, удаляем их
                foreach (Brick m in tmpBricks) bricks.Remove(m);
                ReDraw(); // перерисовка
                figure = null; // удаление фигуры для создания новой

            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyEvent = e;

            if(figure != null)
            {
                figure.Deleter(g, cellSize, keyEvent);
                switch (e.KeyCode)
                {
                    case Keys.D:
                        {
                            bool plus = true; // флаг для смещения фигуры

                            for (int i = 0; i < figure.Shape.Length; i++) // проверка на выход за пределы разрешения
                            {
                                if (figure.Shape[i].X + cellSize >= 200) { plus = false; break; }
                            }

                            if (plus == true)
                            {
                                for (int i = 0; i < figure.Shape.Length; i++) // проверка на пересечения с другими деталями
                                {
                                    foreach (Brick r in bricks)
                                    {
                                        foreach(Rectangle b in r.Shape)
                                        {
                                            if (figure.Shape[i].X + cellSize != b.X || figure.Shape[i].Y != b.Y) plus = true;
                                            else { plus = false; break; }
                                        }
                                        if (plus == false) break;
                                    }
                                    if (plus == false) break;
                                }
                            }
                            for (int i = 0; i < figure.Shape.Length; i++) figure.Shape[i].X += plus == true ? cellSize : 0; // смещение
                            break;
                        }

                    case Keys.A:
                        {
                            bool minus = true;

                            for (int i = 0; i < figure.Shape.Length; i++) // проверка на выход за пределы разрешения
                            {
                                if (figure.Shape[i].X + figure.Shape[i].Width - cellSize <= 0) { minus = false; break; }
                            }

                            if (minus == true)
                            {
                                for (int i = 0; i < figure.Shape.Length; i++)// проверка на пересечения с другими деталями
                                {
                                    foreach (Brick r in bricks)
                                    {
                                        foreach(Rectangle b in r.Shape)
                                        {
                                            if (figure.Shape[i].X - cellSize != b.X || figure.Shape[i].Y != b.Y) minus = true;
                                            else { minus = false; break; }
                                        }
                                        if (minus == false) break;
                                    }
                                    if (minus == false) break;
                                }
                            }
                            for (int i = 0; i < figure.Shape.Length; i++) figure.Shape[i].X -= minus == true ? cellSize : 0; // смещение
                            break;
                        }

                    case Keys.Space: if (figure.GetType().Name != "Square") figure.Rotate(cellSize, bricks); break; // вращение фигур кроме квадрата
                    case Keys.P: if (timer1.Enabled == true) timer1.Enabled = false; else timer1.Enabled = true; break; // Пауза
                    case Keys.S: if(figure.Stop != true) animation(figure, g, cellSize, bricks); break; // ускорение
                }
                figure.Draw(g);
            }
        }
    }
}
