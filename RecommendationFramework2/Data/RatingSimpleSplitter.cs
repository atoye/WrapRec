using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;

namespace WrapRec.Data
{
    public class RatingSimpleSplitter : ISplitter<ItemRating>
    {
        double _validationRatio = 0.1;

        public DataContainer Container { get; set; }

        public float TestPortion { get; set; }

        public RatingSimpleSplitter(DataContainer container, bool includeValidation = false)
        {
            var temp = container.Ratings.Where(r => r.IsTest == false);
            if (includeValidation)
            {
                int trainCount = Convert.ToInt32(temp.Count() * (1 - _validationRatio));
                Train = temp.Take(trainCount);
                Validation = temp.Skip(trainCount);
            }
            else
            {
                Train = temp;
            }

            Test = container.Ratings.Where(r => r.IsTest == true);
        }

        public RatingSimpleSplitter(DataContainer container, float testPortion)
        {
            Container = container;
            TestPortion = testPortion;

            var ratings = Container.Ratings.Shuffle();
            int trainCount = (int) Math.Round(ratings.Count() * (1 - testPortion));
            Train = ratings.Take(trainCount);
            Test = ratings.Skip(trainCount);
        }

        public IEnumerable<ItemRating> Train { get; private set; }

        public IEnumerable<ItemRating> Test { get; private set; }

        public IEnumerable<ItemRating> Validation { get; private set; }
    }
}
