using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HoleFramingModule.Model
{
    internal class HoleFramingTools
    {
        static internal void AddHoleFraming(Hole hole, FramingParameters framingParameters)
        {
            Rebar directLongRebar1 = AddDirectRebar
                (
                    hole,
                    hole.LongCurve,
                    hole.TransverseCurve,
                    framingParameters.LongRebarType,
                    framingParameters.LongRebarAnchLength,
                    framingParameters
                );

            Rebar directLongRebar2 = AddDirectRebar
                (
                    hole,
                    hole.LongCurve,
                    hole.TransverseCurve.CreateReversed(),
                    framingParameters.LongRebarType,
                    framingParameters.LongRebarAnchLength,
                    framingParameters
                );

            for (int i = 1; i < framingParameters.LongRebarCount; i++)
            {
                XYZ copyDirection1 = ((Line)hole.TransverseCurve).Direction * framingParameters.LongRebarStep * i;
                XYZ copyDirection2 = ((Line)hole.TransverseCurve.CreateReversed()).Direction * framingParameters.LongRebarStep * i;

                ElementTransformUtils.CopyElement(framingParameters.Document, directLongRebar1.Id, copyDirection1);
                ElementTransformUtils.CopyElement(framingParameters.Document, directLongRebar2.Id, copyDirection2);
            }

            Rebar directTransverseRebar1 = AddDirectRebar
                (
                    hole,
                    hole.TransverseCurve,
                    hole.LongCurve,
                    framingParameters.TransverseRebarType,
                    framingParameters.TransverseRebarAnchLength,
                    framingParameters
                );

            Rebar directTransverseRebar2 = AddDirectRebar
                (
                    hole,
                    hole.TransverseCurve,
                    hole.LongCurve.CreateReversed(),
                    framingParameters.TransverseRebarType,
                    framingParameters.TransverseRebarAnchLength,
                    framingParameters
                );

            for (int i = 1; i < framingParameters.TransverseRebarCount; i++)
            {
                XYZ copyDirection1 = ((Line)hole.LongCurve).Direction * framingParameters.TransverseRebarStep * i;
                XYZ copyDirection2 = ((Line)hole.LongCurve.CreateReversed()).Direction * framingParameters.TransverseRebarStep * i;

                ElementTransformUtils.CopyElement(framingParameters.Document, directTransverseRebar1.Id, copyDirection1);
                ElementTransformUtils.CopyElement(framingParameters.Document, directTransverseRebar2.Id, copyDirection2);
            }

            Rebar bentLongRebar1 = AddBentRebar(hole, hole.LongCurve, hole.TransverseCurve, framingParameters);
            Rebar bentLongRebar2 = AddBentRebar(hole, hole.LongCurve, hole.TransverseCurve.CreateReversed(), framingParameters);
            Rebar bentTransverseRebar1 = AddBentRebar(hole, hole.TransverseCurve, hole.LongCurve, framingParameters);
            Rebar bentTransverseRebar2 = AddBentRebar(hole, hole.TransverseCurve, hole.LongCurve.CreateReversed(), framingParameters);
        }

        private static Rebar AddBentRebar(Hole hole, Curve mainCurve, Curve subCurve, FramingParameters framingParameters)
        {
            XYZ mainCurveDirection = ((Line)mainCurve).Direction;
            XYZ subCurveDirection = ((Line)subCurve).Direction;

            double bentRebarHeight = 0;
            double verticalOffset = 0;

            switch (hole.HostElementType)
            {
                case HostElementType.Floor:
                    bentRebarHeight = hole.Thickness - (framingParameters.UpRebarCover + framingParameters.BackRebarDiameter + 0.5 * framingParameters.BentRebarType.BarModelDiameter) -
                                                      (framingParameters.DownRebarCover + 0.5 * framingParameters.BentRebarType.BarModelDiameter);

                    verticalOffset = framingParameters.UpRebarCover + framingParameters.BackRebarDiameter + 0.5 * framingParameters.BentRebarType.BarModelDiameter;

                    break;

                case HostElementType.Wall:
                    bentRebarHeight = hole.Thickness - 2 * (framingParameters.UpRebarCover + 0.5 * framingParameters.BentRebarType.BarModelDiameter);

                    verticalOffset = framingParameters.UpRebarCover + 0.5 * framingParameters.BentRebarType.BarModelDiameter;

                    break;

            }

            XYZ point1 = new XYZ
                (
                    mainCurve.GetEndPoint(0).X + subCurveDirection.X * framingParameters.BentRebarLength,
                    mainCurve.GetEndPoint(0).Y + subCurveDirection.Y * framingParameters.BentRebarLength,
                    mainCurve.GetEndPoint(0).Z + subCurveDirection.Z * framingParameters.BentRebarLength
                );

            XYZ point2 = new XYZ
                (
                    mainCurve.GetEndPoint(0).X,
                    mainCurve.GetEndPoint(0).Y,
                    mainCurve.GetEndPoint(0).Z
                );

            XYZ point3 = new XYZ
                (
                    mainCurve.GetEndPoint(0).X - hole.FaceNormal.X * bentRebarHeight,
                    mainCurve.GetEndPoint(0).Y - hole.FaceNormal.Y * bentRebarHeight,
                    mainCurve.GetEndPoint(0).Z - hole.FaceNormal.Z * bentRebarHeight
                );

            XYZ point4 = new XYZ
                (
                    mainCurve.GetEndPoint(0).X - hole.FaceNormal.X * bentRebarHeight + subCurveDirection.X * framingParameters.BentRebarLength,
                    mainCurve.GetEndPoint(0).Y - hole.FaceNormal.Y * bentRebarHeight + subCurveDirection.Y * framingParameters.BentRebarLength,
                    mainCurve.GetEndPoint(0).Z - hole.FaceNormal.Z * bentRebarHeight + subCurveDirection.Z * framingParameters.BentRebarLength
                );

            List<Curve> rebarCurves = new List<Curve>()
            {
                Line.CreateBound(point1, point2),
                Line.CreateBound(point2, point3),
                Line.CreateBound(point3, point4)
            };

            Rebar rebar = Rebar.CreateFromCurves
                (
                    framingParameters.Document,
                    RebarStyle.Standard,
                    framingParameters.BentRebarType,
                    null,
                    null,
                    hole.HostElement,
                    mainCurveDirection,
                    rebarCurves,
                    RebarHookOrientation.Right,
                    RebarHookOrientation.Right,
                    true,
                    false
                );

            double horizontalOffset = 0.5 * subCurve.ApproximateLength + framingParameters.RebarOffset - framingParameters.BentRebarType.BarModelDiameter;
            ElementTransformUtils.MoveElement(framingParameters.Document, rebar.Id, subCurveDirection * horizontalOffset);
            ElementTransformUtils.MoveElement(framingParameters.Document, rebar.Id, -hole.FaceNormal * verticalOffset);

            int rebarCount = Convert.ToInt32(Math.Ceiling(mainCurve.ApproximateLength / framingParameters.BentRebarStep));
            rebarCount = rebarCount >= 1 ? rebarCount : 1;

            if (rebarCount >= 2) rebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(rebarCount, framingParameters.BentRebarStep, true, true, true);

            double alongCurveOffset = (mainCurve.ApproximateLength - (rebarCount - 1) * framingParameters.BentRebarStep) / 2;

            ElementTransformUtils.MoveElement(framingParameters.Document, rebar.Id, mainCurveDirection * alongCurveOffset);

            return rebar;
        }

        private static Rebar AddDirectRebar(Hole hole, Curve mainCurve, Curve subCurve, RebarBarType rebarBarType, double lengthAnch, FramingParameters framingParameters)
        {
            XYZ mainCurveDirection = ((Line)mainCurve).Direction;
            XYZ subCurveDirection = ((Line)subCurve).Direction;

            XYZ point1 = new XYZ
                (
                    mainCurve.GetEndPoint(0).X - mainCurveDirection.X * lengthAnch,
                    mainCurve.GetEndPoint(0).Y - mainCurveDirection.Y * lengthAnch,
                    mainCurve.GetEndPoint(0).Z - mainCurveDirection.Z * lengthAnch
                );

            XYZ point2 = new XYZ
                (
                    mainCurve.GetEndPoint(1).X + mainCurveDirection.X * lengthAnch,
                    mainCurve.GetEndPoint(1).Y + mainCurveDirection.Y * lengthAnch,
                    mainCurve.GetEndPoint(1).Z + mainCurveDirection.Z * lengthAnch
                );

            Curve rebarCurve = Line.CreateBound(point1, point2);

            Rebar rebar = Rebar.CreateFromCurves
                (
                    framingParameters.Document,
                    RebarStyle.Standard,
                    rebarBarType,
                    null,
                    null,
                    hole.HostElement,
                    -hole.FaceNormal,
                    new List<Curve>() { rebarCurve },
                    RebarHookOrientation.Right,
                    RebarHookOrientation.Right,
                    true,
                    false
                );

            double horizontalOffset = 0.5 * subCurve.ApproximateLength + framingParameters.RebarOffset;
            ElementTransformUtils.MoveElement(framingParameters.Document, rebar.Id, subCurveDirection * horizontalOffset);

            double verticalOffset = 0;
            double rebarDistance = 0;

            switch (hole.HostElementType)
            {
                case HostElementType.Floor:
                    verticalOffset = framingParameters.UpRebarCover + 2 * framingParameters.BackRebarDiameter + 0.5 * rebarBarType.BarModelDiameter;

                    rebarDistance = hole.Thickness - (framingParameters.UpRebarCover + 2 * framingParameters.BackRebarDiameter + 0.5 * rebarBarType.BarModelDiameter) -
                                                    (framingParameters.DownRebarCover + framingParameters.BackRebarDiameter + 0.5 * rebarBarType.BarModelDiameter);
                    break;

                case HostElementType.Wall:
                    verticalOffset = framingParameters.UpRebarCover + 0.5 * rebarBarType.BarModelDiameter;

                    rebarDistance = hole.Thickness - 2 * verticalOffset;
                    break;
            }
            
            ElementTransformUtils.MoveElement(framingParameters.Document, rebar.Id, -hole.FaceNormal * verticalOffset);
            rebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(2, rebarDistance, true, true, true);

            return rebar;
        }
    }
}
