using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Xna.Framework;
using StreamAudio.WP.Models;

namespace StreamAudio.WP.Pages
{

    public partial class PlayerPage : PhoneApplicationPage
    {
        DispatcherTimer dt = new DispatcherTimer();
        List<AudioModel> _listsongplayer = new List<AudioModel>();
        bool WaitingForExternalPower;
        bool WaitingForExternalPowerDueToBatterySaverMode;
        bool WaitingForNonVoiceBlockingNetwork;
        bool WaitingForWiFi;
        int times = 0;
        IEnumerable<BackgroundTransferRequest> transferRequests;

        public PlayerPage()
        {
            InitializeComponent();     
            rotateY2.Begin();
            if (IsolatedStorageSettings.ApplicationSettings.Contains("savelistsongtoplayer"))
            {
                listsong.ItemsSource = (List<AudioModel>)IsolatedStorageSettings.ApplicationSettings["savelistsongtoplayer"];
            }
            savecurrentnumber(Getcurrenttrack());
            listsong.SelectedIndex = (App.Current as App).IndexSong;
            descriptionSong.Text = (App.Current as App).DescriptionAudio;
            try
            {
                BackgroundAudioPlayer.Instance.Close();
                BackgroundAudioPlayer.Instance.Play();
            }
            catch(Exception ex)
            {
                BackgroundAudioPlayer.Instance.Close();
                BackgroundAudioPlayer.Instance.Play();
            }

            dt.Interval = TimeSpan.FromMilliseconds(33);
            dt.Tick += delegate { try { FrameworkDispatcher.Update(); } catch { } };
            dt.Tick += OnTimerTick;
            BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);
        }

