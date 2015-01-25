using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VOALearningEnglish.Models;
using Windows.Storage;
using Windows.Web.Http;

namespace VOALearningEnglish.Common
{
    class StorageDataHelper
    {
        public const string BASE_FOLDER_NAME = "VOALearningEnglish";
        public static async Task<T> GetJsonFromLocalAsync<T>(string filename) where T :class
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;// Windows.Storage.KnownFolders.MusicLibrary;

                StorageFolder folder = await localFolder.CreateFolderAsync(BASE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
                StorageFolder subfolder = await folder.CreateFolderAsync("Json", CreationCollisionOption.OpenIfExists);

                var file = await subfolder.GetFileAsync(filename);
                string result = await FileIO.ReadTextAsync(file);

                T obj = JsonConvert.DeserializeObject<T>(result);

                return obj;
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("File not found");

                return null;
            }
        }


        public async static Task SaveJsonFileToDocumentsLibraryAsync<T>(string filename, T obj) where T : class
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;// Windows.Storage.KnownFolders.MusicLibrary;

                string json = JsonConvert.SerializeObject(obj);

                StorageFolder notesFolder = await localFolder.CreateFolderAsync(BASE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
                StorageFolder subfolder = await notesFolder.CreateFolderAsync("Json", CreationCollisionOption.OpenIfExists);
                StorageFile file = await subfolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);

            }
            catch (Exception)
            {
                Debug.WriteLine("save File error");
            }



        }

        public static async Task<string> GetAudioFileFromMusicLibraryAsync(string filename)
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;// Windows.Storage.KnownFolders.MusicLibrary;

                StorageFolder folder = await localFolder.CreateFolderAsync(BASE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
                StorageFolder subfolder = await folder.CreateFolderAsync("audio", CreationCollisionOption.OpenIfExists);
                var file = await subfolder.GetFileAsync(filename);
                return file.Path;
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("File not found");

                return string.Empty;
            }


        }

        public static async Task<string> DownloadAudioFileToMusicLibraryAsync(string downloadUriString, string filename)
        {
            try
            {

            
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;// Windows.Storage.KnownFolders.MusicLibrary;

            Uri downLoadingUri = new Uri(downloadUriString);
            HttpClient client = new HttpClient();
            using (var response = await client.GetAsync(downLoadingUri))
            {
                //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/Hh758325(v=win.10).aspx
                // Quickstart: Reading and writing files (XAML)\
                var buffer = await response.Content.ReadAsBufferAsync();
                var folder = await localFolder.CreateFolderAsync(BASE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
                StorageFolder subfolder = await folder.CreateFolderAsync("audio", CreationCollisionOption.OpenIfExists);
                StorageFile file = await subfolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(file, buffer);

                return file.Path;
            }
            }
            catch 
            {

                return string.Empty;
            }
        }

        public static async Task DeleteAudioFileToMusicLibraryAsync(string filename, string folderName)
        {
            try
            {

            
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;// Windows.Storage.KnownFolders.MusicLibrary;

            //   Uri downLoadingUri = new Uri(downloadUriString);
            //  HttpClient client = new HttpClient();
            //    using (var response = await client.GetAsync(downLoadingUri))
            // {
            //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/Hh758325(v=win.10).aspx
            // Quickstart: Reading and writing files (XAML)\
            //      var buffer = await response.Content.ReadAsBufferAsync();
            var folder = await localFolder.GetFolderAsync(BASE_FOLDER_NAME);
            StorageFolder subfolder = await folder.GetFolderAsync(folderName);
            StorageFile file = await subfolder.GetFileAsync(filename);
            await file.DeleteAsync();
            }
            catch 
            {

               
            }

        }


        public static async Task DeleteFilesFromMusicLibraryAsync(string folderName, int days = 15)
        {
            try
            {


                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;// Windows.Storage.KnownFolders.MusicLibrary;

                //   Uri downLoadingUri = new Uri(downloadUriString);
                //  HttpClient client = new HttpClient();
                //    using (var response = await client.GetAsync(downLoadingUri))
                // {
                //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/Hh758325(v=win.10).aspx
                // Quickstart: Reading and writing files (XAML)\
                //      var buffer = await response.Content.ReadAsBufferAsync();
                var folder = await localFolder.GetFolderAsync(BASE_FOLDER_NAME);
                StorageFolder subfolder = await folder.GetFolderAsync(folderName);
                var files = await subfolder.GetFilesAsync();
                foreach (var file in files)
                {
                    var dto = file.DateCreated;
                    DateTimeOffset currentDto = DateTimeOffset.Now;
                    var diff = currentDto - dto;
                    if (diff.Days > days)
                    {
                        await file.DeleteAsync();
                    }
                }

            }
            catch
            {


            }

        }
    }
}
