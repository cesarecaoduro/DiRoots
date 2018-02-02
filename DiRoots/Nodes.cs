using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Revit.Elements;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Persistence;
using DiRoots;
using Revit.Elements;
using DSCore;
using System.Collections;

namespace DiRoots.QA
{
    public static class Documents
    {

        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        [IsVisibleInDynamoLibrary(false)]
        [MultiReturn(new[] {"Message"})]
        public static  Dictionary<string, object> NodeTemplate()
        {
            string msg = "Executed";

            try
            {
            }
            catch(Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxLinkedRVT"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] {"LinkNames", "IsShared", "ScoreMaxLinks", "ScoreSharedLinks" })]
        public static Dictionary<string, object> GetLinkedRVT(double MaxLinkedRVT  = 10, double MaxScore = 5)
        {
            string msg = "Executed";
            List<string> linkNames = new List<string>();
            List<Boolean> shared = new List<bool>();
            double scoreMaxLinks = -1;
            double scoreSharedLinks = -1;

            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                double countShared = 0;
                if (collector.Count() > 0)
                {
                    foreach (Autodesk.Revit.DB.Element c in collector)
                    {
                        if (c.Name.ToString().Contains("<Not Shared>"))
                        {
                            shared.Add(true);
                            countShared++;
                        }
                        else
                        {
                            shared.Add(false);
                        }
                        linkNames.Add(c.Name);
                    }
                    scoreMaxLinks = f.GetInverseScore(shared.Count, MaxLinkedRVT, MaxScore);
                    scoreSharedLinks = f.GetInverseScore(countShared, shared.Count, MaxScore);
                }
                else
                {
                    msg = "No Linked files available";
                }
            }

            
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }


            return new Dictionary<string, object>
            {
                { "LinkNames", linkNames },
                { "IsShared", shared },
                { "ScoreMaxLinks", scoreMaxLinks },
                { "ScoreSharedLinks", scoreSharedLinks },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxCADInstances"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "CADInstanceNames", "IsLinked", "ScoreMaxCADInstances", "ScoreLinkedInstances" })]
        public static Dictionary<string, object> GetCADImportedInstances(double MaxCADInstances = 10, double MaxScore = 5)
        {
            string msg = "Executed";
            List<string> linkNames = new List<string>();
            List<Boolean> linked = new List<bool>();
            double scoreMaxInstances = -1;
            double scoreLinkedInstances = -1;

            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.ImportInstance)).WhereElementIsNotElementType();
                double countLinked = 0;
                if (collector.Count() > 0)
                {
                    foreach (Autodesk.Revit.DB.ImportInstance c in collector)
                    {
                        
                        if (c.IsLinked)
                        {
                            linked.Add(true);
                            countLinked++;
                        }
                        else
                        {
                            linked.Add(false);
                        }
                        linkNames.Add(c.Category.Name);
                    }
                    scoreMaxInstances = f.GetInverseScore(linked.Count, MaxCADInstances, MaxScore);
                    scoreLinkedInstances = f.GetInverseScore(countLinked, linked.Count, MaxScore);
                }
                else
                {
                    msg = "No Linked files available";
                }
            }
            catch(Exception ex)
            {
                msg = "Error: " + ex.Message;
            }


