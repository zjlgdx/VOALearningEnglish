using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace VOALearningEnglish.Common
{
    class Helper
    {
        public static IconElement SetIcon(string downloadIconLable)
        {
            if (downloadIconLable == "Delete")
            {
                return new SymbolIcon(Symbol.Delete);
            }

            return new SymbolIcon(Symbol.Download);

        }
        public async static Task ShowSystemTrayAsync(string text = "")
        {
            StatusBar statusBar = StatusBar.GetForCurrentView();

            statusBar.ProgressIndicator.Text = text;
            await statusBar.ProgressIndicator.ShowAsync();
        }

        public async static Task HideSystemTrayAsync()
        {
            StatusBar statusBar = StatusBar.GetForCurrentView();
        
            await statusBar.ProgressIndicator.HideAsync();
        }

        public static string GetMp3(string input)
        {
            //string test = "<a id="mp3" href="http://stream.51voa.com/201501/some-parts-of-asylum-seekers-story-on-abuse-in-north-korea-are-untrue.mp3" title="鼠标右键点击下载"></a>";

            Regex reg = new Regex(@"(?is)<a(?:(?!href=).)*href=(['""]?)(?<url>[^""'\s>]*)\1[^>]*>(?<text>(?:(?!</a>).)*)</a>");
            Match match = reg.Match(input);
            return match.Result("${url}");
        }

        public static string GetTitle(string input)
        {
            //string test = "<div id="title">Defector's Story Raised Questions on Report of Abuse in North Korea</div>";

            Regex reg = new Regex(@"(?is)<div[^>]*>(?<text>(?:(?!</div>).)*)</div>");
            Match match = reg.Match(input);
            return match.Result("${text}");
        }

        /// <summary>
        /// 根据id提取任意嵌套标签
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetContentById(string html)
        {
            string[] idList = { "title", "mp3", "content" };
            string pattern = @"<([a-z]+)(?:(?!\bid\b)[^<>])*id=([""']?){0}\2[^>]*>(?><\1[^>]*>(?<o>)|</\1>(?<-o>)|(?:(?!</?\1).)*)*(?(o)(?!))</\1>";
            var result = new Dictionary<string, string>();
            foreach (string id in idList)
            {
                Match match = Regex.Match(html, string.Format(pattern, Regex.Escape(id)),
                               RegexOptions.Singleline | RegexOptions.IgnoreCase);
                //Console.WriteLine("--------begin {0}--------", id);
                if (match.Success)
                {
                    result.Add(id, match.Value);
                    //Console.WriteLine(match.Value);
                }


                // Console.WriteLine("--------end {0}--------", id);
            }

            return result;
        }
    }
}
