using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.RecSys2015
{
    public class AmazonReader : IDatasetReader
    {
        
        public void LoadData(DataContainer container)
        {
            if (!(container is AmazonDataContainer))
                throw new WrapRecException("The data container should have type AmazonDataContainer.");


        }
    }
}
