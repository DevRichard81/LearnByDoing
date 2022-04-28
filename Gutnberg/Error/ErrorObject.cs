using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Error
{
    public class ErrorObject
    {
        public enum ErrorType
        {
            None = 0,
            Warning,
            Error,
            Fatal
        };

        public DateTime timeStamp { get; private set; } = DateTime.Now;
        public ErrorType errorType { get; private set; } = ErrorType.None;
        public int errorNumber { get; private set; } = 0;
        public string errorMessage { get; private set; } = String.Empty;
        public string errorFunction { get; private set; } = String.Empty;
        public string errorParams { get; private set; } = String.Empty;

        public ErrorObject? internErrorObject { get; private set; }

        public ErrorObject Set(ErrorObject errorObject)
        {
            if (this.internErrorObject != null)
                return null;

            this.internErrorObject = errorObject;
            return this;
        }
        public ErrorObject Set(ErrorType errorType, int errorNumber)
        {
            this.errorType = errorType;
            this.errorNumber = errorNumber;
            return this;
        }
        public ErrorObject Set(ErrorType errorType, int errorNumber, string errorMessage)
        {
            this.errorMessage = errorMessage;
            Set(errorType, errorNumber);
            return this;
        }
        public ErrorObject Set(ErrorType errorType, int errorNumber, string errorMessage, string errorFunction)
        {
            this.errorFunction = errorFunction;
            Set(errorType, errorNumber, errorMessage);
            return this;
        }
        public ErrorObject Set(ErrorType errorType, int errorNumber, string errorMessage, string errorFunction, string errorParams)
        {
            this.errorParams = errorParams;
            Set(errorType, errorNumber, errorMessage, errorFunction);
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(timeStamp.ToLongDateString());
            sb.Append(' ');
            sb.Append(errorType.ToString());
            sb.Append(' ');
            sb.Append((int)errorNumber);
            sb.Append(' ');
            sb.Append(errorFunction);
            sb.Append('[');            
            sb.Append(errorParams);
            sb.Append("] ");
            sb.Append(errorMessage);
            return sb.ToString();
        }
    }
}
