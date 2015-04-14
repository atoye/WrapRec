using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class CrossDomainUsersBlock : LibFmBlock
    {
        public int NumAuxRatings { get; set; }

        public Domain TargetDomain { get; set; }

        public CrossDomainUsersBlock(Domain targetDomain, int numAuxRatings)
            : base("xdusers")
        {
            NumAuxRatings = numAuxRatings;
            TargetDomain = targetDomain;
        }
        
        public override int UpdateBlock(ItemRating rating)
        {
            int featId = Mapper.ToInternalID(rating.User.Id);

            if (!BlockIndex.ContainsKey(featId))
            {
                string newBlock = string.Format("{0} {1}:1{2}", rating.Rating, featId, BuildFeatVector(rating.User));
                int newIndex = Blocks.Count;

                Blocks.Add(newBlock);
                BlockIndex.Add(featId, newIndex);

                return newIndex;
            }

            return BlockIndex[featId];
        }

        private string BuildFeatVector(User user)
        {
            string extendedVector = "";

            Func<ItemRating, bool> checkActivation = r => r.Domain.IsActive == true; //r.IsActive == true;

            var domainRatings = user.Ratings.Where(checkActivation).GroupBy(r => r.Domain.Id);
            var avgUserRating = user.Ratings.Average(r => r.Rating);

            foreach (var d in domainRatings)
            {
                if (d.Key != TargetDomain.Id)
                {
                    int ratingCount = d.Count();

                    string domainExtendedVector = d.OrderByDescending(r => r.Item.Ratings.Count)
                        //d.Shuffle()
                        .Take(NumAuxRatings)
                        // ItemIds are concateneated with domain id to make sure that items in different domains are being distingushed
                        //.Select(dr => string.Format("{0}:1", Mapper.ToInternalID(dr.Item.Id + d.Key.Id)))
                        .Select(dr => string.Format("{0}:{1:0.0000}", Mapper.ToInternalID(dr.Item.Id), (double)(dr.Rating - avgUserRating) / 4 + 1)) // dr.Rating / ratingCount))
                        .Aggregate((cur, next) => cur + " " + next);

                    if (!String.IsNullOrEmpty(domainExtendedVector.TrimEnd(' ')))
                        extendedVector += " " + domainExtendedVector;

                }
            }

            return extendedVector;
        }

        private string BuildFeatVectorOptimal(User user)
        {
            string extendedVector = "";

            Func<ItemRating, bool> checkActivation = r => r.Domain.IsActive == true; //r.IsActive == true;

            var domainRatings = user.Ratings.Where(checkActivation).GroupBy(r => r.Domain.Id);
            int userAuxDomains = user.Ratings.Select(r => r.Domain.Id).Distinct().Where(d => d != TargetDomain.Id).Count();

            int perDomainRatings;
            if (userAuxDomains > 0)
                perDomainRatings = (int)Math.Ceiling((double)NumAuxRatings / userAuxDomains);
            else
                perDomainRatings = 0;

            var avgUserRating = user.Ratings.Average(r => r.Rating);

            int take = perDomainRatings;
            int remain = NumAuxRatings;

            foreach (var d in domainRatings)
            {
                if (d.Key != TargetDomain.Id)
                {
                    int ratingCount = d.Count();
                    string domainExtendedVector = "";

                    var ratings = d.OrderByDescending(r => r.Item.Ratings.Count).Take(take);

                    if (ratings.Count() > 0)
                    {
                        domainExtendedVector = ratings
                            .Select(dr => string.Format("{0}:{1:0.0000}", Mapper.ToInternalID(dr.Item.Id), (double)(dr.Rating - avgUserRating) / 4 + 1)) // dr.Rating / ratingCount))
                            .Aggregate((cur, next) => cur + " " + next);
                    }

                    if (!String.IsNullOrEmpty(domainExtendedVector.TrimEnd(' ')))
                        extendedVector += " " + domainExtendedVector;

                    remain = remain - perDomainRatings;
                    if (remain > 0)
                        take = perDomainRatings;
                    else
                        take = 0;
                }
            }

            return extendedVector;
        }

    }
}