        private void savecurrentnumber(int number)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists("_currentnumber"))
                    isoStore.DeleteFile("_currentnumber");

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("_currentnumber", FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.Write(number.ToString());
                    }
                }
            }
        }
        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    {
                        (App.Current as App).IsPlaying = false;
                        IsolatedStorageSettings.ApplicationSettings["checknowplaying"] = (App.Current as App).IsPlaying;
                        IsolatedStorageSettings.ApplicationSettings.Save();

                        rotateY2.Resume();
                        tenbaihatplayer.Text = BackgroundAudioPlayer.Instance.Track.Title;
                        tencasiplayer.Text = BackgroundAudioPlayer.Instance.Track.Artist;
                        Uri uri = new Uri("/images/transport.pause.png", UriKind.Relative);
                        BitmapImage imgSource = new BitmapImage(uri);
                        Image image = new Image();
                        image.Source = imgSource;
                        btnplay.Content = image;
                        slider.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds;
                    };
                    break;
                case PlayState.Paused:
                case PlayState.Stopped:
                    {
                        rotateY2.Pause();
                        Uri uri = new Uri("/images/transport.play.png", UriKind.Relative);
                        BitmapImage imgSource = new BitmapImage(uri);
                        Image image = new Image();
                        image.Source = imgSource;
                        btnplay.Content = image;

                    };
                    break;
            }
        }
        private int Getcurrenttrack()
        {
            int currenttrack = 0;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists("_currenttrack"))
                {
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("_currenttrack", FileMode.Open, isoStore))
                    {
                        using (StreamReader reader = new StreamReader(isoStream))
                        {
                            currenttrack = Int32.Parse(reader.ReadToEnd());
                        }
                    }
                }
            }
            return currenttrack;
        }
        private void OnTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
                {
                    slider.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds;
                    time.Text = String.Format("{00}:{1:D2}",
                            (int)BackgroundAudioPlayer.Instance.Position.TotalMinutes, BackgroundAudioPlayer.Instance.Position.Seconds) + " / ";
                    end.Text = String.Format("{00}:{1:D2}",
                    (int)BackgroundAudioPlayer.Instance.Track.Duration.TotalMinutes, BackgroundAudioPlayer.Instance.Track.Duration.Seconds);
                    if (BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds != 0)
                    {
                        slider.Value = BackgroundAudioPlayer.Instance.Position.TotalSeconds;
                    }
                    times = (int)BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds;
                }
            }
            catch
            {

            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            dt.Start();
            if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
            {
                tenbaihatplayer.Text = BackgroundAudioPlayer.Instance.Track.Title;
                tencasiplayer.Text = BackgroundAudioPlayer.Instance.Track.Artist;
                rotateY2.Begin();
                Uri uri = new Uri("/images/transport.pause.png", UriKind.Relative);
                BitmapImage imgSource = new BitmapImage(uri);
                Image image = new Image();
                image.Source = imgSource;
                btnplay.Content = image;
               
            }
            else
            {
                rotateY2.Pause();
                Uri uri = new Uri("/images/transport.play.png", UriKind.Relative);
                BitmapImage imgSource = new BitmapImage(uri);
                Image image = new Image();
                image.Source = imgSource;
                btnplay.Content = image;             
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
            {
                BackgroundAudioPlayer.Instance.Pause();
            }
            else if(PlayState.Paused == BackgroundAudioPlayer.Instance.PlayerState)
            {
                BackgroundAudioPlayer.Instance.Play();
            }
        }
        private void slider_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                TimeSpan ts = new TimeSpan(0, 0, (int)slider.Value);
                BackgroundAudioPlayer.Instance.Position = ts;
            }

        }
        private void SelectionSong(object sender, SelectionChangedEventArgs e)
        {
            AudioModel item = e.AddedItems[0] as AudioModel;
            descriptionSong.Text = item.Description;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists("_currentnumber"))
                    isoStore.DeleteFile("_currentnumber");

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("_currentnumber", FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.Write(listsong.SelectedIndex.ToString());
                    }
                }
            }
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                if (BackgroundAudioPlayer.Instance.Track.Source.ToString() != item.PathMp3)
                {
                    BackgroundAudioPlayer.Instance.Close();
                    BackgroundAudioPlayer.Instance.Play();
                }
            }
            else
            {
                BackgroundAudioPlayer.Instance.Close();
                BackgroundAudioPlayer.Instance.Play();
            }
        }
        private void NextSong(object sender, RoutedEventArgs e)
        {
            listsong.SelectedIndex = Getcurrenttrack();
            BackgroundAudioPlayer.Instance.SkipNext();

        }
        private void BackSong(object sender, RoutedEventArgs e)
        {

            listsong.SelectedIndex = Getcurrenttrack();
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }
        private void BackPage(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }       
        #region Dowloading
        private void DownLoadSong(object sender, RoutedEventArgs e)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
            if (BackgroundTransferService.Requests.Count() >= 25)
            {
                MessageBox.Show("Have one song is downloading, please waiting this song completed");
                return;
            }
            string linkdownload = BackgroundAudioPlayer.Instance.Track.Source.ToString();
            Uri transferUri = new Uri(Uri.EscapeUriString(linkdownload), UriKind.RelativeOrAbsolute);
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);
            transferRequest.Method = "GET";
            string downloadFile = BackgroundAudioPlayer.Instance.Track.Title + "Test";
            Uri downloadUri = new Uri("shared/transfers/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            transferRequest.Tag = downloadFile;
            try
            {
                BackgroundTransferService.Add(transferRequest);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Have one song is downloading, please waiting this song completed");
                return;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to add background transfer request.");
                return;
            }
            WaitingForExternalPower = false;
            WaitingForExternalPowerDueToBatterySaverMode = false;
            WaitingForNonVoiceBlockingNetwork = false;
            WaitingForWiFi = false;
            InitialTransferStatusCheck();
            var toast = new ToastPrompt
            {
                Title = "Stream Audio",
                Message = "Starting dowload",
                ImageSource = new BitmapImage(new Uri("/images/header1.png", UriKind.RelativeOrAbsolute))
            };
            toast.Show();
        }
        private void UpdateRequestsList()
        {

            if (transferRequests != null)
            {
                foreach (var request in transferRequests)
                {
                    request.Dispose();
                }
            }
            transferRequests = BackgroundTransferService.Requests;
        }
        private void UpdateUI()
        {
            // Update the list of transfer requests
            UpdateRequestsList();

            TransferListBox.ItemsSource = transferRequests;

            if (TransferListBox.Items.Count > 0)
            {
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }

        }
        private void InitialTransferStatusCheck()
        {
            UpdateRequestsList();

            foreach (var transfer in transferRequests)
            {
                transfer.TransferStatusChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferStatusChanged);
                transfer.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferProgressChanged);
                ProcessTransfer(transfer);
            }

            if (WaitingForExternalPower)
            {
                MessageBox.Show("You have one or more file transfers waiting for external power. Connect your device to external power to continue transferring.");
            }
            if (WaitingForExternalPowerDueToBatterySaverMode)
            {
                MessageBox.Show("You have one or more file transfers waiting for external power. Connect your device to external power or disable Battery Saver Mode to continue transferring.");
            }
            if (WaitingForNonVoiceBlockingNetwork)
            {
                MessageBox.Show("You have one or more file transfers waiting for a network that supports simultaneous voice and data.");
            }
            if (WaitingForWiFi)
            {
                MessageBox.Show("You have one or more file transfers waiting for a WiFi connection. Connect your device to a WiFi network to continue transferring.");
            }
        }


        private void ProcessTransfer(BackgroundTransferRequest transfer)
        {
            switch (transfer.TransferStatus)
            {
                case TransferStatus.Completed:
                    (App.Current as App).IsDownLoading = false;
                    (App.Current as App).ListDownload.Clear();
                    progressdownload.Visibility = System.Windows.Visibility.Collapsed;
                    cent.Visibility = System.Windows.Visibility.Collapsed;
                    if (transfer.StatusCode == 200 || transfer.StatusCode == 206)
                    {
                        RemoveTransferRequest(transfer.RequestId);
                        try
                        {
                            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                string filename = transfer.Tag;
                                if (isoStore.FileExists(filename))
                                {
                                    isoStore.DeleteFile(filename);
                                }
                                SongMetadata metaData = new SongMetadata();
                                metaData.Name = BackgroundAudioPlayer.Instance.Track.Title;
                                metaData.ArtistName = BackgroundAudioPlayer.Instance.Track.Artist;
                                MediaLibrary ml = new MediaLibrary();
                                int time = Convert.ToInt32(times);
                                TimeSpan timespan = new TimeSpan(0, 0, time);
                                metaData.Duration = timespan;
                                Uri songUri = new Uri(transfer.DownloadLocation.OriginalString, UriKind.RelativeOrAbsolute);
                                Song song = MediaLibraryExtensions.SaveSong(ml, songUri, metaData, SaveSongOperation.MoveToLibrary);
                            }
                        }
                        catch
                        {

                        }
                        var toast = new ToastPrompt
                        {
                            Title = "Stream Audio",
                            Message = "Download completed",
                            ImageSource = new BitmapImage(new Uri("/images/header1.png", UriKind.RelativeOrAbsolute))
                        };
                        toast.Show();
                    }
                    else
                    {
                        RemoveTransferRequest(transfer.RequestId);
                        if (transfer.TransferError != null)
                        {
                           
                        }
                    }
                    break;
                case TransferStatus.WaitingForExternalPower:
                    WaitingForExternalPower = true;
                    break;

                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    WaitingForExternalPowerDueToBatterySaverMode = true;
                    break;

                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                    WaitingForNonVoiceBlockingNetwork = true;
                    break;

                case TransferStatus.WaitingForWiFi:
                    WaitingForWiFi = true;
                    break;
            }
        }
        void transfer_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            ProcessTransfer(e.Request);
            UpdateUI();

        }
        void transfer_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            UpdateUI();
            progressdownload.Visibility = System.Windows.Visibility.Visible;
            cent.Visibility = System.Windows.Visibility.Visible;
            progressdownload.Maximum = e.Request.TotalBytesToReceive;
            progressdownload.Value = e.Request.BytesReceived;
            cent.Text = "Dowloading: " + String.Format("{0:0.0}", (float)e.Request.BytesReceived / 1024000) + " MB /" + String.Format("{0:0.0}", (float)e.Request.TotalBytesToReceive / 1024000) + " MB ";

        }
        private void RemoveTransferRequest(string transferID)
        {
            BackgroundTransferRequest transferToRemove = BackgroundTransferService.Find(transferID);
            try
            {
                BackgroundTransferService.Remove(transferToRemove);
            }
            catch (Exception e)
            {
                
            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {

            string transferID = ((Button)sender).Tag as string;
            RemoveTransferRequest(transferID);
            UpdateUI();
        }
        private void CancelAllButton_Click(object sender, EventArgs e)
        {
            // Loop through the list of transfer requests
            foreach (var transfer in BackgroundTransferService.Requests)
            {
                RemoveTransferRequest(transfer.RequestId);
            }
            // Refresh the list of file transfer requests.
            UpdateUI();
        }
        #endregion
    }
}