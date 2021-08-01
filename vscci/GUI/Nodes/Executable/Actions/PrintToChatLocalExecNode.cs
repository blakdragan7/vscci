﻿namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Nodes.Attributes;

    [NodeData("Actions", "Show Chat Local")]
    [InputPin(typeof(Exec), 0)]
    [InputPin(typeof(string), 1)]
    [OutputPin(typeof(Exec), 0)]
    class PrintToChatLocalExecNode : ExecutableScriptNode
    {
        public static int MESSAGE_INPUT_INDEX = 1;
        public PrintToChatLocalExecNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("Print To Local Chat", api, nodeTransform, bounds)
        {
            inputs.Add(new ScriptNodeInput(this, "Message", typeof(string)));
        }

        protected override void OnExecute()
        {
            string message = inputs[MESSAGE_INPUT_INDEX].GetInput();

            if(message != null)
            {
                api.Event.EnqueueMainThreadTask(() => api.ShowChatMessage(message) , "Print To Chat Local");
            }

            ExecuteNextNode();
        }
    }
}
