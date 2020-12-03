using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Clubby.Events
{
    public class Event
    {
        public (ConstraintType, string[])[] Constraints = null;
        public string EventId = null;
        public MethodInfo method = null;
    }
}
