using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace DiRootsUI
{
    public abstract class CustomRevitElementDropDown : RevitDropDownBase
    {
        private const string noTypes = "No Types available.";

        public Type ElementType;

        public CustomRevitElementDropDown(string name, Type elementType) : base(name)
        {
            this.ElementType = elementType;
            this.PopulateItems();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            IEnumerable<AssociativeNode> associativeNodes;
            BinaryExpressionNode[] binaryExpressionNodeArray;
            if ((base.Items.Count == 0 || base.Items[0].Name == "No Types available." || base.SelectedIndex == -1 ? false : !(base.Items[base.SelectedIndex].Name == "None")))
            {
                Type type = typeof(Element);
                ElementId id = ((Element)base.Items[base.SelectedIndex].Item).Id;
                List<AssociativeNode> associativeNodes1 = new List<AssociativeNode>()
                {
                    AstFactory.BuildIntNode(id.IntegerValue)
                };
                AssociativeNode associativeNode = AstFactory.BuildFunctionCall("Revit.Elements.ElementSelector", "ByElementId", associativeNodes1, null);
                binaryExpressionNodeArray = new BinaryExpressionNode[] { AstFactory.BuildAssignment(this.GetAstIdentifierForOutputIndex(0), associativeNode) };
                associativeNodes = binaryExpressionNodeArray;
            }
            else
            {
                binaryExpressionNodeArray = new BinaryExpressionNode[] { AstFactory.BuildAssignment(this.GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
                associativeNodes = binaryExpressionNodeArray;
            }
            return associativeNodes;
        }

        public void PopulateItems()
        {
            if (this.ElementType != null)
            {
                base.Items.Clear();
                FilteredElementCollector filteredElementCollector = (new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument)).OfClass(this.ElementType);
                if (filteredElementCollector.ToElements().Count != 0)
                {
                    if (this.ElementType.FullName == "Autodesk.Revit.DB.Structure.RebarHookType")
                    {
                        base.Items.Add(new DynamoDropDownItem("None", null));
                    }
                    foreach (Element element in filteredElementCollector.ToElements())
                    {
                        base.Items.Add(new DynamoDropDownItem(element.Name, element));
                    }
                    base.Items = (ExtensionMethods.ToObservableCollection<DynamoDropDownItem>(
                        from x in base.Items
                        orderby x.Name
                        select x));
                }
                else
                {
                    base.Items.Add(new DynamoDropDownItem("No Types available.", null));
                    base.SelectedIndex = 0;
                }
            }
        }

        protected override DSDropDownBase.SelectionState PopulateItemsCore(string currentSelection)
        {
            this.PopulateItems();
            return 0;
        }
    }

    public abstract class CustomGenericEnumerationDropDown : RevitDropDownBase
    {
        public Type EnumerationType;

        public CustomGenericEnumerationDropDown(string name, Type enumerationType) : base(name)
        {
            this.EnumerationType = enumerationType;
            this.PopulateItems();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if ((base.Items.Count == 0 ? true : base.Items.Count == -1))
            {
                this.PopulateItems();
            }
            StringNode stringNode = AstFactory.BuildStringNode(base.Items[base.SelectedIndex].Name);
            BinaryExpressionNode binaryExpressionNode = AstFactory.BuildAssignment(this.GetAstIdentifierForOutputIndex(0), stringNode);
            return new List<AssociativeNode>()
            {
                binaryExpressionNode
            };
        }

        public void PopulateItems()
        {
            if (this.EnumerationType != null)
            {
                base.Items.Clear();
                string[] names = Enum.GetNames(this.EnumerationType);
                for (int i = 0; i < (int)names.Length; i++)
                {
                    string str = names[i];
                    base.Items.Add(new DynamoDropDownItem(str, Enum.Parse(this.EnumerationType, str)));
                }
                base.Items = (ExtensionMethods.ToObservableCollection<DynamoDropDownItem>(
                    from x in base.Items
                    orderby x.Name
                    select x));
            }
        }

        protected override DSDropDownBase.SelectionState PopulateItemsCore(string currentSelection)
        {
            this.PopulateItems();
            return 0;
        }
    }
}