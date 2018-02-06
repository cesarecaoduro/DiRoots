using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Persistence;

namespace DiRoots
{
    [IsVisibleInDynamoLibrary(false)]
    public class Functions
    {

        public double mb = 1048576.0;
        public double kb = 1024.0;

        [IsVisibleInDynamoLibrary(true)]
        public double GetInverseScore(double f, double tot, double maxScore)
        {
            double percentage = f * 100 / tot;
            double score = maxScore - Math.Round(percentage * maxScore / 100, 2);
            return score;
        }

        [IsVisibleInDynamoLibrary(true)]
        public double GetScore(double f, double tot, double maxScore)
        {
            double percentage = f * 100 / tot;
            double score = Math.Round(percentage * maxScore / 100, 2);
            return score;
        }

    }
}
