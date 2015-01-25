using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VOALearningEnglish.Models
{
    public class ArticleModel : INotifyPropertyChanged
    {
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                this.SetProperty(ref this.title, value);
            }
        }

        private string content;
        public string Content
        {
            get { return content; }
            set
            {
                this.SetProperty(ref this.content, value);
            }
        }

        //mp3url

        private string mp3url;
        public string Mp3url
        {
            get { return mp3url; }
            set
            {
                this.SetProperty(ref this.mp3url, value);
            }
        }

        private string audio;
        public string Audio
        {
            get { return audio; }
            set
            {
                this.SetProperty(ref this.audio, value);
            }
        }

        private string downloadIconLable;
        public string DownloadIconLable
        {
            get
            {
                return downloadIconLable;
            }
            set
            {
                this.SetProperty(ref this.downloadIconLable, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
