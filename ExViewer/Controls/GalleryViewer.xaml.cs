﻿using ExClient;
using ExClient.Galleries;
using ExClient.Galleries.Rating;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class GalleryViewer : UserControl
    {
        public GalleryViewer()
        {
            this.InitializeComponent();
        }

        public Gallery Gallery
        {
            get => (Gallery)GetValue(GalleryProperty);
            set => SetValue(GalleryProperty, value);
        }

        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(Gallery), typeof(GalleryViewer), new PropertyMetadata(null));

        protected override void OnDisconnectVisualChildren()
        {
            ClearValue(GalleryProperty);
            base.OnDisconnectVisualChildren();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = availableSize.Width;
            var leftWidth = width / 3;
            if (leftWidth < 100)
                leftWidth = 100;
            else if (leftWidth > 150)
                leftWidth = 150;
            this.Cover.Height = leftWidth * 1.41428;
            return base.MeasureOverride(availableSize);
        }

        // RatingControl does not support decimal Value but do support decimal PlaceholderValue
        //private double toValue(Score? rating)
        //{
        //    if (rating is Score s)
        //        return s.ToDouble();
        //    return double.NaN;
        //}
    }
}
