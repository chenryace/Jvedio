using System;
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
using static Jvedio.ImageProcess;
using System.Windows.Input;
using System.Drawing;
using DynamicData;
using DynamicData.Binding;
using System.Xml;
using HandyControl.Tools.Extension;
using System.Windows.Media;
using System.ComponentModel;
using Jvedio.Utils;
namespace Jvedio.Comics
{
    public class VieModel_Main : ViewModelBase
    {
        public event EventHandler CurrentMovieListHideOrChanged;
        public event EventHandler CurrentActorListHideOrChanged;
        public event EventHandler MovieFlipOverCompleted;
        public event EventHandler ActorFlipOverCompleted;
        public event EventHandler OnCurrentMovieListRemove;

        public bool IsFlipOvering = false;
        public VedioType CurrentVedioType = VedioType.所有;



        public int SideIdx = 0;



        #region "RelayCommand"
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand GenreCommand { get; set; }
        public RelayCommand ActorCommand { get; set; }
        public RelayCommand LabelCommand { get; set; }

        public RelayCommand FavoritesCommand { get; set; }
        public RelayCommand RecentCommand { get; set; }

        public RelayCommand<bool> RecentWatchCommand { get; set; }

        public RelayCommand AddNewMovie { get; set; }
        public RelayCommand FlipOverCommand { get; set; }
        #endregion


        public VieModel_Main()
        {
            CurrentMovieList = new ObservableCollection<Movie>();

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

        private int _DatabaseSelectedIndex = 0;

        public int DatabaseSelectedIndex
        {
            get { return _DatabaseSelectedIndex; }
            set
            {
                _DatabaseSelectedIndex = value;
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

        private bool _HideSide = false;

        public bool HideSide
        {
            get { return _HideSide; }
            set
            {
                _HideSide = value;
                RaisePropertyChanged();
            }
        }

        private double _SideBorderWidth = 200;

        public double SideBorderWidth
        {
            get { return _SideBorderWidth; }
            set
            {
                _SideBorderWidth = value;
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


        private bool _IsLoadingMovie = false;

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

        private ObservableCollection<string> _DataBases;


        public ObservableCollection<string> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Movie> currentmovielist;


        public ObservableCollection<Movie> CurrentMovieList
        {
            get { return currentmovielist; }
            set
            {
                currentmovielist = value;
                RaisePropertyChanged();
                CurrentMovieListHideOrChanged?.Invoke(this, EventArgs.Empty);
                IsFlipOvering = false;
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





        private ObservableCollection<Movie> selectedMovie = new ObservableCollection<Movie>();

        public ObservableCollection<Movie> SelectedMovie
        {
            get { return selectedMovie; }
            set
            {
                selectedMovie = value;
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


        private ObservableCollection<Actress> actorlist;
        public ObservableCollection<Actress> ActorList
        {
            get { return actorlist; }
            set
            {
                actorlist = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Actress> _CurrentActorList;


        public ObservableCollection<Actress> CurrentActorList
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


        private bool _SortDescending =Jvedio.Comics.Properties.Settings.Default.SortDescending;
        public bool SortDescending
        {
            get { return _SortDescending; }
            set
            {
                _SortDescending = value;
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


        private double _AllVedioCount = 0;
        public double AllVedioCount
        {
            get { return _AllVedioCount; }
            set
            {
                _AllVedioCount = value;
                RaisePropertyChanged();
            }
        }

        private double _FavoriteVedioCount = 0;
        public double FavoriteVedioCount
        {
            get { return _FavoriteVedioCount; }
            set
            {
                _FavoriteVedioCount = value;
                RaisePropertyChanged();
            }
        }

        private double _RecentVedioCount = 0;
        public double RecentVedioCount
        {
            get { return _RecentVedioCount; }
            set
            {
                _RecentVedioCount = value;
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


        public int currentpage = 1;
        public int CurrentPage
        {
            get { return currentpage; }
            set
            {
                currentpage = value;
                FlowNum = 0;
                RaisePropertyChanged();
            }
        }


        public double _CurrentCount = 0;
        public double CurrentCount
        {
            get { return _CurrentCount; }
            set
            {
                _CurrentCount = value;
                RaisePropertyChanged();

            }
        }


        public double _TotalCount = 0;
        public double TotalCount
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



        public double _ActorCurrentCount = 0;
        public double ActorCurrentCount
        {
            get { return _ActorCurrentCount; }
            set
            {
                _ActorCurrentCount = value;
                RaisePropertyChanged();

            }
        }


        public double _ActorTotalCount = 0;
        public double ActorTotalCount
        {
            get { return _ActorTotalCount; }
            set
            {
                _ActorTotalCount = value;
                RaisePropertyChanged();

            }
        }


        public int currentactorpage = 1;
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


        public int totalactorpage = 1;
        public int TotalActorPage
        {
            get { return totalactorpage; }
            set
            {
                totalactorpage = value;
                RaisePropertyChanged();
            }
        }






        public int _FlowNum = 0;
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




        #endregion







    }
}
