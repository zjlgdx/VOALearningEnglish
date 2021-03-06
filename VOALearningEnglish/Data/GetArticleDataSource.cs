﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VOALearningEnglish.Common;
using VOALearningEnglish.Models;
using Windows.Web.Http;

namespace VOALearningEnglish.Data
{
    public sealed class GetArticleDataSource
    {
        private const string JSON_URL = "http://m.hujiang.com/handler/appweb.json?v={0}&op=GetBook&Key=nce&bookKey={1}";

        private static GetArticleDataSource _sampleDataSource = new GetArticleDataSource();

        //private string BookKey { get; set; }
        //public string JsonLocalFileName { get; set; }

        private ArticleModel _bookDataSource;
        public ArticleModel BookDataSource
        {
            get
            {
                return this._bookDataSource;
            }
        }

        public static async Task<ArticleModel> GetBookAsync(string url, string fileName)
        {
            await _sampleDataSource.GetJsonDataSource(url, fileName);

            return _sampleDataSource.BookDataSource;
        }


        //private async Task GetSampleDataAsync(string url, string fileName)
        //{
        //   // this.BookKey = bookKey;
        //   // this.JsonLocalFileName = bookKey + "GetBook.json";
        //    GetJsonDataSource
        //}

        public async Task GetJsonDataSource(string url, string fileName)
        {
            var article = await StorageDataHelper.GetJsonFromLocalAsync<ArticleModel>(fileName);

            if (article == null)
            {
                Uri dataUri = new Uri(url);
                HttpClient client = new HttpClient();

                using (var response = await client.GetAsync(dataUri))
                {
                    response.EnsureSuccessStatusCode();
                    var responseText = await response.Content.ReadAsStringAsync();
                    var contents = Helper.GetContentById(html: responseText);
                    ArticleModel obj = new ArticleModel();
                    obj.Title = Helper.GetTitle(contents["title"]);
                    obj.Audio = Helper.GetMp3(contents["mp3"]);
                    obj.Mp3url = obj.Audio;

                    if (contents.ContainsKey("EnPage"))
                    {
                        var shuangyuUrl = Helper.GetEnPage(contents["EnPage"]);
                        if (!shuangyuUrl.StartsWith("http:"))
                        {
                            shuangyuUrl = url.Substring(0, url.LastIndexOf("/")) + "/" + shuangyuUrl;
                        }
                        obj.TranslationContent = await GetEnPageContent(shuangyuUrl);
                    }

                    obj.Content = contents["content"];

                    SaveImage(obj.Content);


                    obj.DownloadIconLable = "Download";
                    //  obj.DownloadIcon = Helper.SetIcon("Download");
                    //  await StorageDataHelper.writeTextToSDCard(RootFolder, JsonLocalFileName, json);
                    _bookDataSource = obj;
                    //return json;
                    return;
                }
            }

            //article.Audio = await StorageDataHelper.GetAudioFileFromMusicLibraryAsync(article.Audio);
            _bookDataSource = article;
        }


        /// <summary>
        /// javascript regex pattern
        /// var reg = /<img\b(?=(?:(?!name=).)*name=(['"]?)([^'"\s>]+)\1)(?:(?!src=).)*src=(['"]?)([^'"\s>]+)\3[^>]*>/i;  
        /// </summary>
        /// <param name="html"></param>
        private void SaveImage(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return;
            }
            var pattern = @"<(img)(?:(?!\bsrc\b)[^<>])*src=([""']?)(?<src>[^\s]+)\2[^>]*>";
            Match match = Regex.Match(html, pattern,
                   RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var src = match.Result("${src}");
                // todo: saving image 
            }

        }

        private async Task<string> GetEnPageContent(string url)
        {
            Uri dataUri = new Uri(url);
            HttpClient client = new HttpClient();

            using (var response = await client.GetAsync(dataUri))
            {
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();

                var contents = Helper.GetContentById(html: responseText);

                return contents["content"];


            }
        }
    }
}
