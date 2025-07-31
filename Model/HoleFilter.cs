using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoleFramingModule.Model
{
    internal class HoleFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString() == "Обобщенные модели" &&
                (elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("отверстие") || 
                elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().ToLower().Contains("проем")))
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
