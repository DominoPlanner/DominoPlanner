using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform;
using GetText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace DominoPlanner.Usage
{
    public class GettextExtension : MarkupExtension
    {
        public GettextExtension()
        {
        }
        public GettextExtension(string key)
        {
            // Workaround for linebreaks in XAML
            key = key.Replace("\\n", "\n");
            var splitted = key.Split("|");
            if (splitted.Length <= 1)
                this.Key = key;
            else
            {
                this.Key = splitted[0];
                this.Context = splitted[1];
            }
        }
        public GettextExtension(string key, params object[] @params)
        {
            this.Key = key;
            this.Params = @params;
        }
        public object[] Params {get; set;}
        public string Key { get; set; }

        public string Context { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!string.IsNullOrWhiteSpace(Context))
                return Localizer.LocalizerInstance[Key, Context];
            else
                return Localizer.LocalizerInstance[Key];
        }
    }
    public class Localizer : INotifyPropertyChanged
    {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private GetText.Catalog catalog = null;
        public Localizer()
        {
            LoadLanguage(Language);
        }
        public static List<CultureInfo> GetAllLocales()
        {
            List<CultureInfo> result = new List<CultureInfo>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var langs = assets.GetAssets(new Uri("avares://DominoPlanner.Usage/locale/"), new Uri("avares://DominoPlanner.Usage/locale/"));
            foreach (var l in langs)
                if (Path.GetExtension(l.LocalPath) == ".mo")
                {
                    result.Add(new CultureInfo(Directory.GetParent(Directory.GetParent(l.LocalPath).FullName).Name));
                }
            result.Add(new CultureInfo("en-US"));
            return result;
        }

        public bool LoadLanguage(string language)
        {
            Language = language;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Uri uri = new Uri($"avares://DominoPlanner.Usage/locale/{language}/LC_MESSAGES/DominoPlanner.mo");
            if (assets.Exists(uri))
            {
                using (var stream = assets.Open(uri))
                {
                    catalog = new GetText.Catalog(stream, new System.Globalization.CultureInfo(Language));
                }
                Invalidate();

                return true;
            }
            return false;
        } // LoadLanguage

        public string Language { get; private set; } = Properties.Settings.Default.Language;

        public string this[string key]
        {
            get
            {
                if (catalog != null)
                    return catalog.GetString(key);
                return key;
            }
        }
        public string this[string key, string context]
        {
            get
            {
                if (catalog != null)
                    return catalog.GetParticularString(context, key);
                return key;
            }
        }
        public static string _(string key)
        {
            return LocalizerInstance[key];
        }

        public static Localizer LocalizerInstance { get; set; } = new Localizer();
        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
        public static string GetParticularString(string context, string key)
        {
            return LocalizerInstance[key, context];
        }
    }

}
