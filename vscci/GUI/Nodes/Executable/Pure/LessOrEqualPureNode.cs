﻿namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;

    class LessOrEqualPureNode<T> : ExecutableScriptNode where T : IComparable
    {
        public LessOrEqualPureNode(ICoreClientAPI api, Matrix nodeTransform, ElementBounds bounds) : base("<=", api, nodeTransform, bounds, true)
        {
            inputs.Add(new ScriptNodeInput(this, "First", typeof(T)));
            inputs.Add(new ScriptNodeInput(this, "Second", typeof(T)));

            outputs.Add(new ScriptNodeOutput(this, "Result", 1, typeof(bool)));

            shouldAutoExecuteNext = false;
        }

        protected override void OnExecute()
        {
            dynamic first = inputs[0].GetInput();
            dynamic second = inputs[1].GetInput();

            outputs[0].Value = first <= second;
        }
    }
}