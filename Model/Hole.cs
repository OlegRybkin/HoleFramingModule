using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoleFramingModule.Model
{
    internal class Hole
    {
        internal FamilyInstance FamilyInstance { get; set; }

        internal Curve LongCurve { get; set; }
        internal Curve TransverseCurve { get; set; }

        private Element HostElement;

        internal Hole(Element element)
        {
            FamilyInstance = element as FamilyInstance;
            HostElement = ((FamilyInstance)element).Host;

            GetHoleCurves();
        }

        private List<Curve> GetHoleCurves()
        {
            XYZ direction = GetDirection();

            Options options = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
            };

            List<Curve> curves = new List<Curve>();

            GeometryElement geometryElement = FamilyInstance.get_Geometry(options);

            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is GeometryInstance geometryInstance)
                {
                    GeometryElement geoElement = geometryInstance.GetInstanceGeometry();

                    foreach (GeometryObject geoObject in geoElement)
                    {
                        if (geoObject is Line)
                        {
                            curves.Add((Curve)geoObject);
                        }
                    }
                }
            }

            return curves;
        }


        private XYZ GetDirection()
        {
            XYZ direction = XYZ.Zero;

            switch (HostElement.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString())
            {
                case "Перекрытия":

                    direction = new XYZ(0, 0, 1);

                    break;

                case "Стены":

                    direction = ((Wall)HostElement).Orientation;

                    break;
            }

            return direction;
        }
    }
}
