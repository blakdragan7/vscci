namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VSCCI.GUI.Nodes;

    public class ScriptNodePinConnection : IDisposable
    {
        private ScriptNodeOutput output;
        private ScriptNodeInput input;
        public PointD DrawPoint;

        private bool skipFirstRender;
        private ScriptNodePinConnectionManager manager;

        public bool IsConnected => input != null && output != null;
        public bool HasAnyConnection => input != null || output != null;

        public bool NeedsOutput => output == null;
        public bool NeedsInput => input == null;

        public ScriptNodeInput Input => input;
        public ScriptNodeOutput Output => output;

        public Type ConnectionType { get; private set; }

        public ScriptNodePinConnection(ScriptNodePinConnectionManager manage)
        {
            this.input = null;
            this.output = null;
            this.ConnectionType = null;
            this.skipFirstRender = true;
            this.manager = manage;
        }

        public void Render(Context ctx, ImageSurface surface)
        {
            ctx.Save();

            if (output == null && input != null)
            {
                ctx.SetSourceColor(input.PinColor);
                ctx.MoveTo(DrawPoint);
                ctx.LineTo(input.PinConnectionPoint);
            }
            else if (output != null && input == null)
            {
                ctx.SetSourceColor(output.PinColor);
                ctx.MoveTo(output.PinConnectionPoint);
                ctx.LineTo(DrawPoint);
            }
            else if (output != null && input != null)
            {
                ctx.SetSourceColor(input.PinColor);
                ctx.MoveTo(output.PinConnectionPoint);
                ctx.LineTo(input.PinConnectionPoint);
            }

            ctx.Stroke();

            ctx.Restore();
        }

        public void WriteToBytes(BinaryWriter writer)
        {
            if(input != null)
            {
                writer.Write(true);
                writer.Write(input.Guid.ToString());
            }
            else
            {
                writer.Write(false);
            }

            if(output != null)
            {
                writer.Write(true);
                writer.Write(output.Guid.ToString());
            }
            else
            {
                writer.Write(false);
            }
        }

        public bool Connect(ScriptNodeInput input)
        {
            if (output == null)
            {
                if (input.Connect(this))
                {
                    this.input = input;
                    this.ConnectionType = input.PinType;
                    this.DrawPoint = input.PinConnectionPoint;
                }
                return true;
            }

            if (output.CanConnectTo(input, this))
            {
                input.Connect(this);
                this.input = input;
                return true;
            }

            return false;
        }

        public bool Connect(ScriptNodeOutput output)
        {
            if(input == null)
            {
                if(output.Connect(this))
                {
                    this.output = output;
                    this.ConnectionType = output.PinType;
                    this.DrawPoint = output.PinConnectionPoint;
                    return true;
                }
                return false;
            }
            else if (input.CanConnectTo(output, this))
            {
                if (output.Connect(this))
                {
                    this.output = output;
                    return true;
                }
                return false;
            }

            return false;
        }

        public bool DisconnectAll()
        {
            if (HasAnyConnection)
            {
                if (input != null) input.Disconnect(this);
                if (output != null) output.Disconnect(this);

                input = null;
                output = null;

                manager.RemoveConnection(this);
                manager.MarkDirty();

                return true;
            }
            return false;
        }

        public bool DisconnectOutput()
        {
            if (output != null)
            {
                output.Disconnect(this);
                output = null;
                manager.MarkDirty();
                return true;
            }
            return false;
        }

        public bool DisconnectInput()
        {
            if (input != null)
            {
                input.Disconnect(this);
                input = null;
                manager.MarkDirty();
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            DisconnectAll();
        }
    }
}
