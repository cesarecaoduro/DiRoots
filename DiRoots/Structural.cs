using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitServices.Persistence;
using Revit.Elements;
using ads = Autodesk.DesignScript.Geometry;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Autodesk.DesignScript.Geometry;
using DiRoots;

namespace Structural
{
    /// <summary>
    /// 
    /// </summary>
    public static class Query
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Rebar"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] {"Host"})]
        public static Dictionary<string, object> GetHost(Revit.Elements.Element Rebar)
        {
            string msg = "Executed";
            Rebar rebRevit = doc.GetElement(Rebar.UniqueId.ToString()) as Rebar;
            Revit.Elements.Element host = null;

            try
            {
                host = doc.GetElement(rebRevit.GetHostId()).ToDSType(true);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Rebar"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "RebarStyle", "RebarBarType", "RebarHookTypeAtStart", "RebarHookTypeAtEnd", "RebarHookOrientationAtStart", "RebarHookOrientationAtEnd", "RebarShape", "RebarDiameter"})]
        public static Dictionary<string, object> GetRebarProperties(Revit.Elements.Element Rebar)
        {
            string msg = "Executed";
            Rebar rebRevit = doc.GetElement(Rebar.UniqueId.ToString()) as Rebar;
            ///Revit.Elements.Element rebarBarType = null;
            Revit.Elements.Element host = null;
            Revit.Elements.Element rebarBarType = null;
            Revit.Elements.Element rebarHookTypeAtStart = null;
            Revit.Elements.Element rebarHookTypeAtEnd = null;
            Revit.Elements.Element rebarShape = null;
            string rebarHookOrientationAtStart = "Left";
            string rebarHookOrientationAtEnd = "Left";
            string rebarStyle = null;
            double rebarDiameter = 0;

            try
            {
                rebarStyle = rebRevit.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_STYLE).AsValueString();
                rebarBarType = doc.GetElement(rebRevit.GetTypeId()).ToDSType(true);
                rebarHookTypeAtStart = doc.GetElement(rebRevit.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId()).ToDSType(true);
                rebarHookTypeAtEnd = doc.GetElement(rebRevit.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId()).ToDSType(true);
                rebarHookOrientationAtStart = rebRevit.GetHookOrientation(0).ToString();
                rebarHookOrientationAtEnd = rebRevit.GetHookOrientation(1).ToString();
                rebarShape = doc.GetElement(rebRevit.GetShapeId()).ToDSType(true);
                rebarDiameter = rebRevit.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble() * 304.8;
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "RebarStyle", rebarStyle },
                { "RebarBarType", rebarBarType },
                { "RebarHookTypeAtStart", rebarHookTypeAtStart },
                { "RebarHookTypeAtEnd", rebarHookTypeAtEnd },
                { "RebarHookOrientationAtStart", rebarHookOrientationAtStart },
                { "RebarHookOrientationAtEnd", rebarHookOrientationAtEnd },
                { "RebarShape", rebarShape },
                { "RebarDiameter", rebarDiameter },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Rebar"></param>
        /// <param name="AdjustForSelfIntersection"></param>
        /// <param name="SuppressHooks"></param>
        /// <param name="SuppressBendRadius"></param>
        /// <param name="MultiPlanarOption"></param>
        /// <param name="BarPositionIndex"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] {"PolyCurve" })]
        public static Dictionary<string, object> GetCenterLineCurves(Revit.Elements.Element Rebar, bool AdjustForSelfIntersection = false ,bool SuppressHooks = true, bool SuppressBendRadius = true, bool MultiPlanarOption = true, int BarPositionIndex = 0)
        {
            string msg = "Executed";
            Rebar rebRevit = doc.GetElement(Rebar.UniqueId.ToString()) as Rebar;
            IList<Autodesk.Revit.DB.Curve> centerLineCurves = new List<Autodesk.Revit.DB.Curve>();
            List<ads.Curve> curves = new List<ads.Curve>();
            ads.PolyCurve polyCurve = null;
            MultiplanarOption mp = new MultiplanarOption();
            


            if (MultiPlanarOption == true) { mp = MultiplanarOption.IncludeOnlyPlanarCurves;}
            else { mp = MultiplanarOption.IncludeAllMultiplanarCurves;};

            try
            {
                centerLineCurves =  rebRevit.GetCenterlineCurves(
                    AdjustForSelfIntersection,
                    SuppressHooks,
                    SuppressBendRadius,
                    mp,
                    BarPositionIndex)
                    ;

                foreach(Autodesk.Revit.DB.Curve c in centerLineCurves)
                {
                    curves.Add(RevitToProtoCurve.ToProtoType(c, true));
                }
                polyCurve = PolyCurve.ByJoinedCurves(curves);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "PolyCurve", polyCurve },
                { "Message", msg },
            };
        }

        
    }

