namespace vscci.GUI.Elements
{
    using System.Collections.Generic;
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    public class EventScriptingArea : GuiElement
    {
        private List<ScriptNode> allNodes;
        private int texId;
        private ScriptNode draggedNode;

        private int lastMouseX;
        private int lastMouseY;

        private bool isPanningView;

        private Matrix nodeTransform;
        private Matrix inverseNodeTransform;

        public EventScriptingArea(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
        {
            bounds.IsDrawingSurface = true;
            isPanningView = false;

            draggedNode = null;
            allNodes = new List<ScriptNode>();

            nodeTransform = new Matrix();
            inverseNodeTransform = new Matrix();

            AddTest();
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

        public void AddTest()
        {
            var b = ElementBounds.Fixed(0, 0, 32, 32);

            Bounds.WithChild(b);
            allNodes.Add(new ScriptNode(api, nodeTransform, b));
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            ImageSurface surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            Context ctx = new Context(surface);

            foreach(var node in allNodes)
            {
                node.OnRender(ctx, deltaTime);
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

            switch(args.Button)
            {
                case EnumMouseButton.Left:
                    double transformedX = args.X;
                    double transformedY = args.Y;

                    inverseNodeTransform.TransformPoint(ref transformedX, ref transformedY);

                    foreach (var node in allNodes)
                    {
                        if (node.IsPositionInside((int)transformedX, (int)transformedY))
                        {
                            draggedNode = node;
                            lastMouseX = args.X;
                            lastMouseY = args.Y;
                            break;
                        }
                    }
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
            else if(draggedNode != null)
            {
                draggedNode.Move(args.X - lastMouseX, args.Y - lastMouseY);
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
            else
            {
                draggedNode = null;
            }
        }

        private void DrawBackground(Context ctxStatic)
        {
            ctxStatic.SetSourceRGBA(0, 0.1, 1, 1.0);

            ElementRoundRectangle(ctxStatic, Bounds);
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

            ctxStatic.Fill();
        }
    }
}
