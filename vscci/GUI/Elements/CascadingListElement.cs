namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    using System.Collections.Generic;
    using System;

    public class CascadingListItem
    {
        public string Catagory;
        public string Name;
        public dynamic Value;
    }

    internal class ListItemSelection
    {
        public double highlightDrawX;
        public double highlightDrawY;

        public int index;

        public CascadingListItem selectedItem;
    }

    internal class CategorySelection
    {
        public double highlightDrawX;
        public double highlightDrawY;

        public int index;

        public ElementBounds Bounds;
        public List<CascadingListItem> selectedItems;
    }

    public class CascadingListElement : GuiElement
    {
        private readonly CairoFont font;
        private readonly int maxVisibleItemsPerList;
        private readonly TextDrawUtil util;

        private Dictionary<string, List<CascadingListItem>> items;
        private ListItemSelection itemSelection;
        private CategorySelection categorySelection;

        private double yAdvance;
        private double yScrollOffset;
        private double ySubScrollOffset;

        private int offsetScrollIndex;
        private int offsetSubScrollIndex;

        protected int selectedIndex;


        public event EventHandler<CascadingListItem> OnItemSelected;

        public CascadingListItem SelectedItem => itemSelection?.selectedItem;

        public CascadingListElement(ICoreClientAPI api, ElementBounds bounds, int maxVisibleItemsPerList = 5, CairoFont font = null) : base(api, bounds)
        {
            items = new Dictionary<string, List<CascadingListItem>>();
            bounds.CalcWorldBounds();
            this.maxVisibleItemsPerList = maxVisibleItemsPerList;
            if (font == null)
            {
                this.font = CairoFont.WhiteDetailText();
            }
            else
            {
                this.font = font;
            }
            util = new TextDrawUtil();
            categorySelection = null;
            itemSelection = null;
            yAdvance = Bounds.InnerHeight / maxVisibleItemsPerList;
            yScrollOffset = 0;
            ySubScrollOffset = 0;
            offsetScrollIndex = 0;
            offsetSubScrollIndex = 0;
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
            ySubScrollOffset = 0;

            offsetScrollIndex = 0;
            offsetSubScrollIndex = 0;

            if (categorySelection != null)
            {
                Bounds.ParentBounds.ChildBounds.Remove(categorySelection.Bounds);
                categorySelection = null;
            }

            itemSelection = null;
        }

        public void OnRender(Context ctx, ImageSurface surface, float deltaTime)
        {
            RenderBackground(ctx, surface);
            RenderCategories(ctx, surface);
            RenderCategorySelection(ctx, surface);
        }

        public void AddListItems(List<CascadingListItem> newItems)
        {
            foreach(var item in newItems)
            {
                AddListItem(item);
            }
        }

        public void AddListItem(CascadingListItem item)
        {
            List<CascadingListItem> list = null;
            if (items.TryGetValue(item.Catagory, out list))
            {
                list.Add(item);
            }
            else
            {
                list = new List<CascadingListItem>();
                list.Add(item);

                items.Add(item.Catagory, list);
            }
        }

        public void AddListItem(string Category, string Name, dynamic Value)
        {
            AddListItem(new CascadingListItem() { Catagory = Category, Name = Name, Value = Value } );
        }

        public void RemoveListItem(CascadingListItem item)
        {
            List<CascadingListItem> list = null;
            if (items.TryGetValue(item.Catagory, out list))
            {
                list.Remove(item);
                if(list.Count == 0)
                {
                    items.Remove(item.Catagory);
                }
            }
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
                itemSelection = null;

                int index = (int)Math.Floor(((args.Y - Bounds.absY) - yScrollOffset) / yAdvance);

                if (index < 0) index = 0;

                if (items.Keys.Count > index)
                {
                    if (categorySelection != null && categorySelection.index == index)
                    {
                        return;
                    }

                    ySubScrollOffset = 0;
                    offsetSubScrollIndex = 0;

                    var drawY = Bounds.drawY + (index * yAdvance);

                    string[] keys = new string[items.Keys.Count];
                    items.Keys.CopyTo(keys, 0);
                    List<CascadingListItem> list = null;

                    var bounds = ElementBounds.Fixed(Bounds.fixedX + Bounds.fixedWidth, Bounds.fixedY + ((index - offsetScrollIndex) * yAdvance) - (yAdvance / 2.0), Bounds.fixedWidth, Bounds.fixedHeight);
                    Bounds.ParentBounds.WithChild(bounds);
                    bounds.CalcWorldBounds();

                    if (items.TryGetValue(keys[index], out list))
                    {
                        categorySelection = new CategorySelection()
                        {
                            highlightDrawX = Bounds.drawX,
                            highlightDrawY = drawY,
                            index = index,
                            Bounds = bounds,
                            selectedItems = list
                        };
                    }
                }
                else if (categorySelection != null)
                {
                    Bounds.ParentBounds.ChildBounds.Remove(categorySelection.Bounds);
                    categorySelection = null;
                }
            }
            else if(categorySelection != null && categorySelection.Bounds.PointInside(args.X, args.Y))
            {
                int index = (int)Math.Floor((args.Y - categorySelection.Bounds.absY - ySubScrollOffset) / yAdvance);

                if (index < 0) index = 0;

                if(itemSelection != null && itemSelection.index == index)
                {
                    return;
                }

                var drawY = categorySelection.Bounds.drawY + (index * yAdvance);

                if (categorySelection.selectedItems.Count > index)
                {
                    itemSelection = new ListItemSelection()
                    {
                        highlightDrawX = categorySelection.Bounds.drawX,
                        highlightDrawY = drawY,
                        index = index,
                        selectedItem = categorySelection.selectedItems[index]
                    };
                }
                else
                {
                    itemSelection = null;
                }
            }
            else if(categorySelection != null)
            {
                Bounds.ParentBounds.ChildBounds.Remove(categorySelection.Bounds);
                categorySelection = null;
                itemSelection = null;
            }
        }

        public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
        {
            base.OnMouseWheel(api, args);

            if(categorySelection != null && itemSelection != null && maxVisibleItemsPerList < categorySelection.selectedItems.Count)
            {
                var indexDiff = (categorySelection.selectedItems.Count - maxVisibleItemsPerList);

                ySubScrollOffset += args.delta * 2;
                ySubScrollOffset = Math.Min(ySubScrollOffset, 0);
                ySubScrollOffset = Math.Max(ySubScrollOffset, -(indexDiff * yAdvance));

                offsetSubScrollIndex = (int)Math.Floor(Math.Abs(ySubScrollOffset) / yAdvance);
            }
            else if(itemSelection == null && categorySelection != null && maxVisibleItemsPerList < items.Count)
            {
                var indexDiff = (items.Count - maxVisibleItemsPerList);

                ySubScrollOffset = 0;

                yScrollOffset += args.delta * 2;
                yScrollOffset = Math.Min(yScrollOffset, 0);
                yScrollOffset = Math.Max(yScrollOffset, -(indexDiff * yAdvance));

                offsetScrollIndex = (int)Math.Floor(Math.Abs(yScrollOffset) / yAdvance);
            }
            else if (itemSelection == null && categorySelection == null)
            {
                yScrollOffset = 0;
                ySubScrollOffset = 0;
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

        private void RenderBackground(Context ctx, Surface surface)
        {
            ctx.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2], GuiStyle.DialogDefaultBgColor[3]);
            ElementRoundRectangle(ctx, Bounds, true);
            ctx.Fill();

            EmbossRoundRectangleElement(ctx, Bounds);
        }

        private void RenderCategories(Context ctx, Surface surface)
        {
            var x = Bounds.drawX + Bounds.InnerWidth / 2.0;
            var y = Bounds.drawY + (yAdvance / 2.0) + yScrollOffset;

            var cellDrawY = Bounds.drawY + yScrollOffset;

            var currentRendered = 0;

            var triangleSize = double.MaxValue;

            var lclIndex = offsetScrollIndex;

            foreach (var category in items.Keys)
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
                var extents = ctx.TextExtents(category);
                util.DrawTextLine(ctx, font, category, x - (extents.Width / 2.0), y - (extents.Height / 2.2));
                ctx.Restore();

                var currentSize = (Bounds.drawX + Bounds.InnerWidth) - (x - (extents.Width / 2.2) + extents.Width);

                triangleSize = Math.Min(triangleSize, currentSize);

                y += yAdvance;
                currentRendered++;
                if(currentRendered >= maxVisibleItemsPerList)
                {
                    break;
                }
            }

            triangleSize = Math.Min(triangleSize, yAdvance / 3.0);

            x = (Bounds.drawX + Bounds.InnerWidth)  - (triangleSize + 1);
            triangleSize -= 1;

            y = Bounds.drawY + (yAdvance / 2.0) - (triangleSize / 2.0);

            ctx.SetSourceRGBA(1.0,1.0,1.0,1.0);

            for(var i=0;i<currentRendered;i++)
            {
                RenderTriangle(ctx, surface, x, y, triangleSize, triangleSize);
                ctx.Fill();

                y += yAdvance;
            }


            RenderCategorySelection(ctx, surface);
        }

        private void RenderCategorySelection(Context ctx, Surface surface)
        {
            if (categorySelection != null)
            {
                var x = categorySelection.highlightDrawX;
                var y = categorySelection.highlightDrawY + yScrollOffset;

                // draw selection highlight

                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
                RoundRectangle(ctx, x, y, Bounds.InnerWidth, yAdvance, 1);
                ctx.Fill();

                x = categorySelection.Bounds.drawX;
                y = categorySelection.Bounds.drawY + ySubScrollOffset;

                // render selected list background

                ctx.SetSourceRGBA(GuiStyle.DialogDefaultBgColor[0], GuiStyle.DialogDefaultBgColor[1], GuiStyle.DialogDefaultBgColor[2], GuiStyle.DialogDefaultBgColor[3]);
                ElementRoundRectangle(ctx, categorySelection.Bounds, true);
                ctx.Fill();

                EmbossRoundRectangleElement(ctx, categorySelection.Bounds);

                var currentRendered = 0;
                var ccl = offsetSubScrollIndex;

                foreach (var item in categorySelection.selectedItems)
                {
                    if(ccl > 0)
                    {
                        ccl--;
                        y += yAdvance;
                        continue;
                    }

                    RenderListItem(ctx, surface, x, y, item, ref currentRendered);
                    y += yAdvance;

                    if (currentRendered >= maxVisibleItemsPerList)
                    {
                        break;
                    }
                }

                if(itemSelection != null)
                {
                    ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
                    RoundRectangle(ctx, itemSelection.highlightDrawX, itemSelection.highlightDrawY + ySubScrollOffset, categorySelection.Bounds.InnerWidth, yAdvance, 1);
                    ctx.Fill();
                }
            }
        }

        private void RenderListItem(Context ctx, Surface surface, double x, double y, CascadingListItem item, ref int currentRendered)
        {
            ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor[0], GuiStyle.DialogStrongBgColor[1], GuiStyle.DialogStrongBgColor[2], GuiStyle.DialogStrongBgColor[3]);
            RoundRectangle(ctx, x + 2, y + 2, categorySelection.Bounds.InnerWidth - 4, yAdvance - 4, 1);
            ctx.Fill();

            ctx.Save();
            font.SetupContext(ctx);
            var extents = ctx.TextExtents(item.Name);
            util.DrawTextLine(ctx, font, item.Name, x + (categorySelection.Bounds.InnerWidth / 2.0) - (extents.Width / 2.0), y + (yAdvance / 2.0) - (extents.Height / 2.2));
            ctx.Restore();

            currentRendered = currentRendered + 1;
        }

        private void RenderTriangle(Context ctx, Surface surface, double x, double y, double width, double height)
        {
            ctx.NewPath();
            ctx.LineTo(x, y);
            ctx.LineTo(x + width, y + (height / 2.0));
            ctx.LineTo(x, y + height);
            ctx.ClosePath();
        }
    }
}
