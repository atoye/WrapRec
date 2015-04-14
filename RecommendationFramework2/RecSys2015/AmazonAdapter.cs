using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using CsvHelper.Configuration;
using WrapRec.Readers.NewReaders;

namespace WrapRec.RecSys2015
{
    public enum Split { Train, Test, All }

    public class AmazonAdapter
    {
        static double _trainRatio = 0.75;
        static double _testRatio = 0.25;
        
        public static string GetPath(string domainId, Split split, double ratio = -1)
        {
            // The datasets ending with selected4 are the ones containing users who has at least 
            // one rating in all domains (see JournalExperiments.cs)
            return string.Format(@"D:\Data\Datasets\Amazon\RecSys2015\{0}_selected4{1}{2}.csv",
                domainId,
                split == Split.All ? "" : "_" + split.ToString().ToLower(),
                ratio == -1 ? "" : (ratio * 100).ToString());

        }

        public static void CreateTrainTestSplits(string domainId)
        {
            Utilities.FileHelper.SplitLines(
                GetPath(domainId, Split.All),
                GetPath(domainId, Split.Train, _trainRatio),
                GetPath(domainId, Split.Test, _testRatio),
                _trainRatio,
                true,
                true);
        }

        public static List<Domain> AmazonDomains = new List<Domain>() 
        {
            new Domain("book"),
            new Domain("music"),
            new Domain("video"),
            new Domain("dvd")
        };

        // this is one time only call to prepare files and other stuff for doing experiments
        public static void PrepareDatasets()
        {
            AmazonDomains.ForEach(d => CreateTrainTestSplits(d.Id));
        }

        public AmazonDataContainer Container { get; set; }
        public ISplitter<ItemRating> Splitter { get; set; }

        public AmazonAdapter()
        {
            Container = new AmazonDataContainer();

            var splits = new List<Split>() { Split.Train, Split.Test };

            foreach (var ad in AmazonDomains)
            {
                foreach (var split in splits)
                {
                    Log.Logger.Trace("Loading {0} {1} into AmazonDataContainer...", ad.Id, split.ToString());
                    var reader = GetReader(ad, split);
                    reader.LoadData(Container);
                }
            }

            Container.PrintStatistics();
        }

        private CsvReader GetReader(Domain domain, Split split)
        {
            var config = new CsvConfiguration()
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            string path;
            if (split == Split.All)
                path = GetPath(domain.Id, split);
            else
                path = GetPath(domain.Id, split, split == Split.Train ? _trainRatio : _testRatio);

            return new CsvReader(path, config, domain, (split == Split.Test));
        }

        public Dictionary<string, ISplitter<ItemRating>> GetSimpleSplitters()
        {
            var splitters = new Dictionary<string, ISplitter<ItemRating>>();

            foreach (var ad in AmazonDomains)
            {
                splitters.Add(ad.Id, new AmazonSimpleSplitter(Container, ad));
            }

            return splitters;
        }

    }
}
