namespace VSCCI.GUI.Elements
{
    using Cairo;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using VSCCI.GUI.Nodes;

    public class EventScriptingArea : GuiElement, IByteSerializable
    {
        private readonly List<ScriptNode> allNodes;
        private int texId;
        private ScriptNode selectedNode;

        private int lastMouseX;
        private int lastMouseY;

        private bool isPanningView;
        private bool didMoveNode;

        private readonly Matrix nodeTransform;
        private Matrix inverseNodeTransform;

        private MouseEvent selectListActiveDownEvent;
        private bool selectListActive; 
        private readonly CascadingListElement nodeSelectList;

        public EventScriptingArea(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            bounds.IsDrawingSurface = true;
            isPanningView = false;
            didMoveNode = false;
            selectListActive = false;

            selectedNode = null;
            allNodes = new List<ScriptNode>();

            nodeTransform = new Matrix();
            inverseNodeTransform = new Matrix();

            var b = ElementBounds.Fixed(0, 0, 100, 150);
            bounds.WithChild(b);

            nodeSelectList = new CascadingListElement(api, b, 6);
            nodeSelectList.OnItemSelected += NewNodeSelected;

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

            foreach(var node in allNodes)
            {
                node.OnRender(ctx, surface, deltaTime);
            }

            if (selectListActive)
            {
                nodeSelectList.OnRender(ctx, surface, deltaTime);
            }

            generateTexture(surface, ref texId);

            ctx.Dispose();
            surface.Dispose();

            api.Render.Render2DTexture(texId, Bounds);
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

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDownOnElement(api, args);

            double transformedX = args.X;
            double transformedY = args.Y;

            inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

            if (selectListActive)
            {
                nodeSelectList.OnMouseDownOnElement(api, args);
            }

            foreach (var node in allNodes)
            {
                if (node.MouseDown(args.X, args.Y, transformedX, transformedY, args.Button))
                {
                    if(selectedNode != null && selectedNode != node)
                    {
                        selectedNode.OnFocusLost();
                    }

                    didMoveNode = false;
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
                        nodeSelectList.SetPosition(args.X - (nodeSelectList.Bounds.OuterWidth / 4.0), args.Y - (nodeSelectList.Bounds.OuterHeight / 4.0));
                        selectListActive = true;
                        selectListActiveDownEvent = args;
                    }
                    break;
            }

        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);

            if(selectListActive)
            {
                nodeSelectList.OnMouseMove(api, args);
            }

            if (isPanningView)
            {
                nodeTransform.Translate(args.X - lastMouseX, args.Y - lastMouseY);

                foreach(var node in allNodes)
                {
                    node.MarkDirty();
                }
            }
            else if (selectedNode != null)
            {
                if(selectedNode.ActiveConnection == null)
                {
                    didMoveNode = true;
                }

                double transformedX = args.X;
                double transformedY = args.Y;

                inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

                selectedNode.MouseMove(args.X, args.Y, transformedX, transformedY, args.X - lastMouseX, args.Y - lastMouseY);
            }

            lastMouseX = args.X;
            lastMouseY = args.Y;
        }

        public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseUpOnElement(api, args);

            if (selectListActive)
            {
                nodeSelectList.OnMouseUpOnElement(api, args);

                if (nodeSelectList.IsPositionInside(args.X, args.Y) == false)
                {
                    nodeSelectList.ResetSelections();
                    selectListActive = false;
                }
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

                if (selectedNode.ActiveConnection != null)
                {
                    foreach (var node in allNodes)
                    {
                        if (node.ConnectionWillConnecttPoint(selectedNode.ActiveConnection, transformedX, transformedY))
                        {
                            break;
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
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyPress(api, args);

            if (selectedNode != null)
            {
                selectedNode.OnKeyPress(api, args);
                args.Handled = true;
            }
        }

        private void NewNodeSelected(object sender, CascadingListItem item)
        {
            if (item != null)
            {
                double spawnX = selectListActiveDownEvent.X - Bounds.absX;
                double spawnY = selectListActiveDownEvent.Y - Bounds.absY;

                inverseNodeTransform.TransformPoint(ref spawnX, ref spawnY);

                var bounds = MakeBoundsAtPoint((int)spawnX, (int)spawnY);
                var type = item.Value as System.Type;
                if (type != null && type.IsSubclassOf(typeof(ScriptNode)))
                {
                    api.Event.EnqueueMainThreadTask(() =>
                    {
                        ScriptNode newNode = (ScriptNode)System.Activator.CreateInstance(type, api, nodeTransform, bounds);

                        allNodes.Add(newNode);
                    }, "Spawn Node");
                }
            }
        }

        private void PopulateNodeSelectionList()
        {
            nodeSelectList.AddListItem("Event", "Bit Event", typeof(BitsEventExecNode));
            nodeSelectList.AddListItem("Event", "Donation Event", typeof(DonationEventExecNode));
            nodeSelectList.AddListItem("Event", "Follow Event", typeof(FollowEventExecNode));
            nodeSelectList.AddListItem("Event", "Host Event", typeof(HostEventExecNode));
            nodeSelectList.AddListItem("Event", "Point Redemption", typeof(PointRedemptionEventExecNode));
            nodeSelectList.AddListItem("Event", "Raid Event", typeof(RaidEventExecNode));
            nodeSelectList.AddListItem("Event", "Sub Event", typeof(SubEventExecNode));
            nodeSelectList.AddListItem("Event", "Super Chat", typeof(SuperChatEventExecNode));

            nodeSelectList.AddListItem("Basic", "Add Int", typeof(AddPureNode<int>));
            nodeSelectList.AddListItem("Basic", "Add Float", typeof(AddPureNode<float>));
            nodeSelectList.AddListItem("Basic", "Add Double", typeof(AddPureNode<double>));
            nodeSelectList.AddListItem("Basic", "Append String", typeof(AddPureNode<string>));

            nodeSelectList.AddListItem("Basic", "Subtract Int", typeof(SubtractPureNode<int>));
            nodeSelectList.AddListItem("Basic", "Subtract Float", typeof(SubtractPureNode<float>));
            nodeSelectList.AddListItem("Basic", "Subtract Double", typeof(SubtractPureNode<double>));

            nodeSelectList.AddListItem("Flow", "For Loop", typeof(ForLoopExecNode));
            nodeSelectList.AddListItem("Flow", "If Then", typeof(IfThenExecNode));
            nodeSelectList.AddListItem("Flow", "Delay", typeof(DelayExecutableNode));

            nodeSelectList.AddListItem("Util", "Show Chat Local", typeof(PrintToChatLocalExecNode));

            nodeSelectList.AddListItem("Constants", "Constant Int", typeof(ConstantIntScriptNode));
            nodeSelectList.AddListItem("Constants", "Constant String", typeof(ConstantStringScriptNode));
            nodeSelectList.AddListItem("Constants", "Constant Float", typeof(ConstantFloatScriptNode));
            nodeSelectList.AddListItem("Constants", "Constant Double", typeof(ConstantDoubleScriptNode));

            nodeSelectList.AddListItem("Conversions", "Int To String", typeof(ToStringPureNode<int>));
            nodeSelectList.AddListItem("Conversions", "Float To String", typeof(ToStringPureNode<float>));
            nodeSelectList.AddListItem("Conversions", "Double To String", typeof(ToStringPureNode<double>));
            nodeSelectList.AddListItem("Conversions", "Bool To String", typeof(ToStringPureNode<bool>));
        }

        private static IEnumerable<System.Type> GetSubclasses<A>()
        {
            return typeof(A).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(A)));
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
