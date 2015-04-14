using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WrapRec.Utilities;
using WrapRec.Recommenders;

namespace WrapRec.RecSys2015
{
    [Obsolete("Using of this class does not make any more sense. Use excel pivot chart instead.")]
    public class ResultAnalyzer
    {
        public IEnumerable<TestConfig> Results { get; private set; }

        public static void Run()
        {
            var ra = new ResultAnalyzer("log1.txt");
            ra.CompareLearningMethods("1,1,5", 100, 0.01);
        }
        
        public ResultAnalyzer(string resultsFile)
        {
            Results = File.ReadAllLines(resultsFile).ToCsvDictionary('\t')
                .Select(l => new TestConfig()
                {
                    Name = l["Name"],
                    FinalRMSE = double.Parse(l["FinalRMSE"]),
                    LowestRMSE = double.Parse(l["LowestRMSE"]),
                    FinalMAE = double.Parse(l["FinalMAE"]),
                    Duration = int.Parse(l["Duration"]),
                    LibFmTrainTester = new LibFmTrainTester(
                        alg: (FmLearnigAlgorithm) Enum.Parse(typeof(FmLearnigAlgorithm), l["LearningMethod"]),
                        dimensions: l["Dimensionality"],
                        numIterations: int.Parse(l["NumIteration"]),
                        learningRate: double.Parse(l["LearningRate"]))
                });
        }

        public void CompareLearningMethods(string dim, int iter, double lr)
        {
            var output = Results.Where(r => r.LibFmTrainTester.Dimensions == dim && r.LibFmTrainTester.Iterations == iter && r.LibFmTrainTester.LearningRate == lr)
                .OrderBy(r => r.Name)
                .Select(r => string.Format("{0}\t{1}\t{2}\t{3}", 
                    r.Name, 
                    r.LibFmTrainTester.LearningAlgorithm.ToString(),
                    r.FinalRMSE,
                    r.Duration));

            var header = new List<string> { "Dataset\tLearning Method\tRMSE\tDuration" };
            header.Concat(output).ToList().ForEach(Console.WriteLine);
        }

    }
}
