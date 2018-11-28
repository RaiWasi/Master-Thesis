using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUC.Data.HDMap;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Baselabs.Data.Coordinates;
using ServiceHDMapPedPathPredition;

namespace erviceHDMapPedPathPredition
{
    public class HDMapReader : TrajectoryPredictor
    {
        #region instance fields

        readonly MapDatabase _mapDatabase;
       
        #endregion

        public HDMapReader(UTM utm, PointQ gpsCoordinate_previous)
        {
            //Reading HDMap.bin binary file from MapDatabase
            _mapDatabase = MapDatabase.LoadFromFile("HDMap.bin");

            //Extract Road Surfaces form MapDatabase
            System.Collections.ObjectModel.ReadOnlyCollection<RoadSurface> rsurf = new System.Collections.ObjectModel.ReadOnlyCollection<RoadSurface>(_mapDatabase.GetRoadSurfaces());

            //Initialize Pedestrian GPS coordinates of type PoinQ from Recorded Stream
            PointQ gpsCoordinates = new PointQ { X = utm.X, Y = utm.Y }; 
           
            Console.WriteLine();
            foreach (RoadSurface rs in rsurf) 
            {
                //Initialize Polygon List
                List<PointQ> polygon = new List<PointQ>();

                //Initialize a list of type PointQ with all possible end points/coordinates
                List<PointQ> allEndPoints = new List<PointQ>();

                //Extract Accessors from Road Surfaces
                System.Collections.ObjectModel.ReadOnlyCollection<Accessor> accessors = new System.Collections.ObjectModel.ReadOnlyCollection<Accessor>(rs.GetAccessors());
                foreach (Accessor a in accessors)
                {
                    //Console.WriteLine("Id: " + a.Id.ToString());
                    // Console.WriteLine("LeftRoadSurface: " + a.LeftRoadSurface.ToString());
                    // Console.WriteLine("Line: " + a.Line.ToString());
                    // Console.WriteLine("Point A:" + a.Line.Begin.ToString());
                    // Console.WriteLine("Point B:" + a.Line.End.ToString());
                    // Console.WriteLine("RightRoadSurface: " + a.RightRoadSurface);
                    // Console.WriteLine("StartingConnections: " + a.StartingConnections.ToString());

                    //Creat Road surface points using Accessors as boundary points
                    PointQ strt = new PointQ();
                    strt.X = a.Line.Begin.X;
                    strt.Y = a.Line.Begin.Y;

                    PointQ finish = new PointQ();
                    finish.X = a.Line.End.X;
                    finish.Y = a.Line.End.Y;

                    polygon.Add(strt);
                    polygon.Add(finish);

                    //Extract Lanes Types
                    System.Collections.Generic.IList<Lane> lane = new List<Lane>(rs.GetLanes());
                    foreach (Lane l in lane)
                    {
                    }

                }
                //Extract Borders 
                System.Collections.Generic.IList<Border> border = new List<Border>(rs.GetBorders());
                System.Collections.Generic.IList<IParametricCurve> outline = new List<IParametricCurve>(rs.GetOutline());
                //Extract Connections
                System.Collections.Generic.IList<TUC.Data.HDMap.Connection> connections = new List<TUC.Data.HDMap.Connection>(rs.GetConnections());

                foreach (Connection c in connections)
                {
                    //  Console.WriteLine("Connection 1 Begin Point: " + ((TUC.Data.HDMap.QuinticHermiteCurve)c.CenterCurve).Begin.ToString());
                    // Console.WriteLine("Connection 1 End Point: " + ((TUC.Data.HDMap.QuinticHermiteCurve)c.CenterCurve).End.ToString());

                    //Inialize Connection Points
                    PointQ begin = new PointQ();
                    begin.X = ((QuinticHermiteCurve)c.CenterCurve).Begin.X;
                    begin.Y = ((QuinticHermiteCurve)c.CenterCurve).Begin.Y;

                    PointQ end = new PointQ();
                    end.X = ((QuinticHermiteCurve)c.CenterCurve).End.X;
                    end.Y = ((QuinticHermiteCurve)c.CenterCurve).End.Y;

                    //List<PointQ> allBeginPoints = new List<PointQ>();

                    // List<PointQ> allEndPoints = new List<PointQ>();

                    allEndPoints.Add(begin);
                    allEndPoints.Add(end);

                    //  Console.WriteLine("All Points:" + allEndPoints.ToString());

                    //allEndPoints.Add(Connection.endpoint); // add all connection end points like this


                    //AlgorithProbabailty(gpsCoordinates, allPoints);
                    // allEndPoints.Add((TUC.Data.HDMap.QuinticHermiteCurve)c.CenterCurve).Begin;
                    // allEndPoints.Add((QuinticHermiteCurve)c.CenterCurve).Begin;
                    // allEndPoints.Add((TUC.Data.HDMap.QuinticHermiteCurve)c.CenterCurve).

                }
           //
            bool x = IsPointInPolygon(polygon, new PointQ { X = utm.X, Y = utm.Y });
            if (x == true)
            {
                Console.WriteLine("Pedestriian is Inside of Surface ID: " + rs.Id.ToString());
                Console.ReadLine();
                    predictTrajectory(gpsCoordinates, allEndPoints, gpsCoordinate_previous);
                }
            else
            {
                Console.WriteLine("Pedestrian is outside of surface ID: " + rs.Id.ToString());
                Console.ReadLine();
            }

                // TrajectoryPredictor tp = new TrajectoryPredictor();
               
            //   tp.predictTrajectory(gpsCoordinates, allEndPoints);
            // tp.gpsCoordinate_previous = gpsCoordinates;
        }
}
        //Pedestrian GPS Point in Road Surface for current Road Surace Determination
        public static bool IsPointInPolygon( List<PointQ> polygon, PointQ p)
        {
            double minX = polygon[0].X;
            double maxX = polygon[0].X;
            double minY = polygon[0].Y;
            double maxY = polygon[0].Y;
            for (int i = 1; i < polygon.Count; i++)
            {
                // Determine Minimum & Maximum boundary points of Road Surface
                PointQ q = polygon[i];  
                minX = Math.Min(q.X, minX);
                maxX = Math.Max(q.X, maxX);
                minY = Math.Min(q.Y, minY);
                maxY = Math.Max(q.Y, maxY);
            }

            //Jordan curve theorem
            if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY) 
            {
                return false;
            }

            bool inside = false; 
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y) &&
                     p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    inside = !inside; 
                }
            }

            return inside;
        }
    }
}


















































































