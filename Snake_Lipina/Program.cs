using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Snake_Lipina
{
    internal class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static readonly int localPort = 5001;
        public static int MaxSpeed = 15;
        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                    IPAddress.Parse(User.IPAdress),
                    int.Parse(User.Port));
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelGames.Find(x => x.IdSnake == User.IdSnake)));

                    sender.Send(bytes, bytes.Length, endPoint);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Отправил данные пользователю: {User.IPAdress}:{User.Port}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }
        }
        static void Main(string[] args) 
        {
        }
    }
}
