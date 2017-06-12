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

namespace EEG_Test
{

    public partial class Form : System.Windows.Forms.Form
    {
        public int errCode = 0;
        private Thread mainThread;
        private Double[] dataAtt = new Double[100];
        private Double[] dataMed = new Double[100];
        public Int32[] signalVal = new Int32[10] { 2, 3, 5, 6, 7, 8, 9, 10, 11, 12 };
        public List<Signal> Signals = new List<Signal>();
        public Boolean START = false;
        public Boolean INIT = false;
        public Boolean controllType = false;
        public Double bufAtt = 0;
        public Double bufMed = 0;
        public Boolean A, M;
        public Boolean newData;
        public Double[][] Data;
        public Int32 readyFor = 0;
        public Int32 sizeFor = 50;
        public Boolean Forc = false;

        Boolean Run = true;
        DateTime Date;
        float Cm = 0, Ca = 0;
        public long Tick;

        //AForge libary
        BackPropagationLearning teachAtt,teachMed;
        protected Int32[] Configuration = new Int32[] { 1, 5, 1 };
        public ActivationNetwork network;
        public Random rnd = new Random(2);
        public int IterationsCount = 30000;
        public double[][] LearningInputs;
        public double[][] LearningAnswers;
        public double[] LearningOutputs;


        //Accord MLR
        MultipleLinearRegression MLR;
        Double [][] linearCoeff;

        //Accord PR
        public PolynomialRegression PR;
        public Double [][] polynomCoeff;
        public Int32 Grade = 3;

        private Double[] linAtt = new Double[100];
        private Double[] linMed = new Double[100];
        private Double[] polyAtt = new Double[100];
        private Double[] polyMed = new Double[100];
        public Int32 Att = 0;
        public Int32 Med = 1;

        public Form()
        {
            InitializeComponent();
            rbCheck();
            chartType();

            
            network = new ActivationNetwork(new Tanh(0.4),Configuration[0],Configuration.Skip(1).ToArray());
            network.ForEachWeight(z => rnd.NextDouble() * 2 - 1);
            teachAtt = new BackPropagationLearning(network);
            teachAtt.LearningRate = 1;
            teachMed = new BackPropagationLearning(network);
            teachMed.LearningRate = 1;
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
                            Ca++;
                            if (bufAtt != 0)
                            {
                                A = true;
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
                            Cm++;
                            if (bufMed != 0)
                            {
                                M = true;
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
                            dataAtt[dataAtt.Length - 1] = bufAtt;
                            Array.Copy(dataAtt, 1, dataAtt, 0, dataAtt.Length - 1);
                            dataMed[dataMed.Length - 1] = bufMed;
                            Array.Copy(dataMed, 1, dataMed, 0, dataMed.Length - 1);

                            readyFor++;
                            if (readyFor == sizeFor) { toData();Forecast();Forc = true; }
                            if (Chart.IsHandleCreated)
                            {
                                if (Forc) { toForc();}
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

        private void UpdateChart()
        {

            if (controllType)
            {
                Chart.Series["Сосредоточенность"].Points.Clear();
                Chart.Series["Расслабленность"].Points.Clear();
                if(Forc)
                {
                    ForChart.Series["Р(ЛР)"].Points.Clear();
                    ForChart.Series["Р(ПР)"].Points.Clear();
                    ForChart.Series["С(ЛР)"].Points.Clear();
                    ForChart.Series["С(ПР)"].Points.Clear();
                }

                for (int i = 0; i < dataAtt.Length - 1; ++i)
                {
                    Chart.Series["Сосредоточенность"].Points.AddY(dataAtt[i]);
                    Chart.Series["Расслабленность"].Points.AddY(dataMed[i]);

                    if (Forc)
                    {
                        ForChart.Series["С(ЛР)"].Points.AddY(linAtt[i]);
                        ForChart.Series["С(ПР)"].Points.AddY(polyAtt[i]);
                        ForChart.Series["Р(ЛР)"].Points.AddY(linMed[i]);
                        ForChart.Series["Р(ПР)"].Points.AddY(polyMed[i]);
                    }
                }
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
                timer.Start();
                START = !START;
            }
            else
            {
                mainThread.Suspend();
                timer.Stop();
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
            chartType();
        }

        private void ListSign_SelectedIndexChanged(object sender, EventArgs e)
        {
            chartType();
        }

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

                ForChart.Series.Add("Р(ЛР)");
                ForChart.Series["Р(ЛР)"].Points.Clear();
                ForChart.Series["Р(ЛР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("Р(ПР)");
                ForChart.Series["Р(ПР)"].Points.Clear();
                ForChart.Series["Р(ПР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("С(ЛР)");
                ForChart.Series["С(ЛР)"].Points.Clear();
                ForChart.Series["С(ЛР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                ForChart.Series.Add("С(ПР)");
                ForChart.Series["С(ПР)"].Points.Clear();
                ForChart.Series["С(ПР)"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            }
        }

        protected virtual void PrepareData(Double [][] X,Double [][] Y)
        {
            LearningInputs = X;
            LearningAnswers = Y;
        }

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
                LearningEnds();
                if (counter > IterationsCount) break;

            }
        }

        protected virtual void LearningIteration()
        {
            teachAtt.RunEpoch(LearningInputs, LearningAnswers);
            teachMed.RunEpoch(LearningInputs, LearningAnswers);
        }

        protected virtual void AccountError()
        {
            //LearningErrors.Enqueue(GetError(LearningInputs, LearningAnswers));
        }

        protected virtual void LearningEnds()
        {
            LearningOutputs = LearningInputs.Select(z => network.Compute(z)[0]).ToArray();
        }

        protected double GetError(double[][] Inputs, double[][] Answers)
        {
            double sum = 0;
            for (int i = 0; i < Inputs.Length; i++)
            {
                sum += Math.Abs(network.Compute(Inputs[i])[0] - Answers[i][0]);
            }
            sum /= Inputs.Length;
            return sum;
        }

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
        }

        public void toData()
        {
            Data = new Double[2][];
            Data[Att] = new Double[sizeFor];
            Data[Med] = new Double[sizeFor];
            for (int j = 0; j < sizeFor; j++)
            {
                Data[Att][j] = dataAtt[100-sizeFor+j];
                Data[Med][j] = dataMed[100-sizeFor+j];
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
