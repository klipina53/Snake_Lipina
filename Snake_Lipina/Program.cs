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
using System.Threading;
using System.IO;

namespace Snake_Lipina
{
    public class Program
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
        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();

            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                Points = new List<Snakes.Point>()
                {
                    new Snakes.Point() { X = 30, Y = 10 },
                    new Snakes.Point() { X = 20, Y = 10 },
                    new Snakes.Point() { X = 10, Y = 10 },
                },

                direction = Snakes.Direction.Start
            };

            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));

            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }
        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);

                List<ViewModelGames> RemoteSnakes = viewModelGames.FindAll(x => x.SnakesPlayers.GameOver);

                // Удаление змеи
                if (RemoteSnakes.Count > 0)
                {
                    foreach (ViewModelGames DeadSnake in RemoteSnakes)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Отключил пользователя: {remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).IPAdress}" +
                            $":{remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake)}");

                        remoteIPAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);
                    }
                    viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver);
                }

                // Перебираем игроков
                foreach (ViewModelUserSettings User in remoteIPAddress)
                {
                    Snakes snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;
                    // Отображение точек змеи
                    for (int i = snake.Points.Count - 1; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            snake.Points[i] = snake.Points[i - 1];
                        }
                        else
                        {
                            int Speed = 10 + (int)Math.Round(snake.Points.Count / 20f);

                            if (Speed > MaxSpeed) Speed = MaxSpeed;

                            if (snake.direction == Snakes.Direction.Right)
                            {
                                snake.Points[i] = new Snakes.Point() { X = snake.Points[i].X + Speed, Y = snake.Points[i].Y };
                            }
                            else if (snake.direction == Snakes.Direction.Down)
                            {
                                snake.Points[i] = new Snakes.Point() { X = snake.Points[i].X, Y = snake.Points[i].Y + Speed };
                            }
                            else if (snake.direction == Snakes.Direction.Up)
                            {
                                snake.Points[i] = new Snakes.Point() { X = snake.Points[i].X, Y = snake.Points[i].Y + Speed };
                            }
                            else if (snake.direction == Snakes.Direction.Left)
                            {
                                snake.Points[i] = new Snakes.Point() { X = snake.Points[i].X + Speed, Y = snake.Points[i].Y };
                            }
                        }
                    }

                    //проверяем выход за карту
                    if (snake.Points[0].X <= 0 || snake.Points[0].X >= 793)
                    {
                        snake.GameOver = true;
                    }
                    else if (snake.Points[0].Y <= 0 || snake.Points[0].Y >= 420)
                    {
                        snake.GameOver = true;
                    }


                    // проверяем столкновение сами с собой
                    if (snake.direction != Snakes.Direction.Start)
                    {
                        for (int i = 1; i < snake.Points.Count; i++)
                        {
                            if (snake.Points[0].X >= snake.Points[i].X - 1 && snake.Points[0].X <= snake.Points[i].X + 1)
                            {
                                if (snake.Points[0].Y >= snake.Points[i].Y - 1 && snake.Points[0].Y <= snake.Points[i].Y + 1)
                                {
                                    snake.GameOver = true;

                                    break;
                                }
                            }
                        }
                    }

                    // Проверяем косается ли первая точка змеи яблока
                    if (snake.Points[0].X >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 &&
                        snake.Points[0].X <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15)
                    {
                        if (snake.Points[0].Y >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 &&
                        snake.Points[0].Y <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                        {
                            // создаем новое яблоко
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(
                                new Random().Next(10, 783),
                                new Random().Next(10, 410));

                            snake.Points.Add(new Snakes.Point()
                            {
                                X = snake.Points[snake.Points.Count - 1].X,
                                Y = snake.Points[snake.Points.Count - 1].Y
                            });

                            LoadLeaders();

                            Leaders.Add(new Leaders()
                            {
                                Name = User.Name,
                                Points = snake.Points.Count - 3
                            });

                            Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();

                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top =
                                Leaders.FindIndex(x => x.Points == snake.Points.Count - 3 && x.Name == User.Name) + 1;

                        }
                    }
                    // Если игра закончена
                    if (snake.GameOver)
                    {
                        LoadLeaders();

                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = snake.Points.Count - 3
                        });
                    }
                }

                Send();
            }
        }
        public static void SaveLeaders()
        {
            string json = JsonConvert.SerializeObject(Leaders);

            StreamWriter SW = new StreamWriter("./leader.txt");

            SW.WriteLine(json);

            SW.Close();
        }
        public static void LoadLeaders()
        {
            if (File.Exists("./leaders.txt"))
            {
                StreamReader SR = new StreamReader("./leaders.txt");

                string json = SR.ReadLine();

                SR.Close();

                if (!string.IsNullOrEmpty(json))
                {
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                }
                else
                {
                    Leaders = new List<Leaders>();
                }
            }
            else
            {
                Leaders = new List<Leaders>();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                // создаем поток для прослушивания сообщений от клиетов
                Thread tRec = new Thread(new ThreadStart(Receiver));

                tRec.Start();

                Thread tTime = new Thread(Timer);

                tTime.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n" + ex.Message);
            }

        }
    }
    
}
