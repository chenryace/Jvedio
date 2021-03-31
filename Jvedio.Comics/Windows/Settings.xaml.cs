using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Jvedio.Comics.Global;
using Jvedio.Style;
namespace Jvedio.Comics
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : BaseWindow
    {
        public static string GrowlToken = "SettingsGrowl";
        public Settings()
        {
            InitializeComponent();
            TabControl.SelectedIndex = Properties.Settings.Default.SettingsIndex;
        }

        private void Restore(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, Jvedio.Language.Resources.Message_IsToReset).ShowDialog() == true)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }

        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {

            GlobalVariable.InitVariable();
            Scan.InitSearchPattern();
            Net.Init();
        }


        private void SetSkin(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            MainWindow main =GetWindowByName("MainWindow") as MainWindow;
            main?.SetSkin();
            //main?.SetSelected();
            //main?.ActorSetSelected();
        }

        private void SetBackgroundImage(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.Choose;
            OpenFileDialog1.FileName = "background.jpg";
            OpenFileDialog1.Filter = "(jpg;jpeg;png)|*.jpg;*.jpeg;*.png";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = OpenFileDialog1.FileName;
                if (File.Exists(path))
                {
                    //设置背景
                    GlobalVariable.BackgroundImage = null;
                    GC.Collect();
                    GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);
                    Properties.Settings.Default.BackgroundImage = path;
                    //(GetWindowByName("Main") as MainWindow)?.SetSkin();
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //设置皮肤选中
            var rbs = SkinWrapPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in rbs)
            {
                if (item.Content.ToString() == Properties.Settings.Default.Themes)
                {
                    item.IsChecked = true;
                    return;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.SettingsIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }
}
