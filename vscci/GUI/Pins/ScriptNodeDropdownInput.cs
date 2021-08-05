namespace VSCCI.GUI.Nodes
{
    using Cairo;
    using System;
    using System.IO;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using VSCCI.GUI.Elements;

    class ScriptNodeDropdownInput : ScriptNodeInput
    {
        GuiTinyDropdown dropElement;

        private bool dropdownNeedsCompose;
        private int currentSelectionIndex;
        private dynamic currentSelection;

        private ICoreClientAPI api;

        public event EventHandler<dynamic> OnItemSelection;

        public ScriptNodeDropdownInput(ScriptNode owner, ICoreClientAPI api, Type pinType) : base(owner, "", pinType)
        {
            dropElement = new GuiTinyDropdown(api, OnItemSelected, ElementBounds.Fixed(0, 0).WithEmptyParent(), CairoFont.WhiteDetailText().WithFontSize(10));
            dropdownNeedsCompose = true;
            currentSelectionIndex = 0;
            currentSelection = null;
            this.api = api;
        }

        public ScriptNodeDropdownInput(ScriptNode owner, ICoreClientAPI api, string[] names, dynamic[] values, Type pinType) : base(owner, "", pinType)
        {
            dropElement = new GuiTinyDropdown(api, values, names, 0, OnItemSelected, ElementBounds.Fixed(0, 0).WithEmptyParent(), CairoFont.WhiteDetailText().WithFontSize(10));
            dropdownNeedsCompose = true;
            currentSelectionIndex = 0;
            currentSelection = null;
            this.api = api;
        }

        private void OnItemSelected(dynamic selection, bool selected)
        {
            currentSelection = selection;
            OnItemSelection?.Invoke(this, selection);
        }

        public override void RenderOther(Context ctx, ImageSurface surface, double deltaTime)
        {
            if (hasConnection == false)
            {
                var bounds = dropElement.Bounds;
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.2);
                RoundRectangle(ctx, bounds.drawX, bounds.drawY, bounds.InnerWidth, bounds.InnerHeight, 1.0);
                ctx.Fill();
            }
        }

        public override void RenderText(TextDrawUtil textUtil, CairoFont font, Context ctx, ImageSurface surface, double deltaTime)
        {
            if(dropdownNeedsCompose)
            {
                dropdownNeedsCompose = false;
                dropElement.ComposeElements(ctx, null);
            }
        }

        public override void RenderInteractive(double deltaTime)
        {
            if (hasConnection == false && dropdownNeedsCompose == false)
            {
                dropElement.RenderInteractiveElements((float)deltaTime);
            }
        }

        public override void RenderPin(Context ctx, ImageSurface surface, double deltaTime)
        {
            if (hasConnection)
            {
                ctx.SetSourceColor(PinColor);
                ctx.LineWidth = 2;
                RoundRectangle(ctx, dropElement.Bounds.drawX, dropElement.Bounds.drawY, dropElement.Bounds.InnerHeight, dropElement.Bounds.InnerHeight, GuiStyle.ElementBGRadius);
                ctx.Fill();
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

        public override void Compose(double colx, double coly, double drawx, double drawy, Context ctx, CairoFont font)
        {
            X = drawx;
            Y = drawy;

            owner.Bounds.ParentBounds.ChildBounds.Remove(dropElement.Bounds);
            dropElement.Bounds = ElementBounds.Fixed(X, Y, 100, 30);
            owner.Bounds.ParentBounds.WithChild(dropElement.Bounds);
            dropElement.Bounds.CalcWorldBounds();

            extents.Width = dropElement.Bounds.OuterWidth;
            extents.Height = dropElement.Bounds.OuterHeight;

            isDirty = false;

            if (pinSelectBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(pinSelectBounds);
            }

            pinSelectBounds = ElementBounds.Fixed(colx, coly, extents.Width, extents.Height);
            owner.Bounds.ParentBounds.WithChild(pinSelectBounds);
            pinSelectBounds.CalcWorldBounds();

            if (hoverBounds != null)
            {
                owner.Bounds.ParentBounds.ChildBounds.Remove(hoverBounds);
            }

            hoverBounds = ElementBounds.Fixed(colx, coly, extents.Width, extents.Height);
            owner.Bounds.ParentBounds.WithChild(hoverBounds);
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

            api.Event.EnqueueMainThreadTask(() => dropElement.SetSelectedIndex(currentSelectionIndex),
                "update dropdown pin selection");

        }

        public override void ToBytes(BinaryWriter writer)
        {
            base.ToBytes(writer);
            writer.Write(currentSelectionIndex);
        }

        public override bool OnMouseDown(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {
            var mevent = new MouseEvent((int)x, (int)y, button);
            dropElement.OnMouseDown(api, mevent);

            return mevent.Handled;
        }

        public override void OnMouseMove(ICoreClientAPI api, double x, double y, double deltaX, double deltaY)
        {
            var mevent = new MouseEvent((int)x, (int)y, (int)deltaX, (int)deltaY);
            dropElement.OnMouseMove(api, mevent);
        }

        public override bool OnMouseUp(ICoreClientAPI api, double x, double y, EnumMouseButton button)
        {
            var mevent = new MouseEvent((int)x, (int)y, button);
            dropElement.OnMouseUp(api, mevent);

            return mevent.Handled;
        }

        public override void Dispose()
        {
            base.Dispose();

            dropElement.Dispose();
        }
    }
}
