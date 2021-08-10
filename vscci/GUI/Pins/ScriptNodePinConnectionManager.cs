namespace VSCCI.GUI.Pins
{
    using Cairo;
    using Vintagestory.API.Client;

    using System;
    using System.Collections.Generic;
    using VSCCI.GUI.Nodes;
    using System.IO;
    using VSCCI.Data;

    public class ScriptNodePinConnectionManager : IDisposable
    {
        private static ScriptNodePinConnectionManager theManager = new ScriptNodePinConnectionManager();

        private ScriptNodePinConnection activeConnection;
        private List<ScriptNodePinConnection> connections;
        private LoadedTexture texture;

        protected bool isDirty;
        protected ICoreClientAPI api;
        protected ElementBounds bounds;


        public static ScriptNodePinConnectionManager TheManage => theManager;

        public bool HasActiveConnection => activeConnection != null;
        public ScriptNodeOutput ActiveOutput => activeConnection?.Output;
        public System.Type ActiveType => activeConnection?.ConnectionType;

        private ScriptNodePinConnectionManager()
        {
            if (theManager != null)
            {
                throw new Exception("There can only be one ScriptNodePinConnectionManager");
            }

            theManager = this;
        }

        public void SetupManager(ICoreClientAPI api, ElementBounds bounds)
        {
            this.api = api;
            this.bounds = bounds;
            this.isDirty = true;
            this.activeConnection = null;

            connections = new List<ScriptNodePinConnection>();

            texture = new LoadedTexture(api);
        }

        public void Dispose()
        {
            foreach(var conn in connections)
            {
                conn.Dispose();
            }

            texture.Dispose();
        }

        public void RenderConnections(float deltaTime)
        {
            if (isDirty)
            {
                
                ComposeTexture();
            }

            api.Render.Render2DTexture(texture.TextureId, bounds, Constants.SCRIPT_NODE_CONNECTION_Z_POS);
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        public ScriptNodePinConnection CreateConnection()
        {
            var connection = new ScriptNodePinConnection(this);
            connections.Add(connection);
            isDirty = true;
            activeConnection = connection;

            return connection;
        }

        public bool ConnectActiveConnectionToNodeAtPoint(ScriptNode node, double x, double y)
        {
            if (HasActiveConnection)
            {
                if (node.ConnectionWillConnectToPoint(activeConnection, x, y))
                {
                    ResetActiveConnection();
                    isDirty = true;
                    return true;
                }
            }
            
            return false;
        }

        public ScriptNodePinConnection CreateConnectionBetween(ScriptNodeOutput output, ScriptNodeInput input)
        {
            var connection = new ScriptNodePinConnection(this);

            if (connection.Connect(output) && connection.Connect(input))
            {
                connections.Add(connection);
                isDirty = true;
                return connection;
            }

            return null;
        }

        public ScriptNodePinConnection CreateConnectionFromBytes(BinaryReader reader, List<ScriptNode> allNodes)
        {
            Guid inputGuid;
            Guid outputGuid;

            ScriptNodeInput input = null;
            ScriptNodeOutput output = null;

            if (reader.ReadBoolean())
            {
                inputGuid = System.Guid.Parse(reader.ReadString());
            }
            else
            {
                return null;
            }

            if (reader.ReadBoolean())
            {
                outputGuid = System.Guid.Parse(reader.ReadString());
            }
            else
            {
                return null;
            }

            foreach (var node in allNodes)
            {
                if (input == null)
                {
                    input = node.InputForGuid(inputGuid);
                }

                if (output == null)
                {
                    output = node.OutputForGuid(outputGuid);
                }

                if (input != null && output != null) break;
            }

            if (input != null && output != null)
            {
                return CreateConnectionBetween(output, input);
            }

            return null;
        }

        public void RemoveConnection(ScriptNodePinConnection conn)
        {
            conn.Dispose();
            connections.Remove(conn);
            isDirty = true;
        }

        public void FromBytes(BinaryReader reader, List<ScriptNode> allNodes)
        {
            var numConnections = reader.ReadInt32();
            for (var i = 0; i < numConnections; i++)
            {
                CreateConnectionFromBytes(reader, allNodes);
            }
        }

        public void ToBytes(BinaryWriter writer)
        {
            writer.Write(connections.Count);

            foreach (var connection in connections)
            {
                connection.WriteToBytes(writer);
            }
        }

        public void UpdateActiveConnection(double x, double y)
        {
            activeConnection.DrawPoint.X = x;
            activeConnection.DrawPoint.Y = y;

            isDirty = true;
        }

        public void RemoveActiveConnection()
        {
            activeConnection.Dispose();
            connections.Remove(activeConnection);
            activeConnection = null;

            isDirty = true;
        }

        public void ResetActiveConnection()
        {
            activeConnection = null;
            isDirty = true;
        }

        private void ComposeTexture()
        {
            ImageSurface surface = new ImageSurface(Format.ARGB32, bounds.OuterWidthInt, bounds.OuterHeightInt);
            Context ctx = new Context(surface);
            ctx.SetSourceRGBA(0, 0, 0, 0);
            ctx.Paint();
            ctx.Antialias = Antialias.Best;

            foreach(var con in connections)
            {
                con.Render(ctx, surface);
            }

            api.Gui.LoadOrUpdateCairoTexture(surface, true, ref texture);

            ctx.Dispose();
            surface.Dispose();

            isDirty = false;
        }
    }
}
