namespace VSCCI.GUI.Elements
{
    using Cairo;
    using Vintagestory.API.Client;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;

    public class GuiTinyDropdown : GuiElement
    {
        string[] values;
        string[] names;

        private bool listOpen;

        private CairoFont font;
        private int selectedIndex;

        private LoadedTexture listTexture;
        private LoadedTexture currentSelectionTexture;

        private TextDrawUtil util;
        private ElementBounds listBounds;

        public override bool Focusable => true;
        public GuiTinyDropdown(ICoreClientAPI capi, string[] values, string[] names, int selectedIndex, SelectionChangedDelegate onSelectionChanged, ElementBounds bounds, CairoFont font) : base(capi, bounds)
        {
            this.values = values;
            this.names = names;
            this.font = font;
            this.selectedIndex = selectedIndex;

            util = new TextDrawUtil();
            listTexture = new LoadedTexture(capi);
            currentSelectionTexture = new LoadedTexture(capi);

            listOpen = false;
            listBounds = null;
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

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

            if(listOpen)
            {
                api.Render.Render2DTexturePremultipliedAlpha(listTexture.TextureId, listBounds, 110);
            }
        }

        private void ComposeDynamicElements()
        {
            ComposeSelection();

            ComposeList();
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

        public override void OnMouseDown(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseDown(api, args);

            if (IsPositionInside(args.X, args.Y))
            {
                listOpen = !listOpen;
                if (listOpen)
                {
                    api.Gui.PlaySound("menubutton");
                }
                args.Handled = true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            currentSelectionTexture.Dispose();
            listTexture.Dispose();
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
