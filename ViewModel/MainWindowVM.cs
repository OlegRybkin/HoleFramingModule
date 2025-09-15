using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Prism.Commands;
using Prism.Mvvm;
using RevitTools;
using RevitTools.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HoleFramingModule.ViewModel
{
    public class MainWindowVM : BindableBase
    {
        public ObservableCollection<RebarBarType> RebarTypes { get; private set; } // список типов арматурных стержней

        //ПРЯМЫЕ СТЕРЖНИ В ПРОДОЛЬНОМ НАПРАВЛЕНИИ
        public RebarBarType SelectedLongRebar { get; set; }
        public int LongAnchLength { get; set; }
        public int LongRebarCount { get; set; }
        public int LongRebarStep { get; set; }

        //ПРЯМЫЕ СТЕРЖНИ В ПОПЕРЕЧНОМ НАПРАВЛЕНИИ
        public RebarBarType SelectedTransverseRebar { get; set; }
        public int TransverseAnchLength { get; set; }
        public int TransverseRebarCount { get; set; }
        public int TransverseRebarStep { get; set; }

        //П-ОБРАЗНЫЕ СТЕРЖНИ
        public RebarBarType SelectedBentRebar { get; set; }
        public int BentRebarLength { get; set; }
        public int BentRebarStep { get; set; }

        //ОБЩИЕ НАСТРОЙКИ
        public List<int> RebarDiametersList { get; private set; }
        public int BackRebarDiameter { get; set; }

        private bool isControlEnabled;
        public bool IsControlEnabled
        {
            get { return isControlEnabled; }
            set 
            { 
                isControlEnabled = value;
                RaisePropertyChanged(nameof(IsControlEnabled));
            }
        }

        private bool isRebarCoverFromModel;
        public bool IsRebarCoverFromModel
        {
            get { return isRebarCoverFromModel; }
            set 
            { 
                isRebarCoverFromModel = value;
                IsControlEnabled = !value;
                RaisePropertyChanged(nameof(IsRebarCoverFromModel));
            }
        }

        private bool isRebarCoverFromUser;
        public bool IsRebarCoverFromUser
        {
            get { return isRebarCoverFromUser; }
            set
            {
                isRebarCoverFromUser = value;
                RaisePropertyChanged(nameof(IsRebarCoverFromUser));
            }
        }

        public int RebarCoverUp {  get; set; }
        public int RebarCoverDown {  get; set; }
        public int RebarOffset {  get; set; }

        public DelegateCommand<Window> OkBtnCommand { get; private set; }
        public DelegateCommand<Window> CancelBtnCommand { get; private set; }

        private string SettingsPath { get; set; }

        public MainWindowVM(Document doc) 
        {
            RebarTypes = RevitModel.GetRebarTypesFromModel(doc);
            RebarDiametersList = StructConstants.RebarDiameters;

            OkBtnCommand = new DelegateCommand<Window>(OkBtnFunc);
            CancelBtnCommand = new DelegateCommand<Window>(CancelBtnFunc);

            SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "A101 Mod", "2022", $"{doc.Title}_HoleFraming.json");

            if (File.Exists(SettingsPath))
            {
                SettingsStore settings = SettingsStore.Load(SettingsPath);

                string longRebarName = settings.Get("SelectedLongRebar", "abc");
                string transverseRebarName = settings.Get("SelectedTransverseRebar", "abc");
                string bentRebarName = settings.Get("SelectedBentRebar", "abc");

                SelectedLongRebar = RebarTypes.Where(x => x.Name == longRebarName).FirstOrDefault();
                LongAnchLength = settings.Get("LongAnchLength", 1);
                LongRebarCount = settings.Get("LongRebarCount", 1);
                LongRebarStep = settings.Get("LongRebarStep", 1);

                SelectedTransverseRebar = RebarTypes.Where(x => x.Name == transverseRebarName).FirstOrDefault();
                TransverseAnchLength = settings.Get("TransverseAnchLength", 1);
                TransverseRebarCount = settings.Get("TransverseRebarCount", 1);
                TransverseRebarStep = settings.Get("TransverseRebarStep", 1);

                SelectedBentRebar = RebarTypes.Where(x => x.Name == bentRebarName).FirstOrDefault();
                BentRebarLength = settings.Get("BentRebarLength", 1);
                BentRebarStep = settings.Get("BentRebarStep", 1);

                BackRebarDiameter = settings.Get("BackRebarDiameter", 1);
                IsRebarCoverFromModel = settings.Get("IsRebarCoverFromModel", true);
                RebarCoverUp = settings.Get("RebarCoverUp", 1);
                RebarCoverDown = settings.Get("RebarCoverDown", 1);
                RebarOffset = settings.Get("RebarOffset", 1);

                if (!IsRebarCoverFromModel) IsRebarCoverFromUser = true;
            }
            else
            {
                IsRebarCoverFromModel = true;
                RebarCoverUp = 20;
                RebarCoverDown = 20;
                RebarOffset = 50;
            }
        }

        private void OkBtnFunc(Window window)
        {
            List<RebarBarType> rebarParameters = new List<RebarBarType>() { SelectedLongRebar, SelectedTransverseRebar, SelectedBentRebar };
            List<int> intParameters = new List<int>
            {
                LongAnchLength,
                LongRebarCount,
                LongRebarStep,

                TransverseAnchLength,
                TransverseRebarCount,
                TransverseRebarStep,

                BentRebarLength,
                BentRebarStep,

                BackRebarDiameter,
                RebarCoverUp,
                RebarCoverDown,
                RebarOffset
            };

            if (rebarParameters.All(x => x != null) && (intParameters.All(y => y > 0)))
            {
                SaveSettings();
                window.DialogResult = true;

                window.Close();
            }
        }

        private void CancelBtnFunc(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }

        private void SaveSettings()
        {
            SettingsStore settings = new SettingsStore();

            settings.Set("SelectedLongRebar", SelectedLongRebar.Name);
            settings.Set("LongAnchLength", LongAnchLength);
            settings.Set("LongRebarCount", LongRebarCount);
            settings.Set("LongRebarStep", LongRebarStep);

            settings.Set("SelectedTransverseRebar", SelectedTransverseRebar.Name);
            settings.Set("TransverseAnchLength", TransverseAnchLength);
            settings.Set("TransverseRebarCount", TransverseRebarCount);
            settings.Set("TransverseRebarStep", TransverseRebarStep);

            settings.Set("SelectedBentRebar", SelectedBentRebar.Name);
            settings.Set("BentRebarLength", BentRebarLength);
            settings.Set("BentRebarStep", BentRebarStep);

            settings.Set("BackRebarDiameter", BackRebarDiameter);
            settings.Set("IsRebarCoverFromModel", IsRebarCoverFromModel);
            settings.Set("RebarCoverUp", RebarCoverUp);
            settings.Set("RebarCoverDown", RebarCoverDown);
            settings.Set("RebarOffset", RebarOffset);

            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "A101 Mod", "2022");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            settings.Save(SettingsPath);
        }
    }
}
