// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace x2net
{
    /// <summary>
    /// Network address scope options (bit flags).
    /// </summary>
    public enum AddressOptions
    {
        None = 0x00,
        Public = 0x01,
        Private = 0x02
    }

    /// <summary>
    /// Socket address utility class.
    /// </summary>
    public static class AddressUtil
    {
        /// <summary>
        /// List the IP addresses of the specified family on this host.
        /// </summary>
        public static IPAddress[] GetIPAddresses(AddressFamily addressFamily, AddressOptions options)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            var result = new List<IPAddress>();
            foreach (IPAddress ipAddress in hostEntry.AddressList)
            {
                if (ipAddress.AddressFamily == addressFamily)
                {
                    bool isPrivate = IsPrivate(ipAddress);
                    if (isPrivate &&
                        (options & AddressOptions.Private) == AddressOptions.None
                        ||
                        !isPrivate &&
                        (options & AddressOptions.Public) == AddressOptions.None)
                    {
                        continue;
                    }

                    result.Add(ipAddress);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Determines whether the specified IP (v4/v6) address is a private
        /// (local) address or not.
        /// </summary>
        public static bool IsPrivate(IPAddress ipAddress)
        {
            bool result = false;
            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    byte[] bytes = ipAddress.GetAddressBytes();
                    // 10.x.x.x
                    if (bytes[0] == 10)
                    {
                        result = true;
                    }
                    // 172.16.x.x
                    else if (bytes[0] == 172 && bytes[1] == 16)
                    {
                        result = true;
                    }
                    // 192.168.x.x
                    else if (bytes[0] == 192 && bytes[1] == 168)
                    {
                        result = true;
                    }
                    // 169.254.x.x
                    else if (bytes[0] == 169 && bytes[1] == 254)
                    {
                        result = true;
                    }
                    break;
                case AddressFamily.InterNetworkV6:
                    string firstWord = ipAddress.ToString().Split(
                        new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (ipAddress.IsIPv6SiteLocal)
                    {
                        result = true;
                    }
                    // These days Unique Local Addresses (ULA) are used in place of Site Local. 
                    // ULA has two variants: 
                    // - fc00::/8 is not defined yet, but might be used in the future for internal-use addresses that are registered in a central place (ULA Central). 
                    // - fd00::/8 is in use and does not have to registered anywhere.
                    else if (firstWord.Substring(0, 2) == "fc" && firstWord.Length >= 4)
                    {
                        result = true;
                    }
                    else if (firstWord.Substring(0, 2) == "fd" && firstWord.Length >= 4)
                    {
                        result = true;
                    }
                    // Link local addresses (prefixed with fe80) are not routable
                    else if (firstWord == "fe80")
                    {
                        result = true;
                    }
                    // Discard Prefix
                    else if (firstWord == "100")
                    {
                        result = true;
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
