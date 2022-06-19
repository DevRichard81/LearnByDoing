using Project_Gutenberg.Configuration;
using Project_Gutenberg.Statistics;
using Project_Gutenberg.Error;
using Project_Gutenberg.Types;
using Project_Gutenberg.Statistic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Project_Gutenberg.GutenbergShared;

namespace Project_Gutenberg
{
    public class Gutenberg : IGutenberg
    {                
        protected IConfiguration? configuration;
        private IConnectionType? type;
        public StatisticInterface? statistic { get; set; }
        public ErrorObject? errorObject { get; set; }
        public IGutenbergBuffers gutenbergBuffers { get; set; }
        public GutenbergThreads gutenbergThreads { get; set; }
        public bool isServer { get; set; }

        public Gutenberg()
        {
            statistic = new StatisticInterface();
            errorObject = new ErrorObject();
            gutenbergBuffers = new GutenbergBuffers();
            gutenbergThreads = new GutenbergThreads();
        }

        ~Gutenberg() {
            Terminated();
            Thread.Sleep(100);
        }
        public void Configuration(IConfiguration newConfiguration)
        {
            configuration = newConfiguration;
        }

        public Gutenberg Configuration(IConfiguration newConfiguration, IConnectionType newType)
        {
            Configuration(newConfiguration);
            type = newType as IConnectionType;
            return this;
        }

        public virtual void Init()
        {
            if (configuration == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Configuration can not be null", "Init");
                return;
            }
            if (type == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, nameof(type) + " can not be null", "Init");
                return;
            }
            type.Init(configuration as IConfiguration, this);
        }
        public virtual void Start()
        {
            gutenbergThreads.Start();
        }
        public virtual void Terminated() => type?.Close();
        public bool HasMessage() => gutenbergBuffers.HasMessage();
        public bool HasConnection() => type.HasConnection();      
        public virtual void HasError()
        {
            if (errorObject != null && errorObject.errorNumber != 0)
            {
                Console.WriteLine("Error in " + nameof(Gutenberg));
                Console.WriteLine(errorObject.ToString());
            }

            if (type?.ErrorObject != null)
            {
                Console.WriteLine("Error in ConnectionType" + type.GetType().Name);
                Console.WriteLine(type.ErrorObject.ToString());
            }
        }
        public void Status()
        {
            Console.WriteLine("Threads:");
            Console.WriteLine(gutenbergThreads.ToString());
            Console.WriteLine("Buffers:");
            Console.WriteLine(gutenbergBuffers.ToString());
        }        

        public virtual void Put(byte[] setMessage)
        {
            gutenbergBuffers.AddSendMessage(setMessage);
        }

        public virtual byte[] Get()
        {
            return gutenbergBuffers.GetReceivedMessage();
        }
    }
}