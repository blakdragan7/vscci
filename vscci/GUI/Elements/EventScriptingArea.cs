namespace VSCCI.GUI.Elements
{
    using Cairo;
    using System.Collections.Generic;

    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using VSCCI.GUI.Nodes;

    public class EventScriptingArea : GuiElement
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

        public EventScriptingArea(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            bounds.IsDrawingSurface = true;
            isPanningView = false;
            didMoveNode = false;

            selectedNode = null;
            allNodes = new List<ScriptNode>();

            nodeTransform = new Matrix();
            inverseNodeTransform = new Matrix();

            AddTests();
        }

        public void Deserialize(string json)
        {
            allNodes.Clear();

            // parse json and load nodes
        }

        public string Seriealize()
        {
            return "";
        }

        public void AddTests()
        {
            allNodes.Add(new ConstantStringScriptNode(api, nodeTransform, MakeBoundsAtPoint(0, 0)));
            allNodes.Add(new ConstantIntScriptNode(api, nodeTransform, MakeBoundsAtPoint(0, 200)));
            allNodes.Add(new BitsEventExecNode(api, nodeTransform, MakeBoundsAtPoint(200, 0)));
            allNodes.Add(new PrintToChatLocalExecNode(api, nodeTransform, MakeBoundsAtPoint(400, 0)));
            //allNodes.Add(new DelayExecutableNode(api, nodeTransform, MakeBoundsAtPoint(0, 200)));
            //allNodes.Add(new AddPureNode<string>(api, nodeTransform, MakeBoundsAtPoint(200, 200)));
        }

        public ElementBounds MakeBoundsAtPoint(int x, int y)
        {
            var b = ElementBounds.Fixed(x, y);
            Bounds.WithChild(b);

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

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDownOnElement(api, args);

            double transformedX = args.X;
            double transformedY = args.Y;

            inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

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
                    break;
            }

        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);

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
