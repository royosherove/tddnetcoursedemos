using System;
using System.IO;

namespace TddDemos.InstanceDelegator
{
    public static class MyClass
    {
        public static bool IsMachingNameTooLong()
        {
            int maxLength = GetSetting("MaxLen");

            return GetMachineName().Length <= maxLength;
        }

        private static string GetMachineName()
        {
            return Environment.MachineName; 
        }

        private static int GetSetting(string setting)
        {
            return int.Parse(File.ReadAllText("./" + setting + ".txt"));
        }
    }
}

namespace Refactored
{
    public class MyClass
    {
        public static bool IsMachingNameTooLong()
        {
            return new MyClass().IsMachingNameLongerThanNeeded();
        }

        public bool IsMachingNameLongerThanNeeded()
        {
            int maxLength = GetSetting("MaxLen");

            return GetMachineName().Length >= maxLength;
        }

        protected virtual string GetMachineName()
        {
            return Environment.MachineName;
        }

        protected virtual  int GetSetting(string setting)
        {
            return int.Parse(File.ReadAllText("./" + setting + ".txt"));
        }
    }
}

