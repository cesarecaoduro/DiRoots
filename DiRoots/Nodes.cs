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
using Dynamo.Graph.Nodes;

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
        /// <param name="MaxFileDimension"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "FloorPlan", "StructuralPlan", "CeilingPlan", "3D", "Section", "Elevation", "DraftingView", "Legend", "Detail", "Schedule", "Viewports", "TotalNumber", "ViewsOnSheet", "Message" })]
        public static Dictionary<string, object> GetProjectViewsByType()
        {
            string msg = "Executed";
            List<Revit.Elements.Element> floorPlan = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> ceilingPlan = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> draftingView = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> sections = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> structuralPlan = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> elevations = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> legends = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> schedules = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> threeD = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> viewports = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> details = new List<Revit.Elements.Element>();
            int tot = 0;

            try
            {
                ElementClassFilter filterView = new ElementClassFilter(typeof(View));
                ElementClassFilter filterSchedule = new ElementClassFilter(typeof(ViewSchedule));
                IList<ElementFilter> filters = new List<ElementFilter> { filterView, filterSchedule };

                LogicalOrFilter orFilter = new LogicalOrFilter(filters);

                FilteredElementCollector coll = new FilteredElementCollector(doc).WherePasses(orFilter).WhereElementIsNotElementType();
                foreach (View v in coll)
                {
                    if (!v.IsTemplate)
                    {
                        ViewType vT = v.ViewType;
                        switch (vT)
                        {
                            case ViewType.FloorPlan: floorPlan.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.CeilingPlan: ceilingPlan.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.Section: sections.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.Elevation: elevations.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.EngineeringPlan: structuralPlan.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.DraftingView: draftingView.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.Legend: legends.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.Schedule: schedules.Add(v.ToDSType(true)); tot++; break;
                            case ViewType.Detail: details.Add(v.ToDSType(true)); tot++; break;
                                
                        }
                        
                    }
                }

                FilteredElementCollector viewp = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Viewports).WhereElementIsNotElementType();
                foreach (Viewport c in viewp)
                {
                    viewports.Add(c.ToDSType(true));
                }

                FilteredElementCollector sheets = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType();
                foreach (ViewSheet c in sheets)
                {
                    FilteredElementCollector sched = new FilteredElementCollector(doc, c.Id).OfClass(typeof(ScheduleSheetInstance)).WhereElementIsNotElementType();
                    foreach (ScheduleSheetInstance s in sched)
                    {
                        viewports.Add(doc.GetElement(s.ScheduleId).ToDSType(true));

                    }
                }


            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {

                { "FloorPlan", floorPlan },
                { "StructuralPlan", structuralPlan },
                { "CeilingPlan", ceilingPlan },
                { "3D", threeD },
                { "Section", sections },
                { "Elevation", elevations },
                { "DraftingView", draftingView },
                { "Legend", legends },
                { "Detail", details },
                { "Schedule", schedules },
                { "Viewports", viewports },
                { "TotalNumber", tot },
                { "ViewsOnSheet", viewports.Count },
                { "Message", msg },
            };
        }


    }

    public static class Alerts
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "FailureMessageDescription", "FailureSeverity", "FailingElements" })]
        [CanUpdatePeriodically(true)]
        public static Dictionary<string, object> GetWarnings()
        {
            string msg = "Executed";
            IList<Autodesk.Revit.DB.FailureMessage> fMessages = doc.GetWarnings();
            List<string> failureDescription = new List<string>();
            List<List<Revit.Elements.Element>> failingElements = new List<List<Revit.Elements.Element>>();
            List<string> failureSeverity = new List<string>();

            try
            {
                foreach (Autodesk.Revit.DB.FailureMessage f in fMessages)
                {
                    List<Revit.Elements.Element> el = new List<Revit.Elements.Element>();
                    failureDescription.Add(f.GetDescriptionText());
                    ICollection<ElementId> elements = f.GetFailingElements();
                    foreach (ElementId e in elements)
                    {
                        el.Add(doc.GetElement(e).ToDSType(true));
                    }
                    failureSeverity.Add(f.GetSeverity().ToString());
                    failingElements.Add(el);
                }

            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "FailureMessageDescription", failureDescription },
                { "FailureSeverity", failureSeverity },
                { "FailingElements", failingElements },
                { "Message", msg },
            };
        }

    }

    public static class Parameters
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParameterList"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "ParameterFound", "ParameterNotFound", "TotalCheckedParameters", "TotalFoundParameters", "TotalNotFoundParameters", "ParameterElements", "Score" })]
        public static Dictionary<string, object> CheckListOfParameters(List<string> ParameterList, double MaxScore = 5)
        {
            string msg = "Executed";
            List<string> parameterFound = new List<string>();
            List<string> parameterNotFound = new List<string>();
            int totalCheckedParameter = ParameterList.Count;
            int totalFoundParameter = 0;
            int totalNotFoundParameter = 0;
            List<ParameterElement> parameterElement = new List<ParameterElement>();
            double score = -1;

            try
            {
                FilteredElementCollector paramElement = new FilteredElementCollector(doc).OfClass(typeof(ParameterElement));
                foreach (string  s in ParameterList)
                {
                    bool found = false;
                    foreach (ParameterElement p in paramElement)
                    {
                        if (s == p.Name)
                        {
                            found = true;
                            parameterElement.Add(p);
                        }
                    }
                    if (found)
                    {
                        parameterFound.Add(s);
                        totalFoundParameter++;
                    }
                    else
                    {
                        parameterNotFound.Add(s);
                        totalNotFoundParameter++;
                    }
                }
                score = f.GetScore(totalFoundParameter, totalCheckedParameter, MaxScore);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "ParameterFound", parameterFound },
                { "ParameterNotFound", parameterNotFound },
                { "TotalCheckedParameters", totalCheckedParameter },
                { "TotalFoundParameters", totalFoundParameter },
                { "TotalNotFoundParameters", totalNotFoundParameter },
                { "ParameterElements", parameterElement },
                { "Score", score },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParameterElements"></param>
        /// <param name="CategoriesList"></param>
        /// <param name="MaxScore"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        [CanUpdatePeriodically(true)]
        [MultiReturn(new[] { "ParameterFound", "Score"})]
        public static Dictionary<string, object> CheckParametersBinding(List<ParameterElement> ParameterElements, List<List<string>> CategoriesList, double MaxScore = 5)
        {
            string msg = "Executed";
            double score = -1;
            BindingMap bindings = doc.ParameterBindings;

            for (int i = 0; i < ParameterElements.Count; i++)
            {
                Definition def =  ParameterElements[i].GetDefinition();
                bindings.get_Item(def);
            }

            try
            {
               
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "Score", score },
                { "Message", msg },
            };
        }

    }
}
