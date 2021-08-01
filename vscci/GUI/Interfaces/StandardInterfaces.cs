namespace VSCCI.GUI.Interfaces
{
    using Cairo;
    using System;
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using VSCCI.GUI.Elements;

    interface ISelectableList : IGraphElement
    {
        event EventHandler<ListItem> OnItemSelected;
        void AddListItems(List<ListItem> newItems);
        void AddListItem(ListItem item);
        void AddListItem(string Category, string Name, dynamic Value);
        void RemoveListItem(ListItem item);
        void ResetSelections();
        ElementBounds ListBounds { get; }
    }

    interface IGraphElement
    {
        void OnRender(Context ctx, ImageSurface surface, float deltaTime);
        void SetPosition(double x, double y);
        void OnMouseMove(ICoreClientAPI api, MouseEvent args);
        void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args);
        void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args);
        void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args);
        bool IsPositionInside(int x, int y);
    }
}
