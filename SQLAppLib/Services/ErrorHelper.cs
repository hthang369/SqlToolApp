using System;
using System.Collections.Generic;

namespace SQLAppLib.Services
{
    public class ErrorHelper
    {
        public ErrorHelper()
        {
            this.Errors = new List<ErrorDescriber>();
        }

        public List<ErrorDescriber> AddError(ErrorCodes error)
        {
            return this.AddError(error, null);
        }

        public List<ErrorDescriber> AddError(Exception exception)
        {
            return this.AddError(ErrorCodes.Exception, exception);
        }

        public List<ErrorDescriber> AddError(ErrorCodes error, Exception exception)
        {
            ErrorDescriber describer1 = new ErrorDescriber();
            describer1.Code = error;
            describer1.Exception = exception;
            this.Errors.Add(describer1);
            return this.Errors;
        }

        public List<ErrorDescriber> AddErrorRange(IEnumerable<ErrorDescriber> ErrorsRange)
        {
            this.Errors.AddRange(ErrorsRange);
            return this.Errors;
        }

        public List<ErrorDescriber> Errors { get; set; }
    }
}
