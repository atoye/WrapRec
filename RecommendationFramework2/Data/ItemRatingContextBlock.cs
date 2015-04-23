using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Data
{
    public class ItemRatingContextBlock : LibFmBlock
    {
        public List<Func<ItemRating, string>> Contexts { get; private set; }

        // This map is used to map item ids which are used as key to the block dictionary
        // this map should be diffetent with the original map since this translation is not used in the actual feature vector
        Mapping _itemIdMaps;

        public ItemRatingContextBlock(params Func<ItemRating, string>[] contextSelectors)
            : base("itemratingcontext")
        {
            Contexts = new List<Func<ItemRating, string>>();
            Contexts.AddRange(contextSelectors.ToArray());
            _itemIdMaps = new Mapping();
        }

        public override int UpdateBlock(ItemRating rating)
        {
            string extension = Contexts.SelectMany(f =>
            {
                string context = f(rating);

                // if context contains | it means it has multiple values
                if (context.Contains('|'))
                {
                    return context.Split('|');
                }
                else
                    return new string[] { context };
            })
            .Select(c => string.Format("{0}:1", Mapper.ToInternalID(c)))
            .Aggregate((a, b) => a + " " + b);
            
            int featId = _itemIdMaps.ToInternalID(rating.Item.Id);

            if (!BlockIndex.ContainsKey(featId))
            {
                string newBlock = string.Format("{0} {1}", rating.Rating, extension);
                int newIndex = Blocks.Count;

                Blocks.Add(newBlock);
                BlockIndex.Add(featId, newIndex);

                return newIndex;
            }

            return BlockIndex[featId];
        }
    }
}
