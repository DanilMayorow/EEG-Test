using System;
using System.Windows.Forms;
using System.Threading;
using libStreamSDK;
using System.Collections;
using System.Collections.Generic;
using AForge.Neuro;
using AForge.Neuro.Learning;
using System.Linq;
using Common;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Statistics.Models.Regression.Linear;
using Accord.MachineLearning;
using Accord.Math.Random;
using System.Drawing;

namespace EEG_Test
{

    public partial class Form : System.Windows.Forms.Form
    {
        #region Переменные
        public int errCode = 0;
        private Thread mainThread;
        private Double[] dataAtt = new Double[1000];
        private Double[] dataMed = new Double[1000];
        public Int32[] signalVal = new Int32[10] { 2, 3, 5, 6, 7, 8, 9, 10, 11, 12 };
        public List<Signal> Signals = new List<Signal>();
        public Boolean START = false;
        public Boolean INIT = false;
        public Boolean controllType = false;
        public Double bufAtt = 0;
        public Double bufMed = 0;
        public Boolean A, M;
        public Boolean newData;
        public static Double[][] Data;
        public Int32 readyFor = 0;
        public static Int32 sizeFor = 15;
        public Boolean Clust = false;
        public Boolean[][] Graph = new Boolean[101][];
        public Int32 Att = 0;
        public Int32 Med = 1;
        public Boolean Refr = false;

        Boolean Run = true;
        DateTime Date;
        float Cm = 0, Ca = 0;
        public long Tick;
        #endregion

        /* Инициализация регрессоров
        //AForge libary
        BackPropagationLearning teachAtt,teachMed,teacher;
        protected Int32[] Configuration = new Int32[] { 2, 6, 2 };
        public ActivationNetwork netAtt,netMed,network;
        public Random rnd = new Random(2);
        public int IterationsCount = 80;
        //public Double[][] LearnAtt;
        //public Double[][] LearnMed;
        public Double[][] Inputs;
        public Double[][] Outputs;
        public Double[] EndLearnAtt = new Double[100];
        public Double[] EndLearnMed = new Double[100];

        
        //Accord MLR
        MultipleLinearRegression MLR;
        Double [][] linearCoeff;

        //Accord PR
        public PolynomialRegression PR;
        public Double [][] polynomCoeff;
        public Int32 Grade = 4;

        private Double[] linAtt = new Double[100];
        private Double[] linMed = new Double[100];
        private Double[] polyAtt = new Double[100];
        private Double[] polyMed = new Double[100];
        
        */

        public static int clustSize = 5;
        public KMeans kmeans = new KMeans(clustSize);

        public KMeansClusterCollection clusters;
        public int[][] Clusters = new int[101][];

        #region SOM

        public class MyUserControl : UserControl
        {
            public MyUserControl()
            {
                DoubleBuffered = true;
            }
        }

        public static int NetworkWidth = 7;
        public static int NetworkHeight = 7;
        public int LearningRadius = 3;
        public double LearningRate = 0.2;
        public double NoiseLevel = 0;
        static DistanceNetwork network;
        static SOMLearning learning;
        static MapElement[,] map;
        static MyUserControl pointsPanel;
        static MyUserControl networkPanel;
        static MyUserControl networkGraphControl;
        static int[,] space;
        static Bitmap spaceBitmap;
        static int selected = -1;
        static double[][] Inputs;
        public static Random Rnd = new Random(1);
        static int iterationsBetweenDrawing = 10;

        class MapElement
        {
            public float X;
            public float Y;
            public int Id;
            public Point DisplayLocation;
            public bool IsActive;
            public int MapX { get { return Id / NetworkHeight; } }
            public int MapY { get { return Id % NetworkHeight; } }

        }

        void GenerateInputs(List<double[]> points, double x, double y, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var r = Rnd.NextDouble() * NoiseLevel;
                var angle = Rnd.NextDouble() * Math.PI * 2;
                var xx = x + r * Math.Cos(angle);
                var yy = y + r * Math.Sin(angle);
                points.Add(new[] { xx, yy });
            }

        }

        public virtual List<double[]> GenerateInputs()
        {
            var list = new List<double[]>();
            for(int i=0;i<sizeFor;i++)
                GenerateInputs(list, Data[i][1]/100, 1-Data[i][0]/100, 1);
            /*GenerateInputs(list, 0.2, 0.2, 50);
            GenerateInputs(list, 0.8, 0.8, 50);
            GenerateInputs(list, 0.2, 0.8, 50);
            GenerateInputs(list, 0.8, 0.2, 50);*/
            return list;
        }
        #endregion

