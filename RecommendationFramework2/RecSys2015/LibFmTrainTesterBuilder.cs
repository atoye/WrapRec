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
            //FmLearnigAlgorithm.SGDA
            //FmLearnigAlgorithm.ALS
            FmLearnigAlgorithm.MCMC
        };

        int[] _iterations = new int[] { 50 };

        double[] _learningRates = new double[] { 0.001 };
        
        string[] _dimenstions = new string[] { "1,1,5" };

        string[] _regulars = new string[] { "0.1,0.1,0.5" };

        public List<Func<ItemRating, string>> ContextSelectors { get; private set; }

        public List<LibFmTrainTester> LibFms { get; private set; }

        public bool UseBlocks { get; set; }
        public bool CrossDomain { get; set; }

        /// <summary>
        /// Note that for SGD the features are always build by featureBuilders as they don't support block structure
        /// </summary>
        public LibFmTrainTesterBuilder(bool useBlocks, bool crossDomain)
        {
            UseBlocks = useBlocks;
            CrossDomain = crossDomain;
            ContextSelectors = new List<Func<ItemRating, string>>();
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
                            foreach (var reg in _regulars)
                            {
                                var libfm = new LibFmTrainTester(alg: la, numIterations: iter, learningRate: lr, dimensions: dim, regularization: reg) 
                                { CreateBinaryFiles = true };

                                LibFms.Add(libfm);
                            }
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

                if (UseBlocks && recommender.LearningAlgorithm != FmLearnigAlgorithm.SGD && recommender.LearningAlgorithm != FmLearnigAlgorithm.SGDA)
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
                if (UseBlocks && recommender.LearningAlgorithm != FmLearnigAlgorithm.SGD && recommender.LearningAlgorithm != FmLearnigAlgorithm.SGDA)
                {
                    recommender.Blocks.Clear();
                    recommender.Blocks.Add(new ItemsBlock());
                    recommender.Blocks.Add(new UsersBlock());

                    if (ContextSelectors.Count > 0)
                    {
                        recommender.Blocks.Add(new ItemRatingContextBlock(ContextSelectors.ToArray()));
                    }
                }
                else
                {
                    recommender.FeatureBuilder = GetFeatureBuilder();
                }
            }
        }

        private LibFmFeatureBuilder GetFeatureBuilder()
        {
            if (ContextSelectors.Count > 0)
            {
                return new ContextAwareLibFmFeatureBuilder(ContextSelectors.ToArray());
            }
            else
                return new LibFmFeatureBuilder();
        }


    }
}
