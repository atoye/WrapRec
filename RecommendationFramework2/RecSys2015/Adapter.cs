using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.RecSys2015
{
    public abstract class Adapter
    {
        public bool IncludeValidation { get; set; }

        public abstract Dictionary<string, ISplitter<ItemRating>> GetSplitters();
    }
}
