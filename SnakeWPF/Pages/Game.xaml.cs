using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public Game()
        {
            InitializeComponent();
        }
        public int StepCadr = 0;

        public void CreateUI()
        {
            // Выполняем вне потока
            Thread.Sleep(10);
            Dispatcher.Invoke(() =>
            {
                // Если кадр 0 то кадр 1
                if (StepCadr == 0) StepCadr = 1;
                // Если кадр 1 то 0
                else StepCadr = 0;
                // Чистим интерфейс
                canvas.Children.Clear();
                // Перебираем точки змеи


                for (int iPoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 1; iPoint >= 0; iPoint--)
                {
                    // Получаем точку
                    Snakes.Point SnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint];
                    // Смещение точек змеи 
                    if (iPoint != 0)
                    {
                        // Получаем следующую точку змеи

                        Snakes.Point NextSnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint - 1];
                        // Если точка находится по горизонтали
                        if (SnakePoint.X > NextSnakePoint.X || SnakePoint.X < NextSnakePoint.X)
                        {
                            // если точка чётная
                            if (iPoint % 2 == 0)
                            {

                                // если кадр чётный 
                                if (StepCadr % 2 == 0)
                                    // смещаем в одну сторону
                                    SnakePoint.Y -= 1;
                                else
                                    // смещаем в другую сторону
                                    SnakePoint.Y += 1;
                            }
                            else
                            {
                                // если кадр не чётный 
                                if (StepCadr % 2 == 0)

                                    // Смещаем в одну сторону)
                                    SnakePoint.Y += 1;

                                else

                                    // Смещаем в дугую сторону 
                                    SnakePoint.Y -= 1;

                            }

                        }

                        // Если точка находится по вертикали

                        else if (SnakePoint.Y > NextSnakePoint.Y || SnakePoint.Y < NextSnakePoint.Y)
                        {
                            // если точка чётная

                            if (iPoint % 2 == 0)
                            {

                                // если кадр чётный
                                if (StepCadr % 2 == 0)
                                    // смещаем в одну сторону 
                                    SnakePoint.X -= 1;

                                else
                                    // смещаем в другую сторону 
                                    SnakePoint.X += 1;

                            }
                            else
                            {
                                // если кадр не чётный 
                                if (StepCadr % 2 == 0)
                                    // Смещаем в одну сторону
                                    SnakePoint.X += 1;
                                else
                                    // Смещаем в дугую сторону
                                    SnakePoint.X -= 1;
                            }
                        }

                    }
                    // Цвет для точки 
                    Brush Color;
                    // Если первая точка
                    if (iPoint == 0)
                        // Тёмно зелёный
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 127, 14));

                    else
                        // Светло зелёный
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 198, 19));
                    // Рисуем точку

                    Ellipse ellipse = new Ellipse()
                    {
                        // Ширина
                        Width = 20,
                        // Высота
                        Height = 20,
                        // Отступы (-10 - центрирование по середине)

                        Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                        // Цвет точки
                        Fill = Color, // Обводка

                        Stroke = Brushes.Black

                    };
                    // Добавляем на canvas 
                    canvas.Children.Add(ellipse);
                }

                // Отрисовка яблока
                // Изображение яблока
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Image/Apple.jpg"));
                // Яблоко на UI;
                Ellipse points = new Ellipse()
                {

                    // Шрина
                    Width = 40,
                    // Высота
                    Height = 40,
                    // Отступы (-20 центрирование яблока)
                    Margin = new Thickness(

                            MainWindow.mainWindow.ViewModelGames.Points.X - 20,
                            MainWindow.mainWindow.ViewModelGames.Points.Y - 20, 0, 0),
                    //Заливка картинкой 
                    Fill = myBrush

                };

                // Добвляем на сцену
                canvas.Children.Add(points);

            });
        }
    }
}
