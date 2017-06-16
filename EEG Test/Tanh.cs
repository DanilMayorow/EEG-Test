using AForge.Neuro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEG_Test
{
    public class Tanh : IActivationFunction
    {
        public double Beta;

        public Tanh(double Beta = 1) { this.Beta = Beta; }

        public double Function(double x)
        {
            return Math.Tanh(x * Beta);
        }

        public double Derivative(double x)
        {
            var f = Function(x);
            return Beta * (1 - f * f);
        }

        public double Derivative2(double y)
        {
            return Beta * (1 - y * y);
        }
    }

    public class Sigm
    {
        public Double Alpha;

        public Sigm(double Alpha = 1) { this.Alpha = Alpha; }

        public double Function(double x)
        {
            return 1 / (1 + Math.Exp(-x*Alpha));
        }

        public double Derivative(double x)
        {
            double s = Function(x);
            return s * (1 - s);
        }

        public double Derivative2(double y)
        {
            return y * (1 - y);
        }
    }
}
