using System.Collections.Concurrent;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types.File
{
    public class FileConnection : IConnectionType
    {
        private ConfigurationFile? Configuration { get; set; }
        private string PickFiles;
        private string SendFiles;
        private string MoveReadedFiles;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationFile;
            CheckDirectorys(Configuration);
        }

        /// <summary>
        /// Not used for FileConnction
        /// </summary>
        public void Close() { }
        /// <summary>
        /// Not used for FileConnction
        /// </summary>
        public void Start() { }


        private void CheckDirectorys(ConfigurationFile configurationFile)
        {
            if(configurationFile == null)
            {
                ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "configuration can not be null", "CheckDirectorys");
                return;
            }
            if (!Directory.Exists(configurationFile.baseDirectory))
            {
                ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Warning, 0, "creating missing directorys", "CheckDirectorys", configurationFile.baseDirectory);
                Directory.CreateDirectory(configurationFile.baseDirectory);
                Directory.CreateDirectory(Path.Combine(configurationFile.baseDirectory, configurationFile.incomeDirectory));
                Directory.CreateDirectory(Path.Combine(configurationFile.baseDirectory, configurationFile.outcomeDirectory));
                Directory.CreateDirectory(Path.Combine(configurationFile.baseDirectory, configurationFile.readedDirectory));
            }
            // Pre Configuration some Configurations
            PickFiles = Path.Combine(configurationFile.baseDirectory, configurationFile.incomeDirectory);
            SendFiles = Path.Combine(configurationFile.baseDirectory, configurationFile.outcomeDirectory);
            MoveReadedFiles = Path.Combine(configurationFile.baseDirectory, configurationFile.readedDirectory);
            Thread.Sleep(100);
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            foreach (var itm in Directory.GetFiles(PickFiles))
            {
                var file = new FileInfo(itm);
                file.Attributes &= ~FileAttributes.Normal;
                file.MoveTo(MoveReadedFiles +"\\"+ file.Name, true);

                using (var x = file.OpenRead())
                {
                    if (x.Length > 0)
                    {
                        byte[] bytes = new byte[x.Length];
                        x.Read(bytes, 0, bytes.Length);
                        buffer.Enqueue(bytes);
                        statisticOfFunction.handelDataLength += (uint)file.Length;
                    }
                }
                statisticOfFunction.handelMessage++;
                statisticOfFunction.type = StatisticOfFunction.Type.Incoming;
                // Finish File
                file.Refresh();
            }
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            byte[]? sendBuffer;
            if (buffer.TryDequeue(out sendBuffer))
            {
                System.IO.File.WriteAllBytes(SendFiles + "\\" 
                    + Configuration.sendFileSuffix 
                    + DateTime.Now.ToString(Configuration.sendFileDateFormat) 
                    + Configuration.sendFilePrefix
                    + "." + Configuration.sendFileExtension, sendBuffer);
                statisticOfFunction.handelMessage++;
                statisticOfFunction.handelDataLength = (uint)sendBuffer.Length;
                statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
            }
        }
    }
}
