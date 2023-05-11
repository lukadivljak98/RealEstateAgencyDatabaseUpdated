using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Threading;
using NAudio.Wave;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        private string songsPath = "C:\\Users\\Luka\\Desktop\\Projekat_A-Audio_Player\\AudioPlayer\\AudioPlayer\\songs\\";
        public Song currentSong { get; set; } = new Song();
        MediaPlayer mediaPlayer = new MediaPlayer();

        private static int playPauseCounter = 0;
        private static int muteCounter = 0;
        private static double lastVolumeValue;
        private readonly DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            Songs.initializeSongs();
            DataContext = this;
            listView.ItemsSource = Songs.songs;
            if (Songs.songs.Count != 0) 
            {
                 currentSong = Songs.songs[0];
                 mediaPlayer.Open(new Uri(currentSong.path));
                 songNameTb.Text = currentSong.name;
            }
            lastVolumeValue = volumeSlider.Value;

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                InfoLabel.Content = mediaPlayer.Position.TotalMinutes.ToString("00") + ":" + mediaPlayer.Position.Seconds.ToString("00");
            }
            else
            {
                InfoLabel.Content = "00:00";
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void addSongButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            bool? response = openFileDialog.ShowDialog();
            if(response == true)
            {
                string songPath = openFileDialog.FileName;
                string songName = System.IO.Path.GetFileName(songPath);
                Mp3FileReader reader = new Mp3FileReader(songPath);
                TimeSpan duration = reader.TotalTime;
                Song newSong = new Song(songPath, songName, duration.ToString(@"mm\:ss"));
                if (Songs.songs.Count == 0)
                {
                    currentSong = newSong;
                    mediaPlayer.Open(new Uri(currentSong.path));
                    songNameTb.Text = currentSong.name;
                }
                Songs.songs.Add(newSong);
                System.IO.File.Copy(openFileDialog.FileName, songsPath + System.IO.Path.GetFileName(openFileDialog.FileName), true);
            }
        }

        private void playPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if(playPauseCounter%2 == 0) 
            {
                mediaPlayer.Play();
                packIckon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
            else
            {
                mediaPlayer.Pause();
                packIckon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            }
            playPauseCounter++;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = Songs.songs.IndexOf(currentSong);
            int listSize = Songs.songs.Count;
            if (Songs.songs.Count != 0)
            {
                if (currentIndex != listSize - 1)
                    currentSong = Songs.songs[currentIndex + 1];
                else
                    currentSong = Songs.songs[0];
            }
            mediaPlayer.Open(new Uri(currentSong.path));
            mediaPlayer.Play();
            songNameTb.Text = currentSong.name;
            if (playPauseCounter % 2 == 0)
            {  
                packIckon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                playPauseCounter++;
            }
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = Songs.songs.IndexOf(currentSong);
            int listSize = Songs.songs.Count;
            if (Songs.songs.Count != 0)
            {
                if (currentIndex != 0)
                    currentSong = Songs.songs[currentIndex - 1];
                else
                    currentSong = Songs.songs[listSize - 1];
            }
            mediaPlayer.Open(new Uri(currentSong.path));
            mediaPlayer.Play();
            songNameTb.Text = currentSong.name;
            if (playPauseCounter % 2 == 0)
            {
                packIckon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                playPauseCounter++;
            }
        }
        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            lastVolumeValue = mediaPlayer.Volume*10;
            mediaPlayer.Volume = volumeSlider.Value/10;
        }

        private void muteButton_Click(object sender, RoutedEventArgs e)
        {
            if (muteCounter % 2 == 0)
            {
                volumeSlider.Value = volumeSlider.Minimum;
                mutePackIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
            }
            else
            {
                volumeSlider.Value = lastVolumeValue;
                mutePackIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
            }
            muteCounter++;
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            currentSong = (Song) listView.SelectedItem;
            mediaPlayer.Open(new Uri(currentSong.path));
            mediaPlayer.Play();
            if (playPauseCounter%2==0) 
            {
                packIckon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                playPauseCounter++;
            }
            songNameTb.Text = currentSong.name;
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            
            Button btn = (Button)sender;
            int index = int.Parse(btn.Tag.ToString());
            Song deleteSong = Songs.songs[index-1];
            if (deleteSong.name.Equals(currentSong.name))
            {
                mediaPlayer.Stop();
                packIckon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                playPauseCounter++;
                if(index-1 == 0 && Songs.songs.Count != 1)
                    currentSong = Songs.songs[index];
                else if(index == Songs.songs.Count && Songs.songs.Count != 1)
                    currentSong = Songs.songs[index-2];
                songNameTb.Text = currentSong.name;
                mediaPlayer.Open(new Uri(currentSong.path));
            }
            Songs.songs.RemoveAt(index - 1);
            Songs.removeSongFromDirectory(deleteSong);
            SetOrder(Songs.songs);
            listView.ItemsSource = null;
            listView.ItemsSource = Songs.songs;
            if (Songs.songs.Count == 0)
            {
                mediaPlayer.Close();
                songNameTb.Text = "";
            }
        }

        public void SetOrder(ObservableCollection<Song> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                Song s = (Song)data[i];
                s.id = i+1;
            }
        }
    }
    public class Song {
        public int id { get; set; }
        private static int counter = 1;
        public string path { get; set; }
        public string name { get; set; }

        public string duration { get; set; }

        public Song(string path, string name, string duration)
        {
            this.id = Songs.songs.Count+1;
            this.path = path;
            this.name = name;
            this.duration = duration;
        }

        public Song() { }

        public override string ToString()
        {
            return name;
        }
    }

    public class Songs
    {
        public static ObservableCollection<Song> songs { set; get; } = new ObservableCollection<Song>();
        public static DirectoryInfo directoryInfo = new DirectoryInfo("C:\\Users\\Luka\\Desktop\\Projekat_A-Audio_Player\\AudioPlayer\\AudioPlayer\\songs\\");

        public static void initializeSongs()
        {
            FileInfo[] files = directoryInfo.GetFiles();    
            foreach(FileInfo file in files)
            {
                string songPath = file.FullName;
                string songName = file.Name;
                Mp3FileReader reader = new Mp3FileReader(songPath);
                TimeSpan duration = reader.TotalTime;
                Song newSong = new Song(songPath, songName, duration.ToString(@"mm\:ss"));
                Songs.songs.Add(newSong);
            }          
        }
        public static void removeSongFromDirectory(Song song)
        {
            if (song == null) return;
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name.Equals(song.name))
                    File.Delete(file.FullName);
            }
        }
    }

}
