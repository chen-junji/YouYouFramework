using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Core
{
    public class BaseAttribute : Attribute
    {
        public Type AttributeType { get; }

        public BaseAttribute()
        {
            AttributeType = GetType();
        }
    }
}