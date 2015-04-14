using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.RecSys2015
{
    public class AmazonSimpleSplitter : ISplitter<ItemRating>
    {
        public AmazonSimpleSplitter(AmazonDataContainer container, Domain targetDomain)
        {
            Train = container.Ratings.Where(r => r.Domain.Id == targetDomain.Id && r.IsTest == false);
            Test = container.Ratings.Where(r => r.Domain.Id == targetDomain.Id && r.IsTest == true);
        }

        public IEnumerable<ItemRating> Train
        {
            get;
            private set;
        }

        public IEnumerable<ItemRating> Test
        {
            get;
            private set;
        }
    }
}
