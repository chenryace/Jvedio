﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using static Jvedio.ImageProcess;
using System.Windows.Input;
using System.Drawing;
using DynamicData;
using DynamicData.Binding;
using System.Xml;
using HandyControl.Tools.Extension;
using DynamicData.Annotations;
using System.Windows.Media;
using System.ComponentModel;
using Jvedio.Utils;
using Jvedio.Entity;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Jvedio.Utils.Common;

namespace Jvedio.ViewModel
{
    public class VieModel_Main : ViewModelBase
    {
        public event EventHandler CurrentMovieListHideOrChanged;
        public event EventHandler CurrentActorListHideOrChanged;
        public event EventHandler MovieFlipOverCompleted;
        public event EventHandler ActorFlipOverCompleted;
        public event EventHandler OnCurrentMovieListRemove;
        public event EventHandler PageChangedCompleted;
        public event EventHandler ActorPageChangedCompleted;

        public bool IsFlipOvering = false;
        public VedioType CurrentVedioType = VedioType.所有;

        public static string PreviousSql = "";
        public static double PreviousOffset = 0;
        public static int PreviousPage = 1;

        Main main = GetWindowByName("Main") as Main;

        public CancellationTokenSource renderVideoCTS;
        public CancellationToken renderVideoCT;


        public CancellationTokenSource renderActorCTS;
        public CancellationToken renderActorCT;

        public static Queue<int> pageQueue = new Queue<int>();
        public static Queue<int> ActorPageQueue = new Queue<int>();


        public AppDatabase CurrentAppDataBase;


        #region "RelayCommand"
        public RelayCommand<object> SelectCommand { get; set; }
        public RelayCommand<object> ShowActorsCommand { get; set; }
        public RelayCommand<object> ShowLabelsCommand { get; set; }
        public RelayCommand GenreCommand { get; set; }
        public RelayCommand LabelCommand { get; set; }

        public RelayCommand FavoritesCommand { get; set; }
        public RelayCommand RecentCommand { get; set; }

        public RelayCommand RecentWatchCommand { get; set; }

        public RelayCommand AddNewMovie { get; set; }
        public RelayCommand FlipOverCommand { get; set; }
        #endregion


        public VieModel_Main()
        {
            SelectCommand = new RelayCommand<object>(t => Select(t));
            ShowActorsCommand = new RelayCommand<object>(t => ShowAllActors(t));
            ShowLabelsCommand = new RelayCommand<object>(t => ShowAllLabels(t));
            GenreCommand = new RelayCommand(GetGenreList);
            LabelCommand = new RelayCommand(GetLabelList);
            FlipOverCommand = new RelayCommand(AsyncFlipOver);
            FavoritesCommand = new RelayCommand(GetFavoritesMovie);
            RecentWatchCommand = new RelayCommand(GetRecentWatch);
            RecentCommand = new RelayCommand(GetRecentMovie);
            AddNewMovie = new RelayCommand(AddSingleMovie);


            DataBases = new ObservableCollection<AppDatabase>();

            refreshVideoRenderToken();
            refreshActorRenderToken();

            //获得所有数据库
            //LoadDataBaseList();
            //LoadSearchHistory();

            //CurrentMovieList.AllowEdit = true;
            //CurrentMovieList.AllowNew = true;
            //CurrentMovieList.AllowRemove = true;
        }


        public void refreshVideoRenderToken()
        {
            Console.WriteLine("刷新 Token");
            renderVideoCTS = new CancellationTokenSource();
            renderVideoCTS.Token.Register(() => { Console.WriteLine("取消加载页码的任务"); });
            renderVideoCT = renderVideoCTS.Token;
        }

        public void refreshActorRenderToken()
        {
            Console.WriteLine("刷新 Token");
            renderActorCTS = new CancellationTokenSource();
            renderActorCTS.Token.Register(() => { Console.WriteLine("取消加载演员页码的任务"); });
            renderVideoCT = renderActorCTS.Token;
        }



        #region "界面显示属性"

        private SolidColorBrush _WebStatusBackground = System.Windows.Media.Brushes.Red;

        public SolidColorBrush WebStatusBackground
        {
            get { return _WebStatusBackground; }
            set
            {
                _WebStatusBackground = value;
                RaisePropertyChanged();
            }
        }


        private Visibility _ShowFirstRun = Visibility.Collapsed;

        public Visibility ShowFirstRun
        {
            get { return _ShowFirstRun; }
            set
            {
                _ShowFirstRun = value;
                RaisePropertyChanged();
            }
        }


        private Visibility _ActorInfoGrid = Visibility.Collapsed;

