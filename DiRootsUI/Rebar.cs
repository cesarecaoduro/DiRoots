using Autodesk.Revit.DB.Structure;
using Dynamo.Graph.Nodes;
using System;

namespace DiRootsUI
{
    [IsDesignScriptCompatible]
    [NodeCategory("DiRoots.Rebar.Info")]
    [NodeDescription("Select Rebar Bar Type")]
    [OutPortDescriptions("Bar Types")]
    [NodeName("RebarBarTypes")]
    public class RebarBarTypes : CustomRevitElementDropDown
    {
        public RebarBarTypes() : base("RebarBarTypes", typeof(RebarBarType)) { }
    }

    [IsDesignScriptCompatible]
    [NodeCategory("DiRoots.Rebar.Info")]
    [NodeName("RebarHookTypes")]
    public class RebarHookTypes : CustomRevitElementDropDown
    {
        public RebarHookTypes() : base("RebarHookTypes", typeof(RebarHookType)) { }
    }

    [IsDesignScriptCompatible]
    [NodeCategory("DiRoots.Rebar.Info")]
    [NodeName("RebarStyleTypes")]
    public class RebarStyleTypes : CustomGenericEnumerationDropDown
    {
        public RebarStyleTypes() : base("RebarStyleTypes", typeof(RebarStyle)) { }
    }

    [IsDesignScriptCompatible]
    [NodeCategory("DiRoots.Rebar.Info")]
    [NodeName("RebarHookOrientation")]
    public class RebarHookOrientationTypes : CustomGenericEnumerationDropDown
    {
        public RebarHookOrientationTypes() : base("RebarHookOrientationTypes", typeof(RebarHookOrientation)) { }
    }

    [IsDesignScriptCompatible]
    [NodeCategory("DiRoots.Rebar.Info")]
    [NodeName("RebarLayoutRules")]
    public class RebarLayoutRules : CustomGenericEnumerationDropDown
    {
        public RebarLayoutRules() : base("RebarLayoutRules", typeof(RebarLayoutRule)) { }
    }
}