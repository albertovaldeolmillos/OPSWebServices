using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMHelpers
{
    public class RegistryService
    {
        /// <summary>
        /// Read a value from a registry key stored in Local Machine context
        /// </summary>
        /// <param name="keyPath">Path to the registry key</param>
        /// <param name="valueName">Name of the value to read</param>
        /// <returns>String stored key path value</returns>
        /// <exception cref="NullReferenceException"></exception>
        public string ReadKeyValue(string keyPath, string valueName)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
            return key.GetValue(valueName).ToString();
        }

        public string[] ReadSubKeys(string keyPath) {
            RegistryKey baseKey = RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, "");
            RegistryKey otsKey = baseKey.OpenSubKey(keyPath);

            return otsKey.GetSubKeyNames();
        }
    }
}
