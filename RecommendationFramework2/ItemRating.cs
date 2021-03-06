﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;
using WrapRec.Data;

namespace WrapRec
{
    public class ItemRating : UserItem
    {
        public float Rating { get; set; }
        public float PredictedRating { get; set; }
        
        private Domain _domain;

        public Domain Domain 
        {
            get
            {
                return _domain;
            }
            set
            {
                if (_domain != null)
                {
                    // remove this rating from the old domain
                    _domain.Ratings.Remove(this);
                }
                // add this rating to the new domain
                value.Ratings.Add(this);
                _domain = value;
            }
        }

        public ItemRating()
        {
            Domain = CrossDomainDataContainer.GetDefualtDomain();
        }
        
        public ItemRating(string userId, string itemId)
            : base(userId, itemId)
        {
            Domain = CrossDomainDataContainer.GetDefualtDomain();
        }

        public ItemRating(string userId, string itemId, float rating)
            : this(userId, itemId)
        {
            Rating = rating;
        }

        public ItemRating(User user, Item item, float rating)
            : base(user, item)
        {
            Rating = rating;
            Domain = CrossDomainDataContainer.GetDefualtDomain();
        }

        public virtual string ToLibFmFeatureVector(Mapping usersItemsMap)
        {
            return string.Format("{0} {1}:1 {2}:1", Rating, usersItemsMap.ToInternalID(User.Id), usersItemsMap.ToInternalID(Item.Id));
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", User.Id, Item.Id, Rating);
        }
    }

}