        public Visibility ActorInfoGrid
        {
            get { return _ActorInfoGrid; }
            set
            {
                _ActorInfoGrid = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _GoToTopCanvas = Visibility.Collapsed;

        public Visibility GoToTopCanvas
        {
            get { return _GoToTopCanvas; }
            set
            {
                _GoToTopCanvas = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _ProgressBarVisibility = Visibility.Collapsed;

        public Visibility ProgressBarVisibility
        {
            get { return _ProgressBarVisibility; }
            set
            {
                _ProgressBarVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _ActorProgressBarVisibility = Visibility.Collapsed;

        public Visibility ActorProgressBarVisibility
        {
            get { return _ActorProgressBarVisibility; }
            set
            {
                _ActorProgressBarVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _CmdVisibility = Visibility.Collapsed;

        public Visibility CmdVisibility
        {
            get { return _CmdVisibility; }
            set
            {
                _CmdVisibility = value;
                RaisePropertyChanged();
            }
        }

        private string _CmdText = "";

        public string CmdText
        {
            get { return _CmdText; }
            set
            {
                _CmdText = value;
                RaisePropertyChanged();
            }
        }

        private int _ActorProgressBarValue = 0;

        public int ActorProgressBarValue
        {
            get { return _ActorProgressBarValue; }
            set
            {
                _ActorProgressBarValue = value;
                RaisePropertyChanged();
            }
        }


        private int _ProgressBarValue = 0;

        public int ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set
            {
                _ProgressBarValue = value;
                RaisePropertyChanged();
            }
        }




        private bool _ShowSearchPopup = false;

        public bool ShowSearchPopup
        {
            get { return _ShowSearchPopup; }
            set
            {
                _ShowSearchPopup = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowMoreListBtn = false;

        public bool ShowMoreListBtn
        {
            get { return _ShowMoreListBtn; }
            set
            {
                _ShowMoreListBtn = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowActorTools = false;

        public bool ShowActorTools
        {
            get { return _ShowActorTools; }
            set
            {
                _ShowActorTools = value;
                RaisePropertyChanged();
            }
        }


        private BitmapSource _BackgroundImage = GlobalVariable.BackgroundImage;

        public BitmapSource BackgroundImage
        {
            get { return _BackgroundImage; }
            set
            {
                _BackgroundImage = value;
                RaisePropertyChanged();
            }
        }

        private int _TabSelectedIndex = 0;

        public int TabSelectedIndex
        {
            get { return _TabSelectedIndex; }
            set
            {
                _TabSelectedIndex = value;
                RaisePropertyChanged();
            }
        }



        private bool _IsLoadingFilter = true;

        public bool IsLoadingFilter
        {
            get { return _IsLoadingFilter; }
            set
            {
                _IsLoadingFilter = value;
                RaisePropertyChanged();
            }
        }


        private bool _IsLoadingMovie = true;

        public bool IsLoadingMovie
        {
            get { return _IsLoadingMovie; }
            set
            {
                _IsLoadingMovie = value;
                RaisePropertyChanged();
            }
        }


        private bool _IsLoadingClassify = false;

        public bool IsLoadingClassify
        {
            get { return _IsLoadingClassify; }
            set
            {
                _IsLoadingClassify = value;
                RaisePropertyChanged();
            }
        }

        private bool _HideToIcon = false;

        public bool HideToIcon
        {
            get { return _HideToIcon; }
            set
            {
                _HideToIcon = value;
                RaisePropertyChanged();
            }
        }


        private Thickness _MainGridThickness = new Thickness(10);

        public Thickness MainGridThickness
        {
            get { return _MainGridThickness; }
            set
            {
                _MainGridThickness = value;
                RaisePropertyChanged();
            }
        }


        #endregion





        #region "enum"
        private VedioType _ClassifyVedioType = 0;
        public VedioType ClassifyVedioType
        {
            get { return _ClassifyVedioType; }
            set
            {
                _ClassifyVedioType = value;
                RaisePropertyChanged();
            }
        }



        private MyImageType _ShowImageMode = 0;

        public MyImageType ShowImageMode
        {
            get { return _ShowImageMode; }
            set
            {
                _ShowImageMode = value;
                RaisePropertyChanged();
                Properties.Settings.Default.ShowImageMode = value.ToString();
            }
        }



        private ViewType _ShowViewMode = 0;

        public ViewType ShowViewMode
        {
            get { return _ShowViewMode; }
            set
            {
                _ShowViewMode = value;
                RaisePropertyChanged();
            }
        }




        private MySearchType _AllSearchType = 0;

        public MySearchType AllSearchType
        {
            get { return _AllSearchType; }
            set
            {
                _AllSearchType = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "ObservableCollection"
        private ObservableCollection<char> _LettersNavigation;
        public ObservableCollection<char> LettersNavigation
        {
            get { return _LettersNavigation; }
            set
            {
                _LettersNavigation = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<MyListItem> _MyList;


        public ObservableCollection<MyListItem> MyList
        {
            get { return _MyList; }
            set
            {
                _MyList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<AppDatabase> _DataBases;


        public ObservableCollection<AppDatabase> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Video> _VideoList;
        public ObservableCollection<Video> VideoList
        {
            get { return _VideoList; }
            set
            {
                _VideoList = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<Video> _CurrentVideoList;
        public ObservableCollection<Video> CurrentVideoList
        {
            get { return _CurrentVideoList; }
            set
            {
                _CurrentVideoList = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Movie> _DetailsDataList;


        public ObservableCollection<Movie> DetailsDataList
        {
            get { return _DetailsDataList; }
            set
            {
                _DetailsDataList = value;
                RaisePropertyChanged();
            }
        }




        private List<Video> _SelectedVideo = new List<Video>();

        public List<Video> SelectedVideo
        {
            get { return _SelectedVideo; }
            set
            {
                _SelectedVideo = value;
                RaisePropertyChanged();
            }
        }

        private List<ActorInfo> _SelectedActors = new List<ActorInfo>();

        public List<ActorInfo> SelectedActors
        {
            get { return _SelectedActors; }
            set
            {
                _SelectedActors = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<TagStamp> _TagStamps = new ObservableCollection<TagStamp>();

        public ObservableCollection<TagStamp> TagStamps
        {
            get { return _TagStamps; }
            set
            {
                _TagStamps = value;
                RaisePropertyChanged();
            }
        }


        public List<Movie> MovieList;

        public List<Movie> FilterMovieList;

        private ObservableCollection<Genre> genrelist;
        public ObservableCollection<Genre> GenreList
        {
            get { return genrelist; }
            set
            {
                genrelist = value;
                RaisePropertyChanged();

            }
        }


        private List<ActorInfo> actorlist;
        public List<ActorInfo> ActorList
        {
            get { return actorlist; }
            set
            {
                actorlist = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<ActorInfo> _CurrentActorList;


        public ObservableCollection<ActorInfo> CurrentActorList
        {
            get { return _CurrentActorList; }
            set
            {
                _CurrentActorList = value;
                RaisePropertyChanged();
                CurrentActorListHideOrChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<string> labellist;
        public ObservableCollection<string> LabelList
        {
            get { return labellist; }
            set
            {
                labellist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> tagllist;
        public ObservableCollection<string> TagList
        {
            get { return tagllist; }
            set
            {
                tagllist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> studiollist;
        public ObservableCollection<string> StudioList
        {
            get { return studiollist; }
            set
            {
                studiollist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> directorList
;
        public ObservableCollection<string> DirectorList

        {
            get { return directorList; }
            set
            {
                directorList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _AllSearchCandidate;


        public ObservableCollection<string> AllSearchCandidate
        {
            get { return _AllSearchCandidate; }
            set
            {
                _AllSearchCandidate = value;
                RaisePropertyChanged();

            }
        }


        private ObservableCollection<string> _FilePathClassification;


        public ObservableCollection<string> FilePathClassification
        {
            get { return _FilePathClassification; }
            set
            {
                _FilePathClassification = value;
                RaisePropertyChanged();

            }
        }

        private ObservableCollection<string> _SearchHistory;


        public ObservableCollection<string> SearchHistory
        {
            get { return _SearchHistory; }
            set
            {
                _SearchHistory = value;
                RaisePropertyChanged();

            }
        }


        private ObservableCollection<string> _CurrentSearchCandidate;


        public ObservableCollection<string> CurrentSearchCandidate
        {
            get { return _CurrentSearchCandidate; }
            set
            {
                _CurrentSearchCandidate = value;
                RaisePropertyChanged();

            }
        }



        private ObservableCollection<Movie> _SearchCandidate;


        public ObservableCollection<Movie> SearchCandidate
        {
            get { return _SearchCandidate; }
            set
            {
                _SearchCandidate = value;
                RaisePropertyChanged();

            }
        }

        #endregion


        #region "Variable"




        private List<string> _CurrentMovieLabelList;

        public List<string> CurrentMovieLabelList
        {
            get { return _CurrentMovieLabelList; }
            set
            {
                _CurrentMovieLabelList = value;
                RaisePropertyChanged();

            }
        }


        private int _ShowStampType = 0;

        public int ShowStampType
        {
            get { return _ShowStampType; }
            set
            {
                _ShowStampType = value;
                RaisePropertyChanged();
            }
        }




        private Sort _SortType = 0;
        public Sort SortType
        {
            get { return _SortType; }
            set
            {
                _SortType = value;
                RaisePropertyChanged();
            }
        }



        private double _VedioTypeACount = 0;
        public double VedioTypeACount
        {
            get { return _VedioTypeACount; }
            set
            {
                _VedioTypeACount = value;
                RaisePropertyChanged();
            }
        }



        private double _VedioTypeBCount = 0;
        public double VedioTypeBCount
        {
            get { return _VedioTypeBCount; }
            set
            {
                _VedioTypeBCount = value;
                RaisePropertyChanged();
            }
        }



        private double _VedioTypeCCount = 0;
        public double VedioTypeCCount
        {
            get { return _VedioTypeCCount; }
            set
            {
                _VedioTypeCCount = value;
                RaisePropertyChanged();
            }
        }

        private double _RecentWatchedCount = 0;
        public double RecentWatchedCount
        {
            get { return _RecentWatchedCount; }
            set
            {
                _RecentWatchedCount = value;
                RaisePropertyChanged();
            }
        }


        private long _AllVideoCount = 0;
        public long AllVideoCount
        {
            get { return _AllVideoCount; }
            set
            {
                _AllVideoCount = value;
                RaisePropertyChanged();
            }
        }

        private double _FavoriteVideoCount = 0;
        public double FavoriteVideoCount
        {
            get { return _FavoriteVideoCount; }
            set
            {
                _FavoriteVideoCount = value;
                RaisePropertyChanged();
            }
        }

        private long _RecentWatchCount = 0;
        public long RecentWatchCount
        {
            get { return _RecentWatchCount; }
            set
            {
                _RecentWatchCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllActorCount = 0;
        public long AllActorCount
        {
            get { return _AllActorCount; }
            set
            {
                _AllActorCount = value;
                RaisePropertyChanged();
            }
        }

        private long _AllLabelCount = 0;
        public long AllLabelCount
        {
            get { return _AllLabelCount; }
            set
            {
                _AllLabelCount = value;
                RaisePropertyChanged();
            }
        }


        public bool _IsScanning = false;
        public bool IsScanning
        {
            get { return _IsScanning; }
            set
            {
                _IsScanning = value;
                RaisePropertyChanged();
            }
        }


        public bool _EnableEditActress = false;

        public bool EnableEditActress
        {
            get { return _EnableEditActress; }
            set
            {
                _EnableEditActress = value;
                RaisePropertyChanged();
            }
        }


        public string movieCount = "总计 0 个";


        private int _CurrentPage = 1;
        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                _CurrentPage = value;
                //FlowNum = 0;
                RaisePropertyChanged();
            }
        }


        private int _PageSize = 10;
        public int PageSize
        {
            get { return _PageSize; }
            set
            {
                _PageSize = value;
                RaisePropertyChanged();
            }
        }


        public int _CurrentCount = 0;
        public int CurrentCount
        {
            get { return _CurrentCount; }
            set
            {
                _CurrentCount = value;
                RaisePropertyChanged();

            }
        }

        public int _CurrentActorCount = 0;
        public int CurrentActorCount
        {
            get { return _CurrentActorCount; }
            set
            {
                _CurrentActorCount = value;
                RaisePropertyChanged();

            }
        }


        public long _TotalCount = 0;
        public long TotalCount
        {
            get { return _TotalCount; }
            set
            {
                _TotalCount = value;
                RaisePropertyChanged();

            }
        }

        public int totalpage = 1;
        public int TotalPage
        {
            get { return totalpage; }
            set
            {
                totalpage = value;
                RaisePropertyChanged();

            }
        }


        private int _ActorPageSize = 10;
        public int ActorPageSize
        {
            get { return _ActorPageSize; }
            set
            {
                _ActorPageSize = value;
                RaisePropertyChanged();
            }
        }

        private long _ActorCurrentCount = 0;
        public long ActorCurrentCount
        {
            get { return _ActorCurrentCount; }
            set
            {
                _ActorCurrentCount = value;
                RaisePropertyChanged();

            }
        }


        private long _ActorTotalCount = 0;
        public long ActorTotalCount
        {
            get { return _ActorTotalCount; }
            set
            {
                _ActorTotalCount = value;
                RaisePropertyChanged();

            }
        }


        private int currentactorpage = 1;
        public int CurrentActorPage
        {
            get { return currentactorpage; }
            set
            {
                currentactorpage = value;
                FlowNum = 0;
                RaisePropertyChanged();
            }
        }


        private long totalactorpage = 1;
        public long TotalActorPage
        {
            get { return totalactorpage; }
            set
            {
                totalactorpage = value;
                RaisePropertyChanged();
            }
        }






        private int _FlowNum = 0;
        public int FlowNum
        {
            get { return _FlowNum; }
            set
            {
                _FlowNum = value;
                RaisePropertyChanged();
            }
        }








        public string textType = Jvedio.Language.Resources.AllVideo;

        public string TextType
        {
            get { return textType; }
            set
            {
                textType = value;
                RaisePropertyChanged();
            }
        }

        public int ClickGridType { get; set; }

        private bool _SearchFirstLetter = false;


        public bool SearchFirstLetter
        {
            get { return _SearchFirstLetter; }
            set
            {
                _SearchFirstLetter = value;
                RaisePropertyChanged();
            }
        }

        private string search = string.Empty;


        public string Search
        {
            get { return search; }
            set
            {
                search = value;
                RaisePropertyChanged();
                BeginSearch();
            }
        }

        private string _SearchHint = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.ID;


        public string SearchHint
        {
            get { return _SearchHint; }
            set
            {
                _SearchHint = value;
                RaisePropertyChanged();
            }
        }


        private Actress actress;
        public Actress Actress
        {
            get { return actress; }
            set
            {
                actress = value;
                RaisePropertyChanged();
            }
        }

        private bool showSideBar = false;

        public bool ShowSideBar
        {
            get { return showSideBar; }
            set
            {
                showSideBar = value;
                RaisePropertyChanged();
            }
        }



        private bool Checkingurl = false;

        public bool CheckingUrl
        {
            get { return Checkingurl; }
            set
            {
                Checkingurl = value;
                RaisePropertyChanged();
            }
        }

        private bool searchAll = true;

        public bool SearchAll
        {
            get { return searchAll; }
            set
            {
                searchAll = value;
            }
        }


        private bool searchInCurrent = false;

        public bool SearchInCurrent
        {
            get { return searchInCurrent; }
            set
            {
                searchInCurrent = value;
            }
        }

        #endregion



        #region "筛选"

        private ObservableCollection<string> _Year;

        public ObservableCollection<string> Year
        {
            get { return _Year; }
            set
            {
                _Year = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _Genre;

        public ObservableCollection<string> Genre
        {
            get { return _Genre; }
            set
            {
                _Genre = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _Actor;

        public ObservableCollection<string> Actor
        {
            get { return _Actor; }
            set
            {
                _Actor = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _Label;

        public ObservableCollection<string> Label
        {
            get { return _Label; }
            set
            {
                _Label = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _Runtime;

        public ObservableCollection<string> Runtime
        {
            get { return _Runtime; }
            set
            {
                _Runtime = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<string> _FileSize;

        public ObservableCollection<string> FileSize
        {
            get { return _FileSize; }
            set
            {
                _FileSize = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<string> _Rating;

        public ObservableCollection<string> Rating
        {
            get { return _Rating; }
            set
            {
                _Rating = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsRefresh = false;

        public bool IsRefresh
        {
            get { return _IsRefresh; }
            set
            {
                _IsRefresh = value;
                RaisePropertyChanged();
            }
        }

        public List<List<string>> Filters;

        public async void GetFilterInfo()
        {
            IsLoadingFilter = true;
            Year = new ObservableCollection<string>();
            Genre = new ObservableCollection<string>();
            Actor = new ObservableCollection<string>();
            Label = new ObservableCollection<string>();
            Runtime = new ObservableCollection<string>();
            FileSize = new ObservableCollection<string>();
            Rating = new ObservableCollection<string>();
            Filters = await DataBase.GetAllFilter();
            if (Filters == null) return;
            Year.AddRange(Filters[0]);
            Genre.AddRange(Filters[1].Take(30));
            Actor.AddRange(Filters[2].Take(30));
            Label.AddRange(Filters[3]);
            Runtime.AddRange(Filters[4]);
            FileSize.AddRange(Filters[5]);
            Rating.AddRange(Filters[6]);
            IsLoadingFilter = false;




            main.GenreItemsControl.ItemsSource = null;
            main.GenreItemsControl.ItemsSource = Genre;

            main.ActorFilterItemsControl.ItemsSource = null;
            main.ActorFilterItemsControl.ItemsSource = Actor;

            main.LabelFilterItemsControl.ItemsSource = null;
            main.LabelFilterItemsControl.ItemsSource = Label;


            //他妈的必须要强制指定，Expander 搞毛啊
            main.SideFilterYear.ItemsSource = null;
            main.SideFilterYear.ItemsSource = Year.OrderByDescending(arg => arg).ToList();

            main.SideFilterFileSize.ItemsSource = null;
            main.SideFilterFileSize.ItemsSource = FileSize;


            main.SideFilterLabel.ItemsSource = null;
            main.SideFilterLabel.ItemsSource = Label;

            main.SideFilterRating.ItemsSource = null;
            main.SideFilterRating.ItemsSource = Rating;

            main.SideFilterRuntime.ItemsSource = null;
            main.SideFilterRuntime.ItemsSource = Runtime;


        }



        #endregion

        public void LoadData()
        {
            Select();
        }
        public void LoadActor()
        {
            SelectActor();
        }


        public void Reset()
        {
            Select();
        }


        public void ShowAllActors(object o)
        {
            TabSelectedIndex = 1;
            SelectActor();
        }

        public void ShowAllLabels(object o)
        {
            TabSelectedIndex = 2;
            GetLabelList();
        }



        public async Task<bool> InitLettersNavigation()
        {
            //LettersNavigation = new ObservableCollection<char>();
            //List<char> _temp = new List<char>();
            //var movies = DataBase.SelectAllID();
            //if (movies == null || movies.Count == 0) return false;
            //foreach (var item in movies)
            //{

            //    if (item.Length >= 1)
            //    {
            //        char firstchar = item.ToUpper()[0];
            //        if (!_temp.Contains(firstchar)) _temp.Add(firstchar);
            //    }
            //}

            //foreach (var item in _temp.OrderBy(arg => arg))
            //{
            //    LettersNavigation.Add(item);
            //}
            return true;
        }


        public async void BeginSearch()
        {
            //GetSearchCandidate(Search.ToProperSql());
            await Query();
            FlipOver();
        }

        public void LoadDataBaseList()
        {
            //DataBases = new ObservableCollection<string>();
            //string scanDir = Path.Combine(GlobalVariable.DataPath, GlobalVariable.CurrentInfoType.ToString());
            //List<string> files = FileHelper.TryScanDIr(scanDir, "*.sqlite", SearchOption.TopDirectoryOnly).ToList();
            //files.ForEach(arg => DataBases.Add(Path.GetFileNameWithoutExtension(arg).ToLower()));

        }







        private void AddSingleMovie()
        {
            Dialog_NewMovie dialog_NewMovie = new Dialog_NewMovie((Main)GetWindowByName("Main"));
            var b = (bool)dialog_NewMovie.ShowDialog();
            NewMovieDialogResult result = dialog_NewMovie.Result;

            if (b && !string.IsNullOrEmpty(result.Text))
            {
                List<string> IDList = GetIDListFromString(result.Text, result.VedioType);
                foreach (var item in IDList)
                {
                    InsertID(item, result.VedioType);
                }
            }


        }

        private void InsertID(string id, VedioType vedioType)
        {
            //Movie movie = DataBase.SelectMovieByID(id);
            //if (movie != null)
            //{
            //    HandyControl.Controls.Growl.Info($"{id} {Jvedio.Language.Resources.Message_AlreadyExist}", "Main");
            //}
            //else
            //{
            //    Movie movie1 = new Movie()
            //    {
            //        id = id,
            //        vediotype = (int)vedioType,
            //        releasedate = "1900-01-01",
            //        otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            //    };
            //    DataBase.InsertScanMovie(movie1);
            //    MovieList.Insert(0, movie1);
            //    CurrentMovieList.Insert(0, movie1);
            //    FilterMovieList.Insert(0, movie1);
            //}
        }


        public List<string> GetIDListFromString(string str, VedioType vedioType)
        {
            List<string> result = new List<string>();
            foreach (var item in str.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None))
            {
                string id = item;
                if (AutoAddPrefix && Prefix != "")
                    id = Prefix + id;


                if (vedioType == VedioType.欧美)
                    id = id.Replace(" ", "");
                else
                    id = id.ToUpper().Replace(" ", "");



                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) result.Add(id);
            }
            return result;
        }

        private delegate void LoadSearchDelegate(string str);
        private void LoadSearch(string str)
        {
            if (!CurrentSearchCandidate.Contains(str)) CurrentSearchCandidate.Add(str);
        }




        public async Task<bool> GetSearchCandidate(string Search)
        {
            return await Task.Run(async () =>
            {

                CurrentSearchCandidate = new ObservableCollection<string>();
                if (Search == "") return false;

                //提取出英文和数字
                string extraSearch = "";
                string number = Identify.GetNum(Search);
                string eng = Identify.GetEng(Search);

                Search = Search.Replace("%", "").Replace("'", "");
                if (!string.IsNullOrEmpty(number)) extraSearch = eng + "-" + number;
                List<Movie> movies = new List<Movie>();
                if (AllSearchType == MySearchType.名称)
                {
                    if (SearchInCurrent)
                        movies = MovieList.Where(m => m.title.ToUpper().Contains(Search.ToUpper())).ToList();
                    else
                        movies = DataBase.SelectMoviesBySql($"SELECT * from movie where title like '%{Search}%'");

                    foreach (Movie movie in movies)
                    {
                        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadSearchDelegate(LoadSearch), movie.title);
                        if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                    }
                }
                else if (AllSearchType == MySearchType.演员)
                {
                    if (SearchInCurrent)
                        movies = MovieList.Where(m => m.actor.ToUpper().Contains(Search.ToUpper())).ToList();
                    else
                        movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{Search}%'");

                    foreach (Movie movie in movies)
                    {
                        string[] actor = movie.actor.Split(actorSplitDict[movie.vediotype]);
                        foreach (var item in actor)
                        {
                            if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            {
                                if (!CurrentSearchCandidate.Contains(item) & item.ToUpper().IndexOf(Search.ToUpper()) >= 0)
                                {
                                    await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadSearchDelegate(LoadSearch), item);
                                }
                                if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                            }
                        }
                        if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                    }
                }
                else if (AllSearchType == MySearchType.识别码)
                {
                    if (SearchInCurrent)
                        movies = MovieList.Where(m => m.id.ToUpper().Contains(Search.ToUpper())).ToList();
                    else
                        movies = DataBase.SelectMoviesBySql($"SELECT * from movie where id like '%{Search}%'");

                    if (movies.Count == 0 && extraSearch != "") movies = MovieList.Where(m => m.id.ToUpper().Contains(extraSearch.ToUpper())).ToList();
                    foreach (Movie movie in movies)
                    {
                        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadSearchDelegate(LoadSearch), movie.id);
                        if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                    }
                }
                return true;

            });
        }


        public void SaveSearchHistory()
        {

            try
            {
                if (SearchHistory.Count <= 0)
                {
                    File.Delete("SearchHistory");
                    main.SearchHistoryStackPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter("SearchHistory"))
                    {
                        sw.Write(string.Join("'", SearchHistory));
                    }
                    main.SearchHistoryStackPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }

        public void LoadSearchHistory()
        {
            SearchHistory = new ObservableCollection<string>();
            if (!File.Exists("SearchHistory")) return;
            string content = "";
            try
            {
                using (StreamReader sr = new StreamReader("SearchHistory"))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }

            if (content != "" && content.IndexOf("'") >= 0)
            {
                foreach (var item in content.Split('\''))
                {
                    if (!SearchHistory.Contains(item) && !string.IsNullOrEmpty(item)) SearchHistory.Add(item);
                }
            }
            else if (content.Length > 0)
            {
                SearchHistory.Add(content);
            }

        }



        /// <summary>
        /// 流动加载影片
        /// </summary>
        public void Flow()
        {
            //if (FilterMovieList == null) return;
            //App.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    main.SetLoadingStatus(true);
            //});

            //CurrentMovieListHideOrChanged?.Invoke(this, EventArgs.Empty); //停止下载
            //int DisPlayNum = Properties.Settings.Default.DisplayNumber;//每页展示数目
            //int SetFlowNum = Properties.Settings.Default.DisplayNumber;//流动数目
            //Movies = new List<Movie>();
            //int min = (CurrentPage - 1) * DisPlayNum + FlowNum * SetFlowNum;
            //int max = (CurrentPage - 1) * DisPlayNum + (FlowNum + 1) * SetFlowNum;
            //for (int i = min; i < max; i++)
            //{
            //    if (CurrentMovieList.Count + Movies.Count < DisPlayNum)
            //    {
            //        if (i <= FilterMovieList.Count - 1)
            //        {
            //            Movie movie = FilterMovieList[i];
            //            //添加标签戳
            //            FileProcess.addTag(ref movie);

            //            if (!string.IsNullOrEmpty(movie.id)) Movies.Add(movie);
            //        }
            //        else { break; }
            //    }
            //    else
            //    {
            //        FlowNum = 0;
            //    }

            //}

            //foreach (Movie item in Movies)
            //{
            //    Movie movie = item;
            //    if (!Properties.Settings.Default.EasyMode) SetImage(ref movie);
            //    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadMovie), movie);
            //}

            //App.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    if (GetWindowByName("Main") is Main main)
            //    {
            //        MovieFlipOverCompleted?.Invoke(this, EventArgs.Empty);
            //    }

            //});


        }


        public async Task<bool> ClearCurrentMovieList()
        {
            //if (CurrentMovieList == null) CurrentMovieList = new BindingList<Movie>();

            //await Task.Run(() => {
            //    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate {
            //        Main main = (Main)GetWindowByName("Main");
            //        //main.MovieItemsControl.ItemsSource = null;
            //    });
            //});



            //await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
            //{
            //    //CurrentMovieList.RaiseListChangedEvents = false;
            //    for (int i = CurrentMovieList.Count - 1; i >= 0; i--)
            //    {
            //        CurrentMovieList[i].bigimage = null;
            //        CurrentMovieList[i].smallimage = null;
            //    }
            //});





            //await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
            //{
            //    CurrentMovieList.Clear();
            //    CurrentMovieList = new ObservableCollection<Movie>();
            //    GC.Collect();
            //    if (main.ImageSlides != null)
            //    {
            //        for (int i = 0; i < main.ImageSlides.Count; i++)
            //        {
            //            main.ImageSlides[i].Stop();
            //        }
            //        main.ImageSlides.Clear();
            //    }
            //    //if (Properties.Settings.Default.EasyMode)
            //    //    main.SimpleMovieItemsControl.ItemsSource = CurrentMovieList;
            //    //else
            //    main.MovieItemsControl.ItemsSource = CurrentMovieList;
            //});

            return true;
        }










        private delegate void LoadActorDelegate(ActorInfo actor, int idx);
        private void LoadActor(ActorInfo actor, int idx)
        {
            if (renderActorCTS.IsCancellationRequested) return;
            if (CurrentActorList.Count < ActorPageSize)
                CurrentActorList.Add(actor);
            else
            {
                CurrentActorList[idx] = null;
                CurrentActorList[idx] = actor;
            }
            CurrentActorCount = CurrentActorList.Count;
        }






        public List<Movie> Movies;

        /// <summary>
        /// 翻页：加载图片以及其他
        /// </summary>
        public bool FlipOver(int page = -1)
        {
            // TabSelectedIndex = 0;
            // if (MovieList == null) return false;
            // App.Current.Dispatcher.Invoke((Action)delegate
            // {
            //     main.SetLoadingStatus(true);//正在加载影片
            //     main.MovieScrollViewer.ScrollToTop();//滚到顶部
            // });

            // if (!Properties.Settings.Default.RandomDisplay) Sort(); //随机展示不排序，否则排序

            // int number = 0;
            // if (FilterMovieList != null) number = FilterMovieList.Count;
            // FilterMovieList = FileProcess.FilterMovie(MovieList);   //筛选影片
            // if (page <= 0 && FilterMovieList.Count < number) CurrentPage = 1;                // FilterMovieList 如果改变了，则回到第一页
            // if (page > 0) CurrentPage = page;
            // Task.Run(async () =>
            //{

            //    await ClearCurrentMovieList();//动态清除当前影片

            //    TotalPage = (int)Math.Ceiling((double)FilterMovieList.Count / Properties.Settings.Default.DisplayNumber);
            //    int DisPlayNum = Properties.Settings.Default.DisplayNumber;
            //    int FlowNum = Properties.Settings.Default.DisplayNumber;

            //    Movies = new List<Movie>();
            //    //从 FilterMovieList 中添加影片到 临时 Movies 中
            //    for (int i = (CurrentPage - 1) * DisPlayNum; i < (CurrentPage - 1) * DisPlayNum + FlowNum; i++)
            //    {
            //        if (i <= FilterMovieList.Count - 1)
            //        {
            //            Movie movie = FilterMovieList[i];
            //            Movies.Add(movie);
            //        }
            //        else { break; }
            //        if (Movies.Count == FlowNum) { break; }

            //    }

            //    //添加标签戳
            //    for (int i = 0; i < Movies.Count; i++)
            //    {
            //        if (Identify.IsHDV(Movies[i].filepath) || Movies[i].genre?.IndexOfAnyString(TagStrings_HD) >= 0 || Movies[i].tag?.IndexOfAnyString(TagStrings_HD) >= 0 || Movies[i].label?.IndexOfAnyString(TagStrings_HD) >= 0) Movies[i].tagstamps += Jvedio.Language.Resources.HD;
            //        if (Identify.IsCHS(Movies[i].filepath) || Movies[i].genre?.IndexOfAnyString(TagStrings_Translated) >= 0 || Movies[i].tag?.IndexOfAnyString(TagStrings_Translated) >= 0 || Movies[i].label?.IndexOfAnyString(TagStrings_Translated) >= 0) Movies[i].tagstamps += Jvedio.Language.Resources.Translated;
            //        if (Identify.IsFlowOut(Movies[i].filepath) || Movies[i].genre?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || Movies[i].tag?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || Movies[i].label?.IndexOfAnyString(TagStrings_FlowOut) >= 0) Movies[i].tagstamps += Jvedio.Language.Resources.FlowOut;
            //    }

            //    //根据标签戳筛选
            //    if (ShowStampType >= 1)
            //        Movies = Movies.Where(arg => arg.tagstamps.IndexOf(ShowStampType.ToString().ToTagString()) >= 0).ToList();




            //    foreach (Movie item in Movies)
            //    {
            //        Movie movie = item;
            //        if (!Properties.Settings.Default.EasyMode) SetImage(ref movie);
            //        await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadMovie), movie);
            //    }

            //    await App.Current.Dispatcher.BeginInvoke((Action)delegate
            //     {
            //         MovieFlipOverCompleted?.Invoke(this, EventArgs.Empty);
            //     });

            //});
            return true;
        }

        public void SetClassifyLoadingStatus(bool loading)
        {
            IsLoadingClassify = loading;
            //IsLoadingMovie = loading;
        }



        private delegate void LoadVideoDelegate(Video video, int idx);
        private void LoadVideo(Video video, int idx)
        {
            if (renderVideoCT.IsCancellationRequested) return;
            if (CurrentVideoList.Count < PageSize)
                CurrentVideoList.Add(video);
            else
            {
                CurrentVideoList[idx] = null;
                CurrentVideoList[idx] = video;
            }
            CurrentCount = CurrentVideoList.Count;
            //Console.WriteLine($"渲染第 {CurrentPage} 页的数据");
        }



        private delegate void LoadLabelDelegate(string str);
        private void LoadLabel(string str) => LabelList.Add(str);
        private void LoadTag(string str) => TagList.Add(str);
        private void LoadStudio(string str) => StudioList.Add(str);
        private void LoadDirector(string str) => DirectorList.Add(str);

        //获得标签
        public async void GetLabelList()
        {
            List<string> labels = new List<string>();
            string sql = "SELECT LabelName,Count(LabelName) as Count  from metadata_to_label GROUP BY LabelName";
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            foreach (Dictionary<string, object> item in list)
            {
                string LabelName = item["LabelName"].ToString();
                long.TryParse(item["Count"].ToString(), out long count);
                labels.Add($"{LabelName}({count})");
            }
            LabelList = new ObservableCollection<string>();
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadLabel), labels[i]);
            }
        }





        public async void GetTagList()
        {
            List<string> labels = DataBase.SelectLabelByVedioType(ClassifyVedioType, "tag");
            TagList = new ObservableCollection<string>();
            SetClassifyLoadingStatus(true);
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadTag), labels[i]);
            }
            SetClassifyLoadingStatus(false);
        }



        public async void GetStudioList()
        {
            List<string> labels = DataBase.SelectLabelByVedioType(ClassifyVedioType, "studio");
            StudioList = new ObservableCollection<string>();
            SetClassifyLoadingStatus(true);
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadStudio), labels[i]);
            }
            SetClassifyLoadingStatus(false);
        }


        public async void GetDirectoroList()
        {
            List<string> labels = DataBase.SelectLabelByVedioType(ClassifyVedioType, "director");
            DirectorList = new ObservableCollection<string>();
            SetClassifyLoadingStatus(true);
            for (int i = 0; i < labels.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadLabelDelegate(LoadDirector), labels[i]);
            }
            SetClassifyLoadingStatus(false);
        }


        //获得演员，信息照片都获取

        #region "演员 => 翻页" 
        public static Dictionary<int, string> ActorSortDict = new Dictionary<int, string>()
        {
            { 0, "actor_info.ActorName" },
            { 1, "Count" },
            { 2, "actor_info.Country" },
            { 3, "Nation" },
            { 4, "BirthPlace" },
            { 5, "Birthday" },
            { 6, "BloodType" },
            { 7, "Height" },
            { 8, "Weight" },
            { 9, "Gender" },
            { 10, "Hobby" },
            { 11, "Cup" },
            { 12, "Chest" },
            { 13, "Waist" },
            { 14, "Hipline" },
            { 15, "actor_info.Grade" },
        };

        public static string[] ActorSelectedField = new string[]
        {
            "count(ActorName) as Count",
            "actor_info.ActorID",
            "actor_info.ActorName",
            "actor_info.Country",
            "Nation",
            "BirthPlace",
            "Birthday",
            "BloodType",
            "Height",
            "Weight",
            "Gender",
            "Hobby",
            "Cup",
            "Chest",
            "Waist",
            "Hipline",
            "WebType",
            "WebUrl",
            "actor_info.Grade",
            "SmallImagePath",
            "PreviewImagePath",
            "actor_info.ExtraInfo",
            "actor_info.CreateDate",
            "actor_info.UpdateDate",
        };

        public void ActorSetActorSortOrder<T>(IWrapper<T> wrapper)
        {
            if (wrapper == null) return;
            string sortField = ActorSortDict[Properties.Settings.Default.ActorSortType];
            if (Properties.Settings.Default.ActorSortDescending) wrapper.Desc(sortField);
            else wrapper.Asc(sortField);
        }


        public string ActorToLimit()
        {

            int row_count = ActorPageSize;
            long offset = ActorPageSize * (CurrentActorPage - 1);
            return $" LIMIT {offset},{row_count}";
        }

        public static Dictionary<string, string> Actor_SELECT_TYPE = new Dictionary<string, string>() {
            { "All","  "  },
            { "Favorite","  "  },
        };



        public async void SelectActor(object o = null)
        {
            TabSelectedIndex = 1; // 演员
            // 判断当前获取的队列
            while (ActorPageQueue.Count > 1)
            {
                int page = ActorPageQueue.Dequeue();
                Console.WriteLine("跳过该页码 ： " + page);
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (renderingActor)
            {
                //if (rendering && !renderVideoCTS.IsCancellationRequested)
                renderActorCTS?.Cancel();// 取消加载
                await Task.Delay(100);
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                //main.SetLoadingStatus(true);//正在加载影片
                main.ActorScrollViewer.ScrollToTop();//滚到顶部
            });

            SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
            ActorSetActorSortOrder(wrapper);

            string count_sql = "SELECT count(*) as Count " +
                         "from (SELECT actor_info.ActorID FROM actor_info join metadatas_to_actor " +
                         "on metadatas_to_actor.ActorID=actor_info.ActorID " +
                         "join metadata " +
                         "on metadatas_to_actor.DataID=metadata.DataID " +
                         $"WHERE metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                         "GROUP BY actor_info.ActorID );";

            ActorTotalCount = actorMapper.selectCount(count_sql);

            string sql = $"{wrapper.Select(ActorSelectedField).toSelect(false)} FROM actor_info " +
                $"join metadatas_to_actor on metadatas_to_actor.ActorID=actor_info.ActorID " +
                $"join metadata on metadatas_to_actor.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={GlobalConfig.Main.CurrentDBId} and metadata.DataType={0} " +
                $"GROUP BY actor_info.ActorID "
                + wrapper.toOrder() + ActorToLimit();
            // 只能手动设置页码，很奇怪
            App.Current.Dispatcher.Invoke(() => { main.actorPagination.Total = ActorTotalCount; });
            RenderCurrentActors(sql);
        }



        public bool renderingActor = false;

        public void RenderCurrentActors(string sql)
        {
            List<Dictionary<string, object>> list = actorMapper.select(sql);
            List<ActorInfo> actors = actorMapper.toEntity<ActorInfo>(list, typeof(ActorInfo).GetProperties(), false);
            ActorList = new List<ActorInfo>();
            if (actors == null || actors.Count <= 0) return;
            ActorList.AddRange(actors);
            renderActor();

        }




        public async void renderActor()
        {
            if (CurrentActorList == null) CurrentActorList = new ObservableCollection<ActorInfo>();
            for (int i = 0; i < ActorList.Count; i++)
            {
                try { renderActorCT.ThrowIfCancellationRequested(); }
                catch (OperationCanceledException ex) { renderVideoCTS?.Dispose(); break; }
                renderingActor = true;
                ActorInfo actorInfo = ActorList[i];
                //加载图片
                string smallImagePath = Video.parseImagePath(actorInfo.SmallImagePath);
                BitmapImage smallimage = ReadImageFromFile(smallImagePath);
                if (smallimage == null) smallimage = DefaultActorImage;
                actorInfo.SmallImage = smallimage;
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actorInfo, i);
            }

            if (renderActorCT.IsCancellationRequested) refreshActorRenderToken();
            renderingActor = false;
            //if (pageQueue.Count > 0) pageQueue.Dequeue();
            ActorPageChangedCompleted?.Invoke(this, null);
        }


        #endregion














        private delegate void LoadGenreDelegate(Genre genre);
        private void LoadGenre(Genre genre)
        {
            GenreList.Add(genre);
        }


        //获得类别
        public async void GetGenreList()
        {
            Statistic();
            List<Genre> Genres = DataBase.SelectGenreByVedioType(ClassifyVedioType);
            SetClassifyLoadingStatus(true);
            GenreList = new ObservableCollection<Genre>();
            for (int i = 0; i < Genres.Count; i++)
            {
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadGenreDelegate(LoadGenre), Genres[i]);
            }
            SetClassifyLoadingStatus(false);
        }



















        /// <summary>
        /// 在数据库中搜索影片
        /// </summary>
        public async Task<bool> Query()
        {
            if (!DataBase.IsTableExist("movie")) { return false; }
            if (Search == "") return false;
            string FormatSearch = Search.ToProperSql();

            if (string.IsNullOrEmpty(FormatSearch)) { return false; }



            string searchContent = FormatSearch;


            List<Movie> oldMovieList = MovieList.ToList();

            if (AllSearchType == MySearchType.识别码)
            {
                string fanhao;
                if (CurrentVedioType == VedioType.欧美)
                    fanhao = Identify.GetEuFanhao(FormatSearch);
                else
                    fanhao = Identify.GetFanhao(FormatSearch);

                if (string.IsNullOrEmpty(fanhao)) searchContent = FormatSearch;
                else searchContent = fanhao;

                TextType = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.ID + " " + searchContent;

                if (SearchInCurrent)
                    MovieList = oldMovieList.Where(arg => arg.id.IndexOf(searchContent) >= 0).ToList();
                else
                {
                    if (SearchFirstLetter)
                        MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where id like '%{searchContent}%'").Where(arg => arg.id.ToUpper()[0] == searchContent.ToUpper()[0]).ToList();
                    else
                        MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where id like '%{searchContent}%'");
                }



            }

            else if (AllSearchType == MySearchType.名称)
            {
                TextType = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.Title + " " + searchContent;
                if (SearchInCurrent)
                    MovieList = oldMovieList.Where(arg => arg.title.IndexOf(searchContent) >= 0).ToList();
                else
                    MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where title like '%{searchContent}%'");
            }

            else if (AllSearchType == MySearchType.演员)
            {
                TextType = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.Actor + " " + searchContent;
                if (SearchInCurrent)
                    MovieList = oldMovieList.Where(arg => arg.actor.IndexOf(searchContent) >= 0).ToList();
                else
                    MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where actor like '%{searchContent}%'");
            }
            if (!SearchHistory.Contains(searchContent) && !SearchFirstLetter)
            {
                SearchHistory.Add(searchContent);
                SaveSearchHistory();
            }
            return true;
        }


        public void RandomDisplay()
        {
            TextType = Jvedio.Language.Resources.ToolTip_RandomShow;
            Statistic();
            MovieList = DataBase.SelectMoviesBySql($"SELECT * FROM movie ORDER BY RANDOM() limit {Properties.Settings.Default.DisplayNumber}");
            FlipOver();
        }



        public void ExecutiveSqlCommand(int sideIndex, string textType, string sql, string dbName = "", bool istorecord = true, bool flip = true)
        {
            if (sql.Length <= 0) return;
            IsLoadingMovie = true;
            TabSelectedIndex = 0;
            Dictionary<string, string> sqlInfo = new Dictionary<string, string>
            {
                { "SideIndex", sideIndex.ToString() },
                { "TextType", textType },
                { "SqlCommand", sql }
            };

            //记录每次执行的 sql
            string[] ignoresql = new[] { " studio = ", "director =", "like" };
            bool record = TextType == Jvedio.Language.Resources.Filter;
            if (!record)
            {

                for (int i = 0; i < ignoresql.Length; i++)
                {
                    if (sql.IndexOf(ignoresql[i]) >= 0)
                    {
                        record = false;
                        break;
                    }
                    record = true;
                }
            }
            if (record && istorecord) PreviousSql = sql;


            TextType = textType;

            string viewText = "";
            int.TryParse(Properties.Settings.Default.ShowViewMode, out int vm);
            if (vm == 1)
            {
                viewText = Jvedio.Language.Resources.WithImage;
            }
            else if (vm == 2)
            {
                viewText = Jvedio.Language.Resources.NoImage;
            }

            if (vm != 0) TextType = TextType + "，" + viewText;
            if (Properties.Settings.Default.OnlyShowPlay) TextType = TextType + "，" + Jvedio.Language.Resources.Playable;
            if (Properties.Settings.Default.OnlyShowSubSection) TextType = TextType + "，" + Jvedio.Language.Resources.OnlyShowSubsection;

            Task.Run(() =>
            {
                MovieList = DataBase.SelectMoviesBySql(sql, dbName);
                Statistic();
                if (flip)
                    FlipOver();
                else
                {
                    FlipOver(PreviousPage);
                }
            });
            //InitLettersNavigation(); todo

        }


        public void Sort()
        {
            if (MovieList != null)
            {
                List<Movie> sortMovieList = new List<Movie>();
                bool SortDescending = Properties.Settings.Default.SortDescending;
                int.TryParse(Properties.Settings.Default.SortType, out int sortindex);
                switch (sortindex)
                {
                    case 0:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.id).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.id).ToList(); }
                        break;
                    case 1:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.filesize).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.filesize).ToList(); }
                        break;
                    case 2:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.scandate).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.scandate).ToList(); }
                        break;
                    case 3:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.otherinfo).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.otherinfo).ToList(); }
                        break;
                    case 4:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.favorites).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.favorites).ToList(); }
                        break;
                    case 5:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.title).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.title).ToList(); }
                        break;
                    case 6:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.visits).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.visits).ToList(); }
                        break;
                    case 7:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.releasedate).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.releasedate).ToList(); }
                        break;
                    case 8:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.rating).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.rating).ToList(); }
                        break;
                    case 9:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.runtime).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.runtime).ToList(); }
                        break;
                    case 10:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.actor.Split(new char[] { ' ', '/' })[0]).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.actor.Split(new char[] { ' ', '/' })[0]).ToList(); }
                        break;
                    default:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.id).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.id).ToList(); }
                        break;
                }
                MovieList = new List<Movie>();
                MovieList.AddRange(sortMovieList);
            }

        }

        public void AsyncFlipOver()
        {
            Task.Run(() =>
            {
                FlipOver();
            });
        }

        #region "影片 => 翻页" 
        public static Dictionary<int, string> SortDict = new Dictionary<int, string>()
        {
            { 0, "VID" },
            { 1, "Size" },
            { 2, "FirstScanDate" },
            { 3, "LastScanDate" },
            { 4, "Grade" },
            { 5, "Title" },
            { 6, "ViewCount" },
            { 7, "ReleaseDate" },
            { 8, "Rating" },
            { 9, "Duration" },
        };


        public static string[] SelectFields = {
            "metadata.DataID",
            "MVID",
            "VID",
            "Grade",
            "Title",
            "Path",
            "SubSection",
            "ImageUrls",
            "ReleaseDate",
            "GifImagePath",
            "BigImagePath",
            "SmallImagePath",
            "PreviewImagePath",
            "WebUrl",
            "WebType",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID)  as TagIDs ",
        };

        public void setSortOrder<T>(IWrapper<T> wrapper)
        {
            if (wrapper == null) return;
            int.TryParse(Properties.Settings.Default.SortType, out int sortindex);
            string sortField = SortDict[sortindex];
            if (Properties.Settings.Default.SortDescending) wrapper.Desc(sortField);
            else wrapper.Asc(sortField);
        }


        public string toLimit()
        {

            int row_count = PageSize;
            long offset = PageSize * (CurrentPage - 1);
            return $" LIMIT {offset},{row_count}";
        }

        public static Dictionary<string, string> SELECT_TYPE = new Dictionary<string, string>() {
            { "All","  "  },
            { "Favorite","  "  },
            { "RecentWatch","  "  },
        };



        public async void Select(object o = null)
        {
            TabSelectedIndex = 0; // 影片
            // 判断当前获取的队列
            while (pageQueue.Count > 1)
            {
                int page = pageQueue.Dequeue();
                Console.WriteLine("跳过该页码 ： " + page);
            }

            // 当前有视频在渲染的时候，打断渲染，等待结束
            while (rendering)
            {
                //if (rendering && !renderVideoCTS.IsCancellationRequested)
                renderVideoCTS?.Cancel();// 取消加载
                await Task.Delay(100);
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                //main.SetLoadingStatus(true);//正在加载影片
                main.MovieScrollViewer.ScrollToTop();//滚到顶部
            });

            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            if (Properties.Settings.Default.OnlyShowSubSection)
                wrapper.NotEq("SubSection", "");
            setSortOrder(wrapper);
            wrapper.Select(SelectFields).Eq("metadata.DBId", GlobalConfig.Main.CurrentDBId).Eq("metadata.DataType", 0);

            // 侧边栏参数
            if (o != null && !string.IsNullOrEmpty(o.ToString()))
            {
                switch (o.ToString())
                {
                    case "Favorite": wrapper.Gt("Grade", 0); break;
                    case "RecentWatch":
                        string date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays).Date.ToString("yyyy-MM-dd");
                        string date2 = DateTime.Now.ToString("yyyy-MM-dd");
                        wrapper.Between("ViewDate", date1, date2);
                        break;
                    default: break;
                }
            }

            string count_sql = $"select count(*) FROM metadata_video " +
                                 "JOIN metadata " +
                                 "on metadata.DataID=metadata_video.DataID " + wrapper.toWhere(false);


            TotalCount = metaDataMapper.selectCount(count_sql);

            string sql = $"{wrapper.toSelect(false)} FROM metadata_video " +
                        "JOIN metadata " +
                        "on metadata.DataID=metadata_video.DataID " + wrapper.toWhere(false) + wrapper.toOrder() + toLimit();
            // 只能手动设置页码，很奇怪
            App.Current.Dispatcher.Invoke(() => { main.pagination.Total = TotalCount; });
            RenderCurrentVideo(sql);
        }



        public bool rendering = false;

        public void RenderCurrentVideo(string sql)
        {
            //if (rendering) return;
            List<Dictionary<string, object>> list = metaDataMapper.select(sql);
            List<Video> Videos = metaDataMapper.toEntity<Video>(list, typeof(Video).GetProperties(), false);
            VideoList = new ObservableCollection<Video>();

            if (Videos == null || Videos.Count <= 0) return;
            Videos.ForEach(video => VideoList.Add(video));
            render();
            //await Task.Run(() => Thread.Sleep(1000));
            //if (renderVideoCT.IsCancellationRequested) refreshVideoRenderToken();

        }


        public void setTag(ref Video video)
        {
            if (!string.IsNullOrEmpty(video.TagIDs))
            {
                video.TagStamp = new List<TagStamp>();
                foreach (var tagID in video.TagIDs.Split(','))
                {
                    long.TryParse(tagID.Trim(), out long id);
                    if (id > 0) video.TagStamp.Add(GlobalVariable.TagStamps.Where(arg => arg.TagID == id).FirstOrDefault());
                }
            }
        }



        public async void render()
        {
            if (CurrentVideoList == null) CurrentVideoList = new ObservableCollection<Video>();
            int.TryParse(Properties.Settings.Default.ShowImageMode, out int imageMode);
            for (int i = 0; i < VideoList.Count; i++)
            {
                try { renderVideoCT.ThrowIfCancellationRequested(); }
                catch (OperationCanceledException ex) { renderVideoCTS?.Dispose(); break; }
                rendering = true;
                Video video = VideoList[i];
                SetImage(ref video, imageMode);
                setTag(ref video);// 设置标签戳
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadVideoDelegate(LoadVideo), video, i);
            }

            if (renderVideoCT.IsCancellationRequested) refreshVideoRenderToken();
            rendering = false;
            //if (pageQueue.Count > 0) pageQueue.Dequeue();
            PageChangedCompleted?.Invoke(this, null);
        }


        #endregion

        public void GetFavoritesMovie()
        {
            ExecutiveSqlCommand(1, Jvedio.Language.Resources.Favorites, "SELECT * from movie where favorites>0 and favorites<=5");
        }

        public void GetRecentMovie()
        {
            string date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays).Date.ToString("yyyy-MM-dd");
            string date2 = DateTime.Now.ToString("yyyy-MM-dd");
            ExecutiveSqlCommand(2, $"{Jvedio.Language.Resources.RecentCreate} ({Properties.Settings.Default.RecentDays})", $"SELECT * from movie WHERE scandate BETWEEN '{date1}' AND '{date2}'");
        }

        public void GetRecentWatch()
        {
            IsLoadingMovie = true;
            List<Movie> movies = new List<Movie>();
            foreach (var keyValuePair in RecentWatched)
            {
                if (keyValuePair.Key <= DateTime.Now && keyValuePair.Key >= DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays))
                {
                    foreach (var item in keyValuePair.Value)
                    {
                        Movie movie = DataBase.SelectMovieByID(item);
                        if (movie != null) movies.Add(movie);
                    }
                }
            }

            TextType = $"{Jvedio.Language.Resources.RecentPlay}（{Properties.Settings.Default.RecentDays} ）";
            Task.Run(() =>
            {
                Statistic();
                MovieList = new List<Movie>();
                MovieList.AddRange(movies);
                FlipOver();
            });
        }



        public void GetSamePathMovie(string path)
        {
            //Bug: 大数据库卡死
            TextType = path;
            List<Movie> movies = DataBase.SelectMoviesBySql($"SELECT * from movie WHERE filepath like '%{path}%'");
            MovieList = new List<Movie>();
            MovieList.AddRange(movies);
            CurrentPage = 1;
            FlipOver();
        }



        public void GetMoviebyStudio(string moviestudio)
        {
            ExecutiveSqlCommand(0, moviestudio, $"SELECT * from movie where studio = '{moviestudio}'");
        }

        public void GetMoviebyTag(string movietag)
        {
            ExecutiveSqlCommand(0, movietag, $"SELECT * from movie where tag like '%{movietag}%'");
        }

        public void GetMoviebyDirector(string moviedirector)
        {
            ExecutiveSqlCommand(0, moviedirector, $"SELECT * from movie where director ='{moviedirector}'");
        }


        public void GetMoviebyGenre(string moviegenre)
        {
            ExecutiveSqlCommand(5, moviegenre, "SELECT * from movie where genre like '%" + moviegenre + "%'");
        }

        public void GetMoviebyLabel(string movielabel, string type = "label")
        {
            ExecutiveSqlCommand(7, movielabel, $"SELECT * from movie where {type} like '%{movielabel}%'");
        }



        public void GetMoviebyActress(Actress actress)
        {
            Statistic();
            int vediotype = (int)ClassifyVedioType;
            //根据视频类型选择演员

            List<Movie> movies;
            if (actress.id == "")
                movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{actress.name}%'");
            else
                movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actorid like '%{actress.id}%'");


            MovieList = new List<Movie>();
            movies?.ForEach(arg =>
            {
                if (arg.actor.Split(actorSplitDict[arg.vediotype]).Any(m => m.ToUpper() == actress.name.ToUpper())) MovieList.Add(arg);
            });

            CurrentPage = 1;
            FlipOver();
        }


        //根据视频类型选择演员
        public async Task<bool> AsyncGetMoviebyActress(Actress actress)
        {
            return await Task.Run(() =>
            {
                Statistic();
                List<Movie> movies;
                if (actress.id == "")
                {
                    if (ClassifyVedioType == 0) { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{actress.name}%'"); }
                    else { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{actress.name}%' and vediotype={(int)ClassifyVedioType}"); }
                }
                else
                {
                    if (ClassifyVedioType == 0) { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actorid like '%{actress.id}%'"); }
                    else { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actorid like '%{actress.id}%' and vediotype={(int)ClassifyVedioType}"); }
                }


                MovieList = new List<Movie>();
                if (movies != null || movies.Count > 0)
                {
                    foreach (var item in movies)
                    {
                        try { if (item.actor.Split(actorSplitDict[item.vediotype]).Any(m => m.ToUpper() == actress.name.ToUpper())) MovieList.Add(item); }
                        catch (Exception e)
                        {
                            Logger.LogE(e);
                        }
                    }
                }
                CurrentPage = 1;
                return true;
            });
        }



        public void ShowDetailsData()
        {
            if (MovieList == null) return;
            Task.Run(() =>
            {

                TextType = Jvedio.Language.Resources.DetailMode;
                Statistic();
                List<Movie> movies = new List<Movie>();

                TotalPage = (int)Math.Ceiling((double)FilterMovieList.Count / (double)Properties.Settings.Default.DisplayNumber);
                if (MovieList.Count > 0)
                {
                    MovieList.ForEach(arg =>
                    {
                        Movie movie = DataBase.SelectMovieByID(arg.id);
                        if (movie != null) movies.Add(movie);
                    });

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //App.Current.Windows[0].Cursor = Cursors.Wait;
                        DetailsDataList = new ObservableCollection<Movie>();
                        DetailsDataList.AddRange(movies);
                    });


                }
                CurrentCount = DetailsDataList.Count;
                IsFlipOvering = false;
                //App.Current.Windows[0].Cursor = Cursors.Arrow;
            });
        }


        /// <summary>
        /// 统计：加载时间 <70ms (15620个信息)
        /// </summary>
        public void Statistic()
        {
            Task.Run(() =>
            {
                long dbid = GlobalConfig.Main.CurrentDBId;
                AllVideoCount = metaDataMapper.selectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0));
                FavoriteVideoCount = metaDataMapper.selectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Gt("Grade", 0));


                string sql = "SELECT count(*) as Count " +
                "from (SELECT actor_info.ActorID FROM actor_info join metadatas_to_actor " +
                "on metadatas_to_actor.ActorID=actor_info.ActorID " +
                "join metadata " +
                "on metadatas_to_actor.DataID=metadata.DataID " +
                $"WHERE metadata.DBId={dbid} and metadata.DataType={0} " +
                "GROUP BY actor_info.ActorID );";
                AllActorCount = actorMapper.selectCount(sql);
                AllLabelCount = metaDataMapper.selectCount("SELECT COUNT(DISTINCT LabelName) as Count  from metadata_to_label");
                DateTime date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays);
                DateTime date2 = DateTime.Now;
                RecentWatchCount = metaDataMapper.selectCount(new SelectWrapper<MetaData>().Eq("DBId", dbid).Eq("DataType", 0).Between("ViewDate", DateHelper.toLocalDate(date1), DateHelper.toLocalDate(date2)));


            });
        }



        public void LoadFilePathClassfication()
        {
            //加载路经筛选
            FilePathClassification = new ObservableCollection<string>();
            foreach (Movie movie in MovieList)
            {
                string path = GetPathByDepth(movie.filepath, Properties.Settings.Default.FilePathClassificationMaxDepth);
                if (!string.IsNullOrEmpty(path) && !FilePathClassification.Contains(path)) FilePathClassification.Add(path);
                if (FilePathClassification.Count > Properties.Settings.Default.FilePathClassificationMaxCount) break;
            }
        }

        private string GetPathByDepth(string path, int depth)
        {

            if (string.IsNullOrEmpty(path) || path.IndexOf("\\") < 0) return "";
            string[] paths = path.Split('\\');
            string result = "";
            for (int i = 0; i < paths.Length - 1; i++)
            {
                result += paths[i] + "\\";
                if (i >= depth - 1) break;
            }
            return result;



        }



    }
}
