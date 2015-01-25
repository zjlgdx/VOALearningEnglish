using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Xml.Linq;
using VOALearningEnglish.Common;
using VOALearningEnglish.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace VOALearningEnglish
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //The Windows.Web.Http.HttpClient class provides the main class for 
        // sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
        private HttpClient httpClient;
        private HttpResponseMessage response;

        private const string sp = "http://www.51voa.com/sp.xml";
        private const string st = "http://www.51voa.com/st.xml";
        // This is the feed address that will be parsed and displayed
        private String feedAddress = sp;//"http://www.51voa.com/sp.xml";
        //http://www.51voa.com/st.xml

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            httpClient = new HttpClient();

            // Add a user-agent header
            var headers = httpClient.DefaultRequestHeaders;

            // HttpProductInfoHeaderValueCollection is a collection of 
            // HttpProductInfoHeaderValue items used for the user-agent header

            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            await StorageDataHelper.DeleteFilesFromMusicLibraryAsync("audio");
            await StorageDataHelper.DeleteFilesFromMusicLibraryAsync("Json");
            await LoadResource();
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadResource();

        }

        private async System.Threading.Tasks.Task LoadResource()
        {
            response = new HttpResponseMessage();

            // if 'feedAddress' value changed the new URI must be tested --------------------------------
            // if the new 'feedAddress' doesn't work, 'feedStatus' informs the user about the incorrect input.

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
            // ---------- end of test---------------------------------------------------------------------

            string responseText;
            feedStatus.Text = "Waiting for response ...";

            try
            {
                response = await httpClient.GetAsync(resourceUri);

                response.EnsureSuccessStatusCode();

                responseText = await response.Content.ReadAsStringAsync();
                statusPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                // Need to convert int HResult to hex string
                feedStatus.Text = "Error = " + ex.HResult.ToString("X") +
                    "  Message: " + ex.Message;
                responseText = "";
            }
            feedStatus.Text = response.StatusCode + " " + response.ReasonPhrase;

            // now 'responseText' contains the feed as a verified text.
            // next 'responseText' is converted as the rssItems class model definition to be displayed as a list

            List<RssItem> lstData = new List<RssItem>();
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

        

        private async void switch_Click(object sender, RoutedEventArgs e)
        {
            if (switchAudio.Label == "慢速")
            {
                switchAudio.Label = "标准";
                feedAddress = st;
                pagetitleName.Text = "VOA Standard English";
            }
            else
            {
                switchAudio.Label = "慢速";
                feedAddress = sp;
                pagetitleName.Text = "VOA Learning English";
            }

            await LoadResource();
        }
    }
}
