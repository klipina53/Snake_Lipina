using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для EndGame.xaml
    /// </summary>
    public partial class EndGame : Page
    {
        public EndGame()
        {

            InitializeComponent();
            // выыводим наименование игрока
            name.Content = MainWindow.mainWindow.ViewModelUserSettings.Name;
            // выводи место игрока среди всего списка
            top.Content = MainWindow.mainWindow.ViewModelGames.Top;
            // выводим сколько очков набрал игрок
            glasses.Content = $"{MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 3} glasses";
            // Закрываем соединение
            MainWindow.mainWindow.receivingUdpClient.Close();
            // отсанавливаем поток MainWindow.mainWindow.tRec.Abort(); // Обнуляем данные о змее
            MainWindow.mainWindow.ViewModelGames = null;

        }
        private void OpenHome(object sender, RoutedEventArgs e)
        {

            // открываем начальную страницу

            MainWindow.mainWindow.OpenPage(MainWindow.mainWindow.Home);

        }

    }
}
