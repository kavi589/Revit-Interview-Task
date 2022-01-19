using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamHoleCreation
{
    class ExternalEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            CommonData.UIDoc = CommonData.UIApp.ActiveUIDocument;
            CommonData.App = CommonData.UIApp.Application;
            CommonData.Doc = CommonData.UIDoc.Document;
            BeamHole beamHole = new BeamHole();
            beamHole.FilterBeam();
        }

        public string GetName()
        {
            return "Hole Creation";
        }
    }
}
