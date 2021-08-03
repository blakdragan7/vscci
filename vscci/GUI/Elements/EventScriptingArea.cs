namespace VSCCI.GUI.Elements
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using VSCCI.GUI.Interfaces;
    using VSCCI.GUI.Nodes;
    using VSCCI.GUI.Nodes.Attributes;

    internal class ContextValue
    {
        public Type NodeType;
        public int Index;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var o = obj as ContextValue;
            if (o is null) return false;
            return NodeType.Equals(o.NodeType);
        }

        public override int GetHashCode()
        {
            return NodeType.GetHashCode();
        }

        public override string ToString()
        {
            return $"{{{NodeType}, {Index}}}";
        }

        public static bool operator ==(ContextValue lhs, ContextValue rhs)
        {
            if (lhs is null)
                return rhs is null;
            if (rhs is null)
                return lhs is null;

            return lhs.NodeType == rhs.NodeType;
        }

        public static bool operator !=(ContextValue lhs, ContextValue rhs)
        {
            if (lhs is null && rhs is null) return false;
            else if (lhs is null || rhs is null) return true;

            return lhs.NodeType != rhs.NodeType;
        }
    }

    public class EventScriptingArea : GuiElement, IByteSerializable
    {
        private readonly Dictionary<Type, ISelectableList> contextSelectionLists;
        private readonly List<ScriptNode> allNodes;

        private int texId;
        private ScriptNode selectedNode;
        private ScriptNodeOutput contextOutput;

        private int lastMouseX;
        private int lastMouseY;

        private bool isPanningView;

        private readonly Matrix nodeTransform;
        private Matrix inverseNodeTransform;

        private MouseEvent selectListActiveDownEvent;
        private ISelectableList activeList;

        public EventScriptingArea(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            bounds.IsDrawingSurface = true;
            isPanningView = false;

            activeList = null;
            selectedNode = null;
            contextOutput = null;
            allNodes = new List<ScriptNode>();

            nodeTransform = new Matrix();
            inverseNodeTransform = new Matrix();

            var b = ElementBounds.Fixed(0, 0, 100, 150);
            bounds.WithChild(b);
            var nodeSelectList = new CascadingListElement(api, b);
            nodeSelectList.OnItemSelected += NewNodeSelected;

            b = ElementBounds.Fixed(200, 200, 300, 150);
            bounds.WithChild(b);

            contextSelectionLists = new Dictionary<Type, ISelectableList>()
            {
                { typeof(DynamicType), nodeSelectList }
            };

            PopulateNodeSelectionList();
        }

        public ElementBounds MakeBoundsAtPoint(int x, int y)
        {
            var b = ElementBounds.Fixed(x, y);
            Bounds.WithChild(b);
            b.CalcWorldBounds();

            return b;
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            var ctx = new Context(surface);

            foreach (var node in allNodes)
            {
                node.OnRender(ctx, surface, deltaTime);
            }

            if (activeList != null)
            {
                activeList.OnRender(ctx, surface, deltaTime);
            }

            generateTexture(surface, ref texId);

            ctx.Dispose();
            surface.Dispose();

            
            api.Render.Render2DTexture(texId, Bounds);

            foreach (var node in allNodes)
            {
                node.RenderInteractiveElements(deltaTime);
            }
        }

        public void AddNode(ScriptNode node)
        {
            allNodes.Add(node);
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            base.ComposeElements(ctxStatic, surface);

            DrawBackground(ctxStatic);
        }

        public void ToBytes(BinaryWriter writer)
        {
            writer.Write(nodeTransform.X0);
            writer.Write(nodeTransform.Xx);
            writer.Write(nodeTransform.Xy);

            writer.Write(nodeTransform.Y0);
            writer.Write(nodeTransform.Yx);
            writer.Write(nodeTransform.Yy);

            writer.Write(allNodes.Count);

            var connections = new List<ScriptNodePinConnection>();

            foreach (var node in allNodes)
            {
                var typeName = node.GetType().AssemblyQualifiedName;

                double x = node.Bounds.fixedX;
                double y = node.Bounds.fixedY;

                writer.Write(typeName);
                writer.Write(node.Guid.ToString());
                writer.Write(x);
                writer.Write(y);

                node.WrtiePinsToBytes(writer);

                node.AddConnectionsToList(connections);
            }

            writer.Write(connections.Count);

            foreach(var connection in connections)
            {
                connection.WriteToBytes(writer);
            }
        }

        public void FromBytes(BinaryReader reader, IWorldAccessor resolver)
        {
            var X0 = reader.ReadDouble();
            var Xx = reader.ReadDouble();
            var Xy = reader.ReadDouble();
             
            var Y0 = reader.ReadDouble();
            var Yx = reader.ReadDouble();
            var Yy = reader.ReadDouble();

            nodeTransform.Init(Xx, Yx, Xy, Yy, X0, Y0);
            inverseNodeTransform = (Matrix)nodeTransform.Clone();
            inverseNodeTransform.Invert();

            var numNode = reader.ReadInt32();

            for(var i=0;i<numNode;i++)
            {
                var typeName = reader.ReadString();
                var guidString = reader.ReadString();

                var x = reader.ReadDouble();
                var y = reader.ReadDouble();

                var type = System.Type.GetType(typeName);
                if(type != null && type.IsSubclassOf(typeof(ScriptNode)))
                {
                    ScriptNode node = (ScriptNode)System.Activator.CreateInstance(type, api, nodeTransform, MakeBoundsAtPoint((int)x, (int)y));
                    node.Guid = System.Guid.Parse(guidString);
                    node.ReadPinsFromBytes(reader);

                    allNodes.Add(node);
                }
                else
                {
                    api.Logger.Error("Error reading Node Type from Byte Stream {0}", typeName);
                }
            }

            var numConnections = reader.ReadInt32();
            for (var i = 0; i < numConnections; i++)
            {
                ScriptNodePinConnection.CreateConnectionFromBytes(reader, allNodes);
            }
        }

        public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
        {
            base.OnMouseWheel(api, args);

            if(activeList != null)
            {
                activeList.OnMouseWheel(api, args);
            }
        }
        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDownOnElement(api, args);

            double transformedX = args.X;
            double transformedY = args.Y;

            inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

            if (activeList != null)
            {
                activeList.OnMouseDownOnElement(api, args);
            }

            foreach (var node in allNodes)
            {
                if (node.MouseDown(args.X, args.Y, transformedX, transformedY, args.Button))
                {
                    if(selectedNode != null && selectedNode != node)
                    {
                        selectedNode.MouseDown(args.X, args.Y, transformedX, transformedY, args.Button);
                    }

                    selectedNode = node;
                    lastMouseX = args.X;
                    lastMouseY = args.Y;
                    return;
                }
            }

            switch (args.Button)
            {
                case EnumMouseButton.Left:
                    break;

                case EnumMouseButton.Middle:
                    isPanningView = true;
                    break;

                case EnumMouseButton.Right:
                    // open context window
                    if(selectedNode == null)
                    {
                        activeList = contextSelectionLists[typeof(DynamicType)];
                        activeList.SetPosition(args.X - (activeList.ListBounds.OuterWidth / 4.0), args.Y - (activeList.ListBounds.OuterHeight / 4.0));
                        selectListActiveDownEvent = args;
                    }
                    break;
            }

        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);

            if(activeList != null)
            {
                activeList.OnMouseMove(api, args);
            }

            if (isPanningView)
            {
                nodeTransform.Translate(args.X - lastMouseX, args.Y - lastMouseY);

                foreach(var node in allNodes)
                {
                    node.MarkDirty();
                }
            }
            else
            {
                double transformedX = args.X;
                double transformedY = args.Y;

                inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

                foreach (var node in allNodes)
                {
                    node.MouseMove(args.X, args.Y, transformedX, transformedY, args.X - lastMouseX, args.Y - lastMouseY);
                }
            }

            lastMouseX = args.X;
            lastMouseY = args.Y;
        }

        public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseUpOnElement(api, args);

            if (activeList != null)
            {
                activeList.OnMouseUpOnElement(api, args);

                if (activeList.IsPositionInside(args.X, args.Y) == false)
                {
                    activeList.ResetSelections();
                    activeList = null;
                }
            }
            else
            {
                contextOutput = null;
            }

            if (isPanningView)
            {
                isPanningView = false;
                inverseNodeTransform = (Matrix)nodeTransform.Clone();
                inverseNodeTransform.Invert();
            }
            else if (selectedNode != null)
            {
                double transformedX = args.X;
                double transformedY = args.Y;

                inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

                var foundConnection = false;
                if (selectedNode.ActiveConnection != null)
                {
                    foreach (var node in allNodes)
                    {
                        if (node.ConnectionWillConnecttPoint(selectedNode.ActiveConnection, transformedX, transformedY))
                        {
                            foundConnection = true;
                            break;
                        }
                    }

                    if(foundConnection == false)
                    {
                        if(contextSelectionLists.TryGetValue(selectedNode.ActiveConnection.ConnectionType, out activeList))
                        {
                            activeList.SetPosition(args.X - (activeList.ListBounds.OuterWidth / 4.0), 
                                args.Y - (activeList.ListBounds.OuterHeight / 4.0));
                            selectListActiveDownEvent = args;
                            contextOutput = selectedNode.ActiveConnection.Output;
                        }
                        else
                        {
                            activeList = null;
                            contextOutput = null;
                        }
                    }
                }

                selectedNode = selectedNode.MouseUp(args.X, args.Y, transformedX, transformedY, args.Button) ? selectedNode : null;
            }
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyDown(api, args);

            if(selectedNode != null)
            {
                if (args.KeyCode == (int)GlKeys.Delete)
                {
                    allNodes.Remove(selectedNode);
                    selectedNode.Dispose();
                    selectedNode = null;
                }
                else
                {
                    selectedNode.OnKeyDown(api, args);
                    args.Handled = true;
                }
            }
            if (activeList != null)
            {
                activeList.OnKeyDown(api, args);
                args.Handled = true;
            }

        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyPress(api, args);

            if (selectedNode != null)
            {
                selectedNode.OnKeyPress(api, args);
                args.Handled = true;
            }
            if (activeList != null)
            {
                activeList.OnKeyPress(api, args);
                args.Handled = true;
            }
        }

        private void NewNodeSelected(object sender, ListItem item)
        {
            if (item != null)
            {
                double spawnX = selectListActiveDownEvent.X - Bounds.absX;
                double spawnY = selectListActiveDownEvent.Y - Bounds.absY;

                inverseNodeTransform.TransformPoint(ref spawnX, ref spawnY);

                var bounds = MakeBoundsAtPoint((int)spawnX, (int)spawnY);
                var type = item.Value as Type;
                if (type != null && type.IsSubclassOf(typeof(ScriptNode)))
                {
                    api.Event.EnqueueMainThreadTask(() =>
                    {
                        ScriptNode newNode = (ScriptNode)Activator.CreateInstance(type, api, nodeTransform, bounds);

                        allNodes.Add(newNode);
                    }, "Spawn Node");

                    if (activeList != null)
                    {
                        activeList.ResetSelections();
                        activeList = null;
                    }
                }
            }
        }

        private void NewNodeSelectedContext(object sender, ListItem item)
        {
            if (item != null)
            {
                double spawnX = selectListActiveDownEvent.X - Bounds.absX;
                double spawnY = selectListActiveDownEvent.Y - Bounds.absY;

                inverseNodeTransform.TransformPoint(ref spawnX, ref spawnY);

                var bounds = MakeBoundsAtPoint((int)spawnX, (int)spawnY);
                var val = item.Value as ContextValue;
                var nodeType = val.NodeType;
                var pinIndex = val.Index;
                if (nodeType != null && nodeType.IsSubclassOf(typeof(ScriptNode)))
                {
                    api.Event.EnqueueMainThreadTask(() =>
                    {
                        ScriptNode newNode = (ScriptNode)Activator.CreateInstance(nodeType, api, nodeTransform, bounds);
                        if(contextOutput != null)
                        {
                            var input = newNode.InputForIndex(pinIndex);
                            if(input != null)
                                ScriptNodePinConnection.CreateConnectionBetween(contextOutput, input);
                        }

                        allNodes.Add(newNode);
                    }, "Spawn Node");

                    if (activeList != null)
                    {
                        activeList.ResetSelections();
                        activeList = null;
                    }
                }
            }
        }

        private void PopulateNodeSelectionList()
        {
            var globalSelectionList = contextSelectionLists[typeof(DynamicType)];

            foreach (Type type in typeof(ScriptNode).Assembly.GetTypes())
            {
                var attrs = (NodeDataAttribute[])type.GetCustomAttributes(typeof(NodeDataAttribute), true);
                if (attrs.Length > 0)
                {
                    globalSelectionList.AddListItem(attrs[0].Category, attrs[0].ListName, type);

                    var inputs = (InputPinAttribute[])type.GetCustomAttributes(typeof(InputPinAttribute), true);

                    foreach(var input in inputs)
                    {
                        // add dynamic to everything
                        if(input.PinType == typeof(DynamicType))
                        {
                            foreach (var contextListPair in contextSelectionLists)
                            {
                                contextListPair.Value.AddListItem(attrs[0].Category, attrs[0].ListName, new ContextValue()
                                {
                                    Index = input.Index,
                                    NodeType = type
                                });
                            }
                            continue;
                        }

                        ISelectableList contextList = null;
                        if(contextSelectionLists.TryGetValue(input.PinType, out contextList))
                        {
                            contextList.AddListItem(attrs[0].Category, attrs[0].ListName, new ContextValue() 
                            {
                                Index = input.Index,
                                NodeType = type
                            });
                        }
                        else
                        {
                            var b = ElementBounds.Fixed(0, 0, 100, 150);
                            Bounds.WithChild(b);
                            b.CalcWorldBounds();

                            contextList = new UniqueSelectableListElement(api, b);
                            contextList.AddListItem(attrs[0].Category, attrs[0].ListName, new ContextValue()
                            {
                                Index = input.Index,
                                NodeType = type
                            });

                            contextList.OnItemSelected += NewNodeSelectedContext;

                            contextSelectionLists.Add(input.PinType, contextList);
                        }
                    }
                }
            }
        }

        private void DrawBackground(Context ctx)
        {
            ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor[0], GuiStyle.DialogStrongBgColor[1], GuiStyle.DialogStrongBgColor[2], GuiStyle.DialogStrongBgColor[3]);
            //ctx.SetSourceRGBA(0, 0.1, 1, 1.0);

            ElementRoundRectangle(ctx, Bounds);
            /*  RoundRectangle(ctx, bounds.drawX, bounds.drawY, bounds.InnerWidth, bounds.InnerHeight, radius);
             *
             *  double degrees = Math.PI / 180.0;
             *
             *  ctx.Antialias = Antialias.Best;
             *  ctx.NewPath();
             *  ctx.Arc(x + width - radius, y + radius, radius, -90 * degrees, 0 * degrees);
             *  ctx.Arc(x + width - radius, y + height - radius, radius, 0 * degrees, 90 * degrees);
             *  ctx.Arc(x + radius, y + height - radius, radius, 90 * degrees, 180 * degrees);
             *  ctx.Arc(x + radius, y + radius, radius, 180 * degrees, 270 * degrees);
             *  ctx.ClosePath();
             */

            ctx.Fill();
        }
    }
}
