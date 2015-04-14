using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Evaluation;
using WrapRec.Readers.NewReaders;
using WrapRec.Recommenders;
using WrapRec.Utilities;

namespace WrapRec.Experiments
{
    public class RecSys2015Experiments
    {
        public void Run(int testNum = 5)
        {
            switch (testNum)
            {
                case (1):
                    TestMovieLensSingle();
                    break;
                case(2):
                    TestMovieLensCrossDomainBlock();
                    break;
                case(3):
                    TestMovieLensCrossDomain();
                    break;
                case(4):
                    TestAmazonBookCrossDomain();
                    break;
                case(5):
                    TestWholeDatasetAmazon();
                    break;
                default:
                    break;
            }
        }

        public void TestMovieLensSingle()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = "::",
                HasHeaderRecord = false
            };

            // load data
            var trainReader = new CsvReader(Paths.MovieLens1MTrain75, config);
            var testReader = new CsvReader(Paths.MovieLens1MTest25, config, true);

            var container = new DataContainer();
            trainReader.LoadData(container);
            testReader.LoadData(container);

            var startTime = DateTime.Now;

            var splitter = new RatingSimpleSplitter(container);

            //var recommender = new MediaLiteRatingPredictor(new MatrixFactorization());
            var recommender = new LibFmTrainTester(libFmPath: "LibFm.Net.64.exe") { CreateBinaryFiles = true };
            //recommender.Blocks.Add(new UsersBlock());
            //recommender.Blocks.Add(new ItemsBlock());

            // evaluation
            var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Run();

            var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;

