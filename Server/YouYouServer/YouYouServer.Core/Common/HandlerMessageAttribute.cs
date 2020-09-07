using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Core
{
    public class HandlerMessageAttribute : BaseAttribute
    {
        public ushort ProtoId
        {
            get;
        }

        public HandlerMessageAttribute(ushort protoId)
        {
            ProtoId = protoId;
        }
    }
}