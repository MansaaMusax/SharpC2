using System.Collections.Generic;

namespace Agent.Modules
{
    public class SleepModel
    {
        public ModuleCommand Command { get; set; }

        public class ModuleCommand
        {
            public string Name { get; set; }
            public List<ModuleParameter> Parameters { get; set; }
        }

        public class ModuleParameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }
    }
}