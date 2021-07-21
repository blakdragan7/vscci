namespace vscci.GUI.Nodes
{
    using Cairo;
    public class ScriptNodePinConnection
    {
        private ScriptNodeOutput output;
        private ScriptNodeInput input;
        public PointD DrawPoint;

        public bool IsConnected => input != null && output != null;
        public bool HasAnyConnection => input != null || output != null;

        public bool NeedsOutput => output == null;
        public bool NeedsInput => input == null;

        public ScriptNodePinConnection(ScriptNodeOutput output)
        {
            this.input = null;
            this.output = output;
            this.output.Connect(this);
            this.DrawPoint = new PointD();
        }

        public ScriptNodePinConnection(ScriptNodeInput input)
        {
            this.output = null;
            this.input = input;
            this.input.Connect(this);
            this.DrawPoint = new PointD();
        }

        public static ScriptNodePinConnection CreateConnectionBetween(ScriptNodeOutput output, ScriptNodeInput input)
        {
            var connection = new ScriptNodePinConnection(output);
            if (connection.Connect(input))
            {
                return connection;
            }

            return null;
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
                ctx.SetSourceColor(output.PinColor);
                ctx.MoveTo(output.PinConnectionPoint);
                ctx.LineTo(input.PinConnectionPoint);
            }

            ctx.Stroke();

            ctx.Restore();
        }

        public bool Connect(ScriptNodeInput input)
        {
            if (output == null)
            {
                this.input = input;
                this.input.Connect(this);
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
                return true;
            }
            return false;
        }
    }
}
