using System.Text;

namespace Project_Gutenberg.Statistics
{
    public record StatisticInterface
    {
        public uint runningThreads = 0;

        private uint _readData = 0;
        public uint readData
        {
            get
            {
                return _readData;
            }
            set
            {
                _readData += value;
            }
        }
        
        private uint _writeData = 0;
        public uint writeData
        {
            get
            {
                return _writeData;
            }
            set
            {
                _writeData += value;
            }
        }

        private uint _incomingMessage = 0;
        public uint IncomingMessage
        {
            get
            {
                return _incomingMessage;
            }
            set
            {
                _incomingMessage += value;
            }
        }

        private uint _outcomingMessage = 0;
        public uint OutcomingMessage
        {
            get
            {
                return _outcomingMessage;
            }
            set
            {
                _outcomingMessage += value;
            }
        }

        public void Reset()
        {
            runningThreads = 0;
            _readData = 0;
            _writeData = 0;
            _outcomingMessage = 0;
            _incomingMessage = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(runningThreads);
            sb.Append(" ReadData:");
            sb.Append(_readData);
            sb.Append(" WriteData:");
            sb.Append(_writeData);
            sb.Append(" IncomingMessages:");
            sb.Append(_incomingMessage);
            sb.Append(" OutcomingMessages:");
            sb.Append(_outcomingMessage);
            return sb.ToString();
        }
    }
}
