using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Client2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string serverIP = "127.0.0.1";
                int serverPort = 7171;

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                using Socket client = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await client.ConnectAsync(ipEndPoint);
                Console.WriteLine("Connected to server.");

                while (true)
                {
                    byte[] buffer = new byte[2048];
                    int received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (received == 0)
                    {
                        Console.WriteLine("Server closed the connection.");
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine("Received message from server: " + message);

                    string[] parts = message.Split(',');
                    if (ValidateSignature(parts))
                    {
                        Console.WriteLine("Signature validated");
                    }
                    else
                    {
                        Console.WriteLine("Signature not valid");
                    }
                }

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static bool ValidateSignature(string[] parts)
        {
            BigInteger n = BigInteger.Parse(parts[0]);
            BigInteger e = BigInteger.Parse(parts[1]);
            string[] xParts = parts[3].Split(' ');
            string[] sParts = parts[4].Split(' ');

            BigInteger[] xBigIntegers = new BigInteger[xParts.Length];
            BigInteger[] sBigIntegers = new BigInteger[sParts.Length];

            for (int i = 0; i < xParts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(xParts[i]))
                {
                    continue;
                }

                if (!BigInteger.TryParse(xParts[i], out xBigIntegers[i]))
                {
                    Console.WriteLine($"Error: Failed to parse '{xParts[i]}' into a BigInteger.");
                    return false;
                }
            }

            for (int i = 0; i < sParts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(sParts[i]))
                {
                    continue;
                }

                if (!BigInteger.TryParse(sParts[i], out sBigIntegers[i]))
                {
                    Console.WriteLine($"Error: Failed to parse '{sParts[i]}' into a BigInteger.");
                    return false;
                }
            }

            for (int i = 0; i < xBigIntegers.Length; i++)
            {
                BigInteger sPowE = BigInteger.ModPow(sBigIntegers[i], e, n);

                if (!sPowE.Equals(xBigIntegers[i]))
                {
                    return false;
                }
            }
            return true;
        }


    }
}
