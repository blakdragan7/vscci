namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.Data;

    public class ScriptNodeOutput : ScriptNodePinBase
    {
        private dynamic value;

        public dynamic Value 
        { 
            get
            {
                var exec = owner as ExecutableScriptNode;
                if(exec != null && exec.IsPure)
                {
                    exec.Execute();
                }

                return this.value;
            }

            set
            {
                this.value = value;
            }
        }

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
            extents.Width += extents.Height + Constants.NODE_SCIPRT_TEXT_PADDING;

            if (isDirty)
            {
                if (pinSelectBounds != null)
                {
                    owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
                }
                pinSelectBounds = ElementBounds.Fixed(X + extents.Width - extents.Height, Y + (extents.Height / 2.0), extents.Height, extents.Height);
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
            RoundRectangle(ctx, X + extents.Width - extents.Height, Y + (extents.Height / 2.0), extents.Height, extents.Height, GuiStyle.ElementBGRadius);
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