            Console.WriteLine("RMSE\tDuration\n{0}\t{1}", ctx["RMSE"], duration);
        }

        public void TestMovieLensCrossDomainBlock()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var trainReader = new CsvReader(Paths.AmazonBooksTrain75, config, bookDomain);
            var testReader = new CsvReader(Paths.AmazonBooksTest25, config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            //var tempReader = new LibFmReader(_ecirTrain, _ecirTest) { MainDomain = bookDomain, AuxDomain = musicDomain, UserDataPath = _musicUsersPath };

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);

            container.PrintStatistics();

            var splitter = new CrossDomainSimpleSplitter(container);
            
            var numAuxRatings = new List<int> { 0, 1, 2, 3, 4, 5 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                LibFmTrainTester recommender = new LibFmTrainTester(experimentId: num.ToString()) { CreateBinaryFiles = true };
                recommender.Blocks.Add(new ItemsBlock());

                if (num == 0)
                {
                    recommender.Blocks.Add(new UsersBlock());
                }
                else
                {
                    recommender.Blocks.Add(new CrossDomainUsersBlock(bookDomain, num));
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestMovieLensCrossDomain()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var trainReader = new CsvReader(Paths.AmazonBooksTrain75, config, bookDomain);
            var testReader = new CsvReader(Paths.AmazonBooksTest25, config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonVideoRatings, config, videoDomain);

            trainReader.LoadData(container);
            testReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);

            container.PrintStatistics();

            var splitter = new CrossDomainSimpleSplitter(container);

            var numAuxRatings = new List<int> { 0, 1, 2, 3, 4, 5 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                LibFmTrainTester recommender = new LibFmTrainTester(experimentId: num.ToString()) { CreateBinaryFiles = true };

                if (num != 0)
                {
                    recommender.FeatureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, num);
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(ctx["RMSE"].ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAmazonBookCrossDomain()
        {
            // step 1: dataset            
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var bookReader = new CsvReader("books_selected4.csv", config, bookDomain);
            //var trainReader = new CsvReader("books_selected1_train.csv", config, bookDomain);
            //var testReader = new CsvReader("books_selected1_test.csv", config, bookDomain, true);
            var musicReader = new CsvReader(Paths.AmazonAllMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonAllDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonAllVideoRatings, config, videoDomain);

            bookReader.LoadData(container);
            //trainReader.LoadData(container);
            //testReader.LoadData(container);
            musicReader.LoadData(container);
            //dvdReader.LoadData(container);
            //videoReader.LoadData(container);

            container.PrintStatistics();
            //musicDomain.CacheUserData();

            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);
            //splitter.SaveSplitsAsCsv("books_selected1_train.csv", "books_selected1_test.csv");

            //return;
            //var splitter = new RatingSimpleSplitter(container, 0.25f);

            var numAuxRatings = new List<int> { 0, 1, 2, 3, 5, 7, 10 };

            var rmse = new List<string>();
            var mae = new List<string>();
            var durations = new List<string>();

            foreach (var num in numAuxRatings)
            {
                var startTime = DateTime.Now;

                // step 2: recommender
                LibFmTrainTester recommender = new LibFmTrainTester(experimentId: num.ToString()) { CreateBinaryFiles = true };
                //CrossDomainLibFmFeatureBuilder featureBuilder = null;
                recommender.Blocks.Add(new ItemsBlock());

                if (num == 0)
                {
                    recommender.Blocks.Add(new UsersBlock());
                }
                else
                {
                    //featureBuilder = new CrossDomainLibFmFeatureBuilder(bookDomain, num);
                    recommender.Blocks.Add(new CrossDomainUsersBlock(bookDomain, num));
                }

                // step3: evaluation
                var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
                var ep = new EvaluationPipeline<ItemRating>(ctx);
                ep.Evaluators.Add(new RMSE());
                ep.Evaluators.Add(new MAE());
                ep.Run();

                //File.WriteAllLines("maps.txt", featureBuilder.Mapper.OriginalIDs.Zip(featureBuilder.Mapper.InternalIDs, (f, s) => f + "\t" + s));

                rmse.Add(recommender.RMSE.ToString());
                mae.Add(ctx["MAE"].ToString());

                var duration = DateTime.Now.Subtract(startTime);
                durations.Add(((int)duration.TotalMilliseconds).ToString());
            }

            Console.WriteLine("NumAuxRatings\tRMSE\tMAE\tDuration");
            for (int i = 0; i < numAuxRatings.Count(); i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", numAuxRatings[i], rmse[i], mae[i], durations[i]);
            }
        }

        public void TestAmazonAllDomain()
        { 
            // test without BS

            // test each domain separately, RMSE and time

            // test the whole dataset

            // test with BS
        }

        private void TestWholeDatasetAmazon()
        {
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var container = new CrossDomainDataContainer();

            var bookDomain = new Domain("book", true);
            var musicDomain = new Domain("music");
            var dvdDomain = new Domain("dvd");
            var videoDomain = new Domain("video");

            var bookReader = new CsvReader("books_selected4.csv", config, bookDomain);
            var musicReader = new CsvReader(Paths.AmazonAllMusicRatings, config, musicDomain);
            var dvdReader = new CsvReader(Paths.AmazonAllDvdRatings, config, dvdDomain);
            var videoReader = new CsvReader(Paths.AmazonAllVideoRatings, config, videoDomain);

            bookReader.LoadData(container);
            musicReader.LoadData(container);
            dvdReader.LoadData(container);
            videoReader.LoadData(container);


            var splitter = new CrossDomainSimpleSplitter(container, 0.25f);

            LibFmTrainTester recommender = new LibFmTrainTester() { CreateBinaryFiles = true };
            recommender.Blocks.Add(new ItemsBlock());
            recommender.Blocks.Add(new UsersBlock());

            var ctx = new EvalutationContext<ItemRating>(recommender, splitter);
            var ep = new EvaluationPipeline<ItemRating>(ctx);
            ep.Evaluators.Add(new RMSE());
            ep.Run();

            Console.WriteLine("RMSE\tDuration");
            Console.WriteLine("{0}\t{1}", recommender.RMSE, recommender.Duration);
        }
    }
}
