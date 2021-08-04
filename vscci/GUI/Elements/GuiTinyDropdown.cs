namespace VSCCI.GUI.Elements
{
    using Cairo;
    using System;
    using Vintagestory.API.Client;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;

    public class GuiTinyDropdown : GuiElement
    {
        string[] values;
        string[] names;

        private bool listOpen;
        private bool arrowDown;

        private CairoFont font;
        private int hoverIndex;
        private int selectedIndex;

        private LoadedTexture listTexture;
        private LoadedTexture arrowTexture;
        private LoadedTexture arrowDownTexture;
        private LoadedTexture listHoverTexture;
        private LoadedTexture currentSelectionTexture;

        private TextDrawUtil util;
        private ElementBounds listBounds;
        private ElementBounds arrowBounds;

        private SelectionChangedDelegate onSelectionChanged;

        public int Selection => selectedIndex;
        public string SelectionValue => values[selectedIndex];
        public string SelectionName => names[selectedIndex];

        public override bool Focusable => true;
        public GuiTinyDropdown(ICoreClientAPI capi, string[] values, string[] names, int selectedIndex, SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, CairoFont font) : base(capi, bounds)
        {
            this.values = values;
            this.names = names;
            this.font = font;
            this.selectedIndex = selectedIndex;
            this.onSelectionChanged = onSelectionChanged;

            util = new TextDrawUtil();
            listTexture = new LoadedTexture(capi);
            currentSelectionTexture = new LoadedTexture(capi);
            listHoverTexture = new LoadedTexture(capi);
            arrowTexture = new LoadedTexture(capi);
            arrowDownTexture = new LoadedTexture(capi);

            listOpen = false;
            listBounds = null;
            arrowDown = false;

            hoverIndex = -1;

            arrowBounds = ElementBounds.Fixed(bounds.fixedWidth - bounds.fixedHeight, 0, bounds.fixedHeight, bounds.fixedHeight);
            bounds.WithChild(arrowBounds);
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();
            arrowBounds.CalcWorldBounds();

            ctx.SetSourceRGBA(0, 0, 0, 0.2);
            RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 3);
            ctx.Fill();
            EmbossRoundRectangleElement(ctx, Bounds, true, 1, 1);

            ComposeDynamicElements();
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            base.RenderInteractiveElements(deltaTime);

            api.Render.Render2DTexturePremultipliedAlpha(currentSelectionTexture.TextureId, Bounds);

            if(arrowDown)
            {
                api.Render.Render2DTexturePremultipliedAlpha(arrowDownTexture.TextureId, arrowBounds, 110);
            }
            else
            {
                api.Render.Render2DTexturePremultipliedAlpha(arrowTexture.TextureId, arrowBounds, 110);
            }

            if (listOpen)
            {
                api.Render.Render2DTexturePremultipliedAlpha(listTexture.TextureId, listBounds, 110);

                if (hoverIndex != -1)
                {
                    api.Render.Render2DTexturePremultipliedAlpha(listHoverTexture.TextureId, listBounds.renderX + 1, listBounds.renderY + 1 + (hoverIndex * Bounds.OuterHeight), Bounds.OuterWidth - 2, Bounds.OuterHeight - 2, 111);
                }
            }
        }

        private void ComposeDynamicElements()
        {
            ComposeSelection();
            ComposeHoverTexture();
            ComposeArrowTexture();
            ComposeArrowDownTexture();
            ComposeList();
        }

        private void ComposeHoverTexture()
        {
            var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt - 2, Bounds.OuterHeightInt - 2);
            var ctx = new Context(surface);

            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.5);
            RoundRectangle(ctx, 0, 0, Bounds.OuterWidth - 2, Bounds.OuterHeight - 2, 1);
            ctx.Fill();

            generateTexture(surface, ref listHoverTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        private void ComposeArrowTexture()
        {
            var surface = new ImageSurface(Format.Argb32, arrowBounds.OuterWidthInt, arrowBounds.OuterHeightInt);
            var ctx = new Context(surface);

            // draw arrow backround

            //ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
            EmbossRoundRectangle(ctx, 0, 0, arrowBounds.OuterWidth, arrowBounds.OuterHeight, 1);
            //ctx.Fill();

            ctx.Save();
            ctx.SetSourceRGBA(1.0, 1.0, 1.0, 1.0);
            ctx.NewPath();

            ctx.MoveTo(2, 2);
            ctx.LineTo(arrowBounds.InnerWidth - 2, 2);
            ctx.LineTo((arrowBounds.InnerWidth - 2)  / 2.0, arrowBounds.InnerHeight - 2);

            ctx.ClosePath();

            ctx.Fill();
            ctx.Restore();

            generateTexture(surface, ref arrowTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        private void ComposeArrowDownTexture()
        {
            var surface = new ImageSurface(Format.Argb32, arrowBounds.OuterWidthInt, arrowBounds.OuterHeightInt);
            var ctx = new Context(surface);

            // draw arrow backround

            EmbossRoundRectangle(ctx, 0, 0, arrowBounds.OuterWidth, arrowBounds.OuterHeight, 1);


            ctx.Save();
            ctx.SetSourceRGBA(0.4, 0.4, 0.4, 1.0);
            ctx.NewPath();

            ctx.MoveTo(2, 2);
            ctx.LineTo(arrowBounds.InnerWidth - 2, 2);
            ctx.LineTo((arrowBounds.InnerWidth - 2) / 2.0, arrowBounds.InnerHeight - 2);

            ctx.ClosePath();

            ctx.Fill();
            ctx.Restore();

            generateTexture(surface, ref arrowDownTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        private void ComposeSelection()
        {
            var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt, Bounds.OuterHeightInt);
            var ctx = new Context(surface);

            ctx.Save();

            font.SetupContext(ctx);
            if(selectedIndex >= 0 && selectedIndex < names.Length)
            {
                var extents = ctx.TextExtents(names[selectedIndex]);

                var drawX = (Bounds.InnerWidth / 2.0) - (extents.Width / 2.0);
                var drawY = 0;

                ctx.Antialias = Antialias.Best;
                util.DrawTextLine(ctx, font, names[selectedIndex], drawX, drawY);
            }

            ctx.Restore();

            generateTexture(surface, ref currentSelectionTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        private void ComposeList()
        {
            if (listBounds != null)
            {
                Bounds.ParentBounds.ChildBounds.Remove(listBounds);
            }

            // create list bounds

            listBounds = ElementBounds.Fixed(Bounds.fixedX, Bounds.fixedY + Bounds.OuterHeight, Bounds.fixedWidth, Bounds.fixedHeight * names.Length);
            Bounds.ParentBounds.WithChild(listBounds);
            listBounds.CalcWorldBounds();

            var surface = new ImageSurface(Format.Argb32, listBounds.OuterWidthInt, listBounds.OuterHeightInt);
            var ctx = new Context(surface);

            // draw list background
            ctx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
            RoundRectangle(ctx, 0, 0, listBounds.OuterWidth, listBounds.OuterHeight, 1);
            ctx.FillPreserve();
            ctx.SetSourceRGBA(0, 0, 0, 0.5);
            ctx.LineWidth = 2;
            ctx.Stroke();

            ctx.Save();
            font.SetupContext(ctx);
            double height = 0;
            foreach (var name in names)
            {
                // draw all options cenetered
                EmbossRoundRectangle(ctx, 1, height + 1, Bounds.InnerWidth - 2, Bounds.InnerHeight - 2, 1);

                ctx.SetSourceRGBA(1.0, 1.0, 1.0, 1.0);
                var extents = ctx.TextExtents(name);
                util.DrawTextLine(ctx, font, name, (Bounds.fixedWidth / 2.0) - (extents.Width / 2.0), height);
                height += Bounds.fixedHeight;
            }

            ctx.Restore();

            generateTexture(surface, ref listTexture);

            ctx.Dispose();
            surface.Dispose();
        }

        public override bool IsPositionInside(int posX, int posY)
        {
            return base.IsPositionInside(posX, posY) || (listOpen && listBounds.PointInside(posX, posY));
        }

        public override void OnMouseDown(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDown(api, args);

            if (arrowBounds.PointInside(args.X, args.Y))
            {
                arrowDown = true;
                args.Handled = true;
                api.Gui.PlaySound("menubutton");
            }
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);

            if(listOpen && IsPositionInside(args.X, args.Y))
            {
                var localY = args.Y - listBounds.absY;

                var index = (int)Math.Floor(localY / Bounds.InnerHeight);

                // clamp the index
                if (index >= 0 && index < names.Length)
                {
                    hoverIndex = index;
                }
                else
                {
                    hoverIndex = -1;
                }

            }
            else
            {
                hoverIndex = -1;
            }
        }

        public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseUp(api, args);

            if (arrowBounds.PointInside(args.X, args.Y))
            {
                arrowDown = false;
                args.Handled = true;
                listOpen = true;
                api.Gui.PlaySound("menubutton");
                return;
            }
            else if (listOpen && listBounds.PointInside(args.X, args.Y))
            {
                if (hoverIndex >= 0 && hoverIndex < values.Length)
                {
                    selectedIndex = hoverIndex;
                    onSelectionChanged.Invoke(values[selectedIndex], true);
                    ComposeSelection();
                    args.Handled = true;
                    api.Gui.PlaySound("menubutton");
                }
            }

            listOpen = false;
        }

        public void SetSelectedIndex(int newIndex)
        {
            if(newIndex != selectedIndex)
            {
                if(newIndex >= 0 && newIndex < values.Length)
                {
                    selectedIndex = newIndex;
                    ComposeSelection();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            currentSelectionTexture.Dispose();
            listTexture.Dispose();
            listHoverTexture.Dispose();
            arrowTexture.Dispose();
            arrowDownTexture.Dispose();
        }
    }

    public static partial class GuiComposerHelpers
    {
        /// <summary>
        /// Adds a tiny dropdown to the current GUI instance.
        /// </summary>
        /// <param name="values">The values of the current drodown.</param>
        /// <param name="names">The names of those values.</param>
        /// <param name="selectedIndex">The default selected index.</param>
        /// <param name="onSelectionChanged">The event fired when the index is changed.</param>
        /// <param name="bounds">The bounds of the index.</param>
        /// <param name="key">The name of this dropdown.</param>
        public static GuiComposer AddTinyDropDown(this GuiComposer composer, string[] values, string[] names, int selectedIndex, SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, string key = null)
        {
            composer.AddInteractiveElement(new GuiTinyDropdown(composer.Api, values, names, selectedIndex, onSelectionChanged, bounds, CairoFont.WhiteSmallText()), key);
            return composer;
        }

        /// <summary>
        /// Adds a tiny dropdown to the current GUI instance.
        /// </summary>
        /// <param name="values">The values of the current drodown.</param>
        /// <param name="names">The names of those values.</param>
        /// <param name="selectedIndex">The default selected index.</param>
        /// <param name="onSelectionChanged">The event fired when the index is changed.</param>
        /// <param name="bounds">The bounds of the index.</param>
        /// <param name="key">The name of this dropdown.</param>
        public static GuiComposer AddTinyDropDown(this GuiComposer composer, string[] values, string[] names, int selectedIndex, SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, CairoFont font, string key = null)
        {
            composer.AddInteractiveElement(new GuiTinyDropdown(composer.Api, values, names, selectedIndex, onSelectionChanged, bounds, font), key);
            return composer;
        }



        /// <summary>
        /// Gets the Drop Down element from the GUIComposer by their key.
        /// </summary>
        /// <param name="key">the name of the dropdown to fetch.</param>
        public static GuiTinyDropdown GetTinyDropDown(this GuiComposer composer, string key)
        {
            return (GuiTinyDropdown)composer.GetElement(key);
        }
    }
}
