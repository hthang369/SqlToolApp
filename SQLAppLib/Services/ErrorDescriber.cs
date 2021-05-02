using System;

namespace SQLAppLib.Services
{
    public class ErrorDescriber
    {
        public ErrorDescriber()
        {
            this.Code = ErrorCodes.None;
            this.Exception = null;
        }

        public ErrorDescriber(ErrorCodes code, System.Exception exception)
        {
            this.Code = code;
            this.Exception = exception;
        }

        public ErrorCodes Code { get; set; }

        public System.Exception Exception { get; set; }
    }

    public enum ErrorCodes
    {
        Empty,
        Exception,
        None,
        NotFound
    }
}