        public Form()
        {
            InitializeComponent();
            rbCheck();
            chartType();
            Accord.Math.Random.Generator.Seed = 0;
            for (int i = 0; i < 101; i++)
            {
                Graph[i] = new Boolean[101];
                Clusters[i] = new int[101];
                for (int j=0;j<100;j++)
                {
                    Graph[i][j] = false;
                    Clusters[i][j] = 0;
                }
            }

            #region Инициализация персептрона
            /*
            netAtt = new ActivationNetwork(new SigmoidFunction(0.2), Configuration[0], Configuration.Skip(1).ToArray());
            netAtt.ForEachWeight(z => rnd.NextDouble());
            teachAtt = new BackPropagationLearning(netAtt);
            teachAtt.LearningRate = 1;

            netMed = new ActivationNetwork(new SigmoidFunction(0.2), Configuration[0], Configuration.Skip(1).ToArray());
            netMed.ForEachWeight(z => rnd.NextDouble());
            teachMed = new BackPropagationLearning(netMed);
            teachMed.LearningRate = 1;
            
            network = new ActivationNetwork(new SigmoidFunction(6), Configuration[0], Configuration.Skip(1).ToArray());
            network.ForEachWeight(z => rnd.NextDouble()*2-1);
            teacher = new BackPropagationLearning(network);
            teacher.LearningRate = 1;
            */
            #endregion

            network = new DistanceNetwork(2,  NetworkWidth *  NetworkHeight);
            for (int x = 0; x <  NetworkWidth; x++)
                for (int y = 0; y <  NetworkHeight; y++)
                {
                    var n = network.Layers[0].Neurons[x *  NetworkHeight + y];
                    n.Weights[0] = Rnd.NextDouble() * 0.2 + 0.4;
                    n.Weights[1] = Rnd.NextDouble() * 0.2 + 0.4;
                }
            learning = new SOMLearning(network,  NetworkWidth,  NetworkHeight);
            learning.LearningRadius =  LearningRadius;
            learning.LearningRate =  LearningRate;

            Timer = new System.Windows.Forms.Timer();
            Timer.Tick += (sender, args) => { Learning(); this.Invalidate(true); };
            Timer.Interval = 1000;
        }

