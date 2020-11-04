using System.Collections.Generic;

namespace Agent.Models
{
    public class TaskParameters
    {
        public List<Parameter> Parameters { get; set; }

        public class Parameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }
    }
}