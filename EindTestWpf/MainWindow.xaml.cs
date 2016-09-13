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
using System.Reflection;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;

namespace EindTestWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //TODO: printpreview

        public MainWindow()
        {
            InitializeComponent();

            //ComboboxKleuren opvullen
            foreach (PropertyInfo info in typeof(Colors).GetProperties())
            {
                BrushConverter bc = new BrushConverter();
                SolidColorBrush deKleur =
                (SolidColorBrush)bc.ConvertFromString(info.Name);
                Kleur kleur = new Kleur();
                kleur.Borstel = deKleur;
                kleur.Naam = info.Name;
                kleur.Hex = deKleur.ToString();
                kleur.Rood = deKleur.Color.R;
                kleur.Groen = deKleur.Color.G;
                kleur.Blauw = deKleur.Color.B;
                ComboBoxKleuren.Items.Add(kleur);
            }
            ComboBoxKleuren.SelectedIndex = 0;

            //ComboBoxLettertypes alfabetisch
            ComboBoxLettertypes.Items.SortDescriptions.Add(
                new SortDescription("Source", ListSortDirection.Ascending));
            ComboBoxLettertypes.SelectedIndex = 0;

        }

        //nieuwe kaart
        private void Nieuw(ImageBrush borstel)
        {
            KaartStatus.Content = "nieuw";
            WensTextBox.Text = "vul hier uw wens in";
            KaartCanvas.Children.Clear();

            KaartCanvas.Background = borstel;
            SaveItem.IsEnabled = true;
            PrintPreviewItem.IsEnabled = true;
        }

        //kaarttype kiezen
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem kaartType = (MenuItem)sender;

            foreach (MenuItem item in Kaarten.Items)
            {
                if (item != kaartType)
                    item.IsChecked = false;
                else
                {
                    item.IsChecked = true;
            //hier problemen gehad om er een relatieve Uri van te maken....nog eens nakijken!!            
                    Uri bestandslocatie =
                        new Uri("pack://application:,,,/Images/" +
                            item.Header.ToString() + ".jpg", UriKind.Absolute);
                    BitmapImage afbeelding = new BitmapImage(bestandslocatie);
                    ImageBrush borstel = new ImageBrush(afbeelding);
                    Nieuw(borstel);
                }
            }
        }

        //fontsize groter en kleiner
        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            int fontSize = int.Parse(FontSizeLabel.Content.ToString());
            if (fontSize < 40)
                fontSize++;
            FontSizeLabel.Content = fontSize.ToString();
        }
        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            int fontSize = int.Parse(FontSizeLabel.Content.ToString());
            if (fontSize > 10)
                fontSize--;
            FontSizeLabel.Content = fontSize.ToString();
        }

        //bal draggen
        Ellipse dragBall = new Ellipse();

        private void DragBall_MouseMove(object sender, MouseEventArgs e)
        {
            dragBall = (Ellipse)sender;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject dragData = new DataObject(typeof(Brush), dragBall.Fill);
                DragDrop.DoDragDrop(dragBall, dragData, DragDropEffects.Move);
            }
        }

        //bal droppen
        private void Ball_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Brush)))
            {
                if (sender is Image && dragBall.Parent is Canvas)
                    KaartCanvas.Children.Remove(dragBall);
                else
                {
                    Ellipse dropBall = new Ellipse();
                    dropBall.Fill = dragBall.Fill;

                    dropBall.MouseMove += new MouseEventHandler(DragBall_MouseMove);

                    Point position = e.GetPosition(KaartCanvas);
                    Canvas.SetLeft(dropBall, position.X - 20);
                    Canvas.SetTop(dropBall, position.Y - 20);
                    if (dragBall.Parent is Canvas)
                        KaartCanvas.Children.Remove(dragBall);
                    KaartCanvas.Children.Add(dropBall);
                }
            }
        }

        //Commands:
        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Kerstkaart.IsChecked = true;
            Geboortekaart.IsChecked = false;
            ImageBrush brush = new ImageBrush(
                new BitmapImage(
                    new Uri("pack://application:,,,/Images/kerstkaart.jpg", UriKind.Absolute)));
            Nieuw(brush);
        }
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Wenskaarten(.txt) | *.txt";
                dlg.FileName = "Wenskaart1";
                dlg.DefaultExt = ".txt";

                if (dlg.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(dlg.FileName))
                    {
                        //pad en naam van de achtergrond                        
                        ImageSource backgroundSource =
                            ((ImageBrush)(KaartCanvas.Background)).ImageSource;
                        writer.WriteLine(backgroundSource.ToString());

                        //aantal ballen
                        writer.WriteLine(KaartCanvas.Children.Count.ToString());

                        //kleur en positie van elke bal
                        foreach (Ellipse ball in KaartCanvas.Children)
                        {
                            writer.WriteLine(ball.Fill.ToString());
                            writer.WriteLine(Canvas.GetLeft(ball).ToString());
                            writer.WriteLine(Canvas.GetTop(ball).ToString());
                        }

                        //wens, lettertype en lettergrootte
                        writer.WriteLine(WensTextBox.Text);
                        writer.WriteLine(WensTextBox.FontFamily.ToString());
                        writer.WriteLine(WensTextBox.FontSize.ToString());

                        KaartStatus.Content = dlg.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij het opslaan: " + ex.Message, "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Wenskaarten(.txt) | *.txt";
                dlg.FileName = "Wenskaart1";
                dlg.DefaultExt = ".txt";

                if (dlg.ShowDialog() == true)
                {
                    using (StreamReader reader = new StreamReader(dlg.FileName))
                    {
                        //juiste background zetten en eventuele oude ballen verwijderen
                        Uri backgroundUri =
                            new Uri(reader.ReadLine(), UriKind.Absolute);
                        Nieuw(new ImageBrush(new BitmapImage(backgroundUri)));

                        //juist kaarttype checken
                        foreach (MenuItem item in Kaarten.Items)
                        {
                            if (backgroundUri.ToString().Contains(item.Header.ToString()))
                                item.IsChecked = true;
                            else
                                item.IsChecked = false;
                        }

                        //aantal ballen
                        int ballCount = int.Parse(reader.ReadLine());

                        //ballen terugzetten
                        for (int i = 1; i <= ballCount; i++)
                        {
                            Ellipse ball = new Ellipse();
                            BrushConverter bc = new BrushConverter();
                            SolidColorBrush brush =
                                (SolidColorBrush)bc.ConvertFromString(reader.ReadLine());
                            ball.Fill = brush;
                            Canvas.SetLeft(ball, double.Parse(reader.ReadLine()));
                            Canvas.SetTop(ball, double.Parse(reader.ReadLine()));
                            KaartCanvas.Children.Add(ball);
                            ball.MouseMove += new MouseEventHandler(DragBall_MouseMove);
                        }

                        //wens
                        WensTextBox.Text = reader.ReadLine();
                        ComboBoxLettertypes.SelectedValue = new FontFamily(reader.ReadLine());
                        FontSizeLabel.Content = int.Parse(reader.ReadLine());

                        //indien deze 2 volgende lijnen niet toegevoegd, wordt font en size niet goed ingeladen
                        //afgetoetst bij Steven, maar vindt dit goed
                        WensTextBox.FontFamily = (FontFamily)ComboBoxLettertypes.SelectedValue;
                        WensTextBox.FontSize = (int)FontSizeLabel.Content;

                        KaartStatus.Content = dlg.FileName;
                        SaveItem.IsEnabled = true;
                        PrintPreviewItem.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Probleem bij het openen: " + ex.Message, "Fout",
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Sorry, printpreview currently not available!","Error",MessageBoxButton.OK);
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Wilt u het programma sluiten?", "Afsluiten",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

    }
}
