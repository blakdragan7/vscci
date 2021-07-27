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
        public string Value;
    }

    internal class CategorySelection
    {
        public double drawX;
        public double drawY;

        public List<CascadingListItem> selectedItems;
    }

    public class CascadingListElement : GuiElement
    {
        private int texId;
        private readonly CairoFont font;
        private readonly int maxVisibleItemsPerList;
        private readonly TextDrawUtil util;
        private Dictionary<string, List<CascadingListItem>> items;
        private CascadingListItem selectedItem;
        private CategorySelection categorySelection;

        protected int selectedIndex;

        public CascadingListItem SelectedItem => selectedItem;

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
            selectedItem = null;
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            base.ComposeElements(ctxStatic, surface);
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);
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

        public void AddListItem(string Category, string Name, string Value)
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
                var yAdvance = Bounds.InnerHeight / maxVisibleItemsPerList;

                int index = (int)Math.Floor((args.Y - Bounds.absY) / yAdvance);
                var drawY = Bounds.drawY + (index * yAdvance);

                string[] keys = new string[items.Keys.Count];
                items.Keys.CopyTo(keys, 0);
                List<CascadingListItem> list = null;

                if (items.TryGetValue(keys[index], out list))
                {
                    categorySelection = new CategorySelection()
                    {
                        drawX = Bounds.drawX,
                        drawY = drawY,
                        selectedItems = list
                    };
                }
            }
            else
            {
                categorySelection = null;
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
            var yAdvance = Bounds.InnerHeight / maxVisibleItemsPerList;

            var x = Bounds.drawX + Bounds.InnerWidth / 2.0;
            var y = Bounds.drawY + (yAdvance / 2.0);

            var cellDrawY = Bounds.drawY;

            var currentRendered = 0;

            var triangleSize = double.MaxValue;
            foreach(var category in items.Keys)
            {
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

            triangleSize = Math.Min(triangleSize, yAdvance - 4);

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
                var x = categorySelection.drawX;
                var y = categorySelection.drawY;

                var yAdvance = Bounds.InnerHeight / maxVisibleItemsPerList;

                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
                RoundRectangle(ctx, x, y, Bounds.InnerWidth, yAdvance, 1);
                ctx.Fill();

                x = Bounds.drawX + Bounds.OuterWidth;

                /*foreach (var item in categorySelection.selectedItems)
                {
                    RenderListItem(ctx, surface, x, y, Bounds.InnerWidth, yAdvance, item);
                }*/
            }
        }

        private void RenderListItem(Context ctx, Surface surface, double x, double y, double width, double height, CascadingListItem item)
        {

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
