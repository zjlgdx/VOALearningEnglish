using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VOALearningEnglish.Common;
using VOALearningEnglish.Data;
using VOALearningEnglish.Models;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace VOALearningEnglish
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Article : Page
    {
        private MediaPlayer _mediaPlayer;

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        //private HttpClient httpClient;
        //private HttpResponseMessage response;

        private string articleFileName;
        // private string mp3downloadUriString;

        public Article()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //    httpClient = new HttpClient();

            // Add a user-agent header
            // var headers = httpClient.DefaultRequestHeaders;

            // HttpProductInfoHeaderValueCollection is a collection of 
            // HttpProductInfoHeaderValue items used for the user-agent header

            // headers.UserAgent.ParseAdd("ie");
            // headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var link = (string)e.NavigationParameter;

            //  response = new HttpResponseMessage();
            StatusBar statusBar = StatusBar.GetForCurrentView();
            Uri resourceUri;
            if (!Uri.TryCreate(link, UriKind.Absolute, out resourceUri))
            {
                statusBar.ProgressIndicator.Text = "Invalid URI, please re-enter a valid URI";
                return;
            }
            if (resourceUri.Scheme != "http" && resourceUri.Scheme != "https")
            {
                statusBar.ProgressIndicator.Text = "Only 'http' and 'https' schemes supported. Please re-enter URI";
                return;
            }

            //string responseText;
            statusBar.ProgressIndicator.Text = "Waiting for response ...";

            try
            {
                // response = await httpClient.GetAsync(resourceUri);

                // response.EnsureSuccessStatusCode();

                //responseText = await response.Content.ReadAsStringAsync();
                //statusPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //var contents = GetContentById(html: responseText);
                var filename = link.Split('/').Last();
                filename = Regex.Replace(filename, "\\.html$", ".json");
                articleFileName = filename;
                var obj = await GetArticleDataSource.GetBookAsync(link, filename);
                this.DefaultViewModel["Article"] = obj;
                // if (obj.DownloadIcon == "")
                // {
                DownloadAppBarButton.Icon = Helper.SetIcon(obj.DownloadIconLable);
                //}
                //this.DefaultViewModel["MP3"] = obj.Audio;
                article.NavigateToString(obj.Content);
                if (string.IsNullOrEmpty(obj.TranslationContent))
                {
                    translationarticle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    translationarticle.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    translationarticle.NavigateToString(obj.TranslationContent);
                }
                var message = new ValueSet();
                message.Add("bookTextKey", obj.Title);
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            catch (Exception ex)
            {
                // Need to convert int HResult to hex string
                statusBar.ProgressIndicator.Text = "Error = " + ex.HResult.ToString("X") +
                    "  Message: " + ex.Message;
                // responseText = "";
            }
            //statusBar.ProgressIndicator.Text = response.StatusCode + " " + response.ReasonPhrase;


        }



        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _mediaPlayer = BackgroundMediaPlayer.Current;
            _mediaPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            this.navigationHelper.OnNavigatedTo(e);
        }

        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            ValueSet valueSet = e.Data;

            foreach (string key in valueSet.Keys)
            {
                switch (key)
                {
                    case "updateplaybuttonstatus":
                        Debug.WriteLine("updateplaybuttonstatus:");
                        //
                        var _bookTextKey = e.Data[key].ToString();
                        //Debug.WriteLine(bookTextKey);
                        ArticleModel obj = this.DefaultViewModel["Article"] as ArticleModel;
                        var bookTextKey = obj.Title;
                        if (_bookTextKey == bookTextKey)
                        {
                            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                            {
                                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    PlayAppBarButton.Label = "pause";
                                    PlayAppBarButton.Icon = new SymbolIcon(Symbol.Pause);
                                });
                            }
                            else
                            {
                                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    PlayAppBarButton.Label = "play";
                                    PlayAppBarButton.Icon = new SymbolIcon(Symbol.Play);
                                });
                            }
                        }

                        break;
                }
            }
        }

        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PlayAppBarButton.Label = "pause";
                        PlayAppBarButton.Icon = new SymbolIcon(Symbol.Pause);
                    }
                        );

                    break;
                case MediaPlayerState.Paused:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PlayAppBarButton.Label = "play";
                        PlayAppBarButton.Icon = new SymbolIcon(Symbol.Play);
                    }
                    );

                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _mediaPlayer.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //bool failed = false;
            try
            {
                // await Helper.ShowSystemTrayAsync(text: "loading audio...");

                if (PlayAppBarButton.Tag != null)
                {
                    var audioFile = PlayAppBarButton.Tag.ToString();
                    if (!audioFile.StartsWith("http"))
                    {
                        audioFile = await StorageDataHelper.GetAudioFileFromMusicLibraryAsync(PlayAppBarButton.Tag.ToString());
                    }
                    ArticleModel obj = this.DefaultViewModel["Article"] as ArticleModel;
                    await ReadVoice(audioFile, obj.Title);
                }
            }
            catch
            {
                // failed = true;
            }

            //if (failed)
            //{
            //    await Helper.HideSystemTrayAsync();
            //}
        }

        private static async Task ReadVoice(string voiceFile, string title)
        {
            
            if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
            {
                BackgroundMediaPlayer.Current.Pause();
            }
            else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState ||
                     MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
            {
                if (!string.IsNullOrEmpty(voiceFile))
                {
                    string[] fileInfo = new[] { title, voiceFile };
                    var message = new ValueSet
                    {
                        {
                            "Play",
                            fileInfo
                        }
                    };
                    BackgroundMediaPlayer.SendMessageToBackground(message);
                }
                else
                {
                    await Helper.ShowSystemTrayAsync(text: "No file to play!");
                    await Task.Delay(2000).ContinueWith(async c => { await Helper.HideSystemTrayAsync(); });
                }
            }
        }

        private async void DownloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            PlayAppBarButton.IsEnabled = false;
            DownloadAppBarButton.IsEnabled = false;
            ArticleModel obj = this.DefaultViewModel["Article"] as ArticleModel;
            // obj.Title = "";
            //http://stream.51voa.com/201501/some-parts-of-asylum-seekers-story-on-abuse-in-north-korea-are-untrue.mp3
            var mp3url = obj.Mp3url;
            var mp3 = mp3url.Split('/').LastOrDefault();
            obj.Audio = mp3;
            if (DownloadAppBarButton.Label == "Download")
            {
                await Helper.ShowSystemTrayAsync("downloading audio....");
                
                obj.DownloadIconLable = "Delete";
                DownloadAppBarButton.Icon = Helper.SetIcon(obj.DownloadIconLable);// = Helper.SetIcon("Delete");
                //var articleFileName = link.Split('/').Last();
                //articleFileName = Regex.Replace(filename, "\\.html$", ".json");
                await StorageDataHelper.SaveJsonFileToDocumentsLibraryAsync<ArticleModel>(articleFileName, obj);
                var localMp3file = await StorageDataHelper.DownloadAudioFileToMusicLibraryAsync(mp3url, mp3);
                if (!string.IsNullOrEmpty(localMp3file))
                {
                    //obj.DownloadIcon = "Delete";
                    await Helper.HideSystemTrayAsync();

                }

            }
            else
            {
                await StorageDataHelper.DeleteAudioFileToMusicLibraryAsync(articleFileName,"Json");
                await StorageDataHelper.DeleteAudioFileToMusicLibraryAsync(mp3,"audio");
                obj.DownloadIconLable = "Download";
                DownloadAppBarButton.Icon = Helper.SetIcon(obj.DownloadIconLable);// = Helper.SetIcon("Delete");
            }

            PlayAppBarButton.IsEnabled = true;
            DownloadAppBarButton.IsEnabled = true;
        }


    }
}
