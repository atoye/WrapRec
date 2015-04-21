using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;
using System.IO;
using System.Diagnostics;
using LinqLib.Sequence;
using WrapRec.Data;
using WrapRec.Utilities;

namespace WrapRec.Recommenders
{
    public class LibFmTrainTester : ITrainTester<ItemRating>
    {
        // Mapping _usersItemsMap;
        
        // path to a folder to save temprorary converted files
        string _dataStorePath;

        // libFm parameters
        public string LibFmPath { get; set; }
        public double LearningRate { get; set; }
        public int Iterations { get; set; }
        public string Dimensions { get; set; }
        public string Regularization { get; set; }
        public FmLearnigAlgorithm LearningAlgorithm { get; set; }
        public string TrainFile { get; set; }
        public string TestFile { get; set; }
        public string ValidationFile { get; set; }
        public bool CreateBinaryFiles { get; set; }
        public List<LibFmBlock> Blocks { get; private set; }
        public double RMSE { get; private set; }

        string _experimentId;

        public LibFmFeatureBuilder FeatureBuilder { get; set; }

        public LibFmTrainTester(string experimentId = "", LibFmFeatureBuilder featureBuilder = null, string dataStorePath = "",
            string libFmPath = "LibFm.Net.64.exe",
            double learningRate = 0.05, 
            int numIterations = 50, 
            string dimensions = "1,1,5", 
            FmLearnigAlgorithm alg = FmLearnigAlgorithm.MCMC,
            string regularization = "0,0,0.1",
            string trainFile = "",
            string testFile = "")
        {
            _experimentId = experimentId;

            //_usersItemsMap = new Mapping();
            _dataStorePath = !String.IsNullOrEmpty(dataStorePath) && dataStorePath.Last() != '\\' ? dataStorePath + "\\" : dataStorePath;

            if (featureBuilder == null)
                FeatureBuilder = new LibFmFeatureBuilder();
            else
                FeatureBuilder = featureBuilder;
            
            // default properties
            LibFmPath = libFmPath;
            LearningRate = learningRate;
            Iterations = numIterations;
            Dimensions = dimensions;
            LearningAlgorithm = alg;
            Regularization = regularization;
            TrainFile = trainFile;
            TestFile = testFile;
            Blocks = new List<LibFmBlock>();
            CreateBinaryFiles = false;
        }

        public void TrainAndTest(IEnumerable<ItemRating> trainSet, IEnumerable<ItemRating> testSet, IEnumerable<ItemRating> validSet = null)
        {
            string expIdExtension = string.IsNullOrEmpty(_experimentId) ? "" : "." + _experimentId;

            if (TrainFile == "")
            {
                TrainFile = _dataStorePath + "train.libfm" + expIdExtension;
                TestFile = _dataStorePath + "test.libfm" + expIdExtension;
                ValidationFile = _dataStorePath + "eval.libfm" + expIdExtension;
            }

            string testOutput = _dataStorePath + "test.out" + expIdExtension;

            // converting train and test data to libFm files becuase libfm.exe only get file names as input
            SaveLibFmFile(trainSet, TrainFile, true);
            SaveLibFmFile(testSet, TestFile, false);

            if (validSet != null)
                SaveLibFmFile(validSet, ValidationFile, true);

            if (CreateBinaryFiles)
            {
                ConvertAndTransform(TrainFile);
                ConvertAndTransform(TestFile);
            }

            if (Blocks.Count > 0)
            { 
                
            }

            // initialize the process
            var libFm = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = LibFmPath,
                    Arguments = BuildArguments(TrainFile, TestFile, testOutput),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            string libFMTrainRMSE = "", libFMTestRMSE = "";
            double lowestRMSE = double.MaxValue;
            int iter = 0, lowestIteration = 0;

            libFm.OutputDataReceived += (p, dataLine) =>
            {
                var data = dataLine.Data;

                if (data != null && (data.StartsWith("Loading") || data.StartsWith("#")))
                {
                    Log.Logger.Trace(dataLine.Data);

                    if (data.StartsWith("#Iter"))
                    {
                        libFMTrainRMSE = data.Substring(data.IndexOf("Train") + 6, 6);
                        libFMTestRMSE = data.Substring(data.IndexOf("Test") + 5);

                        double current = double.Parse(libFMTestRMSE);

                        if (current < lowestRMSE)
                        {
                            lowestRMSE = current;
                            lowestIteration = iter;
                        }

                        iter++;
                    }
                }
            };

            Log.Logger.Trace("libfm {0}", libFm.StartInfo.Arguments);

            var startTime = DateTime.Now;

            libFm.Start();
            libFm.BeginOutputReadLine();
            libFm.WaitForExit();

            var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;

            Log.Logger.Trace("Lowest RMSE on test set reported by LibFm is: {0:0.0000} at iteration {1}", lowestRMSE, lowestIteration);
            Log.Logger.Trace("LibFm pure train and test time: {0:N0} ms", duration);

            Duration = duration;
            LowestIteration = lowestIteration;
            LibFmArgs = libFm.StartInfo.Arguments;

            RMSE = lowestRMSE;
            UpdateTestSet(testSet, testOutput);

            // write actual ratings in the test set
            File.WriteAllLines(_dataStorePath + "test.act", testSet.Select(ir => ir.Rating.ToString()).ToList());
        }

