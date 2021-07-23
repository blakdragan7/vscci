namespace vscci.GUI.Elements
{
    using Cairo;
    using System.Collections.Generic;

    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    using vscci.GUI.Nodes;

    public class EventScriptingArea : GuiElement
    {
        private readonly List<ScriptNode> allNodes;
        private int texId;
        private ScriptNode activeNode;

        private int lastMouseX;
        private int lastMouseY;

        private bool isPanningView;

        private readonly Matrix nodeTransform;
        private Matrix inverseNodeTransform;

        public EventScriptingArea(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            bounds.IsDrawingSurface = true;
            isPanningView = false;

            activeNode = null;
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
            allNodes.Add(new BitsEventExecNode(api, nodeTransform, MakeBoundsAtPoint(0, 0)));
            allNodes.Add(new PrintToChatLocalExecNode(api, nodeTransform, MakeBoundsAtPoint(300, 0)));
            allNodes.Add(new AddPureNode<string>(api, nodeTransform, MakeBoundsAtPoint(150, 0)));
            allNodes.Add(new AddPureNode<string>(api, nodeTransform, MakeBoundsAtPoint(150, 100)));
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
                if (node.MouseDown(transformedX, transformedY, args.Button))
                {
                    activeNode = node;
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
            }
            else if (activeNode != null)
            {
                activeNode.MouseMove(args.X - lastMouseX, args.Y - lastMouseY);
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
            else if (activeNode != null)
            {
                double transformedX = args.X;
                double transformedY = args.Y;

                inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

                if (activeNode.ActiveConnection != null)
                {
                    foreach (var node in allNodes)
                    {
                        if (node.ConnectionWillConnecttPoint(activeNode.ActiveConnection, transformedX, transformedY))
                        {
                            break;
                        }
                    }
                }

                activeNode.MouseUp(transformedX, transformedY);
                activeNode = null;
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
