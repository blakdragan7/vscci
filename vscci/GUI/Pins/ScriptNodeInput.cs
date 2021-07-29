namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.Data;

    public class ScriptNodeInput : ScriptNodePinBase
    {
        public bool HasInputValue => connections.Count > 0;

        public ScriptNodeInput(ScriptNode owner, string name, System.Type pinType) : base(owner, name, 1, pinType)
        {

        }
        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface, double deltaTime)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);

            textUtil.DrawTextLine(ctx, font, name, X + extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING, Y);
        }

        public override void RenderPin(Context ctx, ImageSurface surface, double deltaTime)
        {
            ctx.SetSourceColor(PinColor);
            ctx.LineWidth = 2;
            RoundRectangle(ctx, X, Y + (extents.Height / 2.0), extents.Height, extents.Height, GuiStyle.ElementBGRadius);
            if (hasConnection)
            {
                ctx.Fill();
            }
            else
            {
                ctx.Stroke();
            }
        }
        public override ScriptNodePinConnection CreateConnection()
        {
            if (CanCreateConnection)
            {
                return new ScriptNodePinConnection(this);
            }

            if (connections.Count > 0)
                return TopConnection();

            return null;
        }
        public override void Compose(double colx, double coly, double drawx, double drawy, Context ctx, CairoFont font)
        {
            X = drawx;
            Y = drawy;

            extents = ctx.TextExtents(name);
            extents.Width += extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(colx, coly + (extents.Height / 2.0), extents.Height, extents.Height);
            owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();
            pinConnectionPoint.X = X + (pinSelectBounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = Y + (pinSelectBounds.OuterHeight / 2.0);
            isDirty = false;
        }

        public virtual dynamic GetInput()
        {
            if(connections.Count > 0)
            {
                return connections[0].Output.Value;
            }

            return GetDefault();
        }

        public dynamic GetDefault()
        {
            if (pinValueType.IsValueType)
            {
                return Activator.CreateInstance(pinValueType);
            }
            return null;
        }
    }
}
