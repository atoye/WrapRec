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
        //int[] _numAuxRatings = new int[] { 1, 2 };
        int[] _numAuxRatings = new int[] { 0 };

        public Experiments(string outputPath)
        {
            resultWriter = new StreamWriter(new FileStream(outputPath, FileMode.Create));
        }
        
        public void Run(int num = 6)
        {
            switch (num)
            {
                case(1):
                    if (_numAuxRatings.Count() > 1)
                        throw new WrapRecException("NumAuxRatings array should have only one element!");

                    AmazonSingleDomains();
                    break;
                case(2):
                    AmazonCrossDomains();
                    break;
                case(3):
                    MovieLensSliceAndTrain();
                    break;
                case(4):
                    MovieLensSingle();
                    break;
                case(5):
                    MovieLensContextAware();
                    break;
                case(6):
                    AmazonContextAware();
                    break;
                case(7):
                    MovieLensIndependentContextAwareSlices();
                    break;
                case(8):
                    MovieLensAllUserItemsContext();
                    break;
                default:
                    break;
            }
            
            // close the file
            resultWriter.Close();
        }

        public void MovieLensContextAware()
        {
            var adapter = new MovieLensContextAwareAdapter(true);
            var builder = new LibFmTrainTesterBuilder(true, false);
            builder.ContextSelectors.Add(ir => ir.Item.Properties["genres"]);

            ExecuteExperiments(builder, adapter.GetSplitters());
        }

        public void MovieLensAllUserItemsContext()
        {
            var adapter = new MovieLensContextAwareAdapter(true);
            var builder = new LibFmTrainTesterBuilder(false, false);
            var userCandidateRatings = adapter.Container.Users.Values
                .ToDictionary(u => u.Id, u => 
                    u.Ratings.Select(r => r.Item.Id + "aux")
                    .Take(5)
                    .Aggregate((a, b) => a + "|" + b));
            
            builder.ContextSelectors.Add(ir => userCandidateRatings[ir.User.Id]);

            ExecuteExperiments(builder, adapter.GetSplitters());
        }

        public void AmazonContextAware()
        {
            var adapter = new AmazonAdapter(true, true);
            var builder = new LibFmTrainTesterBuilder(false, false);
            //builder.ContextSelectors.Add(ir => ir.Domain.Id);

            ExecuteExperiments(builder, adapter.GetSplitters());
        }

        public void AmazonCrossDomains()
        {
            var adapter = new AmazonAdapter(true);
            var splitters = adapter.GetSplitters();

            ExecuteExperiments(new LibFmTrainTesterBuilder(false, true), splitters);
        }

        public void AmazonSingleDomains()
        {
            var adapter = new AmazonAdapter(true);
            var splitters = adapter.GetSplitters();

            ExecuteExperiments(new LibFmTrainTesterBuilder(false, false), splitters);
        }

        public void MovieLensSingle()
        {
            var adapter = new MovieLensAdapter(true);
            var splitters = adapter.GetSplitters();

            ExecuteExperiments(new LibFmTrainTesterBuilder(true, false), splitters);
        }

        public void MovieLensSliceAndTrain()
        {
            int[] numSlices = new int[] { 2, 3 };

            foreach (int num in numSlices)
            {
                var adapter = new MovieLensCrossDomainAdapter(num);
                ExecuteExperiments(new LibFmTrainTesterBuilder(false, true), adapter.GetSplitters(), num);
            }
        }

        public void MovieLensIndependentContextAwareSlices()
        {
            int[] numSlices = new int[] { 2, 3 };

            foreach (int num in numSlices)
            {
                var adapter = new MovieLensCrossDomainAdapter(num, true);
                var builder = new LibFmTrainTesterBuilder(false, false);
                //builder.ContextSelectors.Add(ir => ir.Item.Properties["genres"]);

                ExecuteExperiments(builder, adapter.GetSplitters(), num);
            }
        }
        
        /// <summary>
        /// This is the main method to generate final results of the experiments
        /// </summary>
        /// <param name="libFmBuilder">Provide configurations of libFM</param>
        /// <param name="splitters">Provides configurations of splits</param>
        public void ExecuteExperiments(LibFmTrainTesterBuilder libFmBuilder, Dictionary<string, ISplitter<ItemRating>> splitters, int numSlice = 1)
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
                            NumSlices = numSlice,
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
            
            
        }


    }
}
