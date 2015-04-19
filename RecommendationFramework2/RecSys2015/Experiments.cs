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
        //int[] _numAuxRatings = new int[] { 1, 2, 3, 5, 7, 10 };
        int[] _numAuxRatings = new int[] { 0 };

        public Experiments(string outputPath)
        {
            resultWriter = new StreamWriter(new FileStream(outputPath, FileMode.Create));
        }
        
        public void Run(int num = 1)
        {
            switch (num)
            {
                case(1):
                    if (_numAuxRatings.Count() > 1)
                        throw new WrapRecException("NumAuxRatings array should have only one element!");

                    CompareAmazonSingleDomains();
                    break;
                case(2):
                    CompareAmazonCrossDomains();
                    break;
                default:
                    break;
            }
        }

        public void CompareAmazonCrossDomains()
        {
            var adapter = new AmazonAdapter();
            var splitters = adapter.GetSplitters();

            ExecuteExperiments(new LibFmTrainTesterBuilder(true, true), splitters);
        }

        public void CompareAmazonSingleDomains()
        {
            var adapter = new AmazonAdapter();
            var splitters = adapter.GetSplitters();

            ExecuteExperiments(new LibFmTrainTesterBuilder(false, false), splitters);
        }
        
        /// <summary>
        /// This is the main method to generate final results of the experiments
        /// </summary>
        /// <param name="libFmBuilder">Provide configurations of libFM</param>
        /// <param name="splitters">Provides configurations of splits</param>
        public void ExecuteExperiments(LibFmTrainTesterBuilder libFmBuilder, Dictionary<string, ISplitter<ItemRating>> splitters)
        {
            // prepare evaluation 
            var ctx = new EvalutationContext<ItemRating>();
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Evaluators.Add(new MAE());

            // list to store test results
            var configs = new List<TestConfig>();
            
            resultWriter.WriteLine(TestConfig.GetToStringHeader());
            resultWriter.Flush();

            Log.Logger.Info(TestConfig.GetToStringHeader());

            // iterate over all splits and all libfm configs
            foreach (var splitter in splitters)
            {
                // update context splitter
                ctx.Splitter = splitter.Value;

                foreach (int numAux in _numAuxRatings)
                {
                    foreach (var recommender in libFmBuilder.LibFms)
                    {
                        // update the feature construction logic in recommender based on splitter
                        libFmBuilder.UpdateRecommender(recommender, splitter.Value, numAux);

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
                            NumAuxRatings = libFmBuilder.CrossDomain ? numAux : 0,
                            LibFmTrainTester = recommender
                        };

                        configs.Add(testConfig);

                        // Log results
                        //Log.Logger.Info(testConfig.LibFmTrainTester.LibFmArgs);
                        Log.Logger.Info(testConfig.ToString());
                        resultWriter.WriteLine(testConfig.ToString());
                        resultWriter.Flush();
                    }
                }
                
            }
            
            // Write all results
            Console.WriteLine(TestConfig.GetToStringHeader());
            configs.Select(c => c.ToString()).ToList().ForEach(Console.WriteLine);
            
            resultWriter.Close();
        }


    }
}
