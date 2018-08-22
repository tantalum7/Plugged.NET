#region Imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace Plugged.NET
{
    public class ssdpPacket : Dictionary<string, string>
    {

        public static ssdpPacket Parse(byte[] packetBytes)
        {
            // Create ssdpPacket instance
            ssdpPacket packetObject = new ssdpPacket();

            // Convert bytes into UTF8 string
            string packet = Encoding.UTF8.GetString(packetBytes);

            // Split the string on every new line
            IEnumerable<string> lines = packet.Split('\n');

            // Iterate through each line
            foreach(string line in lines)
            {
                // If this line doesn't have a colon, skip it
                if (!line.Contains(':')) { continue; }

                // Grab the key by splitting on the first colon found, and force lowercase and trim spaces
                string key = line.Substring(0, line.IndexOf(':')).ToLower().Trim();

                // Grab the value as everything after the first colon, stripping out spaces
                string value = line.Substring(line.IndexOf(':') + 1).Trim();

                // Store the key:value pair in packetObject dictionary-like
                packetObject[key] = value;
            }

            // Return the packetObject
            return packetObject;
        }


    }
}
