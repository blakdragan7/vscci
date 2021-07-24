namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;

    public class Exec // stub for exec type
    {

    }

    public abstract class ScriptNodePinBase
    {
        protected bool isDirty;

        protected TextExtents extents;
        protected bool hasConnection;
        protected ElementBounds pinSelectBounds;
        protected PointD pinConnectionPoint;

        protected readonly string name;
        protected readonly Color color;
        protected readonly Type pinValueType;
        protected readonly int maxNumberOfConnections;

        protected readonly ScriptNode owner;

        protected readonly List<ScriptNodePinConnection> connections;

        public double X { get; set; }
        public double Y { get; set; }

        /*
         * @PinConnectionPoint is the point at which a ScriptNodePinConnection will draw too.
         * Invalid until first render
         */
        public PointD PinConnectionPoint => pinConnectionPoint;
        /*
         *  @Extents represents the rendered size of this pin
         *  This only returns a valid size after a call to Render
         */
        public TextExtents Extents => extents;
        /*
         *  @PinColor represents the color of this pin based on the value type
         */
        public Color PinColor => color;
        /*
         * @CanCreateConnection returns true if this pin is capable of creating another connection
         */
        public bool CanCreateConnection => connections.Count < maxNumberOfConnections;
        /*
         * @Connections is simply a list of all connections to this pin.
         */
        public List<ScriptNodePinConnection> Connections => connections;

        public ScriptNodePinBase(ScriptNode owner, string name, int maxNumberOfConnections, Type pinValueType)
        {
            this.name = name;
            this.pinValueType = pinValueType;
            this.color = ColorForValueType(pinValueType);
            this.hasConnection = false;
            this.maxNumberOfConnections = maxNumberOfConnections;
            this.isDirty = true;
            this.owner = owner;
            this.connections = new List<ScriptNodePinConnection>();
            this.pinConnectionPoint = new PointD();
        }
        /*
         * Rendered before Pin but after "RenderOther
         * All text rendering should be done here
         */
        public abstract void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface);
        /*
         *  Renders after RenderText, used to render the connection pin
         */
        public abstract void RenderPin(Context ctx, ImageSurface surface);
        /*
         *  Renders Before text, used for rendering backgrounds and other misc things that should be
         *  behind the text
         */
        public abstract void RenderOther(Context ctx, ImageSurface surface);

        public virtual void MarkDirty()
        {
            this.isDirty = true;
        }

        public virtual bool Connect(ScriptNodePinConnection connection)
        {
            if(CanCreateConnection == false)
            {
                return false;
            }

            connections.Add(connection);

            hasConnection = true;

            return true;
        }

        public virtual bool Disconnect(ScriptNodePinConnection connection)
        {
            if (hasConnection && connections.Contains(connection))
            {
                connections.Remove(connection);

                if (connections.Count <= 0)
                {
                    hasConnection = false;
                }

                return true;
            }

            return false;
        }

        public static void RoundRectangle(Context ctx, double x, double y, double width, double height, double radius)
        {
            var degrees = Math.PI / 180.0;

            ctx.Antialias = Antialias.Best;
            ctx.NewPath();
            ctx.Arc(x + width - radius, y + radius, radius, -90 * degrees, 0 * degrees);
            ctx.Arc(x + width - radius, y + height - radius, radius, 0 * degrees, 90 * degrees);
            ctx.Arc(x + radius, y + height - radius, radius, 90 * degrees, 180 * degrees);
            ctx.Arc(x + radius, y + radius, radius, 180 * degrees, 270 * degrees);
            ctx.ClosePath();
        }
        public static Color ColorForValueType(Type type)
        {
            if(type.Equals(typeof(Exec)))
            {
                return new Color(1.0, 1.0, 1.0, 1.0);
            }
            else if (type.IsAssignableFrom(typeof(int)))
            {
                return new Color(0.0, 1.0, 0.0, 1.0);
            }
            else if (type.IsAssignableFrom(typeof(string)))
            {
                return new Color(0.4980392156862745, 0, 1.0, 1.0);
            }
            else if (type.IsAssignableFrom(typeof(bool)))
            {
                return new Color(0.8, 0.1, 0.1, 1.0);
            }

            return new Color(0.8,0.8,0.8,1.0);
        }

        public bool CanConnectTo(ScriptNodePinBase other, ScriptNodePinConnection forConnection)
        {
            return this != other && pinValueType == other.pinValueType 
                && (CanCreateConnection || connections.Contains(forConnection)) 
                && (other.CanCreateConnection ||  other.connections.Contains(forConnection));
        }

        public bool PointIsWithinSelectionBounds(double x, double y)
        {
            return pinSelectBounds.PointInside(x, y);
        }

        public ScriptNodePinConnection TopConnection()
        {
            if (connections.Count > 0)
                return connections[connections.Count - 1];

            return null;
        }
}
