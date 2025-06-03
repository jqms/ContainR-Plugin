using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;

namespace ContainR {
    public class UiRenderer {
        public void RenderDarkBackground(RendererGame gfx) {
            Vec2 screenSize = Onix.Gui.ScreenSize;
            Rect screenRect = new Rect(0, 0, screenSize.X, screenSize.Y);
            gfx.FillRectangle(screenRect, ColorF.Black.WithOpacity(0.4f));
        }
        public bool HasCachedTextures;
        public void CacheTexturesIfNeeded(RendererGame gfx) {
            if (HasCachedTextures) return;
            CacheTextures(gfx);
            HasCachedTextures = true;
        }

        private static void CacheTextures(RendererGame gfx) {
            Vec2 screenSize = Onix.Gui.ScreenSize;
            Rect screenRect = new(0, 0, screenSize.X, screenSize.Y);
            TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, screenRect, 0f, TextureManager.DialogBackgroundTexture.Uv);
            gfx.RenderTexture(screenRect, TextureManager.SlotTexture.Path, 0, TextureManager.SlotTexture.Uv);
            gfx.RenderTexture(screenRect, TextureManager.UpArrow, 0);
            gfx.RenderTexture(screenRect, TextureManager.DownArrow, 0);
            gfx.RenderTexture(screenRect, TextureManager.SlotHover, 0);
            gfx.RenderTexture(screenRect, TextureManager.TrashTexture, 0);
        }
        
        public void RenderContainerUi(RendererGame gfx, string screenName) {
            Vec2 screenSize = Onix.Gui.ScreenSize;

            const int slotsX = 1;
            const int slotsY = 2;

            bool isLargeChest = screenName == "large_chest_screen";
            bool isHopper = screenName == "hopper_screen";
            
            Vec2 slotGridOffset = new(6, isLargeChest ? 2 : 1);
            Vec2 posOffset = new(-5f, isLargeChest ? 30f : isHopper ? 4.5f : 21f);

            const float slotSize = 18f;
            Vec2 gridSize = new(slotsX * slotSize, slotsY * slotSize);

            Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
            Vec2 gridTopLeft = screenCenter - (gridSize / 2f);

            const float padding = 4f;
            Vec2 backgroundTopLeft = gridTopLeft + new Vec2((slotGridOffset.X * slotSize) - padding, (slotGridOffset.Y * slotSize) - padding) + posOffset;
            Vec2 backgroundSize = new((slotsX * slotSize) + (padding * 2), (slotsY * slotSize) + (padding * 2));
            Rect backgroundRect = Rect.FromSize(backgroundTopLeft.X, backgroundTopLeft.Y, backgroundSize.X, backgroundSize.Y);

            TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, backgroundRect, 0.85f, TextureManager.DialogBackgroundTexture.Uv);
            //TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, new Rect(10,10,Onix.Gui.MousePosition), 0.85f, TextureManager.DialogBackgroundTexture.Uv);

            UiState.Hovering = false;
            UiState.HoveringTake = false;
            UiState.HoveringDelete = false;

