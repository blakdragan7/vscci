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

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface, double deltaTime)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);

            textUtil.DrawTextLine(ctx, font, name, X, Y);
        }

        public override void RenderPin(Context ctx, ImageSurface surface, double deltaTime)
        {
            ctx.SetSourceColor(PinColor);
            ctx.LineWidth = 2;
            RoundRectangle(ctx, X + extents.Width - DefaultPinSize, Y + (extents.Height / 2.0), DefaultPinSize, DefaultPinSize, GuiStyle.ElementBGRadius);
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
            extents.Width += DefaultPinSize + Constants.NODE_SCIPRT_TEXT_PADDING;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(colx + extents.Width - DefaultPinSize, coly + (extents.Height / 2.0), DefaultPinSize, DefaultPinSize);
            owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(colx, coly, extents.Width, extents.Height);
            owner.Bounds.ParentBounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();


            pinConnectionPoint.X = X + extents.Width - extents.Height + (pinSelectBounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = Y + (extents.Height / 2.0) + (pinSelectBounds.OuterHeight / 2.0);
            isDirty = false;
        }
    }
}
