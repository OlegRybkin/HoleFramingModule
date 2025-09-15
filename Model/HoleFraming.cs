using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HoleFramingModule.View;
using HoleFramingModule.ViewModel;
using RevitTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoleFramingModule.Model
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class HoleFraming : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                IList<Reference> holeReferences = uidoc.Selection.PickObjects(ObjectType.Element, new HoleFilter(), "Выберите отверстия");

                List<Hole> holes = new List<Hole>();

                foreach (Reference holeRef in holeReferences) 
                {
                    holes.Add(new Hole(doc.GetElement(holeRef)));
                }

                MainWindowVM mainWindowVM = new MainWindowVM(doc);
                MainWindow mainWindow = new MainWindow()
                {
                    DataContext = mainWindowVM
                };

                bool? mainWindowResult = mainWindow.ShowDialog();

                if (mainWindowResult != true) return Result.Cancelled;

                FramingParameters framingParameters = new FramingParameters()
                {
                    Document = doc,
                    LongRebarType = mainWindowVM.SelectedLongRebar,
                    TransverseRebarType = mainWindowVM.SelectedTransverseRebar,

                    LongRebarAnchLength = Calculator.FromMmToFeet(mainWindowVM.LongAnchLength),
                    TransverseRebarAnchLength = Calculator.FromMmToFeet(mainWindowVM.TransverseAnchLength),

                    LongRebarCount = mainWindowVM.LongRebarCount,
                    TransverseRebarCount = mainWindowVM.TransverseRebarCount,

                    LongRebarStep = Calculator.FromMmToFeet(mainWindowVM.LongRebarStep),
                    TransverseRebarStep = Calculator.FromMmToFeet(mainWindowVM.TransverseRebarStep),

                    RebarOffset = Calculator.FromMmToFeet(mainWindowVM.RebarOffset),

                    BentRebarType = mainWindowVM.SelectedBentRebar,
                    BentRebarLength = Calculator.FromMmToFeet(mainWindowVM.BentRebarLength),
                    BentRebarStep = Calculator.FromMmToFeet(mainWindowVM.BentRebarStep),

                    BackRebarDiameter = Calculator.FromMmToFeet(mainWindowVM.BackRebarDiameter)
                };

                using (Transaction trans = new Transaction(doc, "Обрамление отверстий"))
                {
                    trans.Start();

                    foreach (Hole hole in holes)
                    {
                        if (mainWindowVM.IsRebarCoverFromModel)
                        {
                            framingParameters.UpRebarCover = RevitModel.GetUpRebarCoverFromElement(hole.HostElement);
                            framingParameters.DownRebarCover = RevitModel.GetDownRebarCoverFromElement(hole.HostElement);
                        }
                        else
                        {
                            framingParameters.UpRebarCover = Calculator.FromMmToFeet(mainWindowVM.RebarCoverUp);
                            framingParameters.DownRebarCover = Calculator.FromMmToFeet(mainWindowVM.RebarCoverDown);
                        }

                        HoleFramingTools.AddHoleFraming(hole, framingParameters);
                    }

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", ex.ToString());
                return Result.Failed;
            }
        }
    }
}
