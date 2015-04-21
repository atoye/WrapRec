using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Recommenders;

namespace WrapRec.RecSys2015
{
    public class TestConfig
    {
        public TestConfig()
        { }

        public double LowestRMSE { get; set; }
        public double FinalRMSE { get; set; }
        public double FinalMAE { get; set; }
        public int Duration { get; set; }
        public int NoTrain { get; set; }
        public int NoTest { get; set; }
        public string Name { get; set; }
        public int NumAuxRatings { get; set; }
        public int NumSlices { get; set; }

        public LibFmTrainTester LibFmTrainTester { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}", 
                Name, LowestRMSE, FinalRMSE, FinalMAE, Duration, NoTrain, NoTest, NumAuxRatings, NumSlices, LibFmTrainTester.ToString());
        }

        public static string GetToStringHeader()
        {
            return "Name\tLowestRMSE\tFinalRMSE\tFinalMAE\tDuration\tNoTrain\tNoTest\tNumAuxRatings\tNumSlices\t" + LibFmTrainTester.GetToStringHeader();
        }

    }
}
