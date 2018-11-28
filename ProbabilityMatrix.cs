using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erviceHDMapPedPathPredition
{
    public class ProbabilityMatrix
    {
        public PointQ allEndPoints;
       // public PointQ all accessorpoints;
        public PointQ gpsCoordinate_current { get; internal set; }
        public PointQ gpsCoordinate_previous { get; internal set; }

        //pointQ is endpoint and bool indicates if endpoint is valid or invalid
        public bool isValid; 

        public ProbabilityMatrix(PointQ allEndPoints, bool isValid)
        {
            this.allEndPoints = allEndPoints;
            this.isValid = isValid;
        }
    }
}
