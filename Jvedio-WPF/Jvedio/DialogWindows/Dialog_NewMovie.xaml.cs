using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using Jvedio.Utils;
using Jvedio.Style;
using Jvedio.Core.Enums;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_NewMovie : Jvedio.Style.BaseDialog
    {
        public NewVideoDialogResult Result { get; private set; }


        public bool AutoAddPrefix { get; set; }
        public string Prefix { get; set; }
        public VideoType VideoType { get; set; }
        public Dialog_NewMovie(Window owner, bool autoAddPrefix = false, string prefix = "") : base(owner)
        {
            InitializeComponent();
            Prefix = prefix;
            AutoAddPrefix = autoAddPrefix;
            VideoType = VideoType.Censored;
            autoPrefix.IsChecked = AutoAddPrefix;
            PrefixTextBox.Text = Prefix;
        }

        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            Result = new NewVideoDialogResult(AddMovieTextBox.Text, AutoAddPrefix ? Prefix : "", VideoType);
            base.Confirm(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //WindowTools windowTools = (WindowTools)FileProcess.GetWindowByName("WindowTools");
            //if (windowTools == null) windowTools = new WindowTools();
            //windowTools.Show();
            //windowTools.Activate();
            //windowTools.TabControl.SelectedIndex = 0;
            //this.Close();
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            AddMovieTextBox.Focus();

        }

        private void SetChecked(object sender, RoutedEventArgs e)
        {
            AutoAddPrefix = (bool)(sender as CheckBox).IsChecked;
        }



        private void PrefixTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Prefix = (sender as TextBox).Text;
        }

        private void PrefixTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Prefix = (sender as TextBox).Text;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoType = (VideoType)((sender as ComboBox).SelectedIndex);
        }
    }



    public class NewVideoDialogResult
    {
        public string Text { get; set; }
        public string Prefix { get; set; }
        public VideoType VideoType { get; set; }


        public NewVideoDialogResult(string text, string prefix, VideoType videoType)
        {
            Text = text;
            Prefix = prefix;
            VideoType = videoType;

        }
    }


}