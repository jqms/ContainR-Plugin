using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;

namespace ContainR {
    public static class TextureManager {
        public static readonly TexturePath SlotHover = TexturePath.Game("textures/ui/slot_disabled_hover");
        private static readonly TexturePath DialogBackgroundOpaque = TexturePath.Game("textures/gui/newgui/dialog-background-atlas");
        private static readonly TexturePath Gui = TexturePath.Game("textures/gui/gui");
        public static readonly TexturePath DownArrow = TexturePath.Assets("container_move_to");
        public static readonly TexturePath UpArrow = TexturePath.Assets("container_move_from");
        private static readonly TexturePath PurpleBorder = TexturePath.Game("textures/ui/purpleBorder");
        public static readonly TexturePath TrashTexture = TexturePath.Game("textures/ui/trash_default");
        private static readonly TexturePath EditBoxIndent = TexturePath.Game("textures/ui/edit_box_indent");
        private static readonly TexturePath EditBoxIndentHover = TexturePath.Game("textures/ui/edit_box_indent_hover");
        public static readonly TexturePath MagnifyingGlass = TexturePath.Game("textures/ui/magnifying_glass");

        private static readonly Rect SlotUv = Rect.FromSize(188f, 184f, 22f, 22f).NormalizeWith(256f);
        private static readonly Rect DialogBackgroundOpaqueUv = Rect.FromSize(0f, 106f, 16f, 16f).NormalizeWith(128f);

        public readonly struct UvTextureInfo(TexturePath path, Rect uv) {
            public TexturePath Path { get; } = path;
            public Rect Uv { get; } = uv;
        }

        public static readonly UvTextureInfo SlotTexture = new(Gui, SlotUv);
        public static readonly UvTextureInfo DialogBackgroundTexture = new(DialogBackgroundOpaque, DialogBackgroundOpaqueUv);
        
        public static readonly NineSlice DialogBackgroundOpaqueNineSlice = new(DialogBackgroundTexture.Path, "textures/ui/dialog_background_opaque");
        public static readonly NineSlice PurpleBorderNineSlice = new(PurpleBorder);
        public static readonly NineSlice EditBoxIndentNineSlice = new(EditBoxIndent);
        public static readonly NineSlice EditBoxIndentHoverNineSlice = new(EditBoxIndentHover);
    }
}