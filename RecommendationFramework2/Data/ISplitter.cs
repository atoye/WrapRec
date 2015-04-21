﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public interface ISplitter<T>
    {
        IEnumerable<T> Train { get; }
        IEnumerable<T> Test { get; }
        IEnumerable<T> Validation { get; }
    }
}
