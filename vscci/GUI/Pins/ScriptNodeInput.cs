namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using VSCCI.Data;
    using VSCCI.GUI.Nodes;

    public class ScriptNodeInput : ScriptNodePinBase
    {
        public bool HasInputValue => connections.Count > 0;

        public ScriptNodeInput(ScriptNode owner, string name, System.Type pinType) : base(owner, name, 1, pinType)
        {

        }
        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            ctx.SetSourceRGBA(1, 1, 1, 1.0);

            textUtil.DrawTextLine(ctx, font, name, X + DefaultPinSize + Constants.NODE_SCIPRT_TEXT_PADDING, Y);
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
        public override ScriptNodePinConnection CreateConnection()
        {
            if (CanCreateConnection)
            {
                isDirty = true;
                var conn = ScriptNodePinConnectionManager.TheManage.CreateConnection();
                if(conn.Connect(this))
                    return conn;

                return null;
            }

            if (connections.Count > 0)
                return TopConnection();

            return null;
        }

        public override bool Connect(ScriptNodePinConnection connection)
        {
            if (pinValueType == typeof(DynamicType))
            {
                color = connection.Output.PinColor;
            }

            return base.Connect(connection);
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            extents = ctx.TextExtents(name);
            extents.Width += DefaultPinSize + Constants.NODE_SCIPRT_TEXT_PADDING;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(x, (y + (extents.Height / 2.0)), DefaultPinSize, DefaultPinSize);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, extents.Width, extents.Height);
            owner.Bounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = owner.Bounds.drawX + X + (pinSelectBounds.OuterWidth / 2.0);
            pinConnectionPoint.Y = owner.Bounds.drawY + Y + (pinSelectBounds.OuterHeight / 2.0);

            isDirty = true;
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
