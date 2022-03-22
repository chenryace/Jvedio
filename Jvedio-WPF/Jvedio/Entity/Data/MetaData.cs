﻿using DynamicData.Annotations;
using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Entity
{

    [Table(tableName: "metadata")]
    public class MetaData
    {

        [TableId(IdType.AUTO)]
        public long DataID { get; set; }
        public long DBId { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public string Country { get; set; }
        public string ReleaseDate { get; set; }
        public int ReleaseYear { get; set; }
        public int ViewCount { get; set; }
        public DataType DataType { get; set; }
        public float Rating { get; set; }
        public int RatingCount { get; set; }
        public int FavoriteCount { get; set; }
        public string Genre { get; set; }
        public string Tag { get; set; }
        public float Grade { get; set; }
        public string Label { get; set; }
        public string ViewDate { get; set; }
        public string FirstScanDate { get; set; }
        public string LastScanDate { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
