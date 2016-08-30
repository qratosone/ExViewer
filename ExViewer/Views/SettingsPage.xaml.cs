﻿using ExViewer.Settings;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.pv_root.ItemsSource = SettingCollection.Current.GroupedSettings;
            SettingTemplateSelector.Parent = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode == NavigationMode.New)
                this.pv_root.SelectedIndex = 0;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SwitchSplitView();
        }
    }

    class SettingTemplateSelector : DataTemplateSelector
    {
        ResourceDictionary templateDictionary = new ResourceDictionary() { Source = new Uri("ms-appx:///Settings/SettingPresenterTemplates.xaml") };

        internal static SettingsPage Parent
        {
            get; set;
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var i = (SettingInfo)item;
            switch(i.Type)
            {
            case SettingType.Int32:
            case SettingType.Int64:
            case SettingType.Single:
            case SettingType.Double:
                return (DataTemplate)Parent.Resources["Number"];
            case SettingType.String:
                return (DataTemplate)Parent.Resources["String"];
            case SettingType.Enum:
                return (DataTemplate)Parent.Resources["Enum"];
            case SettingType.Boolean:
                return (DataTemplate)Parent.Resources["Boolean"];
            case SettingType.Custom:
                return (DataTemplate)templateDictionary[i.SettingPresenterTemplate];
            default:
                return base.SelectTemplateCore(item);
            }
        }
    }

    class SettingSlider : Slider
    {
        protected override void OnDisconnectVisualChildren()
        {
            base.OnDisconnectVisualChildren();
            ClearValue(SettingValueProperty);
        }

        public SettingInfo SettingValue
        {
            get
            {
                return (SettingInfo)GetValue(SettingValueProperty);
            }
            set
            {
                SetValue(SettingValueProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SettingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingValueProperty =
            DependencyProperty.Register("SettingValue", typeof(SettingInfo), typeof(SettingSlider), new PropertyMetadata(null, SettingValueChangedCallback));

        private static void SettingValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (SettingSlider)d;
            s.ValueChanged -= s.ValueChangedCallback;
            if(e.OldValue != null)
            {
                ((SettingInfo)e.OldValue).PropertyChanged -= s.SettingInfoPropertyChanged;
            }
            var sv = e.NewValue as SettingInfo;
            if(sv != null)
            {
                var min = Convert.ToDouble(sv.Range.Min);
                var max = Convert.ToDouble(sv.Range.Max);

                s.Minimum = min;
                s.Maximum = max;
                s.Value = Convert.ToDouble(sv.Value);

                var small = (max - min) / 100;
                var large = small * 10;

                if(!double.IsNaN(sv.Range.Small))
                    small = sv.Range.Small;
                if(!double.IsNaN(sv.Range.Large))
                    large = sv.Range.Large;

                s.SmallChange = small;
                s.LargeChange = large;
                s.StepFrequency = small;

                if(!double.IsNaN(sv.Range.Tick))
                    s.TickFrequency = sv.Range.Tick;
                else
                    s.ClearValue(TickFrequencyProperty);

                s.ValueChanged += s.ValueChangedCallback;
                sv.PropertyChanged += s.SettingInfoPropertyChanged;
            }
            else
            {
                s.ClearValue(ValueProperty);
                s.ClearValue(MinimumProperty);
                s.ClearValue(MaximumProperty);

                s.ClearValue(TickFrequencyProperty);
                s.ClearValue(SmallChangeProperty);
                s.ClearValue(LargeChangeProperty);
                s.ClearValue(StepFrequencyProperty);
            }
        }

        private void SettingInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(Value))
                return;
            var s = (SettingInfo)sender;
            this.Value = Convert.ToDouble(s.Value);
        }

        private void ValueChangedCallback(object sender, RangeBaseValueChangedEventArgs e)
        {
            var s = (SettingSlider)sender;
            s.SettingValue.Value = ConvertToBack(e.NewValue, s.SettingValue.Type);
        }

        public static object ConvertToBack(double value, SettingType parameter)
        {
            Type targetType = null;
            switch(parameter)
            {
            case SettingType.Int32:
                targetType = typeof(int);
                break;
            case SettingType.Int64:
                targetType = typeof(long);
                break;
            case SettingType.Single:
                targetType = typeof(float);
                break;
            case SettingType.Double:
                targetType = typeof(double);
                break;
            }
            return Convert.ChangeType(value, targetType);
        }
    }

    class SettingComboBox : ComboBox
    {
        protected override void OnDisconnectVisualChildren()
        {
            base.OnDisconnectVisualChildren();
            ClearValue(SettingValueProperty);
        }

        public SettingInfo SettingValue
        {
            get
            {
                return (SettingInfo)GetValue(SettingValueProperty);
            }
            set
            {
                SetValue(SettingValueProperty, value);
            }
        }

        private Type settingType;

        // Using a DependencyProperty as the backing store for SettingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingValueProperty =
            DependencyProperty.Register("SettingValue", typeof(SettingInfo), typeof(SettingComboBox), new PropertyMetadata(null, SettingValueChangedCallback));

        private static void SettingValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (SettingComboBox)d;
            if(e.OldValue != null)
            {
                ((SettingInfo)e.OldValue).PropertyChanged -= s.SettingInfoPropertyChanged;
            }
            s.SelectionChanged -= s.SelectionChangedCallback;
            var sv = e.NewValue as SettingInfo;
            if(sv != null)
            {
                s.settingType = sv.PropertyInfo.PropertyType;
                var selections = Enum.GetNames(s.settingType)
                    .Select(name =>
                        new EnumSelection(name, LocalizedStrings.Settings.GetString(sv.EnumRepresent.ResourcePrefix + name)))
                    .ToList();
                s.ItemsSource = selections;
                s.SelectedItem = selections.Single(sel => sel.EnumName == sv.Value.ToString());
                s.SelectionChanged += s.SelectionChangedCallback;
                sv.PropertyChanged += s.SettingInfoPropertyChanged;
            }
            else
            {
                s.ClearValue(SelectedItemProperty);
                s.ClearValue(ItemsSourceProperty);
            }
        }

        private void SelectionChangedCallback(object sender, SelectionChangedEventArgs e)
        {
            var s = (SettingComboBox)sender;
            s.SettingValue.Value = Enum.Parse(s.settingType, ((EnumSelection)s.SelectedItem).EnumName);
        }

        private void SettingInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(SettingInfo.Value))
                return;
            var s = (SettingInfo)sender;
            this.SelectedItem = ((IEnumerable<EnumSelection>)ItemsSource).Single(sel => sel.EnumName == s.Value.ToString());
        }

        private class EnumSelection
        {
            public EnumSelection(string name, string friendlyName)
            {
                FriendlyName = friendlyName;
                EnumName = name;
            }

            public string FriendlyName
            {
                get;
            }

            public string EnumName
            {
                get;
            }

            public override string ToString()
            {
                return FriendlyName;
            }
        }
    }
}
