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
    public class MovieLensCrossDomainAdapter : Adapter
    {
        float _testPortion = 0.25f;

        public MovieLensCrossDomainContainer Container { get; set; }

        public MovieLensCrossDomainAdapter(int numDomains)
        {
            var movieLensReader = new MovieLensCrossDomainReader(Paths.MovieLens1MMovies, Paths.MovieLens1M);
            Container = new MovieLensCrossDomainContainer(numDomains);
            movieLensReader.LoadData(Container);

            Container.PrintStatistics();
        }

        public override Dictionary<string, ISplitter<ItemRating>> GetSplitters()
        {
            var splitters = new Dictionary<string, ISplitter<ItemRating>>();

            foreach (var d in Container.Domains.Values)
            {
                splitters.Add(d.Id, new CrossDomainSimpleSplitter(Container, d, _testPortion));
            }

            return splitters;
        }
    }
}
