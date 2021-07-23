namespace vscci.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using vscci.Data;

    public class ScriptNodeInput : ScriptNodePinBase
    {
        public bool HasInputValue => connections.Count > 0;

        public ScriptNodeInput(ScriptNode owner, string name, System.Type pinType) : base(owner, name, 1, pinType)
        {

        }
        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);
            extents = ctx.TextExtents(name);

            textUtil.DrawTextLine(ctx, font, name, X + extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING, Y);

            extents.Width += extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;

            if (isDirty)
            {
                if(pinSelectBounds != null)
                {
                    owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
                }
                pinSelectBounds = ElementBounds.Fixed(X, Y + (extents.Height / 2.0), extents.Height, extents.Height);
                owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
                pinSelectBounds.CalcWorldBounds();
                pinConnectionPoint.X = pinSelectBounds.drawX + (pinSelectBounds.OuterWidth / 2.0);
                pinConnectionPoint.Y = pinSelectBounds.drawY + (pinSelectBounds.OuterHeight / 2.0);
                isDirty = false;
            }
        }

        public override void RenderPin(Context ctx, ImageSurface surface)
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

        public dynamic GetInput()
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
