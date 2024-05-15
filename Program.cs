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
                Console.WriteLine("Prisijungta prie serverio.");

                while (true)
                {

                    byte[] buffer = new byte[1024];
                    int received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, received);


                    string[] parts = message.Split(',');

                    Console.WriteLine($"n: {parts[0]}");
                    Console.WriteLine($"e: {parts[1]}");
                    Console.WriteLine($"x: {parts[2]}");
                    Console.WriteLine($"s: {parts[3]}");

                    if (ValidateSignature(parts))
                    {
                        Console.WriteLine("Parasas patvirtintas");
                        break;

                    }
                    else
                    {
                        Console.WriteLine("Parasas nepatvirtintas");
                        break;
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
            string[] xParts = parts[2].Split(' ');
            string[] sParts = parts[3].Split(' ');

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