        public int Duration { get; set; }
        public int LowestIteration { get; set; }
        public string LibFmArgs { get; set; }

        private void SaveLibFmFile(IEnumerable<ItemRating> dataset, string libfmFile, bool isTrain)
        {
            List<string> featVectors;

            if (Blocks.Count > 0)
            {
                List<string>[] blockIndices = new List<string>[Blocks.Count];
                for (int i = 0; i < Blocks.Count; i++)
                {
                    blockIndices[i] = new List<string>();
                }

                // update blocks while creating feature vectors
                featVectors = dataset.Select(ir => 
                {
                    for (int i = 0; i < Blocks.Count; i++)
                    {
                        blockIndices[i].Add(Blocks[i].UpdateBlock(ir).ToString());
                    }

                    // only return ratings (class variable), the prediction variables zijn niet nodig
                    return ir.Rating.ToString();
                }).ToList();

                // save block index files and actual blocks
                for (int i = 0; i < Blocks.Count; i++)
                {
                    Log.Logger.Trace("Creating Block files for Block: {0}", Blocks[i].Name);
                    
                    // The .bin extension is added for name convension that is expected by libFm and to be compatible with ConvertAndTransform convension
                    string blockFile = string.Format("{0}.bin.{1}", Blocks[i].Name, isTrain ? "train" : "test");
                    File.WriteAllLines(blockFile, blockIndices[i]);

                    File.WriteAllLines(Blocks[i].Name, Blocks[i].Blocks);
                    ConvertAndTransform(Blocks[i].Name);
                }
            }
            else
            {
                featVectors = dataset.Select(ir => FeatureBuilder.GetLibFmFeatureVector(ir)).ToList();
            }

            File.WriteAllLines(libfmFile, featVectors);

            //if (LearningAlgorithm == FmLearnigAlgorithm.SGDA && isTrain)
            //{
            //    CreateValidationSet(libfmFile, 0.2);
            //}
        }

        public void CreateValidationSet(string trainFile, double ratio)
        {
            FileHelper.SplitLines(trainFile, trainFile, trainFile + ".val", ratio, false, true);
        }

        public void ConvertAndTransform(string libfmFile)
        {
            string convertArgs = string.Format("--ifile {0} --ofilex {0}.bin.x --ofiley {0}.bin.y", libfmFile);
            string transposeArgs = string.Format("--ifile {0}.bin.x --ofile {0}.bin.xt", libfmFile);

            var convert = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "convert.exe",
                    Arguments = convertArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var transpose = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "transpose.exe",
                    Arguments = transposeArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
           
            Log.Logger.Trace("Converting LibFm file to binary format. Input file: {0}", libfmFile);
            convert.Start();
            convert.WaitForExit();
            
            Log.Logger.Trace("Transposing LibFm binary file {0}.bin.x", libfmFile);
            transpose.Start();
            transpose.WaitForExit();

            Log.Logger.Trace("Transposing finished.");
        }

        private string BuildArguments(string trainFile, string testFile, string testOutput)
        {
            string trainFileOrg = trainFile;

            if (CreateBinaryFiles)
            {
                trainFile += ".bin";
                testFile += ".bin";
            }

            string blockParams = "";
            if (Blocks.Count > 0)
            {
                blockParams = " -relation " + Blocks.Select(b => b.Name + ".bin").Aggregate((a, b) => a + "," + b);
            }

            return String.Format("-task r -train {0} -test {1} -method {2} -iter {3} -dim {4} -learn_rate {5} -out {6} -regular {7}{8}{9}",
                trainFile, testFile, LearningAlgorithm.ToString().ToLower(), Iterations, Dimensions, LearningRate, testOutput, Regularization, blockParams,
                LearningAlgorithm == FmLearnigAlgorithm.SGDA ? " -validation " + ValidationFile : "");
        }

        private void UpdateTestSet(IEnumerable<ItemRating> testSet, string testOutput)
        {
            var predictedRatings = File.ReadAllLines(testOutput).ToList();

            // it is important that the order of test samples and predicted ratings in the output file remains the same
            // the testSet should already be a list to make sure that the updates on items applies on the original set
            int i = 0;
            foreach (var itemRating in testSet)
            {
                itemRating.PredictedRating = float.Parse(predictedRatings[i++]);
            }
        }

        public void PrintConfiguration()
        { 
        
        }

        public override string ToString()
        {
            // LearningMethod Dimensionality NumIteration LearningRate NoBlocks BinaryInput
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                LearningAlgorithm.ToString(),
                Dimensions,
                Iterations,
                LowestIteration + 1,
                LearningRate,
                Regularization,
                Blocks.Count,
                CreateBinaryFiles ? "yes" : "no");
        }

        public static string GetToStringHeader()
        {
            return "LearningMethod\tDimensionality\tNumIteration\tLowestIteration\tLearningRate\tRegularization\tNoBlocks\tBinaryInput";
        }
    }

    public enum FmLearnigAlgorithm
    {
        MCMC,
        SGD,
        SGDA,
        ALS
    }
}
