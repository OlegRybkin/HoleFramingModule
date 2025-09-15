using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoleFramingModule.Model
{
    internal class FramingParameters
    {
        internal Document Document {  get; set; }

        internal double UpRebarCover {  get; set; }
        internal double DownRebarCover {  get; set; }
        internal double BackRebarDiameter { get; set; }

        internal RebarBarType LongRebarType {  get; set; }
        internal RebarBarType TransverseRebarType {  get; set; }

        internal double LongRebarAnchLength { get; set; }
        internal double TransverseRebarAnchLength { get; set; }

        internal int LongRebarCount { get; set; }
        internal int TransverseRebarCount { get; set; }

        internal double LongRebarStep {  get; set; }
        internal double TransverseRebarStep { get; set; }

        internal RebarBarType BentRebarType { get; set; }
        internal double BentRebarLength { get; set; }
        internal double BentRebarStep { get; set; }

        internal double RebarOffset { get; set; }

        internal FramingParameters() { }
    }
}
