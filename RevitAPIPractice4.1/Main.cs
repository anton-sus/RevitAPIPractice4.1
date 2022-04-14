using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Structure;

namespace RevitAPIPractice4._1
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        //public Level level1 { get; }
        //public Level level2 { get; }
        //public List<Wall> walls { get; set; }=new List<Wall>();  

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> listLevel = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .OfType<Level>()
            .ToList();

            Level level1 = listLevel
                .Where(el => el.Name == "Уровень 1")
                .FirstOrDefault();
            Level level2 = listLevel
             .Where(el => el.Name == "Уровень 2")
             .FirstOrDefault();


            var walls = AddWalls(doc, level1, level2);
            AddDoor(doc, level1, walls[0]);
            AddWindow(doc, level1, walls[1]);
            AddWindow(doc, level1, walls[2]);
            AddWindow(doc, level1, walls[3]);

            return Result.Succeeded;
        }

        public List<Wall> AddWalls(Document doc, Level level1, Level level2)
        {
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>();
            Transaction ts = new Transaction(doc, "wall create");
            ts.Start();
           
            
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }
            ts.Commit();
            return walls ;
        } 
        public void AddDoor(Document doc, Level level1,Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .Where(x => x.Name.Equals("0915 x 2134 мм"))
                .OfType<FamilySymbol>()
                .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                .FirstOrDefault();


            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            Transaction tx = new Transaction(doc, "door create");
            tx.Start();
            if (!doorType.IsActive)
                doorType.Activate();
            doc.Create.NewFamilyInstance(point, doorType, wall, level1, StructuralType.NonStructural);
            tx.Commit();
        }
        public void AddWindow(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .Where(x => x.Name.Equals("0610 x 1830 мм"))
                .OfType<FamilySymbol>()
                .Where(x => x.FamilyName.Equals("Фиксированные"))
                .FirstOrDefault();


            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            BoundingBoxXYZ bounds = wall.get_BoundingBox(null);
            XYZ winPoint = (bounds.Max + bounds.Min)/3 + point;


            Transaction tx = new Transaction(doc, "window create");
            tx.Start();
            if (!windowType.IsActive)
                windowType.Activate();
            doc.Create.NewFamilyInstance(winPoint, windowType, wall, level1, StructuralType.NonStructural);
            tx.Commit();
        }
    }
}
