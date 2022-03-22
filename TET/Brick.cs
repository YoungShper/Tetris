using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TET
{
    public abstract class Brick
    {
        private Rectangle[] _shape; // форма фигуры
        private int _posX = 5; // начальные позиции
        private int _posY = 0; //
        private bool _stop = false;
        public Color brickColor;
        public int removes = 0; // убрано линий
        public int PosX
        {
            get { return _posX; }
        }

        public int PosY
        {
            get { return _posY; }
        }
        public Rectangle[] Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        public bool Stop
        {
            get { return _stop; }
            set { _stop = value; }

        }
        public void Draw(Graphics g) // отрисовка фигуры
        {
            foreach (Rectangle r in Shape)
            {
                if (r.Y >= 0 && r.Y < 500) g.FillRectangle(new SolidBrush(brickColor), r);
            }
        }

        public abstract void Constructor(int cell);

        public void Deleter(Graphics g, int cell, KeyEventArgs e) // закрашивание предыдущих позиций
        {
            foreach (Rectangle r in Shape)
            {
                if ((r.Y + cell != 400) || (r.Y + cell == 400 && (e != null && (e.KeyCode == Keys.D || e.KeyCode == Keys.A)))) g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(r.X, r.Y, cell, cell));
            }
        }

        public void Move(int cell, List<Brick> bricks) // движение фигуры
        {
            foreach (Rectangle r in Shape) // Если следующая позиция = концу поля, или координатам уже стоящей фигуры, текущая фигура останавливается
            {
                Stop = r.Y + cell == 400 ? true : false; 

                foreach(Brick m in bricks)
                {
                    foreach(Rectangle b in m.Shape)
                    {
                        if (r.Y + cell == b.Y && r.X == b.X)
                        {
                            Stop = true;
                            break;
                        }
                        if (Stop == true) break;
                    }
                }
                if (Stop == true) break;
            }

            if (Stop != true)
            {
                for (int i = 0; i < Shape.Length; i++) Shape[i].Y += cell;
            }
            else { bricks.Add(this); }
        }

        public void GameOver(List<Brick> bricks, Timer t)
        {
            foreach (Brick r in bricks)
            {
                foreach(Rectangle b in r.Shape)
                {
                    for (int i = 0; i < bricks.Count; i++)
                    {
                        if (b.Y < 0)
                        {
                            Stop = true;
                            t.Enabled = false;
                            MessageBox.Show("Game over");
                            break;
                        }
                    }
                    if (b.Y < 0) break;
                }
            } 
        }

        public void Rotate(int cellSize, List<Brick> bricks)
        {
            Point[] shapeCheck = new Point[4];
            int count = 0;
            bool access = true;

            foreach (Rectangle p in Shape) // заполняем временный массив
            {
                shapeCheck[count] = new Point(p.X, p.Y);
                count++;
            }

            Point tmp = new Point(shapeCheck[1].X, shapeCheck[1].Y);

            for (int i = 0; i < shapeCheck.Length; i++) // проверяем на выход за пределы поля и пересечения с фигурами во время вращения
            {
                Point currentFigure = new Point(shapeCheck[i].X, shapeCheck[i].Y);
                shapeCheck[i].X = tmp.X + tmp.Y - currentFigure.Y;
                shapeCheck[i].Y = currentFigure.X + tmp.Y - tmp.X;

                if(shapeCheck[i].Y + cellSize > 400 || (shapeCheck[i].X + cellSize > 200 || shapeCheck[i].X < 0)) access = false;
                foreach (Brick b in bricks)
                {
                    foreach(Rectangle r in b.Shape)
                    {
                        if (((r.X + cellSize == shapeCheck[i].X || r.X - cellSize == shapeCheck[i].X) && r.Y == shapeCheck[i].Y) && access == true) { access = false; break; }
                    }
                }
                if (access == false) break;
            }

            if(access == true)
            {
                for (int i = 0; i < shapeCheck.Length; i++)
                {
                    Shape[i].X = shapeCheck[i].X;
                    Shape[i].Y = shapeCheck[i].Y;
                }
            }
        }

        public void LineDelete(List<Brick> bricks, int cell)
        {
            int lastY = 400;
            int counterOfRemoves = 0;
            var tmp = this.Shape.GroupBy(rec => rec.Y).Select(grp => grp.First()).ToArray(); // находим разные значения Y упавшей детали
            foreach(Rectangle f in tmp) // проверяем всю область рисования
            {
                List<int> indexOfBrick = new List<int>(); // хранение индексов объектов в которых удаляются квадраты
                int counter = 0;
                
                foreach (Brick b in bricks)
                {
                    for(int j = 0; j < 4; j++)
                    {
                        if (b.Shape[j].Y == f.Y) { indexOfBrick.Add(bricks.IndexOf(b)); counter++; } // проверяем, есть ли 10 квадратов на одной линии
                        if (counter == 10) break;                                                  // если есть - прерываем цикл 
                    }

                    if(counter == 10) 
                    {
                        foreach(int i in indexOfBrick) // удаляем квадраты по полученым индексам объектов(перемещаем за пределы разрешения, чтобы не мешали)
                        {
                            for(int j = 0; j < 4; j++)
                            {
                                if (bricks[i].Shape[j].Y == f.Y) { bricks[i].Shape[j] = new Rectangle(0, 500, 0, 0); counterOfRemoves++; lastY = lastY > f.Y ? f.Y : lastY; };
                            }
                        }
                    }
                }
            }

            counterOfRemoves /= 10;
            removes += counterOfRemoves;
            foreach (Brick m in bricks) // смещаем все элементы на количество удаленных строк
            {
                for (int k = 0; k < 4; k++)
                {
                    if(m.Shape[k].Y < 500) m.Shape[k].Y = m.Shape[k].Y < lastY ? m.Shape[k].Y + counterOfRemoves * cell : m.Shape[k].Y;
                }
            }
        }
    }

    public class Line : Brick // палка
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4]; 
            for(int i = 0; i < Shape.Length; i++)
            {
                Shape[i].X = PosX * cell;
                Shape[i].Y = (PosY - i) * cell;
                Shape[i].Width = cell;
                Shape[i].Height = cell;
            }
        }
    }

    public class Square : Brick
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4]; 
            for (int i = 0; i < Shape.Length; i++)
            {
                if(i < 2)
                {
                    Shape[i].X = PosX * cell;
                    Shape[i].Y = (PosY - i) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
                else
                {
                    Shape[i].X = (PosX - 1) * cell;
                    Shape[i].Y = (PosY - i + 2) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
            }
        }
    }

    public class Zet : Brick
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4]; 
            for (int i = 0; i < Shape.Length; i++)
            {
                if (i < 2)
                {
                    Shape[i].X = PosX * cell;
                    Shape[i].Y = (PosY - 1 + i) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
                else
                {
                    Shape[i].X = (PosX - 1) * cell;
                    Shape[i].Y = (PosY - i + 3) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
            }
        }
    }

    public class Es : Brick
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4]; 
            for (int i = 0; i < Shape.Length; i++)
            {
                if (i < 2)
                {
                    Shape[i].X = PosX * cell;
                    Shape[i].Y = (PosY - i) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
                else
                {
                    Shape[i].X = (PosX - 1) * cell;
                    Shape[i].Y = (PosY - i + 1) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
            }
        }
    }

    public class El : Brick 
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4]; 
            for (int i = 0; i < Shape.Length; i++)
            {
                if(i == 3)
                {
                    Shape[i].X = PosX * cell;
                    Shape[i].Y = (PosY - i + 4) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
                else
                {
                    Shape[i].X = (PosX - 1) * cell;
                    Shape[i].Y = (PosY - i + 1) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
            }

        }
    }

    public class Jei : Brick
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4];
            for (int i = 0; i < Shape.Length; i++)
            {
                if (i == 3)
                {
                    Shape[i].X = PosX * cell;
                    Shape[i].Y = (PosY - i + 4) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
                else
                {
                    Shape[i].X = (PosX + 1) * cell;
                    Shape[i].Y = (PosY - i + 1) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
            }

        }
    }

    public class Tet : Brick
    {
        public override void Constructor(int cell)
        {
            Shape = new Rectangle[4];
            for (int i = 0; i < Shape.Length; i++)
            {
                if(i < 3)
                {
                    Shape[i].X = PosX * cell;
                    Shape[i].Y = (PosY - i) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
                else
                {
                    Shape[i].X = (PosX - 1) * cell;
                    Shape[i].Y = (PosY - i + 2) * cell;
                    Shape[i].Width = cell;
                    Shape[i].Height = cell;
                }
            }
        }
    }
}