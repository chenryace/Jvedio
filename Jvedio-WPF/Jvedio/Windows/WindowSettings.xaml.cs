using ChaoControls.Style;
using FontAwesome.WPF;
using Jvedio.CommonNet;
using Jvedio.CommonNet.Crawler;
using Jvedio.CommonNet.Entity;
using Jvedio.Core.Crawler;
using Jvedio.Core.Enums;
using Jvedio.Core.Plugins;
using Jvedio.Core.SimpleMarkDown;
using Jvedio.Entity;
using Jvedio.Style;
using Jvedio.Utils;
using Jvedio.Utils.Encrypt;
using Jvedio.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : ChaoControls.Style.BaseWindow
    {

        public const string ffmpeg_url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";
        public static string GrowlToken = "SettingsGrowl";
        public DetailMovie SampleMovie = new DetailMovie()
        {
            id = "AAA-001",
            title = Jvedio.Language.Resources.SampleMovie_Title,
            vediotype = 1,
            releasedate = "2020-01-01",
            director = Jvedio.Language.Resources.SampleMovie_Director,
            genre = Jvedio.Language.Resources.SampleMovie_Genre,
            tag = Jvedio.Language.Resources.SampleMovie_Tag,
            actor = Jvedio.Language.Resources.SampleMovie_Actor,
            studio = Jvedio.Language.Resources.SampleMovie_Studio,
            rating = 9.0f,
            chinesetitle = Jvedio.Language.Resources.SampleMovie_TranslatedTitle,
            label = Jvedio.Language.Resources.SampleMovie_Label,
            year = 2020,
            runtime = 126,
            country = Jvedio.Language.Resources.SampleMovie_Country
        };
        public VieModel_Settings vieModel;
        public Settings()
        {
            InitializeComponent();
            if (GlobalFont != null) this.FontFamily = GlobalFont;
            vieModel = new VieModel_Settings();


            this.DataContext = vieModel;
            vieModel.Reset();



            //绑定事件
            foreach (var item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                item.Click += AddToRename;
            }
            if (Properties.Settings.Default.SettingsIndex == 2)
                TabControl.SelectedIndex = 0;
            else
                TabControl.SelectedIndex = Properties.Settings.Default.SettingsIndex;

        }



        #region "热键"





        private void hotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift)
            {
                if (!funcKeys.Contains(currentKey)) funcKeys.Add(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.NumPad0 && currentKey <= Key.NumPad9))
            {
                key = currentKey;
            }
            else
            {
                //Console.WriteLine("不支持");
            }

            string singleKey = key.ToString();
            if (key.ToString().Length > 1)
            {
                singleKey = singleKey.ToString().Replace("D", "");
            }

            if (funcKeys.Count > 0)
            {
                if (key == Key.None)
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys);
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = Key.None;
                }
                else
                {
                    hotkeyTextBox.Text = string.Join("+", funcKeys) + "+" + singleKey;
                    _funcKeys = new List<Key>();
                    _funcKeys.AddRange(funcKeys);
                    _key = key;
                }

            }
            else
            {
                if (key != Key.None)
                {
                    hotkeyTextBox.Text = singleKey;
                    _funcKeys = new List<Key>();
                    _key = key;
                }
            }




        }

        private void hotkeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {

            Key currentKey = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (currentKey == Key.LeftCtrl | currentKey == Key.LeftAlt | currentKey == Key.LeftShift)
            {
                if (funcKeys.Contains(currentKey)) funcKeys.Remove(currentKey);
            }
            else if ((currentKey >= Key.A && currentKey <= Key.Z) || (currentKey >= Key.D0 && currentKey <= Key.D9) || (currentKey >= Key.F1 && currentKey <= Key.F12))
            {
                if (currentKey == key)
                {
                    key = Key.None;
                }

            }


        }

        private void ApplyHotKey(object sender, RoutedEventArgs e)
        {
            bool containsFunKey = _funcKeys.Contains(Key.LeftAlt) | _funcKeys.Contains(Key.LeftCtrl) | _funcKeys.Contains(Key.LeftShift) | _funcKeys.Contains(Key.CapsLock);


            if (!containsFunKey | _key == Key.None)
            {
                ChaoControls.Style.MessageCard.Error("必须为 功能键 + 数字/字母");
            }
            else
            {
                //注册热键
                if (_key != Key.None & IsProperFuncKey(_funcKeys))
                {
                    uint fsModifiers = (uint)Modifiers.None;
                    foreach (Key key in _funcKeys)
                    {
                        if (key == Key.LeftCtrl) fsModifiers = fsModifiers | (uint)Modifiers.Control;
                        if (key == Key.LeftAlt) fsModifiers = fsModifiers | (uint)Modifiers.Alt;
                        if (key == Key.LeftShift) fsModifiers = fsModifiers | (uint)Modifiers.Shift;
                    }
                    VK = (uint)KeyInterop.VirtualKeyFromKey(_key);


                    UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                    bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, fsModifiers, VK);
                    if (!success) { MessageBox.Show("热键冲突！", "热键冲突"); }
                    {
                        //保存设置
                        Properties.Settings.Default.HotKey_Modifiers = fsModifiers;
                        Properties.Settings.Default.HotKey_VK = VK;
                        Properties.Settings.Default.HotKey_Enable = true;
                        Properties.Settings.Default.HotKey_String = hotkeyTextBox.Text;
                        Properties.Settings.Default.Save();
                        ChaoControls.Style.MessageCard.Success("设置热键成功");
                    }

                }



            }
        }

        #endregion





        public void AddPath(object sender, RoutedEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (vieModel.ScanPath == null) { vieModel.ScanPath = new ObservableCollection<string>(); }
                if (!vieModel.ScanPath.Contains(path) && !vieModel.ScanPath.IsIntersectWith(path))
                {
                    vieModel.ScanPath.Add(path);
                    //保存
                    FileProcess.SaveScanPathToConfig(vieModel.DataBase, vieModel.ScanPath?.ToList());
                }
                else
                {
                    ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.FilePathIntersection);
                }


            }







        }

        public async void TestAI(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];
            if (checkBox.Content.ToString() == Jvedio.Language.Resources.BaiduFaceRecognition)
            {

                string base64 = Resource_String.BaseImage64;
                System.Drawing.Bitmap bitmap = ImageProcess.Base64ToBitmap(base64);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = await TestBaiduAI(bitmap);
                if (result != null && int32Rect != Int32Rect.Empty)
                {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));
                    string clientId = Properties.Settings.Default.Baidu_API_KEY.Replace(" ", "");
                    string clientSecret = Properties.Settings.Default.Baidu_SECRET_KEY.Replace(" ", "");
                    SaveKeyValue(clientId, clientSecret, "BaiduAI.key");
                }
                else
                {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public static Task<(Dictionary<string, string>, Int32Rect)> TestBaiduAI(System.Drawing.Bitmap bitmap)
        {
            return Task.Run(() =>
            {
                string token = AccessToken.getAccessToken();
                string FaceJson = FaceDetect.faceDetect(token, bitmap);
                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = FaceParse.Parse(FaceJson);
                return (result, int32Rect);
            });

        }

        public async void TestTranslate(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().First();
            ImageAwesome imageAwesome = stackPanel.Children.OfType<ImageAwesome>().First();
            imageAwesome.Icon = FontAwesomeIcon.Refresh;
            imageAwesome.Spin = true;
            imageAwesome.Foreground = (SolidColorBrush)Application.Current.Resources["ForegroundSearch"];

            if (checkBox.Content.ToString() == "百度翻译")
            {

            }
            else if (checkBox.Content.ToString() == Jvedio.Language.Resources.Youdao)
            {
                string result = await Translate.Youdao("のマ○コに");
                if (result != "")
                {
                    imageAwesome.Icon = FontAwesomeIcon.CheckCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Color.FromRgb(32, 183, 89));

                    string Youdao_appKey = Properties.Settings.Default.TL_YOUDAO_APIKEY.Replace(" ", "");
                    string Youdao_appSecret = Properties.Settings.Default.TL_YOUDAO_SECRETKEY.Replace(" ", "");

                    //成功，保存在本地
                    SaveKeyValue(Youdao_appKey, Youdao_appSecret, "youdao.key");
                }
                else
                {
                    imageAwesome.Icon = FontAwesomeIcon.TimesCircle;
                    imageAwesome.Spin = false;
                    imageAwesome.Foreground = new SolidColorBrush(Colors.Red);
                }
            }


        }

        public void SaveKeyValue(string key, string value, string filename)
        {
            string v = Encrypt.AesEncrypt(key + " " + value, EncryptKeys[0]);
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, append: false))
                {
                    sw.Write(v);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }

        public void DelPath(object sender, RoutedEventArgs e)
        {
            if (PathListBox.SelectedIndex != -1)
            {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
            if (vieModel.ScanPath != null)
                SaveScanPathToConfig(vieModel.DataBase, vieModel.ScanPath.ToList());

        }

        public void ClearPath(object sender, RoutedEventArgs e)
        {

            vieModel.ScanPath?.Clear();
            SaveScanPathToConfig(vieModel.DataBase, new List<string>());
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

        private void DisplayNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 500)
                {
                    Properties.Settings.Default.DisplayNumber = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void FlowTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 30)
                {
                    Properties.Settings.Default.FlowNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ActorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 50)
                {
                    Properties.Settings.Default.ActorDisplayNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ScreenShotNumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num > 0 & num <= 20)
                {
                    Properties.Settings.Default.ScreenShotNum = num;
                    Properties.Settings.Default.Save();
                }
            }

        }

        private void ScanMinFileSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int num = 0;
            bool success = int.TryParse(textBox.Text, out num);
            if (success)
            {
                num = int.Parse(textBox.Text);
                if (num >= 0 & num <= 2000)
                {
                    Properties.Settings.Default.ScanMinFileSize = num;
                    Properties.Settings.Default.Save();
                }
            }

        }






        private void ListenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsVisible == false) return;
            if ((bool)checkBox.IsChecked)
            {
                //测试是否能监听
                if (!TestListen())
                    checkBox.IsChecked = false;
                else
                    ChaoControls.Style.MessageCard.Info(Jvedio.Language.Resources.RebootToTakeEffect);
            }
        }


        FileSystemWatcher[] watchers;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public bool TestListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            watchers = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++)
            {
                try
                {

                    if (drives[i] == @"C:\") { continue; }
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.EnableRaisingEvents = true;
                    watchers[i] = watcher;
                    watcher.Dispose();
                }
                catch
                {
                    ChaoControls.Style.MessageCard.Error($"{Jvedio.Language.Resources.NoPermissionToListen} {drives[i]}");
                    return false;
                }
            }
            return true;
        }

        private void SetVediaPlaterPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.Choose;
            OpenFileDialog1.Filter = "exe|*.exe";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = OpenFileDialog1.FileName;
                if (File.Exists(exePath))
                    Properties.Settings.Default.VedioPlayerPath = exePath;

            }
        }


        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Opacity_Main >= 0.5)
                App.Current.Windows[0].Opacity = Properties.Settings.Default.Opacity_Main;
            else
                App.Current.Windows[0].Opacity = 1;

            ////UpdateServersEnable();

            bool success = vieModel.SaveServers((msg) =>
             {
                 MessageCard.Error(msg);
             });
            if (success)
            {
                GlobalVariable.InitVariable();
                ScanHelper.InitSearchPattern();
                savePath();
                saveSettings();

                ChaoControls.Style.MessageCard.Success(Jvedio.Language.Resources.Message_Success);
            }

        }

        private void savePath()
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)vieModel.PicPaths[PathType.RelativeToData.ToString()];
            dict["BigImagePath"] = vieModel.BigImagePath;
            dict["SmallImagePath"] = vieModel.SmallImagePath;
            dict["PreviewImagePath"] = vieModel.PreviewImagePath;
            dict["ScreenShotPath"] = vieModel.ScreenShotPath;
            dict["ActorImagePath"] = vieModel.ActorImagePath;
            vieModel.PicPaths[PathType.RelativeToData.ToString()] = dict;
            GlobalConfig.Settings.PicPathJson = JsonConvert.SerializeObject(vieModel.PicPaths);

        }





        private void SetFFMPEGPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseFFmpeg;
            OpenFileDialog1.FileName = "ffmpeg.exe";
            OpenFileDialog1.Filter = "ffmpeg.exe|*.exe";
            OpenFileDialog1.FilterIndex = 1;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = OpenFileDialog1.FileName;
                if (File.Exists(exePath))
                {
                    if (new FileInfo(exePath).Name.ToLower() == "ffmpeg.exe")
                        Properties.Settings.Default.FFMPEG_Path = exePath;
                }
            }
        }

        private void SetSkin(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            OnSetSkin();
        }

        private void OnSetSkin()
        {
            Main main = GetWindowByName("Main") as Main;
            main.SetSkin();
            main?.SetSelected();
            main?.ActorSetSelected();
        }



        private void SetLanguage(object sender, RoutedEventArgs e)
        {
            //https://blog.csdn.net/fenglailea/article/details/45888799
            Properties.Settings.Default.Language = (sender as RadioButton).Content.ToString();
            Properties.Settings.Default.Save();
            string language = Properties.Settings.Default.Language;
            string hint = "";
            if (language == "English")
                hint = "Take effect after restart";
            else if (language == "日本語")
                hint = "再起動後に有効になります";
            else
                hint = "重启后生效";
            ChaoControls.Style.MessageCard.Success(hint);


            //SetLanguageDictionary();


        }

        private void SetLanguageDictionary()
        {
            //设置语言
            string language = Jvedio.Properties.Settings.Default.Language;
            switch (language)
            {
                case "日本語":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("ja-JP");
                    break;
                case "中文":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case "English":
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
                default:
                    Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo("en-US");
                    break;
            }
            //Jvedio.Language.Resources.Culture.ClearCachedData();
        }

        private void Border_MouseLeftButtonUp1(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(Properties.Settings.Default.Selected_BorderBrush);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.Selected_BorderBrush = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                Properties.Settings.Default.Save();
            }


        }

        private void Border_MouseLeftButtonUp2(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(Properties.Settings.Default.Selected_BorderBrush);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.Selected_Background = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
                Properties.Settings.Default.Save();
            }

        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            vieModel.DataBase = e.AddedItems[0].ToString();
            vieModel.Reset();




        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            // 设置 crawlerIndex
            serverListBox.SelectedIndex = (int)GlobalConfig.Settings.CrawlerSelectedIndex;




            //设置当前数据库
            for (int i = 0; i < vieModel.DataBases?.Count; i++)
            {
                if (vieModel.DataBases[i].ToLower() == Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel.DataBases?.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;

            ShowViewRename(Properties.Settings.Default.RenameFormat);

            SetCheckedBoxChecked();


            foreach (ComboBoxItem item in OutComboBox.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.OutSplit)
                {
                    OutComboBox.SelectedIndex = OutComboBox.Items.IndexOf(item);
                    break;
                }
            }


            foreach (ComboBoxItem item in InComboBox.Items)
            {
                if (item.Content.ToString() == Properties.Settings.Default.InSplit)
                {
                    InComboBox.SelectedIndex = InComboBox.Items.IndexOf(item);
                    break;
                }
            }

            //设置主题选中
            bool findTheme = false;
            foreach (var item in SkinWrapPanel.Children.OfType<RadioButton>())
            {
                if (item.Content.ToString() == Properties.Settings.Default.Themes)
                {
                    item.IsChecked = true;
                    findTheme = true;
                    break;
                }
            }
            if (!findTheme)
            {
                for (int i = 0; i < ThemesDataGrid.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)ThemesDataGrid.ItemContainerGenerator.ContainerFromItem(ThemesDataGrid.Items[i]);
                    if (row != null)
                    {
                        var cell = ThemesDataGrid.Columns[0];
                        var cp = (ContentPresenter)cell?.GetCellContent(row);
                        RadioButton rb = (RadioButton)cp?.ContentTemplate.FindName("rb", cp);
                        if (rb != null && rb.Content.ToString() == Properties.Settings.Default.Themes)
                        {
                            rb.IsChecked = true;

                            break;
                        }
                    }

                }
            }




        }

        private void SetCheckedBoxChecked()
        {
            foreach (ToggleButton item in CheckedBoxWrapPanel.Children.OfType<ToggleButton>().ToList())
            {
                if (Properties.Settings.Default.RenameFormat.IndexOf(item.Content.ToString().ToSqlField()) >= 0)
                {
                    item.IsChecked = true;
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(YoudaoUrl);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(BaiduUrl);
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            if (vieModel.ScanPath == null) { vieModel.ScanPath = new ObservableCollection<string>(); }
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in dragdropFiles)
            {
                if (!FileHelper.IsFile(item))
                {
                    if (!vieModel.ScanPath.Contains(item) && !vieModel.ScanPath.IsIntersectWith(item))
                    {
                        vieModel.ScanPath.Add(item);
                    }
                    else
                    {
                        ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.FilePathIntersection);
                    }
                }

            }
            //保存
            FileProcess.SaveScanPathToConfig(vieModel.DataBase, vieModel.ScanPath.ToList());

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //选择NFO存放位置
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (path.Substring(path.Length - 1, 1) != "\\") { path = path + "\\"; }
                Properties.Settings.Default.NFOSavePath = path;

            }
            else
            {
                ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.Message_CanNotBeNull);
            }

        }


        private void NewServer(object sender, RoutedEventArgs e)
        {
            string serverType = getCurrentServerType();
            if (string.IsNullOrEmpty(serverType)) return;
            CrawlerServer server = new CrawlerServer()
            {
                Enabled = true,
                Url = "https://www.baidu.com/",
                Cookies = "",
                Available = 0,
                LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[serverType];
            if (list == null) list = new ObservableCollection<CrawlerServer>();
            list.Add(server);
            vieModel.CrawlerServers[serverType] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }



        private string getCurrentServerType()
        {
            int idx = serverListBox.SelectedIndex;
            if (idx < 0 || vieModel.CrawlerServers == null || vieModel.CrawlerServers.Count == 0) return null;
            return vieModel.CrawlerServers.Keys.ToList()[idx];
        }


        private int CurrentRowIndex = 0;
        private void TestServer(object sender, RoutedEventArgs e)
        {
            int idx = CurrentRowIndex;
            string serverType = getCurrentServerType();
            if (string.IsNullOrEmpty(serverType)) return;
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[serverType];

            list[idx].Available = 2;
            ServersDataGrid.IsEnabled = false;
            CheckUrl(list[idx], (s) =>
            {
                ServersDataGrid.IsEnabled = true;
                list[idx].LastRefreshDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            });
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {

            string serverType = getCurrentServerType();
            if (string.IsNullOrEmpty(serverType)) return;
            Console.WriteLine(CurrentRowIndex);
            ObservableCollection<CrawlerServer> list = vieModel.CrawlerServers[serverType];
            list.RemoveAt(CurrentRowIndex);
            vieModel.CrawlerServers[serverType] = list;
            ServersDataGrid.ItemsSource = null;
            ServersDataGrid.ItemsSource = list;
        }


        private void SetCurrentRowIndex(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            CurrentRowIndex = dgr.GetIndex();
        }

        private async void CheckUrl(CrawlerServer server, Action<int> callback)
        {
            // library 需要保证 Cookies 和 UserAgent完全一致
            RequestHeader header = new RequestHeader();
            if (server.isHeaderProper()) header = CrawlerServer.parseHeader(server);
            string title = await HTTP.AsyncGetWebTitle(server.Url, header);
            if (string.IsNullOrEmpty(title))
            {
                server.Available = -1;
            }
            else
            {
                server.Available = 1;
            }
            await Dispatcher.BeginInvoke((Action)delegate
            {
                ServersDataGrid.Items.Refresh();
                if (!string.IsNullOrEmpty(title))
                    MessageCard.Success(title);
            });
            callback.Invoke(0);
        }




        public static T GetVisualChild<T>(Visual parent) where T : Visual

        {

            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++)

            {

                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                child = v as T;

                if (child == null)

                {

                    child = GetVisualChild<T>

                    (v);

                }

                if (child != null)

                {

                    break;

                }

            }

            return child;

        }


        private void SetServerEnable(object sender, MouseButtonEventArgs e)
        {
            //bool enable = !(bool)((CheckBox)sender).IsChecked;
            //vieModel.Servers[CurrentRowIndex].IsEnable = enable;
            //ServerConfig.Instance.SaveServer(vieModel.Servers[CurrentRowIndex]);
            //InitVariable();
            //ServersDataGrid.Items.Refresh();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //注册热键
            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (modifier != 0 && vk != 0)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success)
                {
                    ChaoControls.Style.MessageCard.Error(Jvedio.Language.Resources.BossKeyError);
                    Properties.Settings.Default.HotKey_Enable = false;
                }
            }
        }

        private void Unregister_HotKey(object sender, RoutedEventArgs e)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
        }


        private void ReplaceWithValue(string property)
        {
            string inSplit = InComboBox.Text.Replace(Jvedio.Language.Resources.Nothing, "");
            PropertyInfo[] PropertyList = SampleMovie.GetType().GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(SampleMovie);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "actor" || property == "genre" || property == "label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (property == "vediotype")
                        {
                            int v = 1;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = Jvedio.Language.Resources.Uncensored;
                            else if (v == 2)
                                value = Jvedio.Language.Resources.Censored;
                            else if (v == 3)
                                value = Jvedio.Language.Resources.Europe;
                        }
                        vieModel.ViewRenameFormat = vieModel.ViewRenameFormat.Replace("{" + property + "}", value);
                    }
                    break;
                }
            }
        }

        private void AddToRename(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            string text = toggleButton.Content.ToString();
            bool ischecked = (bool)toggleButton.IsChecked;
            string formatstring = "{" + text.ToSqlField() + "}";

            string split = OutComboBox.Text.Replace(Jvedio.Language.Resources.Nothing, "");


            if (ischecked)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.RenameFormat))
                {
                    Properties.Settings.Default.RenameFormat += formatstring;
                }
                else
                {
                    Properties.Settings.Default.RenameFormat += split + formatstring;
                }
            }
            else
            {
                int idx = Properties.Settings.Default.RenameFormat.IndexOf(formatstring);
                if (idx == 0)
                {
                    Properties.Settings.Default.RenameFormat = Properties.Settings.Default.RenameFormat.Replace(formatstring, "");
                }
                else
                {
                    Properties.Settings.Default.RenameFormat = Properties.Settings.Default.RenameFormat.Replace(getSplit(formatstring) + formatstring, "");
                }
            }
        }

        private char getSplit(string formatstring)
        {
            int idx = Properties.Settings.Default.RenameFormat.IndexOf(formatstring);
            if (idx > 0)
                return Properties.Settings.Default.RenameFormat[idx - 1];
            else
                return '\0';

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vieModel == null) return;
            TextBox textBox = (TextBox)sender;
            string txt = textBox.Text;
            ShowViewRename(txt);
        }

        private void ShowViewRename(string txt)
        {

            MatchCollection matches = Regex.Matches(txt, "\\{[a-z]+\\}");
            if (matches != null && matches.Count > 0)
            {
                vieModel.ViewRenameFormat = txt;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    ReplaceWithValue(property);
                }
            }
            else
            {
                vieModel.ViewRenameFormat = "";
            }
        }

        private void OutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Properties.Settings.Default.OutSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
        }

        private void InComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Properties.Settings.Default.InSplit = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
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
                    (GetWindowByName("Main") as Main)?.SetSkin();
                    (GetWindowByName("WindowDetails") as WindowDetails)?.SetSkin();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.SettingsIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();

            saveSettings();
            GlobalConfig.Settings.Save();
        }



        private void saveSettings()
        {
            GlobalConfig.Settings.PicPathMode = vieModel.PicPathMode;
            GlobalConfig.Settings.DownloadPreviewImage = vieModel.DownloadPreviewImage;
            GlobalConfig.Settings.OverrideInfo = vieModel.OverrideInfo;
            GlobalConfig.Settings.AutoHandleHeader = vieModel.AutoHandleHeader;

        }

        private void CopyFFmpegUrl(object sender, MouseButtonEventArgs e)
        {
            FileHelper.TryOpenUrl(ffmpeg_url);
        }

        private void LoadTranslate(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("youdao.key")) return;
            string v = GetValueKey("youdao.key");
            if (v.Split(' ').Length == 2)
            {
                Properties.Settings.Default.TL_YOUDAO_APIKEY = v.Split(' ')[0];
                Properties.Settings.Default.TL_YOUDAO_SECRETKEY = v.Split(' ')[1];
            }
        }


        public string GetValueKey(string filename)
        {
            string v = "";
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    v = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            if (v != "")
                return Encrypt.AesDecrypt(v, EncryptKeys[0]);
            else
                return "";
        }

        private void LoadAI(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("BaiduAI.key")) return;
            string v = GetValueKey("BaiduAI.key");
            if (v.Split(' ').Length == 2)
            {
                Properties.Settings.Default.Baidu_API_KEY = v.Split(' ')[0];
                Properties.Settings.Default.Baidu_SECRET_KEY = v.Split(' ')[1];
            }
        }

        private int GetRowIndex(RoutedEventArgs e)
        {
            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null)
                return -1;
            else
                return dgr.GetIndex();
        }



        private void SetScanRe(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ScanRe = (sender as TextBox).Text.Replace("；", ";");
        }

        private void OpenDIY(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(ThemeDIY);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariable.LoadBgImage();
            OnSetSkin();
        }

        private void SetBasePicPath(object sender, MouseButtonEventArgs e)
        {
            var path = FileHelper.SelectPath(this);
            if (Directory.Exists(path))
            {
                if (!path.EndsWith("\\")) path += "\\";
                vieModel.BasePicPath = path;
            }
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            if (vieModel.CrawlerServers != null && vieModel.CrawlerServers.Count > 0)
            {
                string serverType = vieModel.CrawlerServers.Keys.ToList()[idx];
                int index = serverType.IndexOf('.');
                string serverName = serverType.Substring(0, index);
                string name = serverType.Substring(index + 1);
                PluginInfo pluginInfo = Global.Plugins.Crawlers.Where(arg => arg.ServerName.Equals(serverName) && arg.Name.Equals(name)).FirstOrDefault();
                if (pluginInfo != null && pluginInfo.Enabled) vieModel.PluginEnabled = true;
                else vieModel.PluginEnabled = false;

                ServersDataGrid.ItemsSource = null;
                ServersDataGrid.ItemsSource = vieModel.CrawlerServers[serverType];
                GlobalConfig.Settings.CrawlerSelectedIndex = idx;

            }
        }

        private void ShowCrawlerHelp(object sender, MouseButtonEventArgs e)
        {
            MessageCard.Info("左侧是支持的信息刮削器，右侧需要自行填入刮削器对应的网址，Jvedio 不提供任何网站地址！");
        }

        private void ShowContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Border border = sender as Border;
                ContextMenu contextMenu = border.ContextMenu;
                contextMenu.PlacementTarget = border;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
            e.Handled = true;
        }

        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            vieModel.CurrentPlugin = vieModel.InstalledPlugins[idx];
            richTextBox.Document = MarkDown.parse(vieModel.CurrentPlugin.MarkDown);
            pluginDetailGrid.Visibility = Visibility.Visible;
        }

        private void ImageSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ComboBox).SelectedIndex;
            if (idx >= 0 && vieModel != null && idx < vieModel.PIC_PATH_MODE_COUNT)
            {
                PathType type = (PathType)idx;
                if (type != PathType.RelativeToData)
                    vieModel.BasePicPath = vieModel.PicPaths[type.ToString()].ToString();
            }
        }

        private void SavePluginEnabled(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Settings.PluginEnabled = new Dictionary<string, bool>();
            bool enabled = (bool)(sender as Switch).IsChecked;
            foreach (PluginInfo plugin in Global.Plugins.Crawlers)
            {
                if (plugin.getUID().Equals(vieModel.CurrentPlugin.getUID()))
                    plugin.Enabled = enabled;
                GlobalConfig.Settings.PluginEnabled.Add(plugin.getUID(), plugin.Enabled);
            }
            GlobalConfig.Settings.PluginEnabledJson = JsonConvert.SerializeObject(GlobalConfig.Settings.PluginEnabled);
            vieModel.setServers();
        }

        private void url_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchBox searchBox = sender as SearchBox;
            string cookies = searchBox.Text;
            DialogInput dialogInput = new DialogInput(this, "请填入 cookie", cookies);
            if (dialogInput.ShowDialog() == true)
            {
                searchBox.Text = dialogInput.Text;
            }
        }

        CrawlerServer currentCrawlerServer;
        private void url_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            setHeaderPopup.IsOpen = true;
            //currentHeaderBox = sender as SearchBox;
            //parsedTextbox.Text = currentHeaderBox.Text;
            string serverType = getCurrentServerType();
            currentCrawlerServer = vieModel.CrawlerServers[serverType][ServersDataGrid.SelectedIndex];
        }

        private void CancelHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
        }

        private void ConfirmHeader(object sender, RoutedEventArgs e)
        {
            setHeaderPopup.IsOpen = false;
            if (currentCrawlerServer != null)
            {
                currentCrawlerServer.Headers = parsedTextbox.Text.Replace("{" + Environment.NewLine + "    ", "{")
                    .Replace(Environment.NewLine + "}", "}")
                    .Replace($"\",{Environment.NewLine}    \"", "\",\"");
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(currentCrawlerServer.Headers);

                if (dict.ContainsKey("Cookie")) currentCrawlerServer.Cookies = dict["Cookie"];
            }



        }

        private void InputHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = parse((sender as TextBox).Text);
        }

        private string parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] array = text.Split(Environment.NewLine.ToCharArray());
            foreach (string item in array)
            {
                int idx = item.IndexOf(':');
                if (idx <= 0 || idx >= item.Length - 1) continue;
                string key = item.Substring(0, idx).Trim();
                string value = item.Substring(idx + 1).Trim();


                if (!data.ContainsKey(key)) data.Add(key, value);
            }

            if (vieModel.AutoHandleHeader)
            {
                data.Remove("Content-Encoding");
                data.Remove("Accept-Encoding");
                data.Remove("Host");

                data = data.Where(arg => arg.Key.IndexOf(" ") < 0).ToDictionary(x => x.Key, y => y.Value);

            }




            string json = JsonConvert.SerializeObject(data);
            if (json.Equals("{}"))
                return json;

            return json.Replace("{", "{" + Environment.NewLine + "    ")
                .Replace("}", Environment.NewLine + "}")
                .Replace("\",\"", $"\",{Environment.NewLine}    \"");
        }

        private void SetAutoHeader(object sender, RoutedEventArgs e)
        {
            if (parsedTextbox != null)
                parsedTextbox.Text = parse(inputTextbox.Text);
        }
    }


}
