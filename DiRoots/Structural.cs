using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitServices.Persistence;
using DiRoots;
//using Revit.Elements;
using DSCore;
using System.Collections;


namespace DiRoots.Structural
{
    public static class Rebars
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] {"Host"})]
        public static Dictionary<string, object> GetHost(Rebar rebar)
        {
            string msg = "Executed";
            Element host = null;
            try
            {
                host = doc.GetElement(rebar.GetHostId());

            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "Host", host },
                { "Message", msg },
            };
        }
    }
}
