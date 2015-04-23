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
    public class MovieLensContextAwareAdapter : Adapter
    {
        public DataContainer Container { get; private set; }

        public MovieLensContextAwareAdapter(bool includeValidation = false)
        {
            Container = new DataContainer();
            IncludeValidation = includeValidation;

            var reader = GetReader();
            reader.LoadData(Container);

            Console.WriteLine(Container);
        }

        public override Dictionary<string, ISplitter<ItemRating>> GetSplitters()
        {
            var splitters = new Dictionary<string, ISplitter<ItemRating>>();

            splitters.Add("MovieLens", new RatingSimpleSplitter(Container, IncludeValidation));

            return splitters;
        }

        private IDatasetReader GetReader()
        {
            return new MovieLensContextAwareReader(Paths.MovieLens1MMovies, Paths.MovieLens1MTrain75, Paths.MovieLens1MTest25);
        }
    }
}
