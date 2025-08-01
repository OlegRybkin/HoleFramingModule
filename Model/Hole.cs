using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        internal FamilyInstance FamilyInstance { get; set; }

        internal XYZ Location { get; private set; }
        internal XYZ FacingOrientation { get; private set; }

        internal Curve LengthCurve { get; private set; }
        internal Curve WidthCurve { get; private set; }
        internal double Thickness { get; private set; }

        internal Curve LongCurve { get; set; }
        internal Curve TransverseCurve { get; set; }

        private Element HostElement;
        private HostElementType HostElementType;

        internal Hole(Element element)
        {
            FamilyInstance = element as FamilyInstance;

            HostElement = ((FamilyInstance)element).Host;
            HostElementType = GetHostElementType();

            Location = GetLocation();
            FacingOrientation = GetFacingOrienation();

            Thickness = GetThickness();
            Location = GetLocation();




            
        }

        private double GetThickness()
        {
            double thickness = 0;
            Document doc = FamilyInstance.Document;

            Element hostElementType = doc.GetElement(HostElement.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());

            switch (HostElementType)
            {
                case HostElementType.Wall:

                    thickness = hostElementType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble();

                    break;

                case HostElementType.Floor:

                    thickness = hostElementType.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM).AsDouble();

                    break;
            }
            
            return thickness;
        }

        private XYZ GetLocation()
        {
            XYZ location = XYZ.Zero;

            switch (HostElementType) 
            { 
                case HostElementType.Floor:

                    location = ((LocationPoint)FamilyInstance.Location).Point;

                    break;

                case HostElementType.Wall:

                    location = new XYZ
                        (
                            ((LocationPoint)FamilyInstance.Location).Point.X + FacingOrientation.X * 0.5 * Thickness,
                            ((LocationPoint)FamilyInstance.Location).Point.Y + FacingOrientation.Y * 0.5 * Thickness,
                            ((LocationPoint)FamilyInstance.Location).Point.Z
                        );

                    break;
            }

            return location;
        }

        private XYZ GetFacingOrienation()
        {
            XYZ facingOrientation = XYZ.Zero;

            switch (HostElement.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString())
            {
                case "Перекрытия":

                    FacingOrientation = new XYZ(0, 0, 1);

                    break;

                case "Стены":

                    FacingOrientation = ((Wall)HostElement).Orientation;

                    break;
            }

            return facingOrientation;
        }

        private HostElementType GetHostElementType()
        {
            HostElementType hostElementType = HostElementType.Null;

            switch (HostElement.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString())
            {
                case "Перекрытия":

                    HostElementType = HostElementType.Floor;

                    break;

                case "Стены":

                    HostElementType = HostElementType.Wall;

                    break;
            }

            return hostElementType;
        }

    }
}
