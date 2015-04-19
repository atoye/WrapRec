﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Experiments;
using System.Runtime.InteropServices;
using System;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;
using System.IO;
using CenterSpace.NMath.Core;
using CenterSpace.NMath.Stats;
using WrapRec.Utilities;
using System.Diagnostics;
using WrapRec.RecSys2015;

namespace WrapRec
{
    class Program
    {
        static void Main(string[] args)
        {
            //(new MovieLensTester()).Run();
            //(new AmazonTester()).Run();
            //(new Ectel2014Experiments()).Run();
            //(new TrustBasedExperiments()).Run();
            //(new Recsys2014Experiments()).Run();
            //(new Journal2014Experiments()).Run();
            //(new FreeLunchExperiments()).Run();
            //(new TrustExperiments2()).Run();
            //(new RecSys2015Experiments()).Run();
            //AmazonContainer.PrepareDatasets();
            //EpinionsAdapter.SplitDataset();
            (new RecSys2015.Experiments("log.txt")).Run();
            //ResultAnalyzer.Run();

            Console.WriteLine("Finished!.");

            return;
        }

    }
}
