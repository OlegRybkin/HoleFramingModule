using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using RevitTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HoleFramingModule.Model
{
    enum HostElementType
    {
        Floor,
        Wall,
        Null
    }

    internal class Hole
    {
        internal XYZ Location { get; private set; }
        internal XYZ FaceNormal { get; private set; }
        internal XYZ Orientation { get; private set; }

        internal double Thickness { get; private set; }

        internal Curve LongCurve
        {
            get 
            { 
                if (LengthCurve.ApproximateLength > WidthCurve.ApproximateLength) return LengthCurve;
                return WidthCurve; 
            }
            private set { ; }
        }

        internal Curve TransverseCurve
        {
            get
            {
                if (LengthCurve.ApproximateLength > WidthCurve.ApproximateLength) return WidthCurve;
                return LengthCurve;
            }
            private set { ; }
        }

        private double width;
        internal double Width
        {
            get { return Math.Min(length, width); }
            set { width = value; }
        }

        private double length;
        internal double Length
        {
            get { return Math.Max(length, width); }
            set { length = value; }
        }

        internal HostElementType HostElementType { get; private set; }

        internal Element HostElement { get; private set; }

        private Curve LengthCurve;
        private Curve WidthCurve;

        internal Hole(Element element)
        {
            HostElement = GetHostElement(element);
            HostElementType = GetHostElementType(element);

            Thickness = GetThickness(element);
            Length = GetLength(element);
            Width = GetWidth(element);

            FaceNormal = GetFaceNormal(element);
            Orientation = GetOrientation(element);
            Location = GetLocation(element);

            LengthCurve = GetLengthCurve(element);
            WidthCurve = GetWidthCurve(element);
        }

        private Element GetHostElement(Element element)
        {
            return ((FamilyInstance)element).Host;
        }

        private XYZ GetOrientation(Element element)
        {
            XYZ orientation = XYZ.Zero;

            HostElementType hostElementType = GetHostElementType(element);
            FamilyInstance familyInstance = element as FamilyInstance;

            switch (hostElementType)
            {
                case HostElementType.Floor:
                    orientation = familyInstance.FacingOrientation;

                    break;

                case HostElementType.Wall:
                    orientation = familyInstance.HandOrientation;

                    break;
            }

            return orientation;
        }

        private Curve GetWidthCurve(Element element)
        {
            Curve widthCurve = null;

            XYZ location = GetLocation(element);
            XYZ orientation = GetOrientation(element);
            XYZ faceNormal = GetFaceNormal(element);
            double width = GetWidth(element);

            Transform rotation = Transform.CreateRotation(faceNormal, Math.PI / 2);
            XYZ widthVector = rotation.OfVector(orientation) * width;

            XYZ point1 = location - 0.5 * widthVector;
            XYZ point2 = location + 0.5 * widthVector;

            widthCurve = Line.CreateBound(point1, point2);

            return widthCurve;
        }

        private Curve GetLengthCurve(Element element)
        {
            Curve lengthCurve = null;

            XYZ location = GetLocation(element);
            XYZ orientation = GetOrientation(element);
            double length = GetLength(element);

            XYZ lengthVector = orientation * length;

            XYZ point1 = location - 0.5 * lengthVector;
            XYZ point2 = location + 0.5 * lengthVector;

            lengthCurve = Line.CreateBound(point1, point2);

            return lengthCurve;
        }

        private double GetLength(Element element)
        {
            double length = 0;
            string lengthParameterName = string.Empty;

            HostElementType hostElementType = GetHostElementType(element);

            switch (hostElementType)
            {
                case HostElementType.Floor:
                    lengthParameterName = "мод_ФОП_Габарит А";

                    break;

                case HostElementType.Wall:
                    lengthParameterName = "ФОП_РАЗМ_Ширина";

                    break;
            }

            if (RevitTools.Checker.IsParametersExist((FamilyInstance)element, new List<string>() { lengthParameterName }))
            {
                length = ((FamilyInstance)element).LookupParameter(lengthParameterName).AsDouble();
            }

            return length;
        }

        private double GetWidth(Element element)
        {
            double width = 0;
            string widthParameterName = string.Empty;

            HostElementType hostElementType = GetHostElementType(element);

            switch (hostElementType)
            {
                case HostElementType.Floor:
                    widthParameterName = "мод_ФОП_Габарит Б";

                    break;

                case HostElementType.Wall:
                    widthParameterName = "ФОП_РАЗМ_Высота";

                    break;
            }

            if (RevitTools.Checker.IsParametersExist((FamilyInstance)element, new List<string>() { widthParameterName }))
            {
                width = ((FamilyInstance)element).LookupParameter(widthParameterName).AsDouble();
            }

            return width;
        }

        private double GetThickness(Element element)
        {
            double thickness = 0;
            Document doc = element.Document;

            Element hostElement = GetHostElement(element);
            Element hostElementTypeParam = doc.GetElement(hostElement.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());
            HostElementType hostElementType = GetHostElementType(element);

            switch (hostElementType)
            {
                case HostElementType.Wall:
                    thickness = hostElementTypeParam.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble();

                    break;

                case HostElementType.Floor:
                    thickness = hostElementTypeParam.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM).AsDouble();

                    break;
            }
            
            return thickness;
        }

        private XYZ GetLocation(Element element)
        {
            XYZ location = XYZ.Zero;

            HostElementType hostElementType = GetHostElementType(element);
            FamilyInstance familyInstance = element as FamilyInstance;
            XYZ faceNormal = GetFaceNormal(element);
            double thickness = GetThickness(element);
            double width = GetWidth(element);

            switch (hostElementType) 
            { 
                case HostElementType.Floor:
                    location = ((LocationPoint)familyInstance.Location).Point;

                    break;

                case HostElementType.Wall:
                    location = new XYZ
                        (
                            ((LocationPoint)familyInstance.Location).Point.X + faceNormal.X * 0.5 * thickness,
                            ((LocationPoint)familyInstance.Location).Point.Y + faceNormal.Y * 0.5 * thickness,
                            ((LocationPoint)familyInstance.Location).Point.Z + 0.5 * width
                        );

                    break;
            }

            return location;
        }

        private XYZ GetFaceNormal(Element element)
        {
            XYZ facingOrientation = XYZ.Zero;

            HostElementType hostElementType = GetHostElementType(element);

            switch (hostElementType)
            {
                case HostElementType.Floor:
                    facingOrientation = new XYZ(0, 0, 1);

                    break;

                case HostElementType.Wall:
                    Element hostElement = GetHostElement(element);
                    facingOrientation = ((Wall)hostElement).Orientation;

                    break;
            }

            return facingOrientation;
        }

        private HostElementType GetHostElementType(Element element)
        {
            HostElementType hostElementType = HostElementType.Null;
            Element hostElement = GetHostElement(element);

            switch (hostElement.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString())
            {
                case "Перекрытия":
                    hostElementType = HostElementType.Floor;

                    break;

                case "Стены":
                    hostElementType = HostElementType.Wall;

                    break;
            }

            return hostElementType;
        }

    }
}
