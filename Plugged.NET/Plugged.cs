#region Imports
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#endregion


namespace Plugged.NET
{
    public static class Plugged
    {
        private static string broadcastIP = "239.255.255.250";
        private static int broadcastPort = 1900;

        public static void Discover(string filter="ssdp:all", int wait=3, int repeatCount=3, bool ignoreDslForumDevices=true)
        {

            // Prepare discover message
            string ssdpDiscover = string.Join("\r\n", new string[]{
                                    "M-SEARCH * HTTP/1.1",
                                    "HOST: 239.255.255.250:1900",
                                    "\"",
                                    "MX: 1",
                                    $"ST: {filter}" });

            // Send discover message
            List<ssdpPacket> replies = ssdpRequest(ssdpDiscover, new IPEndPoint(IPAddress.Parse(broadcastIP), broadcastPort),
                                                   wait, repeatCount);

            //HashSet

        }

        public static void GetRouter()
        {

        }

        private static List<ssdpPacket> ssdpRequest(string packet, IPEndPoint remoteEP, int wait=10, int repeatCount=1, int maxReplies=999)
        {
            // Instatiante UdpClient and wrap in a using block
            using (UdpListener udpListener = new UdpListener(remoteEP.Port))
            {
                // Encode packet string into bytes
                byte[] packetBytes = Encoding.ASCII.GetBytes(packet);

                // Send the request packet repeatCount times
                udpListener.SendPacket(packetBytes, remoteEP, repeatCount);
                
                // Wait for the replies to reach us
                Thread.Sleep(wait * 1000);

                // Prepare reply list
                List<ssdpPacket> replyList = new List<ssdpPacket>();

                // Iterate through as many as the maxReplies
                for( int i=0; i < maxReplies; i++)
                {
                    // Try and grab the latest packet
                    byte[] replyPacket = udpListener.TryGetLatestPacket();

                    // If reply is null, break from the loop
                    if (replyPacket == null) { break; }

                    // Otherwise parse the packet, and add to the reply list
                    else { replyList.Add(ssdpPacket.Parse(replyPacket)); }
                }

                // Return the reply list
                return replyList;
            }
        }



    }
}
