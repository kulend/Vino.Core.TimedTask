using System;
using System.Collections.Generic;
using System.Text;

namespace Vino.Core.TimedTask.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TimedTaskAttribute : System.Attribute
    {
        public string Name { set; get; }

        public TimedTaskAttribute()
        {
        }

        public TimedTaskAttribute(string name)
        {
            this.Name = name;
        }
    }
}
