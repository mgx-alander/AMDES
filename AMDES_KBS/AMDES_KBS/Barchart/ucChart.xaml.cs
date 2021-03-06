﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Threading.Tasks;

namespace AMDES_KBS
{
    /// <summary>
    /// Interaction logic for ucChart.xaml
    /// </summary>
    public partial class ucChart : UserControl
    {
        public ucChart(List<Tuple<string, double, double>> lst)
        {
            InitializeComponent();
            lblTitle.Content = "Frequency of Tests Conducted on Initial Visits ";
            LoadChart(ProcessTuple(lst.OrderBy(x=>x.Item2).ToList()));
        }

        private System.Collections.ObjectModel.Collection<InitialVisitTest> ProcessTuple(List<Tuple<string, double, double>> lst)
        {
            System.Collections.ObjectModel.Collection<InitialVisitTest> TestCollection = new System.Collections.ObjectModel.Collection<InitialVisitTest>();
            foreach (Tuple<string, double, double> item in lst)
            //Parallel.ForEach(lst, item =>
           {
               InitialVisitTest t = new InitialVisitTest();
               t.TestName = item.Item1 + "  ";
               t.Yes = item.Item2 * 100;
               TestCollection.Add(t);
           }//);
            return TestCollection;
        }

        private void LoadChart(System.Collections.ObjectModel.Collection<InitialVisitTest> TestCollection)
        {
            var yAxis = this.Chart1.ActualAxes.OfType<LinearAxis>().FirstOrDefault(ax => ax.Orientation == AxisOrientation.Y);
            if (yAxis != null)
                yAxis.Maximum = 100;
            Chart1.Axes.Add(new LinearAxis()
            {
                Minimum = 0,
                Maximum = 100,
                Orientation = AxisOrientation.X,
                Location = AxisLocation.Top,
                Interval = 5,
                ShowGridLines = true,
            });

            ((BarSeries)Chart1.Series[0]).ItemsSource = TestCollection;

            Chart1.Height = TestCollection.Count * 30;

        }

    }
}
