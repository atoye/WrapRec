using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.Readers.NewReaders
{
    public class MovieLensContextAwareReader : IDatasetReader
    {
        public string MoviesPath { get; set; }
        public string TrainPath { get; set; }
        public string TestPath { get; set; }

        public MovieLensContextAwareReader(string moviesPath, string trainPath, string testPath)
        {
            MoviesPath = moviesPath;
            TrainPath = trainPath;
            TestPath = testPath;
        }
        public void LoadData(DataContainer container)
        {
            foreach (string l in File.ReadAllLines(MoviesPath))
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                var item = container.AddItem(parts[0]);
                item.Properties["genres"] = parts[2];
            }

            foreach (string l in File.ReadAllLines(TrainPath))
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                var ir = container.AddRating(parts[0], parts[1], float.Parse(parts[2]), false);
                ir.Properties["timestamp"] = parts[3];
            }

            foreach (string l in File.ReadAllLines(TestPath))
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                var ir = container.AddRating(parts[0], parts[1], float.Parse(parts[2]), true);
                ir.Properties["timestamp"] = parts[3];
            }
        }
    }
}
