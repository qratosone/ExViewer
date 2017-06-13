﻿using ExClient.Models;
using Microsoft.EntityFrameworkCore;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Graphics.Imaging;

namespace ExClient.Galleries
{
    public sealed class SavedGallery : CachedGallery
    {
        private sealed class SavedGalleryList : GalleryList<SavedGallery, GalleryModel>
        {
            public static IAsyncOperation<ObservableCollection<Gallery>> LoadList()
            {
                return Task.Run<ObservableCollection<Gallery>>(() =>
                {
                    using (var db = new GalleryDb())
                    {
                        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                        var query = db.SavedSet
                            .OrderByDescending(s => s.saved)
                            .Select(s => s.Gallery);
                        return new SavedGalleryList(query.ToList());
                    }
                }).AsAsyncOperation();
            }

            private SavedGalleryList(List<GalleryModel> galleries)
                : base(galleries)
            {
            }

            protected override SavedGallery Load(GalleryModel model)
            {
                var sg = new SavedGallery(model);
                var ignore = sg.InitAsync();
                return sg;
            }
        }

        public static IAsyncOperation<ObservableCollection<Gallery>> LoadSavedGalleriesAsync()
        {
            return SavedGalleryList.LoadList();
        }

        public static IAsyncActionWithProgress<double> ClearAllGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                progress.Report(double.NaN);
                var folder = GalleryImage.ImageFolder ?? await GalleryImage.GetImageFolderAsync();
                var getFiles = folder.GetFilesAsync();
                using (var db = new GalleryDb())
                {
                    db.SavedSet.RemoveRange(db.SavedSet);
                    db.ImageSet.RemoveRange(db.ImageSet);
                    await db.SaveChangesAsync();
                }
                var files = await getFiles;
                double c = files.Count;
                var i = 0;
                foreach (var item in files)
                {
                    await item.DeleteAsync();
                    progress.Report(++i / c);
                }
            });
        }

        internal SavedGallery(GalleryModel model)
                : base(model)
        {
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            return AsyncWrapper.CreateCompleted();
        }

        public override IAsyncAction DeleteAsync()
        {
            return Task.Run(async () =>
            {
                using (var db = new GalleryDb())
                {
                    var gid = this.Id;
                    db.SavedSet.Remove(db.SavedSet.Single(c => c.GalleryId == gid));
                    await db.SaveChangesAsync();
                }
                await base.DeleteAsync();
            }).AsAsyncAction();
        }
    }
}