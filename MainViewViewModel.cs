using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumberingPlugin
{
    class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public DelegateCommand NumberingCommand { get; }

        public int FirstNumber { get; set; } = 0;

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;

            NumberingCommand = new DelegateCommand(OnNumberingCommand);
        }

        private void OnNumberingCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //get current view plan
            ViewPlan plan = uidoc.ActiveView as ViewPlan;
            if (plan == null)
            {
                TaskDialog.Show("Ошибка!", "Перейдите на вид, на котором отображаются помещения");
                return;
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

            Transaction transaction = new Transaction(doc, "Нумерация помещений");
            transaction.Start();
            int currentNumber = FirstNumber;
            foreach (Room room in sortedRooms)
            {
                room.Number = currentNumber.ToString();
                currentNumber++;
            }
            transaction.Commit();

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
