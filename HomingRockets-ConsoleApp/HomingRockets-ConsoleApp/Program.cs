using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;


namespace Rockets
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Run(new MyForm());
        }
    }

    public class MyForm : Form
    {
        Label label1 = new Label();
        Label label2 = new Label();
        Label label3 = new Label();

        Button button1 = new Button();
        private bool buttonFlick = true;

        Panel panel1 = new Panel();

        Timer timer1 = new Timer();

        Random random = new Random();

        PictureBox pictureBox1 = new PictureBox();

        Graphics graphCalculate;
        Graphics graphShowOnScreen;
        Bitmap btm;
        PointF coordinatesForBitmap;
        RectangleF targetCoordinate;
        RectangleF obstacleCoordinate;
        RectangleF obstacleCoordinate2;

        int lifeSpan = 20;
        int populationSize = 100;
        float mutationRate = 0.01f;
        float averageFitness;
        int populationCounter = 0;
        Point[] triangle = new Point[3];



        List<RocketParameters> Rockets = new List<RocketParameters>();
        List<RocketParameters> populationPool = new List<RocketParameters>();


        public MyForm()
        {
            InitializeComponent();


            btm = new Bitmap((int)(0.65 * ClientSize.Width), (int)(0.9 * ClientSize.Height));
            graphCalculate = Graphics.FromImage(btm);
            //graphCalculate = pictureBox1.CreateGraphics();
            graphShowOnScreen = CreateGraphics();


            //----------punkty do trojkata------------
            triangle[0] = new Point(0, -panel1.Height / 45);
            triangle[1] = new Point(-panel1.Width / 90, (Math.Abs(triangle[0].Y / 2)));
            triangle[2] = new Point(Math.Abs(triangle[1].X), (Math.Abs(triangle[0].Y / 2)));

            //----------wspolrzedne do targetu i przeszkody---------------
            targetCoordinate = new RectangleF(btm.Width / 2, (0.1f * btm.Height), 60, 60);
            obstacleCoordinate = new RectangleF(btm.Width * 0.25f, (0.6f * btm.Height), btm.Width * 0.7f, btm.Height * 0.05f);
            obstacleCoordinate2 = new RectangleF(btm.Width * 0.02f, (0.3f * btm.Height), btm.Width * 0.65f, btm.Height * 0.05f);


            //-----------tworzenie populacji oraz wypelnianie wstepnymi danymi pól poszczegolnych obiektow--------
            for (int i = 0; i < populationSize; i++)
            {
                Rockets.Add(new RocketParameters(lifeSpan));
            }
            foreach (var item in Rockets)
            {
                for (int i = 0; i < lifeSpan; i++)
                {
                    item.angle[i] = random.Next(-60, 60);
                    item.angleCopy[i] = item.angle[i];
                    item.force[i] = random.Next(4, 8);
                }
            }
        }

        private void InitializeComponent()
        {
            Text = "Homing Rockets";
            Width = 1200;
            Height = 1000;
            DoubleBuffered = true;

            //---------------panel----------------
            panel1.Width = (int)(0.65 * ClientSize.Width);
            panel1.Height = (int)(0.9 * ClientSize.Height);
            panel1.Top = (ClientSize.Height - panel1.Height) / 2;
            panel1.Left = panel1.Top;
            panel1.BackColor = Color.White;


            //---------------labels------------
            label1.AutoSize = true;

            //-------------pictureBox-------------
            pictureBox1.Width = (int)(0.65 * ClientSize.Width);
            pictureBox1.Height = (int)(0.9 * ClientSize.Height);
            pictureBox1.Top = (ClientSize.Height - panel1.Height) / 2;
            pictureBox1.Left = panel1.Top;
            pictureBox1.BackColor = Color.White;

            //polozenie bitmapy
            coordinatesForBitmap = new PointF((ClientSize.Height - panel1.Height) / 2, (ClientSize.Height - panel1.Height) / 2);


            //-------------buttons--------------
            button1.Text = "Przyśpiesz";
            button1.AutoSize = true;
            button1.Width += 10;
            button1.Height += 10;
            button1.Left = panel1.Left + pictureBox1.Width + ((ClientSize.Width - button1.Width - pictureBox1.Width - pictureBox1.Left) / 2);
            button1.Top = (int)(0.7 * ClientSize.Height);
            button1.MouseClick += Button1_MouseClick;


            //---------------labels------------
            label1.Text = "                  ";
            label1.AutoSize = true;
            label1.Width += 10;
            label1.Height += 10;
            label1.Left = panel1.Left + pictureBox1.Width + ((ClientSize.Width - button1.Width - pictureBox1.Width - pictureBox1.Left) / 2);
            label1.Top = (int)(0.6 * ClientSize.Height);

            label2.Text = "                  ";
            label2.AutoSize = true;
            label2.Width += 10;
            label2.Height += 10;
            label2.Left = panel1.Left + pictureBox1.Width + ((ClientSize.Width - button1.Width - pictureBox1.Width - pictureBox1.Left) / 2);
            label2.Top = (int)(0.566 * ClientSize.Height);

            label3.Text = "                        ";
            label3.AutoSize = true;
            label3.Width += 10;
            label3.Height += 10;
            label3.Left = panel1.Left + pictureBox1.Width + ((ClientSize.Width - button1.Width - pictureBox1.Width - pictureBox1.Left) / 2);
            label3.Top = (int)(0.533 * ClientSize.Height);


            //Controls.Add(panel1);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(button1);
            //Controls.Add(pictureBox1);

            timer1.Enabled = true;
            timer1.Interval = 20;
            timer1.Tick += Timer1_Tick;
        }

        private void Button1_MouseClick(object sender, MouseEventArgs e)
        {
            if (buttonFlick)
            {
                button1.Text = "Zwolnij";
                timer1.Interval = 1;
                buttonFlick = false;
            }
            else
            {
                button1.Text = "Przyśpiesz";
                timer1.Interval = 20;
                buttonFlick = true;
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {


            DrawRockets();
            CalculateAverageFitness();
            CreateNewPopulation();
            ShowIteration();

        }

        private void ShowIteration()
        {
            label1.Text = "Cykl życia: " + Rockets.Max(r => r.index).ToString();
            label2.Text = "Pokolenie: " + populationCounter;
            label3.Text = "Average fitness: " + averageFitness;
        }

        private void CalculateAverageFitness()
        {

            foreach (var item in Rockets)
            {
                if (item.crashed && !item.hitTarget)
                { item.fitness /= 10; }
                else if (item.hitTarget && item.crashed)
                {
                    item.fitness *= 5;

                    for (int i = 0; i < lifeSpan - item.index; i++)
                    {
                        item.fitness *= 3f; //nagradzanie rakiet, ktore dotarly do punktu docelowego szybciej
                    }
                }
            }
            var maxFitnessFactor = Rockets.Max(r => r.fitness);

            foreach (var item in Rockets)
            {
                item.normalizedFitness = item.fitness / maxFitnessFactor; //normalizowanie wspolczynnika fitness do zakresu (0,1)
                averageFitness += item.normalizedFitness;
            }

            averageFitness /= Rockets.Count;
        }

        private bool CheckForCrash(RocketParameters item)
        {

            if (item.coordinatesX[item.counter] >= btm.Width || item.coordinatesX[item.counter] <= 0 || item.coordinatesY[item.counter] >= btm.Height || item.coordinatesY[item.counter] <= 0)
            {
                item.crashed = true;
                return true;
            }
            else if (item.coordinatesX[item.counter] >= obstacleCoordinate.X && item.coordinatesX[item.counter] <= obstacleCoordinate.X + obstacleCoordinate.Width
                && item.coordinatesY[item.counter] >= obstacleCoordinate.Y && item.coordinatesY[item.counter] <= obstacleCoordinate.Y + obstacleCoordinate.Height)
            {
                item.crashed = true;
                return true;
            }
            else if (item.coordinatesX[item.counter] >= obstacleCoordinate2.X && item.coordinatesX[item.counter] <= obstacleCoordinate2.X + obstacleCoordinate2.Width
                && item.coordinatesY[item.counter] >= obstacleCoordinate2.Y && item.coordinatesY[item.counter] <= obstacleCoordinate2.Y + obstacleCoordinate2.Height)
            {
                item.crashed = true;
                return true;
            }
            else if (item.coordinatesX[item.counter] >= targetCoordinate.X && item.coordinatesX[item.counter] <= targetCoordinate.X + targetCoordinate.Width
                && item.coordinatesY[item.counter] >= targetCoordinate.Y && item.coordinatesY[item.counter] <= targetCoordinate.Y + targetCoordinate.Height)
            {
                item.crashed = true;
                item.hitTarget = true;
                return true;
            }
            else
            {
                return false;
            }


        }

        private void CreateTargetAndObstacle()
        {
            using (Graphics gr = Graphics.FromImage(btm))
            {
                gr.FillEllipse(new SolidBrush(Color.FromArgb(150, 0, 200, 0)), targetCoordinate);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(150, 200, 0, 0)), obstacleCoordinate);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(150, 200, 0, 0)), obstacleCoordinate2);
            }

        }

        private void CalculateFitness(RocketParameters variable)
        {
            //variable.fitness = 1 / (float)(Math.Sqrt((((targetCoordinate.X + targetCoordinate.Width / 2) - variable.coordinatesX[variable.counter]) * ((targetCoordinate.X + targetCoordinate.Width / 2) - variable.coordinatesX[variable.counter]))
            //    + (((targetCoordinate.Y + targetCoordinate.Height/2) - variable.coordinatesY[variable.counter]) * ((targetCoordinate.Y + targetCoordinate.Height / 2) - variable.coordinatesY[variable.counter]))));

            variable.fitness = 1 / ((((targetCoordinate.X + targetCoordinate.Width / 2) - variable.coordinatesX[variable.counter]) * ((targetCoordinate.X + targetCoordinate.Width / 2) - variable.coordinatesX[variable.counter]))
                            + (((targetCoordinate.Y + targetCoordinate.Height / 2) - variable.coordinatesY[variable.counter]) * ((targetCoordinate.Y + targetCoordinate.Height / 2) - variable.coordinatesY[variable.counter])));

        }

        private void CreateNewPopulation()
        {


            if (Rockets.TrueForAll(r => r.crashed == true)) //Rockets.TrueForAll(r => r.index == lifeSpan) || 
            {
                populationCounter++;
                populationPool.Clear();

                foreach (var item in Rockets)
                {
                    item.hitTarget = false;
                    item.crashed = false;
                    item.index = 0;
                    item.counter = 0;
                    item.previousCoordinatesX = 0;
                    item.previousCoordinatesY = 0;



                    if (random.NextDouble() <= item.normalizedFitness)
                    {
                        populationPool.Add(item);
                    }
                }

                for (int i = 0; i < populationSize; i++)
                {
                    var ParentA = populationPool[random.Next(0, populationPool.Count)];
                    var ParentB = populationPool[random.Next(0, populationPool.Count)];
                    Rockets[i] = Crossover(ParentA, ParentB);
                }

                ApplyMutation(mutationRate);
            }
        }


        private RocketParameters Crossover(RocketParameters parentA, RocketParameters parentB)
        {
            RocketParameters child = new RocketParameters(lifeSpan);
            int randomPick = random.Next(0, lifeSpan);

            for (int i = 0; i < lifeSpan; i++)
            {
                if (i <= randomPick)
                {
                    child.angleCopy[i] = parentA.angleCopy[i] + random.Next(-3, 3);
                    child.angle[i] = child.angleCopy[i];
                    child.force[i] = parentA.force[i] * ((float)random.Next(90, 110) / 100);
                }
                else
                {
                    child.angleCopy[i] = parentB.angleCopy[i] + random.Next(-3, 3);
                    child.angle[i] = child.angleCopy[i];
                    child.force[i] = parentB.force[i] * ((float)random.Next(90, 110) / 100);
                }
                if (child.force[i] <= 3)
                {
                    child.force[i] *= 1.4f; //zwiekszenie sily w przypadku wartosci nizszych niz 3 dla poprawnosci dzialania programu
                }
            }

            return child;
        }


        private void ApplyMutation(float mutationRate)
        {
            foreach (var item in Rockets)
            {
                for (int i = 0; i < lifeSpan; i++)
                {
                    if (random.NextDouble() <= mutationRate)
                    {
                        item.angle[i] = random.Next(-60, 60);
                        item.angleCopy[i] = item.angle[i];
                        item.force[i] = random.Next(4, 8);
                    }
                }

            }
        }



        private void MoveRockets(RocketParameters variable)
        {
            variable.ApplyForce();

            for (int i = 0; i < variable.coordinatesX.Count; i++)
            {
                variable.coordinatesX[i] += (0.65F * ClientSize.Width) / 2;
                variable.coordinatesY[i] += (0.9F * ClientSize.Height) - 50;
            }
        }

        private void DrawRockets()
        {

            graphCalculate.Clear(Color.White);

            foreach (var item in Rockets)
            {

                if (item.index >= lifeSpan)
                {
                    item.crashed = true;
                    continue;
                }


                else if (item.counter == 0 || item.counter >= item.coordinatesX.Count - 1)
                {
                    item.counter = 0;
                    MoveRockets(item);
                }
                else if (CheckForCrash(item))
                {
                    item.counter--;
                    //item.index = lifeSpan;
                }

                item.counter++;
                CalculateFitness(item);


                // tworzenie dla kazdej rakiety osobnego "buforu", ktory podlega transformacjom rotacji i zmiany polozenia
                // nastepnie kazda taka transformacja jest przypisywana do bitmapy, ktora raz na Tick timera jest wyswietlana na ekranie

                using (Graphics grp = Graphics.FromImage(btm))
                {
                    //grp.DrawLine(new Pen(Color.FromArgb(50, 255, 0, 0)), new PointF(targetCoordinate.X + 15,
                    //                           targetCoordinate.Y + 15), new PointF(item.coordinatesX[item.counter], item.coordinatesY[item.counter]));

                    grp.RotateTransform(item.angle[item.index - 1]);
                    grp.TranslateTransform(item.coordinatesX[item.counter], item.coordinatesY[item.counter], MatrixOrder.Append);
                    //grp.DrawLine(Pens.LightBlue, new PointF(item.previousCoordinatesX, item.previousCoordinatesY), new PointF(item.coordinatesX, item.coordinatesY));

                    //FromArgb, daje skladnik alpha, ktory odpowiada za przezroczystosc
                    grp.FillPolygon(new SolidBrush(Color.FromArgb(125, 0, 0, 255)), triangle, FillMode.Winding);


                }

            }

            //graphCalculate.DrawLine(new Pen(Color.FromArgb(100, 255, 0, 0)), new PointF(targetCoordinate.X, targetCoordinate.Y), new PointF(Rockets[0].coordinatesX[0], Rockets[0].coordinatesY[0]));
            //label1.Text = "Fitness: " + Rockets.Max(r => r.fitness);
            CreateTargetAndObstacle();

            graphShowOnScreen.DrawImage(btm, coordinatesForBitmap);



            //graphShowOnScreen.DrawImage(btm, coordinatesForBitmap);



            //------------------rysowanie rakiet bez mrugania!!!!!---------------------

            //graphCalculate.Clear(Color.White);

            //foreach (var item in Rockets)
            //{
            //    //tworzenie dla kazdej rakity osobnego "buforu", ktory podlega transformacjom rotacji i zmiany polozenia
            //    // nastepnie kazda taka transformacja jest przypisywana do bitmapy, ktora raz na Tick timera jest wyswietlana na ekranie

            //    using (Graphics grp = Graphics.FromImage(btm))
            //    {

            //        grp.RotateTransform(item.angle[0]);
            //        grp.TranslateTransform(item.coordinatesX, item.coordinatesY, MatrixOrder.Append);
            //        //grp.DrawLine(Pens.LightBlue, new PointF(item.previousCoordinatesX, item.previousCoordinatesY), new PointF(item.coordinatesX, item.coordinatesY));

            //        //FromArgb, daje skladnik alpha, ktory odpowiada za przezroczystosc
            //        grp.FillPolygon(new SolidBrush(Color.FromArgb(125,0,0,255)), triangle,FillMode.Winding);

            //    }
            //}

            ////wyswietlanie na ekranie wczesniej obliczonych (w tle) ksztaltow
            //graphShowOnScreen.DrawImage(btm, coordinatesForBitmap);
        }
    }

    public class RocketParameters
    {

        //Zmienne do algorytmów genetycznych
        public float fitness;
        public float normalizedFitness;


        //Zmienne rakiety
        public float previousCoordinatesX;
        public float previousCoordinatesY;


        public float[] angle;
        public float[] angleCopy;
        public float[] force;
        float[] forceBackup;


        float friction = 1.05f;
        public int counter;
        public int index;

        public bool crashed = false;
        public bool hitTarget = false;

        // static public int size = 40;
        // public float[] crdX = new float[size];
        //  public float[] crdY = new float[size];
        //  public float[] velocity = new float[size + 1];
        public List<float> coordinatesX = new List<float>();
        public List<float> coordinatesY = new List<float>();
        public List<float> velocityList = new List<float>();


        public RocketParameters(int lifeSpan)
        {
            angle = new float[lifeSpan];
            force = new float[lifeSpan];
            forceBackup = new float[lifeSpan];
            angleCopy = new float[lifeSpan];
            counter = 0;
            index = 0;
        }

        public void ApplyForce()
        {

            coordinatesX.Clear();
            coordinatesY.Clear();
            velocityList.Clear();


            velocityList.Add(new float());
            velocityList[0] = this.force[index];
            forceBackup[index] = this.force[index];
            while (this.forceBackup[index] >= 2.2)
            {
                this.forceBackup[index] /= friction;

                coordinatesX.Add(new float());
                coordinatesY.Add(new float());
                velocityList.Add(new float());

            }


            for (int i = 0; i < coordinatesX.Count; i++)
            {
                coordinatesX[i] = Convert.ToSingle((Math.Cos(((this.angle[index] + 270) * Math.PI) / 180)) * velocityList[i]) + previousCoordinatesX;
                coordinatesY[i] = Convert.ToSingle((Math.Sin(((this.angle[index] + 270) * Math.PI) / 180)) * velocityList[i]) + previousCoordinatesY;
                previousCoordinatesX = coordinatesX[i];
                previousCoordinatesY = coordinatesY[i];
                velocityList[i + 1] = velocityList[i] / friction;

                //anglePart[index] += anglePart[index];
            }
            try
            {
                angle[index + 1] += angle[index];
            }
            catch (Exception) { }

            index++;


        }

    }



}



