using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumberingPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RoomNumberingPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //get current view plan
            ViewPlan plan = uidoc.ActiveView as ViewPlan;
            if (plan == null)
            {
                TaskDialog.Show("Ошибка!", "Перейдите на вид, на котором отображаются помещения");
                return Result.Cancelled;
            }

            //get general level for current view plan
            Level level = plan.GenLevel;

            List<Room> rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .Where(x => x.LevelId.Equals(level.Id))
                .ToList();

            var sortedRooms = from room in rooms
                              orderby room.get_BoundingBox(null).Max.Y descending, room.get_BoundingBox(null).Min.X
                              select room;

            return Result.Succeeded;
        }
    }

}
