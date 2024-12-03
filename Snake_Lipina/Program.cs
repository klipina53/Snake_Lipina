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
        public static void Receiver()
        {
            // UdpClient для чтения входящих данных
            UdpClient receivingUdpClient = new UdpClient(localPort);

            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Команды сервера: ");

                // Цикл для прослушки приходящих сообщений
                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду: " + returnData.ToString());

                    // Начало игры
                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] dataMessage = returnData.ToString().Split('|');

                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAdress}:{viewModelUserSettings.Port}");

                        //Добавляем данные в коллекции, чтобы отправлять пользователю
                        remoteIPAddress.Add(viewModelUserSettings);
                        // добавляем змею
                        viewModelUserSettings.IdSnake = AddSnake();
                        // добавляем змею и игрока
                        viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                    }
                    // если команда не является стартом
                    else
                    {

                        // управляем змеёй
                        string[] dataMessage = returnData.ToString().Split('|');

                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        int IdPlayer = -1;

                        IdPlayer = remoteIPAddress.FindIndex(x => x.IPAdress == viewModelUserSettings.IPAdress && x.Port == viewModelUserSettings.Port);

                        if (IdPlayer != -1)
                        {
                            if (dataMessage[0] == "Up" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;

                            else if (dataMessage[0] == "Down" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;

                            else if (dataMessage[0] == "Left" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;

                            else if (dataMessage[0] == "Right" && viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }

        }
        static void Main(string[] args) 
        {
        }
    }
}
