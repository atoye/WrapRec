using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Recommenders;

namespace WrapRec.RecSys2015
{
    public class LibFmTrainTesterBuilder
    {
        FmLearnigAlgorithm[] _learningAlgs = new FmLearnigAlgorithm[] 
        { 
            FmLearnigAlgorithm.SGD,
            FmLearnigAlgorithm.ALS,
            FmLearnigAlgorithm.MCMC
        };

        int[] _iterations = new int[] { 50, 100 };

        double[] _learningRates = new double[] { 0.01, 0.02, 0.05 };
        
        string[] _dimenstions = new string[] { "1,1,5", "1,1,10", "1,1,20"};

        LibFmTrainTester _libFm;
        
        public List<LibFmTrainTester> LibFms { get; private set; }

        public LibFmTrainTesterBuilder()
        {
            LibFms = new List<LibFmTrainTester>();

            foreach (var la in _learningAlgs)
            {
                foreach (var iter in _iterations)
                {
                    foreach (var lr in _learningRates)
                    {
                        foreach (var dim in _dimenstions)
                        {
                            var libfm = new LibFmTrainTester(alg: la, numIterations: iter, learningRate: lr, dimensions: dim) 
                            { CreateBinaryFiles = true };

                            // SGD does not support BlockStructure
                            if (la != FmLearnigAlgorithm.SGD)
                            {
                                libfm.Blocks.Add(new UsersBlock());
                                libfm.Blocks.Add(new ItemsBlock());
                            }

                            LibFms.Add(libfm);
                        }
                    }
                }
            }
        }




        /*
        public bool HasNext()
        {
            return (_index < _libFms.Count);
        }

        public LibFmTrainTester GetNext()
        {
            return _libFms[_index++];
        }
        */
    }
}