    public static class Create
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="Path"></param>
        /// <param name="PolyCurve"></param>
        /// <param name="DistanceFromStart"></param>
        /// <param name="DistanceFromEnd"></param>
        /// <param name="DistanceBetweenRebars"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "Distances", "Parameters", "Points", "CoordinateSystems", "Shapes" })]
        public static Dictionary<string, object> DistributionByShapeAndPath(
            Revit.Elements.Element Host, 
            ads.Curve Path,
            ads.PolyCurve PolyCurve, 
            double DistanceFromStart = 0, 
            double DistanceFromEnd = 0,
            double DistanceBetweenRebars = 200)

        {

            string msg = "Executed";
            Autodesk.Revit.DB.Element host = doc.GetElement(Host.UniqueId.ToString());
            List<double> distances = new List<double>();
            List<double> crvParameters = new List<double>();
            List<ads.Point> points = new List<ads.Point>();
            List<CoordinateSystem> css = new List<CoordinateSystem>();
            ads.Point appPoint = PolyCurve.StartPoint;
            ads.Vector pCurveNormal = PolyCurve.Normal;
            ads.Plane pCurvePlane = ads.Plane.ByOriginNormal(appPoint, pCurveNormal);
            List<ads.Geometry> shapes = new List<ads.Geometry>();

            ads.Point intersectPoint = Path.Intersect(pCurvePlane).First() as ads.Point;
            CoordinateSystem csIntersectParam = Path.CoordinateSystemAtParameter(Path.ParameterAtPoint(intersectPoint));


            try
            {
                    double crvLen = Path.Length;

                    for (double dist = DistanceFromStart; dist < (crvLen - DistanceFromEnd); dist += DistanceBetweenRebars)
                    {
                        distances.Add(dist);
                        crvParameters.Add(Path.ParameterAtSegmentLength(dist));
                        points.Add(Path.PointAtSegmentLength(dist));
                        css.Add(Path.CoordinateSystemAtDistance(dist));
                        shapes.Add(PolyCurve.Transform(csIntersectParam, Path.CoordinateSystemAtDistance(dist)));
                        Vector normal = Path.NormalAtParameter(Path.ParameterAtSegmentLength(dist));
                        XYZ nVector = new XYZ(normal.X, normal.Y, normal.Z);
                    }
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "Distances", distances },
                { "Parameters", crvParameters },
                { "Points", points },
                { "CoordinateSystems", css },
                { "Shapes", shapes },
                { "Message", msg },
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="RebarStyle"></param>
        /// <param name="RebarBarType"></param>
        /// <param name="RebarHookTypeAtStart"></param>
        /// <param name="RebarHookTypeAtEnd"></param>
        /// <param name="RebarHookOrientationAtStart"></param>
        /// <param name="RebarHookOrientationAtEnd"></param>
        /// <param name="PolyCurves"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "Rebars" })]
        public static Dictionary<string, object> RebarByCurves(
            Revit.Elements.Element Host,
            string RebarStyle,
            Revit.Elements.Element RebarBarType,
            Revit.Elements.Element RebarHookTypeAtStart,
            Revit.Elements.Element RebarHookTypeAtEnd,
            string RebarHookOrientationAtStart,
            string RebarHookOrientationAtEnd,
            List<ads.PolyCurve> PolyCurves,
            bool IsRevitOwned = false
            )
        {
            string msg = "Executed";
            List<Revit.Elements.Element> dynRebar = new List<Revit.Elements.Element>();


            ElementId hostId = new ElementId(Host.Id);
            Autodesk.Revit.DB.Element h = doc.GetElement(hostId);

            RebarBarType rebType = RebarBarType.InternalElement as RebarBarType;

            RebarHookType startHook = RebarHookTypeAtStart == null ? null : (RebarHookType)RebarHookTypeAtStart.InternalElement;
            //RebarHookType startHook = RebarHookTypeAtStart.InternalElement as RebarHookType;
            RebarHookType endHook = RebarHookTypeAtEnd == null ? null : (RebarHookType)RebarHookTypeAtEnd.InternalElement;
            //RebarHookType endHook = RebarHookTypeAtEnd.InternalElement as RebarHookType;
            RebarHookOrientation rebHookOrStart, rebHookOrEnd;
            RebarStyle rebStyle;
            Rebar reb = null;

            if (RebarStyle == "Standard") { rebStyle = Autodesk.Revit.DB.Structure.RebarStyle.Standard; } else { rebStyle = Autodesk.Revit.DB.Structure.RebarStyle.StirrupTie; } 
            List<List<Autodesk.Revit.DB.Curve>> revitCurves = new List<List<Autodesk.Revit.DB.Curve>>();

            if (RebarHookOrientationAtStart == "Left") { rebHookOrStart = RebarHookOrientation.Left; } else { rebHookOrStart = RebarHookOrientation.Right; }
            if (RebarHookOrientationAtEnd == "Left") { rebHookOrEnd = RebarHookOrientation.Left; } else { rebHookOrEnd = RebarHookOrientation.Right; }
            

            try
            {
                
                Transaction tx = new Transaction(doc);
                TransactionManager.Instance.EnsureInTransaction(doc);

                string str = "";
                foreach (var p in PolyCurves)
                {
                    XYZ normal = GeometryPrimitiveConverter.ToRevitType(p.Normal);
                    var curves = p.Curves();
                    List<Autodesk.Revit.DB.Curve> convCurves = new List<Autodesk.Revit.DB.Curve>();

                    foreach (var c in curves)
                    {
                        convCurves.Add(ProtoToRevitCurve.ToRevitType(c, true));
                        Autodesk.Revit.DB.Curve cc = ProtoToRevitCurve.ToRevitType(c, true);
                        str += c.ToString();
                    }
                        
                    try
                    {
                            reb = Rebar.CreateFromCurves(
                            doc,
                            rebStyle,
                            rebType,
                            startHook,
                            endHook,
                            h,
                            normal,
                            convCurves,
                            rebHookOrStart,
                            rebHookOrEnd,
                            true,
                            true
                            );
                    }
                    catch (Exception ex)
                    {
                        msg = "Error 2: " + ex.Message;
                    }

                    dynRebar.Add(reb.ToDSType(IsRevitOwned));
                    revitCurves.Add(convCurves);
                }
                TransactionManager.Instance.TransactionTaskDone();
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            ;

            return new Dictionary<string, object>
            {
                { "Rebars", dynRebar },
                { "Message", msg },
            };
        }

    }

    public static class Visibility
    {
        public static Document doc = DocumentManager.Instance.CurrentDBDocument;
        public static UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
        public static UIDocument uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        public static Functions f = new Functions();

        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "Message" })]
        public static Dictionary<string, object> SetSolidInView(Revit.Elements.Views.View View, List<Revit.Elements.Element> Rebars, bool SetSolid = true)
        {
            string msg = "Executed";
            try
            {
                Autodesk.Revit.DB.View3D view = doc.GetElement(View.InternalElement.Id) as Autodesk.Revit.DB.View3D;
                TransactionManager.Instance.EnsureInTransaction(doc);
                foreach(Revit.Elements.Element reb in Rebars)
                {
                   Rebar r = doc.GetElement(reb.InternalElement.Id) as Rebar;
                    r.SetSolidInView(view, SetSolid);
                }
                TransactionManager.Instance.TransactionTaskDone();
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "Message", msg },
            };
        }

        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "Message" })]
        public static Dictionary<string, object> SetUnobscuredInView(Revit.Elements.Views.View View, List<Revit.Elements.Element> Rebars, bool SetUnobscured = true)
        {
            string msg = "Executed";
            try
            {
                Autodesk.Revit.DB.View3D view = doc.GetElement(View.InternalElement.Id) as Autodesk.Revit.DB.View3D;
                TransactionManager.Instance.EnsureInTransaction(doc);
                foreach (Revit.Elements.Element reb in Rebars)
                {
                    Rebar r = doc.GetElement(reb.InternalElement.Id) as Rebar;
                    r.SetUnobscuredInView(view, SetUnobscured);
                }
                TransactionManager.Instance.TransactionTaskDone();
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            return new Dictionary<string, object>
            {
                { "Message", msg },
            };
        }

    }
}


