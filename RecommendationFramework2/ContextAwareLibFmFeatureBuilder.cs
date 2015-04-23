using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class ContextAwareLibFmFeatureBuilder : LibFmFeatureBuilder
    {
        public List<Func<ItemRating, string>> Contexts { get; private set; }
        
        public ContextAwareLibFmFeatureBuilder(params Func<ItemRating, string>[] contextSelectors)
        {
            Contexts = new List<Func<ItemRating, string>>();
            Contexts.AddRange(contextSelectors);
        }

        public override string GetLibFmFeatureVector(ItemRating rating)
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

            return base.GetLibFmFeatureVector(rating) + " " + extension;
        }
    }
}
