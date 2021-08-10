namespace VSCCI.GUI.Elements
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using VSCCI.GUI.Interfaces;
    using VSCCI.GUI.Nodes;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.GUI.Pins;

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

        private readonly List<ScriptNode> selectedNodes;

        private int texId;
        private ScriptNodeOutput contextOutput;

        private int lastMouseX;
        private int lastMouseY;

        private bool isPanningView;

        private readonly Matrix nodeTransform;
        private Matrix inverseNodeTransform;

        private MouseEvent selectListActiveDownEvent;
        private ISelectableList activeList;

        private ScriptNodePinConnectionManager connectionManager;

        public EventScriptingArea(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            bounds.IsDrawingSurface = true;
            isPanningView = false;

            connectionManager = ScriptNodePinConnectionManager.TheManage;
            connectionManager.SetupManager(api, bounds);

            activeList = null;
            contextOutput = null;
            allNodes = new List<ScriptNode>();
            selectedNodes = new List<ScriptNode>();

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

        public MatrixElementBounds MakeBoundsAtPoint(int x, int y)
        {
            var b = MatrixElementBounds.Fixed(x, y, nodeTransform);
            Bounds.WithChild(b);
            b.CalcWorldBounds();

            return b;
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            var ctx = new Context(surface);

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

            connectionManager.RenderConnections(deltaTime);
        }

        public void AddNode(ScriptNode node)
        {
            allNodes.Add(node);
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            base.ComposeElements(ctxStatic, surface);

            DrawBackground(ctxStatic);

            foreach (var node in allNodes)
            {
                node.ComposeElements(ctxStatic, surface);
            }
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
            }

            connectionManager.ToBytes(writer);
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
                    ScriptNode node = SpawnNode(type, MakeBoundsAtPoint((int)x, (int)y));
                    node.Guid = System.Guid.Parse(guidString);
                    node.ReadPinsFromBytes(reader);

                    allNodes.Add(node);
                }
                else
                {
                    api.Logger.Error("Error reading Node Type from Byte Stream {0}", typeName);
                }
            }

            connectionManager.FromBytes(reader, allNodes);
        }

        public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
        {
            base.OnMouseWheel(api, args);

            if(activeList != null)
            {
                activeList.OnMouseWheel(api, args);
            }

            foreach (var node in allNodes)
            {
                node.OnMouseWheel(api, args);
            }
        }
        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDownOnElement(api, args);

            args.Handled = false;

            double transformedX = args.X;
            double transformedY = args.Y;

            inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

            MouseEvent transformedEvent = new MouseEvent((int)transformedX, (int)transformedY, args.DeltaX, args.DeltaY, args.Button);

            if (activeList != null)
            {
                activeList.OnMouseDownOnElement(api, args);
            }

            NodeMouseEvent nodeMouseEvent = new NodeMouseEvent()
            {
                intersectingNode = null,
                mouseEvent = transformedEvent,
                nodeSelectCount = selectedNodes.Count
            };

            foreach (var node in allNodes)
            {
                if(node.IsPositionInside(transformedEvent.X, transformedEvent.Y))
                {
                    nodeMouseEvent.intersectingNode = node;
                    break;
                }
            }

            foreach (var node in allNodes)
            {
                node.OnMouseDown(api, nodeMouseEvent);
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
                    if (transformedEvent.Handled != true)
                    {
                        activeList = contextSelectionLists[typeof(DynamicType)];
                        activeList.SetPosition(args.X - (activeList.ListBounds.OuterWidth / 4.0), args.Y - (activeList.ListBounds.OuterHeight / 4.0));
                        selectListActiveDownEvent = args;
                    }
                    break;
            }

            args.Handled = true;

            lastMouseX = args.X;
            lastMouseY = args.Y;
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);
            args.Handled = false;

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
            else if(connectionManager.HasActiveConnection)
            {
                connectionManager.UpdateActiveConnection(args.X - Bounds.absX, args.Y - Bounds.absY);
            }
            else
            {
                double transformedX = args.X;
                double transformedY = args.Y;

                inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

                MouseEvent transformedEvent = new MouseEvent((int)transformedX, (int)transformedY, args.X - lastMouseX, args.Y - lastMouseY, args.Button);
                
                foreach (var node in allNodes)
                {
                    node.OnMouseMove(api, transformedEvent);
                }

                if(transformedEvent.Handled)
                {
                    connectionManager.MarkDirty();
                }
            }

            lastMouseX = args.X;
            lastMouseY = args.Y;

            args.Handled = true;
        }

        public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseUpOnElement(api, args);
            args.Handled = false;

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
            
            double transformedX = args.X;
            double transformedY = args.Y;

            inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

            MouseEvent transformedEvent = new MouseEvent((int)transformedX, (int)transformedY, args.DeltaX, args.DeltaY, args.Button);
            var foundConnection = false;

            if (connectionManager.HasActiveConnection)
            {
                foreach (var node in allNodes)
                {
                    if (connectionManager.ConnectActiveConnectionToNodeAtPoint(node, transformedX, transformedY))
                    {
                        foundConnection = true;
                        break;
                    }
                }

                if(foundConnection == false)
                {
                    if(contextSelectionLists.TryGetValue(connectionManager.ActiveType, out activeList))
                    {
                        activeList.SetPosition(args.X - (activeList.ListBounds.OuterWidth / 4.0), 
                            args.Y - (activeList.ListBounds.OuterHeight / 4.0));
                        selectListActiveDownEvent = args;
                        contextOutput = connectionManager.ActiveOutput;
                    }
                    else
                    {
                        activeList = null;
                        contextOutput = null;
                    }

                    connectionManager.RemoveActiveConnection();
                }
            }

            NodeMouseEvent nodeMouseEvent = new NodeMouseEvent()
            {
                intersectingNode = null,
                mouseEvent = transformedEvent,
                nodeSelectCount = selectedNodes.Count
            };

            foreach (var node in allNodes)
            {
                if (node.IsPositionInside(transformedEvent.X, transformedEvent.Y))
                {
                    nodeMouseEvent.intersectingNode = node;
                    break;
                }
            }

            foreach (var node in allNodes)
            {
                node.OnMouseUp(api, nodeMouseEvent);
            }

            args.Handled = true;
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyDown(api, args);

            if (args.KeyCode == (int)GlKeys.Delete)
            {
                foreach (var selectedNode in selectedNodes)
                {

                    allNodes.Remove(selectedNode);
                    selectedNode.Dispose();

                    args.Handled = true;
                }

                selectedNodes.Clear();
            }
            if (activeList != null)
            {
                activeList.OnKeyDown(api, args);
                args.Handled = true;
            }
            else
            {
                foreach (var node in allNodes)
                {
                    node.OnKeyDown(api, args);
                }
            }
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyPress(api, args);

            if (activeList != null)
            {
                activeList.OnKeyPress(api, args);
                args.Handled = true;
            }
            else
            {
                foreach(var node in allNodes)
                {
                    node.OnKeyPress(api, args);
                }
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
                        ScriptNode newNode = SpawnNode(type, bounds);

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
                        ScriptNode newNode = SpawnNode(nodeType, bounds);

                        if(contextOutput != null)
                        {
                            var input = newNode.InputForIndex(pinIndex);
                            if(input != null)
                                connectionManager.CreateConnectionBetween(contextOutput, input);
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

        private ScriptNode SpawnNode<t>(ElementBounds bounds)
        {
            ScriptNode newNode = (ScriptNode)Activator.CreateInstance(typeof(t), api, bounds);
            newNode.onSelectedChanged += onSelectedChanged;
            return newNode;
        }

        private ScriptNode SpawnNode(Type nodeType, ElementBounds bounds)
        {
            if (nodeType.IsSubclassOf(typeof(ScriptNode)) == false)
                return null;

            ScriptNode newNode = (ScriptNode)Activator.CreateInstance(nodeType, api, bounds);
            newNode.onSelectedChanged += onSelectedChanged;
            return newNode;
        }

        private void onSelectedChanged(object sender, bool isSelected)
        {
            ScriptNode node = (ScriptNode)sender;
            if(isSelected)
            {
                if(selectedNodes.Contains(node) == false)
                    selectedNodes.Add(node);
            }
            else
            {
                selectedNodes.Remove(node);
            }
        }

        private void PopulateNodeSelectionList()
        {
            ServerSideSpawnEntityNode.PopulateEntitySelectionOptions();

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