        private void getMindData()
        {
            NativeThinkgear thinkgear = new NativeThinkgear();
            Console.WriteLine("Version: " + NativeThinkgear.TG_GetVersion());

            int connectionID = NativeThinkgear.TG_GetNewConnectionId();
            Console.WriteLine("Connection ID: " + connectionID);

            if (connectionID < 0)
            {
                Console.WriteLine("ERROR: TG_GetNewConnectionId() returned: " + connectionID);
                return;
            }

            errCode = NativeThinkgear.TG_SetStreamLog(connectionID, "streamLog.txt");
            Console.WriteLine("errCode for TG_SetStreamLog : " + errCode);
            if (errCode < 0)
            {
                Console.WriteLine("ERROR: TG_SetStreamLog() returned: " + errCode);
                return;
            }

            errCode = NativeThinkgear.TG_SetDataLog(connectionID, "dataLog.txt");
            Console.WriteLine("errCode for TG_SetDataLog : " + errCode);
            if (errCode < 0)
            {
                Console.WriteLine("ERROR: TG_SetDataLog() returned: " + errCode);
                return;
            }

            string comPortName = "\\\\.\\COM4";

            errCode = NativeThinkgear.TG_Connect(connectionID,
                          comPortName,
                          NativeThinkgear.Baudrate.TG_BAUD_57600,
                          NativeThinkgear.SerialDataFormat.TG_STREAM_PACKETS);
            if (errCode < 0)
            {
                Console.WriteLine("ERROR: TG_Connect() returned: " + errCode);
                return;
            }

            int packetsRead = 0;
            while (packetsRead < 10)
            {
                errCode = NativeThinkgear.TG_ReadPackets(connectionID, 1);
                Console.WriteLine("TG_ReadPackets returned: " + errCode);
                if (errCode == 1)
                {
                    packetsRead++;
                    if (NativeThinkgear.TG_GetValueStatus(connectionID, NativeThinkgear.DataType.TG_DATA_ATTENTION) != 0)
                    {

                        /* Get and print out the updated attention value */
                        Console.WriteLine("New ATT value: : " + (int)NativeThinkgear.TG_GetValue(connectionID, NativeThinkgear.DataType.TG_DATA_ATTENTION));
                    }
                } 
            }

            Console.WriteLine("Preparation is complete");
            errCode = NativeThinkgear.TG_EnableAutoRead(connectionID, 1);
            packetsRead = 0;
            if (errCode == 0)
            {
                Date = DateTime.Now;
                NativeThinkgear.MWM15_setFilterType(connectionID, NativeThinkgear.FilterType.MWM15_FILTER_TYPE_50HZ);
                while (Run)
                {
                    if (!controllType)
                    {
                        newData = false;
                        for (int i = 0; i < Signals.Count; i++)
                        {
                            if (NativeThinkgear.TG_GetValueStatus(connectionID, Signals[i].Type) != 0)
                            {
                                Console.WriteLine("New Signal(" + Signals[i].Name + "): " + (int)NativeThinkgear.TG_GetValue(connectionID, Signals[i].Type));
                                Double value = (Double)NativeThinkgear.TG_GetValue(connectionID, Signals[i].Type);
                                Signals[i].Add(value);
                                newData = true;
                            }
                        }
                        if (Chart.IsHandleCreated&&newData)
                        {
                            this.Invoke((MethodInvoker)delegate { UpdateChart(); });
                        }
                    }
                    else
                    {
                        if (NativeThinkgear.TG_GetValueStatus(connectionID, NativeThinkgear.DataType.TG_DATA_ATTENTION) != 0)
                        {
                            bufAtt = (Double)NativeThinkgear.TG_GetValue(connectionID, NativeThinkgear.DataType.TG_DATA_ATTENTION);
                            Console.WriteLine("New ATT value#" + Ca + ": " + (int)bufAtt);
                            if (bufAtt != 0)
                            {
                                A = true;
                                Ca++;
                            }
                            else
                            {
                                A = false;
                            }
                        }
                        else
                        {
                            A= false;
                        }

                        if (NativeThinkgear.TG_GetValueStatus(connectionID, NativeThinkgear.DataType.TG_DATA_MEDITATION) != 0)
                        {
                            bufMed = (Double)NativeThinkgear.TG_GetValue(connectionID, NativeThinkgear.DataType.TG_DATA_MEDITATION);
                            Console.WriteLine("New MED value#" + Cm + ":  " + (int)bufMed);
                            if (bufMed != 0)
                            {
                                M = true;
                                Cm++;
                            }
                            else
                            {
                                M = false;
                            }
                        }
                        else
                        {
                            M= false;
                        }

                        if (A || M)
                        {
                            dataAtt[readyFor] = bufAtt;
                            dataMed[readyFor] = bufMed;
                            Graph[(int)bufAtt][(int)bufMed] = true;
                            readyFor++;
                            if (Clust)
                            {
                                addPoint((int)bufAtt, (int)bufMed);
                            }
                            if (readyFor == sizeFor) { Clusterisation(); Clust = true; }
                            if (Chart.IsHandleCreated)
                            {
                                this.Invoke((MethodInvoker)delegate { UpdateChart(); });
                            }
                        }
                    }
                    packetsRead++;
                }
                NativeThinkgear.TG_Disconnect(connectionID);

                NativeThinkgear.TG_FreeConnection(connectionID);
            }
            else
            {
                Console.WriteLine("Disable to read");
                return;
            }
            ListSign.Enabled = true;
        }

        /* Старая функция обновления графика
        private void UpdateChart()
        {

            if (controllType)
            {
                Chart.Series["Сосредоточенность"].Points.Clear();
                Chart.Series["Расслабленность"].Points.Clear();
                if(Clust)
                {
                    ForChart.Series["Р(ЛР)"].Points.Clear();
                    ForChart.Series["Р(ПР)"].Points.Clear();
                    ForChart.Series["Р(НР)"].Points.Clear();
                    ForChart.Series["С(ЛР)"].Points.Clear();
                    ForChart.Series["С(ПР)"].Points.Clear();
                    ForChart.Series["С(НР)"].Points.Clear();
                }

                for (int i = 0; i < dataAtt.Length - 1; ++i)
                {
                    Chart.Series["Сосредоточенность"].Points.AddY(dataAtt[i]);
                    Chart.Series["Расслабленность"].Points.AddY(dataMed[i]);

                    if (Clust)
                    {
                        ForChart.Series["С(ЛР)"].Points.AddY(linAtt[i]);
                        ForChart.Series["С(ПР)"].Points.AddY(polyAtt[i]);
                        ForChart.Series["С(НР)"].Points.AddY(EndLearnAtt[i]);
                        ForChart.Series["Р(ЛР)"].Points.AddY(linMed[i]);
                        ForChart.Series["Р(ПР)"].Points.AddY(polyMed[i]);
                        ForChart.Series["Р(НР)"].Points.AddY(EndLearnMed[i]);
                    }
                }
               // Console.Write(dataAtt[99] + " " + dataMed[99]);
                if (Clust)
                {
                    Console.Write("C: " + linAtt[99].ToString("F2") + " " + polyAtt[99].ToString("F2") + " " + EndLearnAtt[99].ToString("F2"));
                    Console.Write("Р: " + linMed[99].ToString("F2") + " " + polyMed[99].ToString("F2") + " " + EndLearnMed[99].ToString("F2"));
                }
                Console.WriteLine("");
                
            }
            else
            {
                for (int k = 0; k < Signals.Count; k++)
                {
                    Chart.Series[Signals[k].Name].Points.Clear();
                    for (int j = 0; j < 100; j++)
                    {
                        Chart.Series[Signals[k].Name].Points.AddY(Signals[k].ChartElement(j));
                    }
                }
            }
        }
        */

