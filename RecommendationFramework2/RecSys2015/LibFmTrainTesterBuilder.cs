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
            //FmLearnigAlgorithm.SGD,
            FmLearnigAlgorithm.ALS,
            FmLearnigAlgorithm.MCMC
        };

        int[] _iterations = new int[] { 50 };

        double[] _learningRates = new double[] { 0.01, 0.02 };
        
        string[] _dimenstions = new string[] { "1,1,5", "1,1,10" };

        public List<LibFmTrainTester> LibFms { get; private set; }

        public bool UseBlocks { get; set; }
        public bool CrossDomain { get; set; }

        /// <summary>
        /// In the default constructor, the features are build by standard LibFMFeatureBuilder
        /// A custom featureBuilder can be passed for advanced feature construction (such as CrossDomainFeatureBuilder)
        /// By default the BlockStructure is not being used. If the blocks are added the features are then build by the 
        /// blocks and the FeatureBuilders are not used anymore
        /// Note that for SGD the features are always build by featureBuilders as they don't support block structure
        /// </summary>
        public LibFmTrainTesterBuilder(bool useBlocks, bool crossDomain)
        {
            UseBlocks = useBlocks;
            CrossDomain = crossDomain;
            CreateLibFms();
        }

        private void CreateLibFms()
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

                            LibFms.Add(libfm);
                        }
                    }
                }
            }
        }

        public void UpdateRecommender(LibFmTrainTester recommender, ISplitter<ItemRating> splitter, int numAuxRatings)
        {
            Domain targetDomain;

            if (CrossDomain)
            {
                targetDomain = ((CrossDomainSimpleSplitter)splitter).TargetDomain;

                if (UseBlocks && recommender.LearningAlgorithm != FmLearnigAlgorithm.SGD)
                {
                    recommender.Blocks.Clear();
                    recommender.Blocks.Add(new ItemsBlock());
                    recommender.Blocks.Add(new CrossDomainUsersBlock(targetDomain, numAuxRatings));
                }
                else
                {
                    recommender.FeatureBuilder = new CrossDomainLibFmFeatureBuilder(targetDomain, numAuxRatings);
                }
            }
            else
            {
                if (UseBlocks && recommender.LearningAlgorithm != FmLearnigAlgorithm.SGD)
                {
                    recommender.Blocks.Clear();
                    recommender.Blocks.Add(new ItemsBlock());
                    recommender.Blocks.Add(new UsersBlock());
                }
                else
                {
                    recommender.FeatureBuilder = new LibFmFeatureBuilder();
                }
            }
        }


    }
}
