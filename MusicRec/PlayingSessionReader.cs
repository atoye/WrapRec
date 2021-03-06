﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using WrapRec.Data;
using System.IO;

namespace MusicRec
{
    public class PlayingSessionReader : IDatasetReader
    {
        string _path;
        
        public PlayingSessionReader(string path)
        {
            _path = path;
        }

        public void LoadData(DataContainer container)
        {
            if (!(container is MusicDataContainer))
            {
                throw new Exception("The data container should have type MusicDataContainer.");
            }

            var mContainer = (MusicDataContainer)container;

            foreach (string line in File.ReadAllLines(_path))
            {
                var tokens = line.Split(',');
                var us = new UserSession() { User = mContainer.AddUser(tokens[0]) };
                var tracks = tokens.Skip(2).Aggregate((a, b) => a + "," + b).Split('|');

                foreach (string track in tracks.Take(tracks.Length - 1))
                {
                    var parts = track.Split(',').Reverse().ToList();
                    //mContainer.AddPositiveFeedback(tokens[0], parts[0]);
                    
                    mContainer.AddRating(tokens[0], parts[2], float.Parse(parts[1]) / 120);
                    
                    var item = mContainer.AddItem(parts[2]);
                    us.Items.Add(item);
                }

                mContainer.Sessions.Add(us);
            }
        }
    }
}
