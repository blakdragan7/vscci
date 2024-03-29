﻿namespace VSCCI.GUI.Pins
{
    using Cairo;
    using System;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using VSCCI.GUI.Elements;
    using VSCCI.GUI.Nodes;

    class ScriptNodeDropdownInput : ScriptNodeInput
    {
        GuiTinyDropdown dropElement;

        private bool dropdownNeedsCompose;
        private int currentSelectionIndex;
        private dynamic currentSelection;

        public event EventHandler<dynamic> OnItemSelection;

        public ScriptNodeDropdownInput(ScriptNode owner, Type pinType) : base(owner, "", pinType)
        {
            dropElement = new GuiTinyDropdown(api, EnumVerticalAlign.Middle, EnumHorizontalAlign.Right, OnItemSelected, ElementBounds.Fixed(0, 0).WithEmptyParent(), CairoFont.WhiteDetailText().WithFontSize(10));
            dropdownNeedsCompose = true;
            currentSelectionIndex = 0;
            currentSelection = null;
            dropElement.onSelectionIndexChanged += onSelectionIndexChanged;
        }

        private void onSelectionIndexChanged(object sender, int e)
        {
            currentSelectionIndex = e;
        }

        public ScriptNodeDropdownInput(ScriptNode owner, ICoreClientAPI api, string[] names, dynamic[] values, int currentSelection, Type pinType) : base(owner, "", pinType)
        {
            dropElement = new GuiTinyDropdown(api, values, names, currentSelection, EnumVerticalAlign.Middle, EnumHorizontalAlign.Right, OnItemSelected, ElementBounds.Fixed(0, 0).WithEmptyParent(), CairoFont.WhiteDetailText().WithFontSize(10));
            dropdownNeedsCompose = true;
            currentSelectionIndex = currentSelection;
            dropElement.onSelectionIndexChanged += onSelectionIndexChanged;
            this.currentSelection = values[currentSelection];
            this.api = api;
        }

        private void OnItemSelected(dynamic selection, bool selected)
        {
            currentSelection = selection;
            OnItemSelection?.Invoke(this, selection);
        }

        public override void RenderBackground(Context ctx, ImageSurface surface)
        {
            dropElement.ComposeElements(ctx, null);
            dropdownNeedsCompose = false;
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface)
        {
            
        }

        public override void RenderInteractive(float deltaTime)
        {
            if (hasConnection == false && dropdownNeedsCompose == false)
            {
                dropElement.RenderInteractiveElements((float)deltaTime);
            }
        }

        public override void RenderPin(Context ctx, ImageSurface surface)
        {
            if (hasConnection)
            {
                base.RenderPin(ctx, surface);
            }
        }

        public override void OnPinConneced(ScriptNodePinConnection connection)
        {
            base.OnPinConneced(connection);
            dropElement.Dispose();
        }

        public override void OnPinDisconnected(ScriptNodePinConnection connection)
        {
            base.OnPinDisconnected(connection);
            dropdownNeedsCompose = true;
        }

        public override void SetupSizeAndOffsets(double x, double y, Context ctx, CairoFont font)
        {
            X = x;
            Y = y;

            owner.Bounds.ChildBounds.Remove(dropElement.Bounds);
            dropElement.Bounds = ElementBounds.Fixed(X, Y, 100, 30);
            owner.Bounds.WithChild(dropElement.Bounds);
            dropElement.Bounds.CalcWorldBounds();

            dropElement.UpdateArrowPos();

            pinExtents.Width = dropElement.Bounds.OuterWidth;
            pinExtents.Height = dropElement.Bounds.OuterHeight;

            isDirty = true;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(x, y, pinExtents.Width, pinExtents.Height);
            owner.Bounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(x, y, pinExtents.Width, pinExtents.Height);
            owner.Bounds.WithChild(hoverBounds);
            hoverBounds.CalcWorldBounds();

            pinConnectionPoint.X = dropElement.Bounds.drawX + (dropElement.Bounds.InnerHeight / 2.0);
            pinConnectionPoint.Y = dropElement.Bounds.drawY + (dropElement.Bounds.InnerHeight / 2.0);

            dropElement.ComposeElements(ctx, null);
            dropdownNeedsCompose = false;
        }

        public override ScriptNodePinConnection CreateConnection()
        {
            // we never create a connection from clicking on this but we can drop a connection onto it to connect
            return null;
        }

        public override dynamic GetInput()
        {
            if (hasConnection)
            {
                return base.GetInput();
            }

            return currentSelection;
        }

        public override void FromBytes(BinaryReader reader)
        {
            base.FromBytes(reader);
            currentSelectionIndex = reader.ReadInt32();
            currentSelection = dropElement.GetValue(currentSelectionIndex);

            api.Event.EnqueueMainThreadTask(() => dropElement.SetSelectedIndex(currentSelectionIndex),
                "update dropdown pin selection");
        }

        public override void ToBytes(BinaryWriter writer)
        {
            base.ToBytes(writer);
            writer.Write(currentSelectionIndex);
        }

        public override bool OnMouseDown(ICoreClientAPI api, MouseEvent mouse)
        {
            var pr = mouse.Handled;
            mouse.Handled = false;
            dropElement.OnMouseDown(api, mouse);
            if(mouse.Handled == false)
            {
                mouse.Handled = pr;
                return false;
            }

            return true;
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent mouse)
        {
            dropElement.OnMouseMove(api, mouse);
        }

        public override bool OnMouseUp(ICoreClientAPI api, MouseEvent mouse)
        {
            dropElement.OnMouseUp(api, mouse);

            return mouse.Handled;
        }

        public override void Dispose()
        {
            base.Dispose();

            dropElement.Dispose();
        }
    }
}
