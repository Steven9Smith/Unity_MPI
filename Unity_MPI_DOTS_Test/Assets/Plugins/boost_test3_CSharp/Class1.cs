using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TestClassLibrary;

namespace TestClassLibrary
{

    public class BoostMathExpose
    {
        [DllImport("boot_test3.dll")]
        private static extern double InverseIncompleteBeta(double a, double b, double x);
        [DllImport("boot_test3.dll")]
        private static extern double NormalDistribution(double m, double s, double quantile);

    };
}