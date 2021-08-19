namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Common.Entities;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Server;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes.Attributes;
    using VSCCI.Extensions;
    using VSCCI.GUI.Pins;

    public class ServerSideSpawnEntityExecutable : ServerSideAction
    {
        public override void RunServerSide(IServerPlayer player, ICoreServerAPI api, string data)
        {
            var token = JToken.Parse(data);

            string posd = token.SelectToken("pos").ToString();
            var parts = posd.Split(',');
            Vec3d position = new Vec3d();
            position.X = double.Parse(parts[0]);
            position.Y = double.Parse(parts[1]);
            position.Z = double.Parse(parts[2]);

            var entityString = token.SelectToken("entity").ToString();
            System.Type entityType = System.Type.GetType(entityString);
            Entity e = System.Activator.CreateInstance(entityType) as Entity;
            EntityProperties type = api.World.GetEntityType(new AssetLocation("thrownbeenade"));
            Entity entity = api.World.ClassRegistry.CreateEntity(type);
            if (e != null)
            {
                e.ServerPos.SetPos(position);
                e.Pos.SetFrom(e.ServerPos);
                api.World.SpawnEntity(e);
            }
        }
    }

    //[NodeData("Actions", "Spawn Entity")]
    //[InputPin(typeof(Entity), 1)]
    //[InputPin(typeof(Vec3d), 2)]
    public class ServerSideSpawnEntityNode : ServerSideExecutableNode<ServerSideSpawnEntityExecutable>
    {
        internal static Type[] entityTypes = null;
        internal static string[] entityTypeNames = null;

        public static void PopulateEntitySelectionOptions()
        {
            var entityTypeList = new List<Type>();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type t in a.GetLoadableTypes())
                    {
                        // only get concrete types
                        if (typeof(Entity).IsAssignableFrom(t) && t.IsAbstract == false && t.IsInterface == false && t != typeof(object))
                        {
                            if (entityTypeList.Contains(t) == false)
                            {
                                entityTypeList.Add(t);
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    // continue
                }
            }

            entityTypes = new Type[entityTypeList.Count];
            entityTypeNames = new string[entityTypeList.Count];

            for(var i=0;i<entityTypeList.Count;i++)
            {
                entityTypes[i] = entityTypeList[i];
                entityTypeNames[i] = entityTypeList[i].Name;
            }
        }

        public ServerSideSpawnEntityNode(ICoreClientAPI api, MatrixElementBounds bounds) : base("Spawn Entity", api, bounds)
        {
            inputs.Add(new ScriptNodeDropdownInput(this, api, entityTypeNames, entityTypes, 0, typeof(Entity)));
            inputs.Add(new ScriptNodeInput(this, "Location", typeof(Vec3d)));
        }

        protected override void OnExecute()
        {
            System.Type entityType = inputs[1].GetInput();
            Vec3d pos = inputs[2].GetInput();
            var d = new Dictionary<string, string>
            {
                {"pos", $"{pos.X},{pos.Y},{pos.Z}"},
                { "entity", entityType.AssemblyQualifiedName }
            };

            data = JsonUtil.ToString(d);

            base.OnExecute();
        }

        public override string GetNodeDescription()
        {
            return "This Spawn an Entity at \"Location\". Only those with \"All-Allowed\" Event persmissions can use this node";
        }
    }
}
