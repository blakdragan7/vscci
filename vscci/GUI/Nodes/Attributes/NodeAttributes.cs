namespace VSCCI.GUI.Nodes.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class InputPinAttribute : Attribute
    {
        public InputPinAttribute(Type pinType, int index)
        {
            PinType = pinType;
            Index = index;
        }

        public Type PinType { get; private set; }
        public int Index { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class OutputPinAttribute : Attribute
    {
        public OutputPinAttribute(Type pinType, int index)
        {
            PinType = pinType;
            Index = index;
        }

        public Type PinType { get; private set; }
        public int Index { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeDataAttribute : Attribute
    {
        public NodeDataAttribute(string category, string listName)
        {
            Category = category;
            ListName = listName;
        }

        public string Category { get; private set; }
        public string ListName { get; private set; }
    }
}