//---------obracanie i przemieszczanie--------

//--------------sposob 1--------------------
/* 
 * przypisanie zmiennej typu Graphics, w ktorym elemencie maja byc rysowanie obiekty
 * nastepnie za pomoca metody RotateTransform przypisuje sie obrot o zadany kąt
 * TranslateTransform pozwala przemieszczac obrocony obiekt
 * rysowanie obiektu w srodku ciezkosci
 * 
 *

graph = panel1.CreateGraphics();
graph.RotateTransform(i);
graph.TranslateTransform(i2, i2,MatrixOrder.Append);
graph.FillRectangle(new SolidBrush(Color.Blue), -15, -2.5F, 30, 5);
//graph.TranslateTransform(35, 22.5F);
//graph.RotateTransform(10);
i2 += 1;
i += 10;

graph2 = panel1.CreateGraphics();
graph2.RotateTransform(i3);
graph2.TranslateTransform(i4, i4, MatrixOrder.Append);
graph2.FillRectangle(new SolidBrush(Color.Blue), -15, -2.5F, 30, 5);
i3 -= 10;
i4 -= 1;



    //---------------------sposob 2----------------------
       graph = panel1.CreateGraphics();
       Matrix myMatrix = new Matrix();
       myMatrix.Rotate(i, MatrixOrder.Append);
       myMatrix.Translate(i2, i2, MatrixOrder.Append);
       graph.Transform = myMatrix;
       graph.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 30, 5);
       i += 10;
       i2++;

*/
