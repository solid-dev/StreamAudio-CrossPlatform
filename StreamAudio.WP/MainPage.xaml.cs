using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using StreamAudio.WP.Resources;
using StreamAudio.WP.Models;
using StreamAudio.WP.DataService;
using System.IO.IsolatedStorage;
using StreamAudio.WP.Helper;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Animation;
using StreamAudio.WP.Framework;

namespace StreamAudio.WP
{
    public partial class MainPage : PhoneApplicationPage
    {

        #region Properties for panel
        private double _dragDistanceToOpen = 75.0;
        private double _dragDistanceToClose = 305.0;
        private double _dragDistanceNegative = -75.0;
        private bool _isSettingsOpen = false;
        private FrameworkElement _feContainer;
        #endregion

        #region Properties 

        private List<AudioModel> mListAudio = new List<AudioModel>();
        private List<Menu> mListMenu = new List<Menu>();
        private ApiService mApiService = new ApiService();
        #endregion
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            _feContainer = this.Container as FrameworkElement;
            GetListSong();
            AddMenu();
            listMenu.ItemsSource = mListMenu;
        }
        #region Service

        public async void GetListSong()
        {
            progress.IsIndeterminate = true;
            mListAudio = await mApiService.GetListAudio();
            listsong.ItemsSource = mListAudio;
            progress.IsIndeterminate = false;
        }
        #endregion


        public void AddMenu()
        {
            mListMenu.Add(new Menu() { IconPath = "", Title = "Now playing" });
            mListMenu.Add(new Menu() { IconPath = "", Title = "Search" });
            mListMenu.Add(new Menu() { IconPath = "", Title = "Offline audio" });
            mListMenu.Add(new Menu() { IconPath = "", Title = "Logout" });

        }
        private void SelectionSong(object sender, SelectionChangedEventArgs e)
        {
            (App.Current as App).IndexSong = listsong.SelectedIndex;
            (App.Current as App).DescriptionAudio = mListAudio[listsong.SelectedIndex].Description;
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
            (App.Current as App).ListSong = mListAudio;
            IsolatedStorageSettings.ApplicationSettings["savelistsongtoplayer"] = (App.Current as App).ListSong;
            IsolatedStorageSettings.ApplicationSettings.Save();
            CacheHelper.createlstsong((App.Current as App).ListSong);
            NavigationService.Navigate(new Uri("/Pages/PlayerPage.xaml", UriKind.RelativeOrAbsolute));
        }


        #region Panel Open and Close 


        private void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            if (_isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
        private void CloseSettings()
        {
            var trans = _feContainer.GetHorizontalOffset().Transform;
            trans.Animate(trans.X, 0, TranslateTransform.XProperty, 300, 0, new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            });

            _isSettingsOpen = false;
        }

        private void OpenSettings()
        {
            var trans = _feContainer.GetHorizontalOffset().Transform;
            trans.Animate(trans.X, 380, TranslateTransform.XProperty, 300, 0, new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            });

            _isSettingsOpen = true;
        }
        private void GestureListener_OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange > 0 && !_isSettingsOpen)
            {
                double offset = _feContainer.GetHorizontalOffset().Value + e.HorizontalChange;
                if (offset > _dragDistanceToOpen)
                    this.OpenSettings();
                else
                    _feContainer.SetHorizontalOffset(offset);
            }

            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange < 0 && _isSettingsOpen)
            {
                double offsetContainer = _feContainer.GetHorizontalOffset().Value + e.HorizontalChange;
                if (offsetContainer < _dragDistanceToClose)
                    this.CloseSettings();
                else
                    _feContainer.SetHorizontalOffset(offsetContainer);
            }
        }

        private void GestureListener_OnDragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange > 0 && !_isSettingsOpen)
            {
                if (e.HorizontalChange < _dragDistanceToOpen)
                    this.ResetLayoutRoot();
                else
                    this.OpenSettings();
            }

            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange < 0 && _isSettingsOpen)
            {
                if (e.HorizontalChange > _dragDistanceNegative)
                    this.ResetLayoutRoot();
                else
                    this.CloseSettings();
            }
        }

        private void SettingsStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            ResetLayoutRoot();
        }

        private void ResetLayoutRoot()
        {
            if (!_isSettingsOpen)
                _feContainer.SetHorizontalOffset(0.0);
            else
                _feContainer.SetHorizontalOffset(380.0);
        }
        #endregion
    }
}