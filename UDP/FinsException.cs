﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BodDetect.UDP
{
    public class FinsException : Exception
    {
        public FinsException()
        {
        }

        public FinsException(string message) : base(message)
        {
        }

        public FinsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FinsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
