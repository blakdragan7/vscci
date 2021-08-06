namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Common.Entities;
    using Vintagestory.API.MathTools;

    public class Exec // stub for exec type
    {

    }

    public class DynamicType // stub for any connection type allowed
    {

    }

    public class Number
    {
        int intVal;
        double doubleVal;

        public bool isInteger => Math.Abs(doubleVal % 1) <= (double.Epsilon * 100);

        public static implicit operator int(Number n)
        {
            return n.intVal;
        }

        public static implicit operator double(Number n)
        {
            return n.doubleVal;
        }

        public static implicit operator Number(int i)
        {
            return new Number() { intVal = i, doubleVal = (double)i };
        }

        public static implicit operator Number(double d)
        {
            return new Number() { intVal = (int)d, doubleVal = d };
        }

        public static Number operator +(Number lhs, Number rhs) =>
            new Number(lhs.intVal + rhs.intVal, lhs.doubleVal + rhs.doubleVal);

        public static Number operator -(Number lhs, Number rhs) =>
            new Number(lhs.intVal - rhs.intVal, lhs.doubleVal - rhs.doubleVal);

        public static Number operator *(Number lhs, Number rhs)
            => new Number(lhs.intVal * rhs.intVal, lhs.doubleVal * rhs.doubleVal);

        public static Number operator /(Number lhs, Number rhs)
        {
            int i = rhs.intVal == 0 ? 0 : lhs.intVal / rhs.intVal;
            double d = rhs.doubleVal == 0 ? 0 : lhs.doubleVal / rhs.doubleVal;

            return new Number(i, d);
        }

        public static bool operator ==(Number lhs, Number rhs)
        {
            if (lhs is null)
                return rhs is null;
            if (rhs is null)
                return lhs is null;

            return lhs.isInteger ? (lhs.intVal == rhs.intVal) : (rhs.doubleVal == lhs.doubleVal);
        }

        public static bool operator !=(Number lhs, Number rhs)
        {
            if (lhs is null && rhs is null) return false;
            else if (lhs is null || rhs is null) return true;

            return lhs.isInteger ? (lhs.intVal != rhs.intVal) : (rhs.doubleVal != lhs.doubleVal);
        }
        public static bool operator <=(Number lhs, Number rhs) =>
            lhs.isInteger ? (lhs.intVal <= rhs.intVal) : (rhs.doubleVal <= lhs.doubleVal);

        public static bool operator >=(Number lhs, Number rhs) =>
            lhs.isInteger ? (lhs.intVal >= rhs.intVal) : (rhs.doubleVal >= lhs.doubleVal);

        public static bool operator <(Number lhs, Number rhs) => 
            lhs.isInteger ? (lhs.intVal < rhs.intVal) : (rhs.doubleVal < lhs.doubleVal);

        public static bool operator >(Number lhs, Number rhs) =>
            lhs.isInteger ? (lhs.intVal > rhs.intVal) : (rhs.doubleVal > lhs.doubleVal);

        public Number()
        {
            intVal = 0;
            doubleVal = 0;
        }

        public Number(int i)
        {
            intVal = i;
            doubleVal = i;
        }

        public Number(double d)
        {
            intVal = (int)Math.Floor(d);
            doubleVal = d;
        }

        public Number(int i, double d)
        {
            intVal = i;
            doubleVal = d;
        }

        public dynamic GetValue()
        {
            return isInteger ? intVal : doubleVal;
        }

        public override string ToString()
        {
            return isInteger ? intVal.ToString() : doubleVal.ToString();
        }

        public static Number Parse(string value)
        {
            var nt = new Number();
            try
            {
                nt.intVal = int.Parse(value);
            }
            catch (Exception)
            {
                // ignore erros
                nt.intVal = 0;
            }

            try
            {
                nt.doubleVal = double.Parse(value);
            }
            catch (Exception)
            {
                // ignore erros
                nt.doubleVal = 0;
            }

            return nt;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var o = obj as Number;
            return o is null ? false : this == o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public abstract class ScriptNodePinBase : IDisposable
    {
        protected bool isDirty;

        public static double DefaultPinSize = 15;

        protected TextExtents extents;
        protected bool hasConnection;
        protected ElementBounds pinSelectBounds;
        protected ElementBounds hoverBounds;
        protected PointD pinConnectionPoint;
        protected Color color;

        protected bool allowsConnections;

        protected readonly string name;
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
        public bool CanCreateConnection => (connections.Count < maxNumberOfConnections || maxNumberOfConnections == -1) && allowsConnections;
        /*
         * @Connections is simply a list of all connections to this pin.
         */
        public List<ScriptNodePinConnection> Connections => connections;

        public Type PinType => pinValueType; 

        public Guid Guid;

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
            this.allowsConnections = true;
            this.Guid = Guid.NewGuid();
        }
        /*
         * Rendered before Pin but after "RenderBackground
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
        public virtual void RenderBackground(Context ctx, ImageSurface surface) { }
        /*
         *  Used to render any interactive element
         */
        public virtual void RenderInteractive(float deltaTime) { }

        /*
         *  Used to setup size and position
         */
        public abstract void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font);

        public virtual void OnPinConneced(ScriptNodePinConnection connection) { }
        public virtual void OnPinDisconnected(ScriptNodePinConnection connection) { }

        public virtual void MarkDirty()
        {
            this.isDirty = true;
        }

        public virtual void ToBytes(BinaryWriter writer)
        {
            writer.Write(Guid.ToString());
            MarkDirty();
        }

        public virtual void FromBytes(BinaryReader reader)
        {
            Guid = Guid.Parse(reader.ReadString());
            MarkDirty();
        }

        public abstract ScriptNodePinConnection CreateConnection();

        public virtual bool Connect(ScriptNodePinConnection connection)
        {
            if (CanCreateConnection == false)
            {
                return false;
            }

            connections.Add(connection);

            hasConnection = true;
            OnPinConneced(connection);

            return true;
        }

        public virtual bool Disconnect(ScriptNodePinConnection connection)
        {
            if (hasConnection && connections.Contains(connection))
            {
                connections.Remove(connection);
                OnPinDisconnected(connection);

                if (connections.Count <= 0)
                {
                    hasConnection = false;

                    if(pinValueType == typeof(DynamicType))
                    {
                        color = ColorForValueType(pinValueType);
                    }
                }

                return true;
            }

            return false;
        }

        public void AddConnectionsToList(List<ScriptNodePinConnection> connections)
        {
            foreach (var connection in this.connections)
            {
                if (connections.Contains(connection) == false)
                {
                    connections.Add(connection);
                }
            }
        }

        public virtual void Dispose()
        {
            ScriptNodePinConnection[] copy = new ScriptNodePinConnection[connections.Count];
            connections.CopyTo(copy);
            foreach (var connection in copy)
            {
                connection.DisconnectAll();
            }

            connections.Clear();
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
            if (type.Equals(typeof(Exec)))
            {
                return new Color(1.0, 1.0, 1.0, 1.0);
            }
            else if (type.IsAssignableFrom(typeof(Number)))
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
            else if (type == typeof(DynamicType))
            {
                return new Color(0.6, 0.6, 0.6, 1.0);
            }
            else if(type == typeof(Vec3d))
            {
                return new Color(0.9411764705882353, 1, 0, 1);
            }
            else if (type == typeof(Entity))
            {
                return new Color(0, 0, 1, 1);
            }

            return new Color(0.0, 0.0, 0.0, 1.0);
        }

        public bool CanConnectTo(ScriptNodePinBase other, ScriptNodePinConnection forConnection)
        {
            bool TypesAllowConnection = (pinValueType == other.pinValueType || 
                pinValueType == typeof(DynamicType) || other.pinValueType == typeof(DynamicType));

            return this != other && TypesAllowConnection
                && (CanCreateConnection || connections.Contains(forConnection))
                && (other.CanCreateConnection || other.connections.Contains(forConnection));
        }

        public virtual bool PointIsWithinSelectionBounds(double x, double y)
        {
            if (isDirty) return false;
            return pinSelectBounds.PointInside(x, y);
        }

        public virtual bool PointIsWithinHoverBounds(double x, double y)
        {
            if (isDirty || hoverBounds == null) return false;
            return hoverBounds.PointInside(x, y);
        }

        public ScriptNodePinConnection TopConnection()
        {
            if (connections.Count > 0)
                return connections[connections.Count - 1];

            return null;
        }

        public virtual string GetHoverText()
        {
            string typeString;
            if (pinValueType == typeof(DynamicType))
            {
                typeString = "Any";
            }
            else if (pinValueType == typeof(Exec))
            {
                typeString = "Execution";
            }
            else
            {
                typeString = pinValueType.Name;
            }
            return "This pin excepts " + typeString + " Pin Connections"; 
        }

        public virtual bool OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        { return PointIsWithinSelectionBounds(mouse.X, mouse.Y); }

        public virtual void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {}

        public virtual bool OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        { return PointIsWithinSelectionBounds(mouse.X, mouse.Y); }

        public virtual void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {}

        public virtual void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        { }
    }
}
