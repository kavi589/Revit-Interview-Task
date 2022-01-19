using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamHoleCreation
{
    public class CommonData
    {
        public static UIApplication UIApp { get; set; }
        public static UIDocument UIDoc { get; set; }
        public static Autodesk.Revit.ApplicationServices.Application App { get; set; }
        public static Document Doc { get; set; }
        public static double Tolerance { get; internal set; }

        public static Hole HoleData = new Hole();
    }
}
