namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    public class ScriptNodeOutput : ScriptNodePinBase
    {
        public ScriptNodeOutput(ScriptNode owner, string name, int maxNumberOfConnections, System.Type pinType) : base(owner, name, maxNumberOfConnections, pinType)
        {
        }

        public override void Render(double x, double y, TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.Save();
            ctx.SetSourceRGBA(1, 1, 1, 1.0);
            font.SetupContext(ctx);

            textUtil.DrawTextLine(ctx, font, name, x, y);
            extents = ctx.TextExtents(name);

            ctx.Restore();

            ctx.SetSourceColor(PinColor);
            ctx.LineWidth = 2;
            RoundRectangle(ctx, x + extents.Width, y + extents.Height, extents.Height, extents.Height, GuiStyle.ElementBGRadius);
            if (hasConnection)
            {
                ctx.Fill();
            }
            else
            {
                ctx.Stroke();
            }
            extents.Width += extents.Height;

            foreach (var connection in Connections)
            {
                if (connection.IsConnected)
                {
                    connection.Render(ctx, surface);
                }
            }

            if(isDirty)
            {
                if (pinSelectBounds != null)
                {
                    owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
                }
                pinSelectBounds = ElementBounds.Fixed(x + extents.Width - extents.Height, y + extents.Height, extents.Height, extents.Height);
                owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
                pinSelectBounds.CalcWorldBounds();
                pinConnectionPoint.X = pinSelectBounds.drawX + (pinSelectBounds.OuterWidth / 2.0);
                pinConnectionPoint.Y = pinSelectBounds.drawY + (pinSelectBounds.OuterHeight / 2.0);
                isDirty = false;
            }
        }
    }
}
