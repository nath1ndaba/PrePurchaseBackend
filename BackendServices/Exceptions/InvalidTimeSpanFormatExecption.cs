using System;

namespace BackendServices.Exceptions
{
    public class InvalidTimeSpanFormatExecption : Exception
    {
        public InvalidTimeSpanFormatExecption() : base("Timespan argument must be a number or number string ending in either s,S,m,M,h,H,d,D,w or W!!")
        { }
    }
}
