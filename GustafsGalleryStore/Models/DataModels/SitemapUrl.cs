﻿using System;
using GustafsGalleryStore.Helpers;

namespace GustafsGalleryStore.Models.DataModels
{
    public class SitemapUrl
    {
        public string Url { get; set; }
        public DateTime? Modified { get; set; }
        public ChangeFrequency? ChangeFrequency { get; set; }
        public double? Priority { get; set; }
    }
}