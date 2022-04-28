using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Gutenberg.Configuration;
using Gutenberg.Error;
using Gutenberg.Statistic;

namespace Gutenberg.Types.File
{
    public class FileConnection : IConnectionType
    {
        private IConfiguration? Configuration { get; set; }
        private string PickFiles;
        private string MoveErrorFiles;
        public ErrorObject? ErrorObject { get; private set; }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationFile;
            CheckDirectorys((ConfigurationFile)Configuration);
        }

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
                Directory.CreateDirectory(Path.Combine(configurationFile.baseDirectory, configurationFile.errorDirectory));
            }
            // Pre Configuration some Configurations
            PickFiles = Path.Combine(configurationFile.baseDirectory, configurationFile.incomeDirectory);
            MoveErrorFiles = Path.Combine(configurationFile.baseDirectory, configurationFile.errorDirectory);
            Thread.Sleep(100);
        }

        public void Read(ref StatisticOfFunction statisticOfFunction)
        {
            var config = (ConfigurationFile)Configuration;
            foreach (var itm in Directory.GetFiles(PickFiles))
            {
                var file = new FileInfo(itm);
                file.Attributes &= ~FileAttributes.Normal;
                file.MoveTo(MoveErrorFiles +"\\"+ file.Name, true);

                
                statisticOfFunction.handelMessage++;
                statisticOfFunction.handelDataLength += (uint)file.Length;
                // Finish File
                file.Refresh();
            }
        }

        public void Write(ref StatisticOfFunction statisticOfFunction)
        {

        }
    }
}
