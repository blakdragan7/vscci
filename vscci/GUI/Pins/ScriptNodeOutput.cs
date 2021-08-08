namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes;

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

        public ScriptNodeOutput(ScriptNode owner, string name, System.Type pinType,int maxNumberOfConnections = -1) : base(owner, name, maxNumberOfConnections, pinType)
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
        }

        public override void RenderPin(Context ctx, ImageSurface surface)
        {
            ctx.SetSourceColor(PinColor);
            ctx.LineWidth = 2;
            RoundRectangle(ctx, 0, 0, DefaultPinSize, DefaultPinSize, GuiStyle.ElementBGRadius);
            if (hasConnection)
            {
                ctx.Fill();
            }
            else
            {
                ctx.Stroke();
            }
        }

        public override bool Connect(ScriptNodePinConnection connection)
        {
            if(pinValueType == typeof(DynamicType))
            {
                color = connection.Input.PinColor;
            }

            return base.Connect(connection);
        }

        public override ScriptNodePinConnection CreateConnection()
        {
            if(CanCreateConnection)
            {
                isDirty = true;
                var conn = ScriptNodePinConnectionManager.TheManage.CreateConnection();
                if (conn.Connect(this))
                    return conn;

                return null;
            }

            if (connections.Count > 0)
                return TopConnection();

            return null;
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            extents = ctx.TextExtents(name);
            extents.Width += DefaultPinSize + Constants.NODE_SCIPRT_TEXT_PADDING;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(x + extents.Width - DefaultPinSize, y + (extents.Height / 2.0), DefaultPinSize, DefaultPinSize);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, extents.Width, extents.Height);
            owner.Bounds.ParentBounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();


            pinConnectionPoint.X = owner.Bounds.drawX + X + extents.Width - extents.Height + (pinSelectBounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = owner.Bounds.drawY + Y + (extents.Height / 2.0) + (pinSelectBounds.OuterHeight / 2.0);
            
            isDirty = true;
        }
    }
}
