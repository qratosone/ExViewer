﻿using ExClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ExClient.Settings;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ExcludedLanguagesSelector : UserControl
    {
        public ExcludedLanguagesSelector()
        {
            this.InitializeComponent();
            foreach(var item in filters)
            {
                item.PropertyChanged += Filter_PropertyChanged;
            }
        }
        private List<ExcludedLanguageFilter> filters = new List<ExcludedLanguageFilter>()
        {
            new ExcludedLanguageFilter(0),
            new ExcludedLanguageFilter(ExcludedLanguage.EnglishOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.ChineseOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.DutchOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.FrenchOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.GermanOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.HungarianOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.ItalianOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.KoreanOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.PolishOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.PortugueseOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.RussianOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.SpanishOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.ThaiOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.VietnameseOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.NotApplicableOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.OtherOriginal)
        };

        public string ExcludedLanguages
        {
            get
            {
                return (string)GetValue(ExcludedLanguagesProperty);
            }
            set
            {
                SetValue(ExcludedLanguagesProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ExcludedLanguages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExcludedLanguagesProperty =
            DependencyProperty.Register("ExcludedLanguages", typeof(string), typeof(ExcludedLanguagesSelector), new PropertyMetadata("", ExcludedLanguagesChanged));

        private static char[] split = ", ".ToCharArray();

        private static void ExcludedLanguagesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var oldS = e.OldValue.ToString();
            var newS = e.NewValue.ToString();
            if(string.Equals(oldS, newS))
                return;
            var el = ExcludedLanguagesSettingProvider.FromString(newS).OrderByDescending(k => k).ToList();
            var s = (ExcludedLanguagesSelector)sender;
            s.changing = true;
            var fs = s.filters;
            ushort offset = 0;
            foreach(var item in fs)
            {
                if(el.Count == 0)
                    break;
                var lan = item.Language;
                var last = el[el.Count - 1];
                if(lan + offset == last)
                {
                    el.RemoveAt(el.Count - 1);
                    item.Original = true;
                }
                else
                {
                    item.Original = false;
                }
            }
            offset = 1024;
            foreach(var item in fs)
            {
                if(el.Count == 0)
                    break;
                var lan = item.Language;
                var last = el[el.Count - 1];
                if(lan + offset == last)
                {
                    el.RemoveAt(el.Count - 1);
                    item.Translated = true;
                }
                else
                {
                    item.Translated = false;
                }
            }
            offset = 2048;
            foreach(var item in fs)
            {
                if(el.Count == 0)
                    break;
                var lan = item.Language;
                var last = el[el.Count - 1];
                if(lan + offset == last)
                {
                    el.RemoveAt(el.Count - 1);
                    item.Rewrite = true;
                }
                else
                {
                    item.Rewrite = false;
                }
            }
            s.changing = false;
        }

        private bool changing;

        private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(changing)
                return;
            if(e.PropertyName == nameof(ExcludedLanguageFilter.All))
                return;
            var r = new List<ExcludedLanguage>();
            foreach(var item in filters)
            {
                if(item.Original)
                    r.Add(item.Language);
                if(item.Translated)
                    r.Add(item.Language + 1024);
                if(item.Rewrite)
                    r.Add(item.Language + 2048);
            }
            ExcludedLanguages = ExcludedLanguagesSettingProvider.ToString(r);
        }
    }

    internal class ExcludedLanguageFilter : ObservableObject
    {
        public ExcludedLanguageFilter(ExcludedLanguage language)
        {
            var n = language.ToString();
            n = n.Substring(0, n.Length - "Original".Length);
            Name = LocalizedStrings.Settings.GetString(n);
            Language = language;
        }

        public ExcludedLanguage Language
        {
            get;
        }

        public string Name
        {
            get;
        }

        private bool original;

        public bool Original
        {
            get
            {
                return original;
            }
            set
            {
                var oldAll = All;
                if(Set(ref original, value) && oldAll != All)
                    RaisePropertyChanged(nameof(All));
            }
        }

        private bool translated;

        public bool Translated
        {
            get
            {
                return translated;
            }
            set
            {
                var oldAll = All;
                if(Set(ref translated, value) && oldAll != All)
                    RaisePropertyChanged(nameof(All));
            }
        }

        private bool rewrite;

        public bool Rewrite
        {
            get
            {
                return rewrite;
            }
            set
            {
                var oldAll = All;
                if(Set(ref rewrite, value) && oldAll != All)
                    RaisePropertyChanged(nameof(All));
            }
        }

        public bool All
        {
            get
            {
                if(OriginalEnabled)
                    return original && translated && rewrite;
                return translated && rewrite;
            }
            set
            {
                if(value == All)
                    return;
                if(OriginalEnabled)
                {
                    original = value;
                }
                translated = value;
                rewrite = value;
                RaisePropertyChanged(null);
            }
        }

        public bool OriginalEnabled => Language != 0;
    }
}