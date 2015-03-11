using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class ItemsBlock : LibFmBlock
    {
        public ItemsBlock()
            : base("items")
        { }

        public override int UpdateBlock(ItemRating rating)
        {
            int featId = Mapper.ToInternalID(rating.Item.Id);

            if (!BlockIndex.ContainsKey(featId))
            {
                string newBlock = string.Format("{0} {1}:1", rating.Rating, featId);
                int newIndex = Blocks.Count;

                Blocks.Add(newBlock);
                BlockIndex.Add(featId, newIndex);

                return newIndex;
            }

            return BlockIndex[featId];
        }
    }
}
