namespace vscci.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;

    public class ScriptNodeOutput : ScriptNodePinBase
    {
        public dynamic Value { get; set; }

        public ScriptNodeOutput(ScriptNode owner, string name, int maxNumberOfConnections, System.Type pinType) : base(owner, name, maxNumberOfConnections, pinType)
        {
            if (pinType.IsValueType)
            {
                Value = Activator.CreateInstance(pinType);
            }
            else
            {
                Value = null;
            }
            
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);

            textUtil.DrawTextLine(ctx, font, name, X, Y);
            extents = ctx.TextExtents(name);
            extents.Width += extents.Height;

            if (isDirty)
            {
                if (pinSelectBounds != null)
                {
                    owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
                }
                pinSelectBounds = ElementBounds.Fixed(X + extents.Width - extents.Height, Y + extents.Height, extents.Height, extents.Height);
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
            RoundRectangle(ctx, X + extents.Width - extents.Height, Y + extents.Height, extents.Height, extents.Height, GuiStyle.ElementBGRadius);
            if (hasConnection)
            {
                ctx.Fill();
            }
            else
            {
                ctx.Stroke();
            }

            foreach (var connection in Connections)
            {
                if (connection.IsConnected)
                {
                    connection.Render(ctx, surface);
                }
            }
        }
    }
}
