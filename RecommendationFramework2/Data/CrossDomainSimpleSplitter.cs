﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;
using System.IO;

namespace WrapRec.Data
{
    public class CrossDomainSimpleSplitter : ISplitter<ItemRating>
    {
        public Domain TargetDomain { get; set; }

        public CrossDomainSimpleSplitter(CrossDomainDataContainer container, Domain targetDomain)
        {
            TargetDomain = targetDomain;
            Train = container.Ratings.Where(r => r.IsTest == false && r.Domain.Id == targetDomain.Id);
            Test = container.Ratings.Where(r => r.IsTest == true && r.Domain.Id == targetDomain.Id);
        }

        public CrossDomainSimpleSplitter(CrossDomainDataContainer container)
        {
            Train = container.Ratings.Where(r => r.IsTest == false && r.Domain.IsTarget == true);
            Test = container.Ratings.Where(r => r.IsTest == true && r.Domain.IsTarget == true);
        }

        public CrossDomainSimpleSplitter(CrossDomainDataContainer container, float testPortion)
        {
            var targetRatings = container.Ratings.Where(r => r.Domain.IsTarget == true).Shuffle();
            int trainCount = (int)Math.Round(targetRatings.Count() * (1 - testPortion));

            Train = targetRatings.Take(trainCount); //.Concat(container.Ratings.Where(r => r.Domain.IsTarget == false));
            Test = targetRatings.Skip(trainCount);
        }

        public void SaveSplitsAsCsv(string trainPath, string testPath)
        {
            var header = new string[] { "UserId,ItemId,Rating" };
            File.WriteAllLines(trainPath, header.Concat(Train.Select(r => r.ToString())));
            File.WriteAllLines(testPath, header.Concat(Test.Select(r => r.ToString())));
        }
        
        public IEnumerable<ItemRating> Train { get; private set; }

        public IEnumerable<ItemRating> Test { get; private set; }
    }
}
