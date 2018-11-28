using erviceHDMapPedPathPredition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHDMapPedPathPredition
{
   public class TrajectoryPredictor
    {
        public PointQ gpsCoordinate_current = null;
        // public PointQ gpsCoordinate_previous ;
        List<ProbabilityMatrix> listProbabilityMatrix = new List<ProbabilityMatrix>();
        int probabilityCount = 0;
        List<PointQ> validDestination = new List<PointQ>();
        private int flag = 0;


        public void predictTrajectory(PointQ gpsCoordinates, List<PointQ> allEndPoints, PointQ gpsCoordinate_previous)
        {
            gpsCoordinate_current = gpsCoordinates;

            // {
            //     gpsCoordinate_previous = gpsCoordinate_current;
            //    flag = 1;
            // }

            //Initial condition to check if there exists a previous coordinate
            if (gpsCoordinate_previous == null && gpsCoordinate_current != null) 
            {
                //No prediction

            }
            else
            {
                //Initialize a list of type PointQ with all possible end points/coordinates
                List<PointQ> destinationPoints = allEndPoints;  

                // gpsCoordinate_previous = gpsCoordinate_current;
                makePrediction(gpsCoordinate_current, gpsCoordinate_previous, destinationPoints);
                // gpsCoordinate_previous = gpsCoordinate_current;
            }
            // return  gpsCoordinate_previous;
        }

        private void makePrediction(PointQ currentCoordinate, PointQ previousCoordinate, List<PointQ> allEndPoints)
        {
            foreach (PointQ endPoint in allEndPoints)

            {

                Double previous_distance = GetDistance(previousCoordinate, endPoint);
                Double current_distance = GetDistance(currentCoordinate, endPoint);

                //distance to endpoint is increasing which means that the pedestrian is moving away from this point and therefore moving towards another direction
                if (current_distance > previous_distance)    
                {
                    //rule out this destination/Path
                    ProbabilityMatrix pM = new ProbabilityMatrix(endPoint, false);
                    listProbabilityMatrix.Add(pM);
                }

                //if distance is decreasing towards a certain endpoint this means that pedestrian is moving near to that endpoint

                else
                {

                    ProbabilityMatrix pM = new ProbabilityMatrix(endPoint, true);
                    listProbabilityMatrix.Add(pM);
                }
            }

            foreach (ProbabilityMatrix pM in listProbabilityMatrix)

            {
                //Add valid paths for calculating probabilty
                if (pM.isValid == true)
                {
                    probabilityCount += 1;
                    validDestination.Add(pM.allEndPoints);
                }
                else
                {
                    //Dont include in calculating probabilty
                }
            }

            //Total number of Paths/Routes => 6 in our case
            double totalnumberofendpoints = allEndPoints.Count();

            // total number of valid endpoints => 3 in our case
            double totalnumberofValidDestinations = validDestination.Count();

            foreach (PointQ p in validDestination)

            {
                double probability = (1 / totalnumberofValidDestinations) * 100.00;
                Console.WriteLine("The probability of pedestrian reaching the point (" + p.X + ", " + p.Y + ") is :" + probability.ToString());

                //Write Probability of Pedestrian of Valid paths in CSV file
                using (FileStream fs = new FileStream("Ped.csv", FileMode.Append, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write("The probability of pedestrian reaching the point (" + p.X + ", " + p.Y + ") is :" + probability.ToString());
                    sw.Close();
                }
            }
        }

        private static double GetDistance(PointQ point1, PointQ point2)
        {
            //Pythagorean theorem for distance calculation
           
            double a = (double)(point2.X - point1.X);
            double b = (double)(point2.Y - point1.Y);

            return Math.Sqrt(a * a + b * b);
        }

        internal void predictTrajectory(object gpsCoordinates, object allEndPoints)
        {
            throw new NotImplementedException();
        }
    }
}





