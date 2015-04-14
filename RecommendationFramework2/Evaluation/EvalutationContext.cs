using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec.Evaluation
{
    public class EvalutationContext<T>
    {
        Dictionary<string, object> _items;
        IModel _model;
        ISplitter<T> _splitter;

        public IDataset<T> Dataset { get; set; }

        public IModel Model 
        {
            get
            {
                return _model;
            }
            set 
            {
                _model = value;
                // When the model or splitter is updated the IsTested flag is set to false
                IsTested = false;
            }
        }

        public ISplitter<T> Splitter 
        {
            get
            {
                return _splitter;
            }
            set
            {
                _splitter = value;
                // When the model or splitter is updated the IsTested flag is set to false
                IsTested = false;
            }
        }
        public bool IsTested { get; set; }

        public EvalutationContext()
        {
            _items = new Dictionary<string, object>();  
        }

        [Obsolete("Use EvalutationContext(IModel model, ISplitter<T> splitter) constructor instead.")]
        public EvalutationContext(IModel model, IDataset<T> dataset)
            : this()
        {
            Model = model;
            Dataset = dataset;
        }

        public EvalutationContext(IModel model, ISplitter<T> splitter)
            : this()
        {
            Model = model;
            Splitter = splitter;
        }

        public void RunDefaultTrainAndTest()
        {
            // check if the test is alreay accomplished
            if (IsTested)
                return;

            if (Model is ITrainTester<T>)
            {
                if (Splitter != null)
                    ((ITrainTester<T>)Model).TrainAndTest(Splitter.Train, Splitter.Test);
                else
                    ((ITrainTester<T>)Model).TrainAndTest(Dataset.TrainSamples, Dataset.TestSamples);
            } 
            else if (Model is IPredictor<T>)
            {
                var predictor = (IPredictor<T>)Model;

                if (Splitter != null)
                {
                    // check if the recommender is trained
                    if (!predictor.IsTrained)
                        predictor.Train(Splitter.Train);

                    Log.Logger.Trace("Testing on test set...");

                    foreach (var sample in Splitter.Test)
                    {
                        predictor.Predict(sample);
                    }
                }
                else
                {
                    // check if the recommender is trained
                    if (!predictor.IsTrained)
                        predictor.Train(Dataset.TrainSamples);

                    Log.Logger.Trace("Testing on test set...");

                    foreach (var sample in Dataset.TestSamples)
                    {
                        predictor.Predict(sample);
                    }
                }
            }
            else
                throw new Exception("The model is not supported for test on test set.");
            
            IsTested = true;
        }

        public object this[string name]
        {
            get
            {
                return _items[name];
            }
            set
            {
                _items[name] = value;
            }
        }

        public Dictionary<string, object> Items
        {
            get { return _items; }
        }
    }
}
