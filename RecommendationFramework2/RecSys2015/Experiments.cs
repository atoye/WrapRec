using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;
using WrapRec.Recommenders;
using System.IO;

namespace WrapRec.RecSys2015
{
    public class Experiments
    {
        StreamWriter resultWriter;

        public Experiments(string outputPath)
        {
            resultWriter = new StreamWriter(new FileStream(outputPath, FileMode.Create));
        }
        
        public void Run(int num = 1)
        {
            switch (num)
            {
                case(1):
                    CompareAllFms();
                    break;
                default:
                    break;
            }
        }

        public void CompareAllFms()
        {
            var adapter = new AmazonAdapter();

            // prepare evaluation 
            var ctx = new EvalutationContext<ItemRating>();
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            // get splitters and libfm testers
            var splitters = adapter.GetSimpleSplitters();
            var libFmBuilder = new LibFmTrainTesterBuilder();
            
            // list to store test results
            var configs = new List<TestConfig>();
            
            resultWriter.WriteLine(TestConfig.GetToStringHeader());
            resultWriter.Flush();

            // iterate over all splits and all libfm configs
            foreach (var splitter in splitters)
            {
                // update context splitter
                ctx.Splitter = splitter.Value;

                foreach (var recommender in libFmBuilder.LibFms)
                {
                    // update context model
                    ctx.Model = recommender;

                    // run pipeline
                    ep.Run();

                    // store test results
                    var testConfig = new TestConfig()
                    {
                        LowestRMSE = recommender.RMSE,
                        FinalRMSE = double.Parse(string.Format("{0:0.0000}", ctx["RMSE"])),
                        FinalMAE = double.Parse(string.Format("{0:0.0000}", ctx["MAE"])),
                        Duration = recommender.Duration,
                        Name = splitter.Key,
                        NoTrain = splitter.Value.Train.Count(),
                        NoTest = splitter.Value.Test.Count(),
                        LibFmTrainTester = recommender
                    };

                    configs.Add(testConfig);

                    // Log results
                    Log.Logger.Info(testConfig.ToString());
                    resultWriter.WriteLine(testConfig.ToString());
                    resultWriter.Flush();
                }
            }
            
            // Write all results
            Console.WriteLine(TestConfig.GetToStringHeader());
            configs.Select(c => c.ToString()).ToList().ForEach(Console.WriteLine);
            
            resultWriter.Close();
        }

    }
}
