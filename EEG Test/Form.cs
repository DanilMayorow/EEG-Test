using System;
using System.Windows.Forms;
using System.Threading;
using libStreamSDK;
using System.Collections;
using System.Collections.Generic;

namespace EEG_Test
{

    public partial class Form : System.Windows.Forms.Form
    {
        LinearRegression LR;
        public int errCode = 0;
        private Thread mainThread;
        private Double[] dataAtt = new Double[100];
        private Double[] dataMed = new Double[100];
        public Int32[] signalVal = new Int32[10] { 2, 3, 5, 6, 7, 8, 9, 10, 11, 12 };
        public List<Signal> Signals=new List<Signal>();
        public Boolean START = false;
        public Boolean INIT = false;
        public Boolean controllType = false;
        public Double bufAtt = 0;
        public Double bufMed = 0;
        public Boolean A, M;
        public Boolean newData;
        public Double[][] Data;
        public List<DataSignal> RegSig = new List<DataSignal>();

        Boolean Run = true;
        DateTime Date;
        float Cm = 0, Ca = 0;
        public long Tick;

        public Form()
        {
            InitializeComponent();
            rbCheck();
            chartType();
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
                            A = true;
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
                            M = true;
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

        private void UpdateChart()
        {

            if (controllType)
            {
                Chart.Series["Сосредоточенность"].Points.Clear();
                Chart.Series["Расслабленность"].Points.Clear();

                for (int i = 0; i < dataAtt.Length - 1; ++i)
                {
                    Chart.Series["Сосредоточенность"].Points.AddY(dataAtt[i]);
                    Chart.Series["Расслабленность"].Points.AddY(dataMed[i]);
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
                if (mainThread.ThreadState == (ThreadState)12) { mainThread.Start(); }
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
            if (controllType)
            {
                Chart.Series.Add("Сосредоточенность");
                Chart.Series["Сосредоточенность"].Points.Clear();
                Chart.Series["Сосредоточенность"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                Chart.Series.Add("Расслабленность");
                Chart.Series["Расслабленность"].Points.Clear();
                Chart.Series["Расслабленность"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            }
        }

        public void InitData()
        {
            if (!controllType)
            {
                Data = new Double[Signals.Count][];
                for (int i = 0; i < 100; i++)
                    Data[i] = new Double[100];
            }
            else
            {
                Data = new Double[2][];
                for (int i = 0; i < 100; i++)
                    Data[i] = new Double[100];
            }
        }
        public void rewriteData()
        {
            if (!controllType)
            {
                for (int k = 0; k < Signals.Count; k++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        Data[k][j]=Signals[k].ChartElement(j);
                    }
                }
            }
            else
            {
                for (int i=0;i<100; i++)
                {
                    Data[0][i] = dataAtt[i];
                    Data[1][i] = dataMed[i];
                }
            }
        }
        public void Regression()
        {
            if (controllType)
            {
                DataSignal Sa = new DataSignal("Сосредоточенность");
                DataSignal Sm = new DataSignal("Расслабленность");

                Double[][] d = LinearRegression.Design(Data);
                Sm.Factor=LinearRegression.Solve(d);

                for(int i=0;i<100;i++)
                {
                    Double buf = Data[0][i];
                    Data[0][i] = Data[1][i];
                    Data[1][i] = buf;
                }
                d= LinearRegression.Design(Data);
                Sa.Factor= LinearRegression.Solve(d);
                RegSig.Add(Sa);
                RegSig.Add(Sm);
            }
            else
            {
                DataSignal[] S = new DataSignal[Signals.Count];
                for (int i = 0; i < Signals.Count;i++)
                    S[i].Name = Signals[i].Name;
                

                for (int i = 0; i < Signals.Count; i++)
                    RegSig.Add(S[i]);
            }
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

    public class DataSignal
    {
        public DataSignal() { }
        public DataSignal(String _name)
        {
            name = _name;
        }
        public DataSignal(String _name, Double[] _factor)
        {
            name = _name;
            factor = _factor;
        }
        private String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        private Double[] factor;
        public Double[] Factor
        {
            get { return factor; }
            set { factor = new Double[value.Length]; factor = value; }
        }
        public Double Forecast(Double [] x)
        {
            return LinearRegression.Forecast(x, factor);
        }
    }
}