            float arrowOpacity = 0.5f;
            for (int y = 0; y < slotsY; y++) {
                for (int x = 0; x < slotsX; x++) {
                    Vec2 slotPos = gridTopLeft + new Vec2((x + slotGridOffset.X) * slotSize, (y + slotGridOffset.Y) * slotSize) + posOffset;
                    Rect rect = new Rect(slotPos, slotPos + new Vec2(slotSize, slotSize));
                    gfx.RenderTexture(rect, TextureManager.SlotTexture.Path, 1, TextureManager.SlotTexture.Uv);

                    if (rect.Contains(Onix.Gui.MousePosition)) {
                        if (!(MouseData.LmbDown || MouseData.RmbDown || MouseData.MmbDown)) {
                            gfx.RenderTexture(rect, TextureManager.SlotHover);
                        }

                        UiState.Hovering = true;
                        UiState.HoveringTake = y == 1;
                    }

                    rect.Shrink(1.5f);
                    if (y == 0) {
                        gfx.RenderTexture(rect, TextureManager.UpArrow, ColorF.Black.WithOpacity(arrowOpacity));
                    }
                    else {
                        gfx.RenderTexture(rect, TextureManager.DownArrow, ColorF.Black.WithOpacity(arrowOpacity));
                    }
                }
            }
        }

        public void RenderDeleteUi(RendererGame gfx, string screenName) {
            if (Onix.LocalPlayer?.GameMode != GameMode.Creative) return;
            Vec2 screenSize = Onix.Gui.ScreenSize;

            const int slotsX = 1;
            const int slotsY = 1;
            
            Vec2 slotGridOffset = new(6, 1);
            Vec2 posOffset = new((UiState.LayoutMode is InventoryLayout.RecipeBook or InventoryLayout.Creative) ? 70f : -5f, 51f);

            const float slotSize = 18f;
            Vec2 gridSize = new(slotsX * slotSize, slotsY * slotSize);

            Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
            Vec2 gridTopLeft = screenCenter - (gridSize / 2f);

            const float padding = 4f;
            Vec2 backgroundTopLeft = gridTopLeft + new Vec2((slotGridOffset.X * slotSize) - padding, (slotGridOffset.Y * slotSize) - padding) + posOffset;
            Vec2 backgroundSize = new((slotsX * slotSize) + (padding * 2), (slotsY * slotSize) + (padding * 2));
            Rect backgroundRect = Rect.FromSize(backgroundTopLeft.X, backgroundTopLeft.Y, backgroundSize.X, backgroundSize.Y);

            TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, backgroundRect, 0.85f, TextureManager.DialogBackgroundTexture.Uv);
            //TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, new Rect(10,10,Onix.Gui.MousePosition), 0.85f, TextureManager.DialogBackgroundTexture.Uv);

            UiState.Hovering = false;
            UiState.HoveringTake = false;
            UiState.HoveringDelete = false;

            float arrowOpacity = 0.5f;
            for (int y = 0; y < slotsY; y++) {
                for (int x = 0; x < slotsX; x++) {
                    Vec2 slotPos = gridTopLeft + new Vec2((x + slotGridOffset.X) * slotSize, (y + slotGridOffset.Y) * slotSize) + posOffset;
                    Rect rect = new Rect(slotPos, slotPos + new Vec2(slotSize, slotSize));
                    gfx.RenderTexture(rect, TextureManager.SlotTexture.Path, 1, TextureManager.SlotTexture.Uv);

                    if (rect.Contains(Onix.Gui.MousePosition)) {
                        if (!(MouseData.LmbDown || MouseData.RmbDown || MouseData.MmbDown)) {
                            gfx.RenderTexture(rect, TextureManager.SlotHover);
                        }

                        UiState.Hovering = true;
                        UiState.HoveringDelete = y == 0;
                    }

                    rect.X += 1;
                    rect.Z -= 1;
                    gfx.RenderTexture(rect, TextureManager.TrashTexture, ColorF.White.WithOpacity(arrowOpacity));
                }
            }

            if (UiState.HoveringDelete) {
                Vec2 tooltipPos = new(Onix.Gui.MousePosition.X + 14, Onix.Gui.MousePosition.Y + 10);
                string[] tooltipTextLines = [
                    "Click to delete held item",
                    "Shift + Click to delete hotbar",
                    "Shift + Right Click to delete inventory"
                ]; 
                const float padding2 = 4f;
                Rect tooltipRect = Rect.FromSize(tooltipPos.X - padding2, tooltipPos.Y - padding2, 
                    188 + (padding2 * 2), ((tooltipTextLines.Length * 10) + (padding2 * 2)) - 1);
                //Vec2 textSize = gfx.MeasureText(tooltipTextLines[0]);
                TextureManager.PurpleBorderNineSlice.Render(gfx, tooltipRect, 1);
                for (int i = 0; i < tooltipTextLines.Length; i++) {
                    Vec2 textPos = new(tooltipPos.X, tooltipPos.Y + (i * 10));
                    gfx.RenderText(textPos, ColorF.White, tooltipTextLines[i]);
                }
            }
        }
    }
}
