﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public interface IItemRecommender
    {
        UserRankedList Recommend(User u);
    }
}
