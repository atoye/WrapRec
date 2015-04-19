using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;
using WrapRec.Readers.NewReaders;
using WrapRec.Utilities;

namespace WrapRec.RecSys2015
{
    public class MovieLensAdapter : Adapter
    {
        public DataContainer Container { get; private set; }

        public MovieLensAdapter()
        {
            Container = new DataContainer();
            var splits = new List<Split>() { Split.Train, Split.Test };

            foreach (var split in splits)
            {
                Log.Logger.Trace("Loading {0} into MovieLensContainer...", split.ToString());
                var reader = GetReader(split);
                reader.LoadData(Container);
            }

            Log.Logger.Info(Container.ToString());

        }

        public override Dictionary<string, ISplitter<ItemRating>> GetSplitters()
        {
            var splitters = new Dictionary<string, ISplitter<ItemRating>>();

            splitters.Add("MovieLens", new RatingSimpleSplitter(Container));

            return splitters;
        }

        private CsvReader GetReader(Split split)
        {
            var config = new CsvConfiguration()
            {
                Delimiter = "::",
                HasHeaderRecord = false
            };

            if (split == Split.Train)
                return new CsvReader(Paths.MovieLens1MTrain75, config);
            else
                return new CsvReader(Paths.MovieLens1MTest25, config, true);
        }
    }
}
