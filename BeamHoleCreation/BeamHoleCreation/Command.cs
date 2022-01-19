using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BeamHoleCreation.Presentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamHoleCreation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        public static ExternalEvent externalEvent { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                CommonData.UIApp  = commandData.Application;
                //Register MyExternalEventHandler ahead of time
                externalEvent = ExternalEvent.Create(new ExternalEventHandler());
                // Form instanziieren
                frmHole dataInputForm = new frmHole();
                // Form starten
                dataInputForm.Show();
               
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                return Result.Failed;
            }
        }
    }
}
