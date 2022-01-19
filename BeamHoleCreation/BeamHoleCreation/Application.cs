using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BeamHoleCreation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            String tabName = "Task";
            application.CreateRibbonTab(tabName);
            // Create Ribbon Panel 
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Beams");

            // Create the push button
            PushButton button = ribbonPanel.AddItem(new PushButtonData("Beam Cut", "BeamCut", Assembly.GetExecutingAssembly().Location, "BeamHoleCreation.Command")) as PushButton;
            
            return Result.Succeeded;
        }
    }
}