        private void UpdateChart()
        {
            Chart.Series["Данные"].Points.Clear();
            for (int i=0;i<100;i++)
            {
                for (int j=0;j<100;j++)
                {
                    if (Graph[i][j])
                    {
                        Chart.Series["Данные"].Points.AddXY(j, i);
                        if (Clust) { ForChart.Series["Кластер " + Clusters[i][j]].Points.AddXY(j, i); }
                    }
                }
            }
        }

        private void ButStart_Click(object sender, EventArgs e)
        {
            ListSign.Enabled = false;
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            if (!INIT)
            {
               if (!controllType)
               {
                 UpdateList();
                 NewChart();
               }
               INIT = !INIT;
               Date = DateTime.Now;
               Tick = 0;
               timer.Interval = 10;
               timer.Tick += new EventHandler(Ticks);
            
               mainThread = new Thread(new ThreadStart(this.getMindData));
               mainThread.IsBackground = true;
            }

            if (!START)
            {
                Console.WriteLine((int)mainThread.ThreadState);
                if (mainThread.ThreadState == (System.Threading.ThreadState)12) { mainThread.Start(); }
                else {mainThread.Resume();}
                selected = -1;
                spaceBitmap = null;
                timer.Start();
                START = !START;
            }
            else
            {
                mainThread.Suspend();
                timer.Stop();
                MakeSpace();
                pointsPanel.MouseMove += PointMouseMove;
                networkPanel.MouseMove += NetworkMouseMove;
                START = !START;
            }

        }

        private void Ticks(object Sender,EventArgs e)
        {
            if (START)
            {
                //long tick = DateTime.Now.Ticks - Date.Ticks;
                //Console.WriteLine(tick);
                Tick += 1000000/6;
                DateTime Watch = new DateTime();

                Watch = Watch.AddTicks(Tick);
                TimeLable.Text = String.Format("{0:HH:mm:ss:ff}", Watch);
                if (Refr)
                {
                    #region Создание элементов интерфейса
                    pointsPanel = new MyUserControl() { Dock = DockStyle.Fill };
                    pointsPanel.Paint += DrawPoints;
                    networkPanel = new MyUserControl() { Dock = DockStyle.Fill };
                    networkPanel.Paint += DrawNetwork;
                    networkGraphControl = new MyUserControl { Dock = DockStyle.Fill };
                    networkGraphControl.Paint += DrawGraph;

                    table.Controls.Add(pointsPanel, 0, 0);
                    table.Controls.Add(networkPanel, 0, 1);
                    table.Controls.Add(networkGraphControl, 0, 2);
                    #endregion
                    Refr = false;
                    Timer.Start();
                }
            }
        }

        private void UpdateList()
        {
            IEnumerator myEnumerator;
            myEnumerator = ListSign.CheckedIndices.GetEnumerator();
            int y;
            while (myEnumerator.MoveNext() != false)
            {
                y = (int)myEnumerator.Current;
                if (ListSign.GetItemChecked(y))
                {
                    Signal S = new Signal(signalVal[y],ListSign.Items[y].ToString());
                    Signals.Add(S);
                }
            }
            Signals.Sort(delegate(Signal s1,Signal s2) { return s1.Type.CompareTo(s2.Type); });
        }

