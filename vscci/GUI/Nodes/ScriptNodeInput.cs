namespace vscci.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;

    public class ScriptNodeInput : ScriptNodePinBase
    {
        public ScriptNodeInput(ScriptNode owner, string name, System.Type pinType) : base(owner, name, 1, pinType)
        {

        }
        public override void Render(double x, double y, TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.Save();
            ctx.SetSourceRGBA(1, 1, 1, 1.0);
            font.SetupContext(ctx);
            extents = ctx.TextExtents(name);

            textUtil.DrawTextLine(ctx, font, name, x + extents.Height, y);

            ctx.Restore();

            ctx.SetSourceColor(PinColor);
            ctx.LineWidth = 2;
            RoundRectangle(ctx, x, y + extents.Height, extents.Height, extents.Height, GuiStyle.ElementBGRadius);
            if (hasConnection)
            {
                ctx.Fill();
            }
            else
            {
                ctx.Stroke();
            }

            extents.Width += extents.Height;

            if (isDirty)
            {
                if(pinSelectBounds != null)
                {
                    owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
                }
                pinSelectBounds = ElementBounds.Fixed(x, y + extents.Height, extents.Height, extents.Height);
                owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
                pinSelectBounds.CalcWorldBounds();
                pinConnectionPoint.X = pinSelectBounds.drawX + (pinSelectBounds.OuterWidth / 2.0);
                pinConnectionPoint.Y = pinSelectBounds.drawY + (pinSelectBounds.OuterHeight / 2.0);
                isDirty = false;
            }
        }
    }
}
