namespace VSCCI.GUI.Elements
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using VSCCI.GUI.Interfaces;

    public class UniqueSelectableListElement : GuiElement, ISelectableList
    {
        private readonly CairoFont font;
        private readonly int maxVisibleItems;
        private readonly TextDrawUtil util;

        private List<ListItem> items;
        private List<ListItem> searchedItems;
        private ListItemSelection itemSelection;

        private double yAdvance;
        private double yScrollOffset;

        private int offsetScrollIndex;

        protected int selectedIndex;

        protected string searchText;

        public event EventHandler<ListItem> OnItemSelected;

        public ListItem SelectedItem => itemSelection?.selectedItem;

        public ElementBounds ListBounds => Bounds;

        public UniqueSelectableListElement(ICoreClientAPI api, ElementBounds bounds, int maxVisibleItems = 5, CairoFont font = null) : base(api, bounds)
        {
            items = new List<ListItem>();
            searchedItems = new List<ListItem>();

            bounds.CalcWorldBounds();
            this.maxVisibleItems = maxVisibleItems;
            if (font == null)
            {
                this.font = CairoFont.WhiteDetailText();
            }
            else
            {
                this.font = font;
            }
            util = new TextDrawUtil();
            itemSelection = null;
            yAdvance = Bounds.InnerHeight / maxVisibleItems;
            yScrollOffset = 0;
            offsetScrollIndex = 0;

            searchText = "";
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            base.ComposeElements(ctxStatic, surface);
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);
        }

        public void ResetSelections()
        {
            yScrollOffset = 0;

            offsetScrollIndex = 0;

            itemSelection = null;
            searchText = "";
            searchedItems.Clear();
        }

        public void OnRender(Context ctx, ImageSurface surface, float deltaTime)
        {
            RenderBackground(ctx, surface);
            if (searchText.Length > 0)
            {
                RenderSearchedList(ctx, surface);
                RenderSearchText(ctx, surface);
            }
            else
            {
                RenderList(ctx, surface);
            }
        }

        public void AddListItems(List<ListItem> newItems)
        {
            foreach(var item in newItems)
            {
                AddListItem(item);
            }
        }

        public void AddListItem(ListItem item)
        {
            if(items.Contains(item) == false)
                items.Add(item);
        }

        public void AddListItem(string Category, string Name, dynamic Value)
        {
            AddListItem(new ListItem() { Catagory = Category, Name = Name, Value = Value } );
        }

        public void RemoveListItem(ListItem item)
        {
            items.Remove(item);
        }

        public void SetPosition(double x, double y)
        {
            Bounds.WithFixedPosition(x - Bounds.ParentBounds.absX, y - Bounds.ParentBounds.absY);
            Bounds.CalcWorldBounds();
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);

            if (IsPositionInside(args.X, args.Y))
            {
                int index = (int)Math.Floor(((args.Y - Bounds.absY) - yScrollOffset) / yAdvance);

                if (index < 0) index = 0;

                if (searchText.Length > 0)
                {
                    if (searchedItems.Count > index)
                    {
                        var drawY = Bounds.drawY + (index * yAdvance);

                        itemSelection = new ListItemSelection()
                        {
                            highlightDrawX = Bounds.drawX,
                            highlightDrawY = drawY,
                            index = index,
                            selectedItem = searchedItems[index]
                        };
                    }
                }
                else
                {
                    if (items.Count > index)
                    {
                        var drawY = Bounds.drawY + (index * yAdvance);

                        itemSelection = new ListItemSelection()
                        {
                            highlightDrawX = Bounds.drawX,
                            highlightDrawY = drawY,
                            index = index,
                            selectedItem = items[index]
                        };
                    }
                }
            }
            else
            {
                itemSelection = null;
            }
        }

        public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
        {
            base.OnMouseWheel(api, args);

            if(itemSelection != null && maxVisibleItems < items.Count)
            {
                var indexDiff = (items.Count - maxVisibleItems);

                yScrollOffset += args.delta * 2;
                yScrollOffset = Math.Min(yScrollOffset, 0);
                yScrollOffset = Math.Max(yScrollOffset, -(indexDiff * yAdvance));

                offsetScrollIndex = (int)Math.Floor(Math.Abs(yScrollOffset) / yAdvance);
            }
            else if (itemSelection == null)
            {
                yScrollOffset = 0;
            }
        }

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDownOnElement(api, args);

            if(itemSelection != null)
            {
                OnItemSelected?.Invoke(this, itemSelection.selectedItem);
            }
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyDown(api, args);
            if(args.KeyCode == (int)GlKeys.BackSpace)
            {
                if(searchText.Length > 0)
                    searchText = searchText.Remove(searchText.Length - 1);
            }
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            base.OnKeyPress(api, args);
            searchText += args.KeyChar;
            searchedItems.Clear();

            foreach(var item in items)
            {
                if(item.Name.ToLower().Contains(searchText.ToLower()))
                {
                    searchedItems.Add(item);
                }
            }
        }

        private void RenderBackground(Context ctx, Surface surface)
        {
            ctx.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2], GuiStyle.DialogDefaultBgColor[3]);
            ElementRoundRectangle(ctx, Bounds, true);
            ctx.Fill();

            EmbossRoundRectangleElement(ctx, Bounds);
        }

        private void RenderSearchText(Context ctx, Surface surface)
        {
            // render search text background

            ctx.SetSourceRGBA(0.1568627450980392, 0.0980392156862745, 0.0509803921568627, 0.4);
            RoundRectangle(ctx, Bounds.drawX, Bounds.drawY - 20, Bounds.OuterWidth, 15, 1);
            ctx.Fill();

            ctx.Save();
            font.SetupContext(ctx);
            var extents = ctx.TextExtents(searchText);
            util.DrawTextLine(ctx, font, searchText, Bounds.drawX, Bounds.drawY - 20);
            ctx.Restore();
        }

        private void RenderList(Context ctx, Surface surface)
        {
            var x = Bounds.drawX + Bounds.InnerWidth / 2.0;
            var y = Bounds.drawY + (yAdvance / 2.0) + yScrollOffset;

            var cellDrawY = Bounds.drawY + yScrollOffset;

            var currentRendered = 0;

            var lclIndex = offsetScrollIndex;

            foreach (var item in items)
            {
                if (lclIndex > 0)
                {
                    lclIndex--;
                    cellDrawY += yAdvance;
                    y += yAdvance;

                    continue;
                }

                ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor[0], GuiStyle.DialogStrongBgColor[1], GuiStyle.DialogStrongBgColor[2], GuiStyle.DialogStrongBgColor[3]);
                RoundRectangle(ctx, Bounds.drawX + 2, cellDrawY + 2, Bounds.InnerWidth - 4, yAdvance - 4, 1);
                ctx.Fill();

                cellDrawY += yAdvance;

                ctx.Save();
                font.SetupContext(ctx);
                var extents = ctx.TextExtents(item.Name);
                util.DrawTextLine(ctx, font, item.Name, x - (extents.Width / 2.0), y - (extents.Height / 2.2));
                ctx.Restore();

                y += yAdvance;
                currentRendered++;
                if(currentRendered >= maxVisibleItems)
                {
                    break;
                }
            }

            if(itemSelection != null)
            {
                x = itemSelection.highlightDrawX;
                y = itemSelection.highlightDrawY + yScrollOffset;

                // draw selection highlight

                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
                RoundRectangle(ctx, x, y, Bounds.InnerWidth, yAdvance, 1);
                ctx.Fill();
            }
        }

        private void RenderSearchedList(Context ctx, Surface surface)
        {
            var x = Bounds.drawX + Bounds.InnerWidth / 2.0;
            var y = Bounds.drawY + (yAdvance / 2.0) + yScrollOffset;

            var cellDrawY = Bounds.drawY + yScrollOffset;

            var currentRendered = 0;

            var lclIndex = offsetScrollIndex;

            foreach (var item in searchedItems)
            {
                if (lclIndex > 0)
                {
                    lclIndex--;
                    cellDrawY += yAdvance;
                    y += yAdvance;

                    continue;
                }

                ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor[0], GuiStyle.DialogStrongBgColor[1], GuiStyle.DialogStrongBgColor[2], GuiStyle.DialogStrongBgColor[3]);
                RoundRectangle(ctx, Bounds.drawX + 2, cellDrawY + 2, Bounds.InnerWidth - 4, yAdvance - 4, 1);
                ctx.Fill();

                cellDrawY += yAdvance;

                ctx.Save();
                font.SetupContext(ctx);
                var extents = ctx.TextExtents(item.Name);
                util.DrawTextLine(ctx, font, item.Name, x - (extents.Width / 2.0), y - (extents.Height / 2.2));
                ctx.Restore();

                y += yAdvance;
                currentRendered++;
                if (currentRendered >= maxVisibleItems)
                {
                    break;
                }
            }

            if (itemSelection != null)
            {
                x = itemSelection.highlightDrawX;
                y = itemSelection.highlightDrawY + yScrollOffset;

                // draw selection highlight

                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
                RoundRectangle(ctx, x, y, Bounds.InnerWidth, yAdvance, 1);
                ctx.Fill();
            }
        }
    }
}
