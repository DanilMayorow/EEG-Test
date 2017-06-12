namespace EEG_Test
{
    partial class Form
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
            this.Chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ButStart = new System.Windows.Forms.Button();
            this.TimeLable = new System.Windows.Forms.Label();
            this.ListSign = new System.Windows.Forms.CheckedListBox();
            this.testRB = new System.Windows.Forms.RadioButton();
            this.controllRB = new System.Windows.Forms.RadioButton();
            this.ForChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.Chart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ForChart)).BeginInit();
            this.SuspendLayout();
            // 
            // Chart
            // 
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.Maximum = 100D;
            chartArea1.AxisX.Minimum = 0D;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.AxisY.ScaleBreakStyle.Spacing = 3D;
            chartArea1.Name = "ChartArea";
            this.Chart.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            legend1.TitleAlignment = System.Drawing.StringAlignment.Far;
            legend1.TitleSeparator = System.Windows.Forms.DataVisualization.Charting.LegendSeparatorStyle.Line;
            this.Chart.Legends.Add(legend1);
            this.Chart.Location = new System.Drawing.Point(31, 21);
            this.Chart.Name = "Chart";
            this.Chart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SemiTransparent;
            series1.ChartArea = "ChartArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "Data";
            this.Chart.Series.Add(series1);
            this.Chart.Size = new System.Drawing.Size(430, 283);
            this.Chart.TabIndex = 0;
            this.Chart.Text = "Chart";
            // 
            // ButStart
            // 
            this.ButStart.Location = new System.Drawing.Point(476, 260);
            this.ButStart.Name = "ButStart";
            this.ButStart.Size = new System.Drawing.Size(92, 23);
            this.ButStart.TabIndex = 1;
            this.ButStart.Text = "Запуск/Пауза";
            this.ButStart.UseVisualStyleBackColor = true;
            this.ButStart.Click += new System.EventHandler(this.ButStart_Click);
            // 
            // TimeLable
            // 
            this.TimeLable.AutoSize = true;
            this.TimeLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.TimeLable.Location = new System.Drawing.Point(473, 21);
            this.TimeLable.Name = "TimeLable";
            this.TimeLable.Size = new System.Drawing.Size(95, 18);
            this.TimeLable.TabIndex = 2;
            this.TimeLable.Text = "00:00:00:00";
            // 
            // ListSign
            // 
            this.ListSign.FormattingEnabled = true;
            this.ListSign.Items.AddRange(new object[] {
            "Сосредоточенность",
            "Расслабленность",
            "Дельта-волны",
            "Тетта-Волны",
            "Альфа-Волны",
            "Низкие Альфа-волны",
            "Бета-Волны",
            "Низкие Бета-Волны",
            "Гамма-Волны",
            "Средние Гамма-Волны"});
            this.ListSign.Location = new System.Drawing.Point(476, 54);
            this.ListSign.Name = "ListSign";
            this.ListSign.Size = new System.Drawing.Size(149, 154);
            this.ListSign.TabIndex = 5;
            this.ListSign.SelectedIndexChanged += new System.EventHandler(this.ListSign_SelectedIndexChanged);
            // 
            // testRB
            // 
            this.testRB.AutoSize = true;
            this.testRB.Enabled = false;
            this.testRB.Location = new System.Drawing.Point(476, 214);
            this.testRB.Name = "testRB";
            this.testRB.Size = new System.Drawing.Size(97, 17);
            this.testRB.TabIndex = 7;
            this.testRB.Text = "Тестирование";
            this.testRB.UseVisualStyleBackColor = true;
            this.testRB.CheckedChanged += new System.EventHandler(this.testRB_CheckedChanged);
            // 
            // controllRB
            // 
            this.controllRB.AutoSize = true;
            this.controllRB.Checked = true;
            this.controllRB.Location = new System.Drawing.Point(476, 237);
            this.controllRB.Name = "controllRB";
            this.controllRB.Size = new System.Drawing.Size(123, 17);
            this.controllRB.TabIndex = 8;
            this.controllRB.TabStop = true;
            this.controllRB.Text = "Задача управления";
            this.controllRB.UseVisualStyleBackColor = true;
            this.controllRB.CheckedChanged += new System.EventHandler(this.controllRB_CheckedChanged);
            // 
            // ForChart
            // 
            chartArea2.AxisX.MajorGrid.Enabled = false;
            chartArea2.AxisX.Maximum = 100D;
            chartArea2.AxisX.Minimum = 0D;
            chartArea2.AxisY.MajorGrid.Enabled = false;
            chartArea2.AxisY.Minimum = 0D;
            chartArea2.AxisY.ScaleBreakStyle.Spacing = 3D;
            chartArea2.Name = "ChartArea";
            this.ForChart.ChartAreas.Add(chartArea2);
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.Name = "Legend1";
            legend2.TitleAlignment = System.Drawing.StringAlignment.Far;
            legend2.TitleSeparator = System.Windows.Forms.DataVisualization.Charting.LegendSeparatorStyle.Line;
            this.ForChart.Legends.Add(legend2);
            this.ForChart.Location = new System.Drawing.Point(31, 310);
            this.ForChart.Name = "ForChart";
            this.ForChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            series2.ChartArea = "ChartArea";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series2.Legend = "Legend1";
            series2.Name = "Data";
            this.ForChart.Series.Add(series2);
            this.ForChart.Size = new System.Drawing.Size(430, 283);
            this.ForChart.TabIndex = 9;
            this.ForChart.Text = "chart1";
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 611);
            this.Controls.Add(this.ForChart);
            this.Controls.Add(this.controllRB);
            this.Controls.Add(this.testRB);
            this.Controls.Add(this.ListSign);
            this.Controls.Add(this.TimeLable);
            this.Controls.Add(this.ButStart);
            this.Controls.Add(this.Chart);
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form";
            this.Text = "Тестировщик нейроинтерфейсов";
            ((System.ComponentModel.ISupportInitialize)(this.Chart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ForChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart Chart;
        private System.Windows.Forms.Button ButStart;
        private System.Windows.Forms.Label TimeLable;
        private System.Windows.Forms.CheckedListBox ListSign;
        private System.Windows.Forms.RadioButton testRB;
        private System.Windows.Forms.RadioButton controllRB;
        private System.Windows.Forms.DataVisualization.Charting.Chart ForChart;
    }
}

