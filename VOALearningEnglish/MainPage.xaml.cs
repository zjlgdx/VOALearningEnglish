using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Xml.Linq;
using VOALearningEnglish.Common;
using VOALearningEnglish.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using Windows.UI.Popups;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace VOALearningEnglish
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly NavigationHelper navigationHelper;
       
        private const string VOA = "http://www.51voa.com/voa.xml";
        private const string SP = "http://www.51voa.com/sp.xml";
        private const string ST = "http://www.51voa.com/st.xml";
        private const string EN = "http://www.51voa.com/en.xml";
        // This is the feed address that will be parsed and displayed
        private String feedAddress;
        //http://www.51voa.com/st.xml

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SaveSelectionChannel(feedAddress);
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            LoadSelectionChannel();

            await StorageDataHelper.DeleteFilesFromMusicLibraryAsync("audio");
            await StorageDataHelper.DeleteFilesFromMusicLibraryAsync("Json");
            try
            {
                await LoadResource();
            }
            catch
            {

            }
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var failed = false;
            try
            {
                await LoadResource(true);
            }
            catch
            {
                failed = true;

            }
            if (failed)
            {
                MessageDialog md2 = new MessageDialog("网络异常", "网络连接");
                await md2.ShowAsync();
            }

        }

        private async System.Threading.Tasks.Task LoadResource(bool sync = false)
        {
            var lstData = new List<RssItem>();
            var fileName = feedAddress.Split('/').Last().Replace(".xml", ".json");
            if (sync)
            {
                ///-----------------
                ///
                feedStatus.Text = "Test if URI is valid";

                Uri resourceUri;
                if (!Uri.TryCreate(feedAddress.Trim(), UriKind.Absolute, out resourceUri))
                {
                    feedStatus.Text = "Invalid URI, please re-enter a valid URI";
                    return;
                }
                if (resourceUri.Scheme != "http" && resourceUri.Scheme != "https")
                {
                    feedStatus.Text = "Only 'http' and 'https' schemes supported. Please re-enter URI";
                    return;
                }

                string responseText;
                feedStatus.Text = "Waiting for response ...";

                //Uri dataUri = new Uri(url);

                try
                {
                    using (HttpClient client = new HttpClient())
                    using (var response = await client.GetAsync(resourceUri))
                    {
                        response.EnsureSuccessStatusCode();
                        responseText = await response.Content.ReadAsStringAsync();
                        feedStatus.Text = response.StatusCode + " " + response.ReasonPhrase;
                        statusPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    // Need to convert int HResult to hex string
                    feedStatus.Text = "Error = " + ex.HResult.ToString("X") +
                        "  Message: " + ex.Message;
                    responseText = "";
                    return;
                }


                XElement _xml = XElement.Parse(responseText);
                foreach (XElement value in _xml.Elements("channel").Elements("item"))
                {
                    RssItem _item = new RssItem();

                    _item.Title = value.Element("title").Value;

                    _item.Description = value.Element("description").Value;

                    _item.Link = value.Element("link").Value;

                    _item.PubDate = value.Element("pubDate").Value;

                    lstData.Add(_item);
                }

                await StorageDataHelper.SaveJsonFileToDocumentsLibraryAsync<List<RssItem>>(fileName, lstData);
            }
            else
            {

                lstData = await StorageDataHelper.GetJsonFromLocalAsync<List<RssItem>>(fileName);
            }

            // lstRSS is bound to the lstData: the final result of the RSS parsing
            lstRSS.DataContext = lstData;

        }

        private void lstRSS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if item clicked navigate to the webpage

            var selected = lstRSS.SelectedItem as RssItem;

            //WebView webBrowserTask = new WebView();
            //Uri targetUri = new Uri(selected.Link);

            ////webbrowser task launcher for Windows 8.1
            ////http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh701480.aspx
            //var success = await Windows.System.Launcher.LaunchUriAsync(targetUri);
            if (selected == null)
            {
                return;
            }
            var link = selected.Link;
            if (!Frame.Navigate(typeof(Article), link))
            {
                throw new Exception("Navigation failed.");
            }

        }

        private async void voa_Click(object sender, RoutedEventArgs e)
        {
            feedAddress = VOA;
            pagetitleName.Text = voa.Label;
            await LoadResource();
        }

        private async void sp_Click(object sender, RoutedEventArgs e)
        {
            feedAddress = SP;
            pagetitleName.Text = sp.Label;
            await LoadResource();
        }

        private async void st_Click(object sender, RoutedEventArgs e)
        {
            feedAddress = ST;
            pagetitleName.Text = st.Label;
            await LoadResource();
        }

        private async void en_Click(object sender, RoutedEventArgs e)
        {
            feedAddress = EN;
            pagetitleName.Text = en.Label;
            await LoadResource();
        }

        private void SaveSelectionChannel(string value)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var key = "DefaultChannel";
            if (settings.Values.ContainsKey(key))
            {
                // If the value has changed
                if (settings.Values[key] != value)
                {
                    // Store the new value
                    settings.Values[key] = value;
                    //valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Values.Add(key, value);
                // valueChanged = true;
            }
        }

        private void LoadSelectionChannel()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var key = "DefaultChannel";
            if (settings.Values.ContainsKey(key))
            {
                feedAddress = settings.Values[key].ToString();
            }
            else
            {
                feedAddress = SP;
                // pagetitleName.Text = sp.Label;
            }

            var channel = feedAddress.Substring(feedAddress.LastIndexOf("/") + 1);
            switch (channel)
            {
                case "voa.xml":
                    pagetitleName.Text = voa.Label;
                    break;
                case "sp.xml":
                    pagetitleName.Text = sp.Label;
                    break;
                case "st.xml":
                    pagetitleName.Text = st.Label;
                    break;
                case "en.xml":
                    pagetitleName.Text = en.Label;
                    break;

                default:
                    pagetitleName.Text = sp.Label;
                    break;
            }




        }

    }
}
