using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace WrapRec.Data
{
    public abstract class LibFmBlock
    {
        public Mapping Mapper { get; set; }
        public List<string> Blocks { get; private set; }
        public Dictionary<int, int> BlockIndex { get; private set; }
        public string Name { get; set; }
        
        public LibFmBlock(string name)
        {
            Name = name;
            Mapper = new Mapping();
            Blocks = new List<string>();
            BlockIndex = new Dictionary<int, int>();
        }

        /// <summary>
        /// Create and add the corresponding block (if not exists) for the ItemRating and return the index of block.
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public abstract int UpdateBlock(ItemRating rating);
    }
}