            return new Dictionary<string, object>
            {
                { "CADInstanceNames", linkNames },
                { "IsLinked", linked },
                { "ScoreMaxCADInstances", scoreMaxInstances },
                { "ScoreLinkedInstances", scoreLinkedInstances },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxCADInstances"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "FilePath", "FileSize[MB]", "FileSize[KB]", "ScoreFileSize", "BasicInfo" })]
        public static Dictionary<string, object> GetFileInfo(double MaxFileDimension = 150, double MaxScore = 5)
        {
            string msg = "Executed";
            double scoreMaxFileSize = -1;
            string filePath = null;
            double fileSizeMB = 0;
            double fileSizeKB = 0;
            List<string> basicInfo = new List<string>();

            try
            {
                filePath = doc.PathName;
                System.IO.FileInfo fI = new System.IO.FileInfo(filePath);
                long length = fI.Length;
                fileSizeMB = DSCore.Math.Round(length / f.mb, 3);
                fileSizeKB = length / f.kb;
                scoreMaxFileSize = f.GetInverseScore(fileSizeMB, MaxFileDimension, MaxScore);
                BasicFileInfo bI = BasicFileInfo.Extract(filePath);
                basicInfo.AddRange(new List<string>{
                    "Saved in version: " + bI.SavedInVersion,
                    "Is workshared: " + bI.IsWorkshared.ToString(),
                    "Username: " + bI.Username,
                });
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }


            return new Dictionary<string, object>
            {
                { "FilePath", filePath },
                { "FileSize[MB]", fileSizeMB },
                { "FileSize[KB]", fileSizeKB },
                { "ScoreFileSize", scoreMaxFileSize },
                { "BasicInfo", basicInfo },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxDetailGroup"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "DetailGroups", "DetailGroupUniqueNames", "ScoreDetailGroups"})]
        public static Dictionary<string, object> GetDetailGroup(double MaxDetailGroup = 10, double MaxScore = 5)
        {
            string msg = "Executed";
            List<Revit.Elements.Element> detailGroups = new List<Revit.Elements.Element>();
            List<string> detailGroupsNames = new List<string>();
            List<dynamic> obj = new List<dynamic> { -1, -1 };
            double scoreMaxDetailGroups = -1;

            try
            {
                FilteredElementCollector collector1 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSDetailGroups).WhereElementIsNotElementType();
                FilteredElementCollector collector2 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSAttachedDetailGroups).WhereElementIsNotElementType();
                if (collector1.Count() > 0 || collector2.Count() > 0)
                {
                    foreach (Autodesk.Revit.DB.Element c in collector1)
                    {
                        detailGroups.Add(c.ToDSType(true));
                        detailGroupsNames.Add(c.Name);
                    }
                    foreach (Autodesk.Revit.DB.Element c in collector2)
                    {
                        detailGroups.Add(c.ToDSType(true));
                        detailGroupsNames.Add(c.Name);
                    }
                    
                    ICollection coll = List.GroupByKey(detailGroups, detailGroupsNames).Values;
                    obj.Clear();
                    foreach (object o in coll)
                    {
                        obj.Add(o);   
                    }
                    scoreMaxDetailGroups = f.GetInverseScore(obj[1].Count, MaxDetailGroup, MaxScore);
                }
                else
                {
                    msg = "No Detail Groups available";
                }
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }


            return new Dictionary<string, object>
            {
                { "DetailGroups", obj[0]},
                { "DetailGroupUniqueNames", obj[1]},
                { "ScoreDetailGroups", scoreMaxDetailGroups },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxModelGroup"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "ModelGroups", "ModelGroupUniqueNames", "ScoreModelGroups" })]
        public static Dictionary<string, object> GetModelGroup(double MaxModelGroup = 10, double MaxScore = 5)
        {
            string msg = "Executed";
            List<Revit.Elements.Element> modelGroups = new List<Revit.Elements.Element>();
            List<string> modelGroupsNames = new List<string>();
            List<dynamic> obj = new List<dynamic> { -1, -1 };
            double scoreMaxModelGroups = -1;

            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSModelGroups).WhereElementIsNotElementType();
                if (collector.Count() > 0 )
                {
                    foreach (Autodesk.Revit.DB.Element c in collector)
                    {
                        modelGroups.Add(c.ToDSType(true));
                        modelGroupsNames.Add(c.Name);
                    }
                    
                    ICollection coll = List.GroupByKey(modelGroups, modelGroupsNames).Values;
                    obj.Clear();
                    foreach (object o in coll)
                    {
                        obj.Add(o);
                    }
                    scoreMaxModelGroups = f.GetInverseScore(obj[1].Count, MaxModelGroup, MaxScore);
                }
                else
                {
                    msg = "No Model Groups available";
                }
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }


            return new Dictionary<string, object>
            {
                { "ModelGroups", obj[0]},
                { "ModelGroupUniqueNames", obj[1]},
                { "ScoreModelGroups", scoreMaxModelGroups },
                { "Message", msg },
            };
        }


    }

    public static class Views
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();


        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "StructuralPlan", "CrossSection", "FloorPlan", "CeilingPlan", "BuildingElevation", "3DView", "Detail", "Legend","Schedules", "TotalNumber", "Message" })]
        public static Dictionary<string, object> GetProjectViewsByType()
        {
            string msg = "Executed";
            List<Revit.Elements.Element> structuralPlans = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> crossSection = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> floorPlan = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> ceilingPlan = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> buildingElevation = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> treDView = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> details = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> legends = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> schedule = new List<Revit.Elements.Element>();
            int tot = 0;

            try
            {
                FilteredElementCollector coll = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType();
                tot = coll.Count();
                foreach (View v in coll)
                {
                    ViewType vT = v.ViewType;
                    switch (vT)
                    {
                        case ViewType.CeilingPlan: ceilingPlan.Add(v.ToDSType(true)); break;
                        case ViewType.Elevation: buildingElevation.Add(v.ToDSType(true)); break;
                        case ViewType.Schedule: schedule.Add(v.ToDSType(true)); break;
                    }
                }

            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "StructuralPlan", structuralPlans },
                { "CrossSection", crossSection },
                { "FloorPlan", floorPlan },
                { "CeilingPlan", ceilingPlan },
                { "BuildingElevation", buildingElevation },
                { "3DView", treDView },
                { "Detail", details },
                { "Legend", legends },
                { "Schedule", schedule },
                { "TotalNumber", tot },
                { "Message", msg },
            };
        }


    }
}
