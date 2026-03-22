using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common networking operations
    /// </summary>
    public static class NetworkUtils
    {
        /// <summary>
        /// Checks if the device has internet connectivity
        /// </summary>
        public static bool HasInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the local IP address of the device
        /// </summary>
        public static string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        /// <summary>
        /// Pings a host and returns the response time in milliseconds
        /// </summary>
        public static int Ping(string host, int timeout = 5000)
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = ping.Send(host, timeout);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return (int)reply.RoundtripTime;
                    }
                }
            }
            catch
            {
                // Ignore exceptions and return -1 to indicate failure
            }
            return -1;
        }

        /// <summary>
        /// Downloads a string from a URL
        /// </summary>
        public static string DownloadString(string url)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"NetworkUtils: Failed to download string from {url}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Uploads a string to a URL
        /// </summary>
        public static string UploadString(string url, string data)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Headers[System.Net.HttpRequestHeader.ContentType] = "text/plain";
                    return client.UploadString(url, data);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"NetworkUtils: Failed to upload string to {url}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Downloads a file from a URL to a local path
        /// </summary>
        public static bool DownloadFile(string url, string destinationPath)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.DownloadFile(url, destinationPath);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"NetworkUtils: Failed to download file from {url}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the current download progress for a web client (0-1)
        /// </summary>
        public static float GetDownloadProgress(System.Net.WebClient client)
        {
            if (client == null)
                return 0f;
                
            // Note: WebClient doesn't expose progress directly in .NET Framework
            // This would require using HttpClient or implementing custom progress tracking
            return 0f;
        }

        /// <summary>
        /// Checks if a port is open on a remote host
        /// </summary>
        public static bool IsPortOpen(string host, int port, int timeout = 3000)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
                    if (!success)
                        return false;
                        
                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}