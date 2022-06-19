using Project_Gutenberg.Configuration;
using Project_Gutenberg.Statistics;
using Project_Gutenberg.Error;
using Project_Gutenberg.Types;
using Project_Gutenberg.Statistic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Project_Gutenberg.GutenbergShared;

namespace Project_Gutenberg
{
    public class GutenbergAsync : Gutenberg, IGutenberg
    {
        private IConnectionTypeAsync? typeAsync;      

        ~GutenbergAsync() {
            Terminated();
            Thread.Sleep(100);            
        }
        public GutenbergAsync Configuration(IConfiguration newConfiguration, IConnectionTypeAsync newType)
        {
            Configuration(newConfiguration);
            typeAsync = newType;
            return this;
        }
        public override void Init()
        {
            if (configuration == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Configuration can not be null", "Init");
                return;
            }
            if (typeAsync == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, nameof(typeAsync) + " can not be null", "Init");
                return;
            }
            typeAsync.Init(configuration as IConfiguration, this);            
        }
        public override void Start()
        {
            typeAsync.Start();
            gutenbergThreads.Start();
        }
        
        public override void Terminated()
        {
            base.Terminated();
            typeAsync?.Close();
        }
        public override void HasError()
        {
            if (errorObject != null)
            {
                Console.WriteLine("Error in " + nameof(Gutenberg));
                Console.WriteLine(errorObject.ToString());
            }

            if (typeAsync.ErrorObject != null)
            {
                Console.WriteLine("Error in ConnectionType" + typeAsync.GetType().Name);
                Console.WriteLine(typeAsync.ErrorObject.ToString());
            }
        }      
    }
}