        private void NewChart()
        {
            for (int i = 0; i < Signals.Count; i++)
            {
                Console.WriteLine("Новый сигнал типа:" + Signals[i].Name);
                Chart.Series.Add(Signals[i].Name);
                Chart.Series[Signals[i].Name].Points.Clear();
                Chart.Series[Signals[i].Name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            }
        }

        private void testRB_CheckedChanged(object sender, EventArgs e)
        {
            rbCheck();
        }

        private void controllRB_CheckedChanged(object sender, EventArgs e)
        {
            rbCheck();
        }

        public void rbCheck()
        {
            if (testRB.Checked == true)
            {
                controllType = false;
                ListSign.Enabled = true;
            }
            else if (controllRB.Checked == true)
            {
                controllType = true;
                ListSign.Enabled = false;
            }
            //chartType();
        }

        private void ListSign_SelectedIndexChanged(object sender, EventArgs e)
        {
            //chartType();
        }

        /* Функция выбора типа считывания данных
        public void chartType()
        {
            Chart.Series.Clear();
            ForChart.Series.Clear();
            if (controllType)
            {
                Chart.Series.Add("Сосредоточенность");
                Chart.Series["Сосредоточенность"].Points.Clear();
                Chart.Series["Сосредоточенность"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                Chart.Series.Add("Расслабленность");
                Chart.Series["Расслабленность"].Points.Clear();
                Chart.Series["Расслабленность"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("С(ЛР)");
                ForChart.Series["С(ЛР)"].Points.Clear();
                ForChart.Series["С(ЛР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("С(ПР)");
                ForChart.Series["С(ПР)"].Points.Clear();
                ForChart.Series["С(ПР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("С(НР)");
                ForChart.Series["С(НР)"].Points.Clear();
                ForChart.Series["С(НР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("Р(ЛР)");
                ForChart.Series["Р(ЛР)"].Points.Clear();
                ForChart.Series["Р(ЛР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("Р(ПР)");
                ForChart.Series["Р(ПР)"].Points.Clear();
                ForChart.Series["Р(ПР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("Р(НР)");
                ForChart.Series["Р(НР)"].Points.Clear();
                ForChart.Series["Р(НР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                
            }
        }
        */

        public void chartType()
        {
            Chart.Series.Clear();
            ForChart.Series.Clear();
            Chart.Series.Add("Данные");
            Chart.Series["Данные"].Points.Clear();
            Chart.Series["Данные"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            for (int i=0;i<clustSize;i++)
            {
                String str = "Кластер " + (i + 1);
                Console.WriteLine(str);
                ForChart.Series.Add(str);
                ForChart.Series[str].Points.Clear();
                ForChart.Series[str].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            }
        }
        
        /* Обучение персептрона
        public void Learn()
        {

            int counter = 0;
            while (true)
            {
                var watch = new Stopwatch();
                watch.Start();
                while (watch.ElapsedMilliseconds < 200)
                {
                    LearningIteration();
                    AccountError();
                    counter++;
                    if (counter > IterationsCount) break;

                }
                watch.Stop();
                //LearningEnds();
                if (counter > IterationsCount) break;

            }
        }

        protected virtual void LearningIteration()
        {
            teacher.RunEpoch(Inputs,Outputs);
            /*
            teachAtt.RunEpoch(LearnMed, LearnAtt);
            teachMed.RunEpoch(LearnAtt, LearnMed);
            
        }

        protected virtual void AccountError()
        {
            //LearningErrors.Enqueue(GetError(LearningInputs, LearningAnswers));
        }

        protected virtual void LearningEnds()
        {
            
            EndLearnAtt = LearnMed.Select(z => netAtt.Compute(z)[0]).ToArray();
            EndLearnMed = LearnAtt.Select(z => netMed.Compute(z)[0]).ToArray();
            
        }

        protected Double GetError(Double[][] Inputs, Double[][] Answers,ActivationNetwork network)
        {
            Double sum = 0;
            for (int i = 0; i < Inputs.Length; i++)
            {
                sum += Math.Abs(network.Compute(Inputs[i])[0] - Answers[i][0]);
            }
            sum /= Inputs.Length;
            return sum;
        }
        */

        private void Clusterisation()
        {
            Console.WriteLine("CL");
            Data = new Double[sizeFor][];
            for (int i = 0; i < sizeFor; i++)
            {
                Data[i] = new Double[2];
                Data[i][Att] = dataAtt[i];
                Data[i][Med] = dataMed[i];
            }
            clusters = kmeans.Learn(Data);
            for (int k=0;k<sizeFor;k++)
            {
                addPoint((int)dataAtt[k], (int)dataMed[k]);
            }
            Inputs = GenerateInputs().ToArray();
            Refr = true;
        }

        public void addPoint(int x, int y)
        {
            Double[][] d = { new Double[] { x, y } };
            int[] cl = clusters.Decide(d);
            Clusters[x][y] = cl[0]+1;
            Console.WriteLine("Новая точка кластера #" + cl[0] + 1);
        }

        /* Функции прогнозирования
        public void Forecast()
        {
            linearCoeff = new Double[2][];
            linearCoeff[Att] = new Double[2];
            linearCoeff[Med] = new Double[2];
            polynomCoeff = new Double[2][];

            var ols = new OrdinaryLeastSquares()
            {
                UseIntercept = true
            };

            var ls = new PolynomialLeastSquares()
            {
                Degree = Grade
            };

            Double[] outputs = new Double[sizeFor];
            Double[][] inputs = new Double[sizeFor][];
            Double[] _inputs = new Double[sizeFor];

            for (int l = 0; l < sizeFor; l++)
                outputs[l] = Data[Att][l];

            for (int l = 0; l < sizeFor; l++)
            {
                inputs[l] = new Double[1];
                inputs[l][0] = Data[Med][l];
                _inputs[l]=Data[Med][l];
            }

            MLR = ols.Learn(inputs, outputs);
            linearCoeff[Att][0] = MLR.Weights[0];
            linearCoeff[Att][1] = MLR.Intercept;
            PR = ls.Learn(_inputs, outputs);
            polynomCoeff[Att] = PR.Coefficients;

            for (int l = 0; l < sizeFor; l++)
                outputs[l] = Data[Med][l];
            for (int l = 0; l < sizeFor; l++)
            {
                inputs[l][0] = Data[Att][l];
                _inputs[l] = Data[Att][l];
            }
            MLR = ols.Learn(inputs, outputs);
            linearCoeff[Med][0] = MLR.Weights[0];
            linearCoeff[Med][1] = MLR.Intercept;
            PR = ls.Learn(_inputs, outputs);
            polynomCoeff[Med] = PR.Coefficients;

            for (int i = 0; i < 2; i++)
            {
                Console.Write("AttLinCoeff:");
                for (int l = 0; l < linearCoeff[Att].Length; l++)
                {
                    Console.Write(" " + linearCoeff[Att][l].ToString("F2"));
                }
                Console.Write("\nMedLinCoeff:");
                for (int l = 0; l < linearCoeff[Med].Length; l++)
                {
                    Console.Write(" " + linearCoeff[Med][l].ToString("F2"));
                }
                Console.Write("\nAttPolyCoeff:");
                for (int l = 0; l < polynomCoeff[Att].Length; l++)
                {
                    Console.Write(" " + polynomCoeff[Att][l].ToString("F2"));
                }
                Console.Write("\nMedPolyCoeff:");
                for (int l = 0; l < polynomCoeff[Med].Length; l++)
                {
                    Console.Write(" " + polynomCoeff[Med][l].ToString("F2"));
                }
                Console.WriteLine("");
            }

            Learn();
        }

        public void toData()
        {
            Data = new Double[2][];
            //LearnAtt = new Double[sizeFor][];
            //LearnMed = new Double[sizeFor][];
            Inputs= new Double[sizeFor][];
            Outputs=new Double[sizeFor][];

            Data[Att] = new Double[sizeFor];
            Data[Med] = new Double[sizeFor];
            for (int j = 0; j < sizeFor; j++)
            {
                Data[Att][j] = dataAtt[100-sizeFor+j];
                Data[Med][j] = dataMed[100-sizeFor+j];

                Inputs[j] = new Double[2];
                Outputs[j] = new Double[2];
                Inputs[j][Att] = Data[Med][j] / 100;
                Inputs[j][Med] = Data[Att][j] / 100;
                Outputs[j][Att] = Data[Att][j] / 100;
                Outputs[j][Med] = Data[Med][j] / 100;

                
                LearnAtt[j] = new Double[1];
                LearnMed[j] = new Double[1];
                LearnAtt[j][0] = Data[Att][j]/100;
                LearnMed[j][0] = Data[Med][j]/100;
                
            }
        }
       
        public void toForc()
        {
            Double Ax = linearCoeff[Att][0] * bufMed;
            Double b = linearCoeff[Att][1];
            linAtt[linAtt.Length - 1] =  Ax+b;
            Array.Copy(linAtt, 1, linAtt, 0, linAtt.Length - 1);

            polyAtt[polyAtt.Length - 1] = culcPoly(Att, bufMed);
            Array.Copy(polyAtt, 1, polyAtt, 0, polyAtt.Length - 1);

            Ax = linearCoeff[Med][0] * bufAtt;
            b = linearCoeff[Med][1];
            linMed[linMed.Length - 1] = Ax + b;
            Array.Copy(linMed, 1, linMed, 0, linMed.Length - 1);

            polyMed[polyMed.Length - 1] = culcPoly(Med,bufAtt);
            Array.Copy(polyMed, 1, polyMed, 0, polyMed.Length - 1);


            /*
            Double[] A = new Double[1];
            Double[] M = new Double[1];
            A[0] = bufAtt; M[0] = bufMed;

            EndLearnAtt[EndLearnAtt.Length - 1] = netAtt.Compute(M)[0]*100;
            Array.Copy(EndLearnAtt, 1, EndLearnAtt, 0, EndLearnAtt.Length - 1);

            EndLearnMed[EndLearnMed.Length - 1] = netMed.Compute(A)[0]*100;
            Array.Copy(EndLearnMed, 1, EndLearnMed, 0, EndLearnMed.Length - 1);
            
            Double[] In = new Double[2];
            In[0] = bufMed; In[1] = bufAtt;
            Double[] Out= network.Compute(In);
            EndLearnAtt[EndLearnAtt.Length - 1] = Out[0]*100;
            Array.Copy(EndLearnAtt, 1, EndLearnAtt, 0, EndLearnAtt.Length - 1);

            EndLearnMed[EndLearnMed.Length - 1] = Out[1]*100;
            Array.Copy(EndLearnMed, 1, EndLearnMed, 0, EndLearnMed.Length - 1);

        }

        public Double culcPoly(Int32 ind, Double val)
        {
            Double sum=0;
            for (int i=0;i<Grade;i++)
            {
                sum += polynomCoeff[ind][i] * Math.Pow(val, Grade - i);
            }
            if (sum > 100) { sum = 100; }
            return sum;
        }
        */

        #region Рисование

        static Color GetColor(int mapX, int mapY)
        {
            return Color.FromArgb(200 - 200 * mapY / NetworkHeight, 150, 200 - 200 * mapX / NetworkWidth);
        }

        static Brush GetBrush(MapElement element)
        {
            if (element.Id == selected) return Brushes.Magenta;
            else if (element.IsActive) return new SolidBrush(GetColor(element.MapX, element.MapY));
            else return Brushes.LightGray;
        }



        static void DrawGraph(object sender, PaintEventArgs args)
        {
            Console.WriteLine("Create graph start");
            if (map == null) return;
            var g = args.Graphics;
            var W = pointsPanel.ClientSize.Width - 20;
            var H = pointsPanel.ClientSize.Height - 20;
            g.Clear(Color.White);
            g.TranslateTransform(10, 10);
            var pen = new Pen(Color.FromArgb(100, Color.LightGray));
            foreach (var e in map)
            {
                if (e.MapX != NetworkWidth - 1)
                    g.DrawLine(pen, W * e.X, H * e.Y, W * map[e.MapX + 1, e.MapY].X, H * map[e.MapX + 1, e.MapY].Y);
                if (e.MapY != NetworkHeight - 1)
                    g.DrawLine(pen, W * e.X, H * e.Y, W * map[e.MapX, e.MapY + 1].X, H * map[e.MapX, e.MapY + 1].Y);
            }

            foreach (var e in map)
            {
                g.FillEllipse(GetBrush(e),
                    e.X * W - 3,
                    e.Y * W - 3,
                    6,
                    6);

            }

            Console.WriteLine("Graph create");
        }

        static void DrawPoints(object sender, PaintEventArgs aegs)
        {
            var g = aegs.Graphics;
            g.Clear(Color.White);

            var W = pointsPanel.ClientSize.Width;
            var H = pointsPanel.ClientSize.Height;

            if (spaceBitmap != null)
            {
                g.DrawImage(spaceBitmap, 0, 0);

                if (selected != -1)
                {
                    var highlight = new SolidBrush(Color.FromArgb(100, Color.White));
                    for (int x = 0; x < W; x++)
                        for (int y = 0; y < H; y++)
                            if (space[x, y] == selected)
                                g.FillRectangle(highlight, x, y, 1, 1);
                }
            };


            foreach (var e in Inputs)
            {
                g.FillEllipse(Brushes.Black,
                    (int)(W * e[0]) - 2,
                    (int)(H * e[1]) - 2,
                    4, 4);
            }
            Console.WriteLine("Points create");
        }

        static void DrawConnection(Graphics g, MapElement n1, MapElement n2)
        {
            Console.WriteLine("Create connection start");
            if (!n1.IsActive || !n2.IsActive) return;
            var distance = Math.Sqrt(Math.Pow(n1.X - n2.X, 2) + Math.Pow(n1.Y - n2.Y, 2));
            distance = Math.Min(1, distance * 5);
            var Pen = new Pen(Color.FromArgb((int)(distance * 128 + 120), GetColor(n1.MapX, n1.MapY)), 2);
            g.DrawLine(Pen, n1.DisplayLocation, n2.DisplayLocation);
            Console.WriteLine("Connections create");
        }

        static void DrawNetwork(object sender, PaintEventArgs aegs)
        {
            Console.WriteLine("Create network start");
            if (map == null) return;
            var W = pointsPanel.ClientSize.Width - 20;
            var H = pointsPanel.ClientSize.Height - 20;

            var g = aegs.Graphics;
            for (int x = 0; x < NetworkWidth; x++)
                for (int y = 0; y < NetworkHeight; y++)
                    map[x, y].DisplayLocation = new Point(10 + x * W / NetworkWidth, 10 + y * H / NetworkHeight);

            for (int x = 0; x < NetworkWidth; x++)
                for (int y = 0; y < NetworkHeight; y++)
                {
                    if (x != NetworkWidth - 1) DrawConnection(g, map[x, y], map[x + 1, y]);
                    if (y != NetworkHeight - 1) DrawConnection(g, map[x, y], map[x, y + 1]);
                    g.FillEllipse(
                       GetBrush(map[x, y]),
                       map[x, y].DisplayLocation.X - 5,
                       map[x, y].DisplayLocation.Y - 5,
                       10, 10);
                }
            Console.WriteLine("Draw network complete");
        }
        #endregion

        #region Исследование после обучения

        static System.Windows.Forms.Timer Timer;
        static bool paused;

        private void PauseResume(object sender, EventArgs e)
        {
            paused = !paused;
            if (paused)
            {
                Timer.Stop();
                MakeSpace();
                pointsPanel.MouseMove += PointMouseMove;
                networkPanel.MouseMove += NetworkMouseMove;
            }
            else
            {
                selected = -1;
                spaceBitmap = null;
                Timer.Start();
            }
            this.Invalidate(true);
        }


        static void MakeSpace()
        {
            var Colors = new[] { Color.Orange, Color.LightGray, Color.LightGreen, Color.LightBlue, Color.LightYellow, Color.LightCoral, Color.LightCyan };

            space = new int[pointsPanel.ClientSize.Width, pointsPanel.ClientSize.Height];
            spaceBitmap = new Bitmap(pointsPanel.ClientSize.Width, pointsPanel.ClientSize.Height);
            for (int x = 0; x < spaceBitmap.Width; x++)
                for (int y = 0; y < spaceBitmap.Height; y++)
                {
                    network.Compute(new double[] { (double)x / spaceBitmap.Width, (double)y / spaceBitmap.Height });
                    var winner = network.GetWinner();
                    space[x, y] = winner;
                    var n = map.Cast<MapElement>().Where(z => z.Id == winner).First();
                    if (n.IsActive)
                        spaceBitmap.SetPixel(x, y, GetColor(n.MapX, n.MapY));
                }

        }


        private void PointMouseMove(object sender, MouseEventArgs e)
        {
            selected = space[e.X, e.Y];
            this.Invalidate(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Timer.Start();
        }

        private void NetworkMouseMove(object sender, MouseEventArgs e)
        {
            if (map == null) return;
            foreach (var m in map)
            {
                if (Math.Abs(e.X - m.DisplayLocation.X) < 5 && Math.Abs(e.Y - m.DisplayLocation.Y) < 5)
                {
                    if (selected != m.Id)
                    {
                        selected = m.Id;
                        this.Invalidate(true);
                    }
                    return;
                }
            }
            if (selected != -1)
            {
                selected = -1;
                this.Invalidate(true);
            }

        }
        #endregion

        #region Обучение сети 


        static void Learning()
        {
            Console.WriteLine("L");
            for (int i = 0; i < iterationsBetweenDrawing; i++)
                learning.Run(Inputs[Rnd.Next(Inputs.Length)]);


            map = new MapElement[NetworkWidth,NetworkHeight];
            int number = 0;
            for (int x = 0; x < NetworkWidth; x++)
                for (int y = 0; y < NetworkHeight; y++)
                {
                    var neuron = network.Layers[0].Neurons[x * NetworkHeight + y];
                    map[x, y] = new MapElement { X = (float)neuron.Weights[0], Y = (float)neuron.Weights[1], Id = number++ };
                }

            foreach (var e in Inputs)
            {
                network.Compute(e);
                var winner = network.GetWinner();
                map[winner / NetworkHeight, winner % NetworkHeight].IsActive = true;
            }
        }




        #endregion
    }
    public class Signal
    {
        public Signal()
        {
            Name = "None";
            Type = 0;
            for (int i = 0; i < 100; i++)
            {
                Chart[i] = 0;
            }
        }
        public Signal(Int32 _type, String _name)
        {
            Name = _name;
            Type = _type;
            for (int i = 0; i < 100; i++)
            {
                Chart[i] = 0;
            }
        }
        public void Add(Double _val)
        {
            Chart[99] = _val;
            Array.Copy(Chart, 1, Chart, 0, 99);
        }
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        public Int32 Type
        {
            get { return type; }
            set { type = value; }
        }
        public Double ChartElement(int i)
        {
            return Chart[i];
        }
        private String name;
        private Int32 type;
        public Double[] Chart = new Double[100];
    };

}
