using OnixRuntime.Api;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;

namespace ContainR.ShulkerBox;

public class Shulker {
    //local COLOURS = {
    //    white = "e4e9e9",
    //    light_gray = "8b8282",
    //    gray = "3e4246",
    //    black = "1f1f23",
    //    brown = "724728",
    //    red = "9a2422",
    //    orange = "f4730f",
    //    yellow = "fcc724",
    //    lime = "71bc18",
    //    green = "546d1c",
    //    cyan = "168691",
    //    light_blue = "3ab3da",
    //    blue = "33359b",
    //    purple = "7426a9",
    //    magenta = "b93eae",
    //    pink = "f38aa9",
    //    undyed = "ffffff"
    //}
    public static class ShulkerColors {
        public static readonly ColorF White = new(228, 233, 233);
        public static readonly ColorF LightGray = new(139, 130, 130);
        public static readonly ColorF Gray = new(62, 66, 70);
        public static readonly ColorF Black = new(31, 31, 35);
        public static readonly ColorF Brown = new(114, 71, 40);
        public static readonly ColorF Red = new(154, 36, 34);
        public static readonly ColorF Orange = new(244, 115, 15);
        public static readonly ColorF Yellow = new(252, 199, 36);
        public static readonly ColorF Lime = new(113, 188, 24);
        public static readonly ColorF Green = new(84, 109, 28);
        public static readonly ColorF Cyan = new(22, 134, 145);
        public static readonly ColorF LightBlue = new(58, 179, 218);
        public static readonly ColorF Blue = new(51, 53, 155);
        public static readonly ColorF Purple = new(116, 38, 169);
        public static readonly ColorF Magenta = new(185, 62, 174);
        public static readonly ColorF Pink = new(243, 138, 169);
        public static readonly ColorF Undyed = new(255, 255, 255);
    }

    public static bool IsShulkerScreenSupported(string screenName) {
        return screenName is "inventory_screen" or "crafting_screen" ||
               (screenName == "small_chest_screen") ||
               (screenName == "ender_chest_screen") ||
               (screenName == "barrel_screen") ||
               (screenName == "shulker_box_screen") ||
               (screenName == "large_chest_screen") ||
               (screenName == "furnace_screen") ||
               (screenName == "blast_furnace_screen") ||
               (screenName == "smoker_screen") ||
               (screenName == "dropper_screen") ||
               (screenName == "dispenser_screen") ||
               (screenName == "hopper_screen") ||
               (screenName == "anvil_screen") ||
               (screenName == "loom_screen") ||
               (screenName == "enchanting_screen") ||
               (screenName == "cartography_screen") ||
               (screenName == "beacon_screen") ||
               (screenName == "trade_screen") ||
               (screenName == "horse_screen") ||
               (screenName == "brewing_stand_screen") ||
               (screenName == "smithing_table_screen") ||
               (screenName == "grindstone_screen") ||
               (screenName == "stonecutter_screen");
    }

    public static float GetScreenHeightOffset() {
        string s = Onix.Gui.ScreenName;
        if (s == "large_chest_screen") {
            return 57f;
        } else if (s == "hopper_screen") {
            return 14f;
        }
        return 30f;
    }
    public static float GetScreenHeightOffsetHotbar() {
        string s = Onix.Gui.ScreenName;
        float val = 15f;
        if (s == "large_chest_screen") {
            val = 42f;
        } else if (s == "hopper_screen") {
            val = -1f;
        }
        return val;
    }

    public static int GetHoveredSlotIndex(Vec2 mousePos) {
        Vec2 screenSize = Onix.Gui.ScreenSize;
        Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
        string currentScreen = Onix.Gui.ScreenName;

        if (IsShulkerScreenSupported(currentScreen)) {
            var (containerSize, containerOffset) = Containers.GetContainerInfo(currentScreen);

            if (containerSize.Z > 0 && containerSize.W > 0) {
                const float slotSize = 18f;

                Vec2 containerPos = screenCenter + containerOffset;
                float containerWidth = containerSize.Z * slotSize;
                float containerHeight = containerSize.W * slotSize;

                Rect containerBox = new(
                    containerPos.X,
                    containerPos.Y,
                    containerPos.X + containerWidth,
                    containerPos.Y + containerHeight
                );

                if (containerBox.Contains(mousePos)) {
                    int columns = (int)containerSize.Z;

                    int col = (int)((mousePos.X - containerBox.X) / slotSize);
                    int row = (int)((mousePos.Y - containerBox.Y) / slotSize);

                    if (col >= 0 && col < columns && row >= 0 && row < containerSize.W) {
                        return -2 - (row * columns + col);
                    }
                }

                if (currentScreen == Containers.Furnaces.Furnace ||
                    currentScreen == Containers.Furnaces.BlastFurnace ||
                    currentScreen == Containers.Furnaces.Smoker) {

                    Rect ingredientRect = new(
                        containerPos.X + Containers.Furnaces.IngredientSize.X * slotSize,
                        containerPos.Y + Containers.Furnaces.IngredientSize.Y * slotSize,
                        containerPos.X + (Containers.Furnaces.IngredientSize.X + 1) * slotSize,
                        containerPos.Y + (Containers.Furnaces.IngredientSize.Y + 1) * slotSize
                    );

                    if (ingredientRect.Contains(mousePos)) {
                        return -2;
                    }

                    Rect fuelRect = new(
                        containerPos.X + Containers.Furnaces.FuelSize.X * slotSize,
                        containerPos.Y + Containers.Furnaces.FuelSize.Y * slotSize,
                        containerPos.X + (Containers.Furnaces.FuelSize.X + 1) * slotSize,
                        containerPos.Y + (Containers.Furnaces.FuelSize.Y + 1) * slotSize
                    );

                    if (fuelRect.Contains(mousePos)) {
                        return -3;
                    }

                    Rect resultRect = new(
                        containerPos.X + Containers.Furnaces.ResultSize.X * slotSize,
                        containerPos.Y + Containers.Furnaces.ResultSize.Y * slotSize,
                        containerPos.X + (Containers.Furnaces.ResultSize.X + 1) * slotSize,
                        containerPos.Y + (Containers.Furnaces.ResultSize.Y + 1) * slotSize
                    );

                    if (resultRect.Contains(mousePos)) {
                        return -4;
                    }
                }
                else if (currentScreen == Containers.Anvils.Anvil) {
                    Rect inputRect = new(
                        containerPos.X + Containers.Anvils.InputItem.X * slotSize,
                        containerPos.Y + Containers.Anvils.InputItem.Y * slotSize,
                        containerPos.X + (Containers.Anvils.InputItem.X + 1) * slotSize,
                        containerPos.Y + (Containers.Anvils.InputItem.Y + 1) * slotSize
                    );

                    if (inputRect.Contains(mousePos)) {
                        return -2;
                    }

                    Rect materialRect = new(
                        containerPos.X + Containers.Anvils.MaterialItem.X * slotSize,
                        containerPos.Y + Containers.Anvils.MaterialItem.Y * slotSize,
                        containerPos.X + (Containers.Anvils.MaterialItem.X + 1) * slotSize,
                        containerPos.Y + (Containers.Anvils.MaterialItem.Y + 1) * slotSize
                    );

                    if (materialRect.Contains(mousePos)) {
                        return -3;
                    }

                    Rect resultRect = new(
                        containerPos.X + Containers.Anvils.ResultItem.X * slotSize,
                        containerPos.Y + Containers.Anvils.ResultItem.Y * slotSize,
                        containerPos.X + (Containers.Anvils.ResultItem.X + 1) * slotSize,
                        containerPos.Y + (Containers.Anvils.ResultItem.Y + 1) * slotSize
                    );

                    if (resultRect.Contains(mousePos)) {
                        return -4;
                    }
                }
            }
        }

        if (currentScreen == "crafting_screen") {
            Containers.CraftingGrid.ContainerOffset.X = UiState.LayoutMode is (InventoryLayout.Survival or InventoryLayout.UnknownNotRelevantToThisScreen) ? 16 : -59;
        }
        
        float inventoryItemsWidthOffset = (UiState.LayoutMode is (InventoryLayout.Survival or InventoryLayout.UnknownNotRelevantToThisScreen) or InventoryLayout.Creative) ? 0 : 75f;

        float inventoryItemsWidth = 162;
        float inventoryItemsHeight = 54;
        float inventoryItemsHeightOffset = GetScreenHeightOffset();
        Rect inventoryItemsBox = new(
            screenCenter.X - inventoryItemsWidth / 2 + inventoryItemsWidthOffset,
            screenCenter.Y - inventoryItemsHeight / 2 + inventoryItemsHeightOffset,
            screenCenter.X + inventoryItemsWidth / 2 + inventoryItemsWidthOffset,
            screenCenter.Y + inventoryItemsHeight / 2 + inventoryItemsHeightOffset
        );

        float hotbarItemsWidth = 162;
        float hotbarItemsHeight = 18;
        float hotbarItemsHeightOffset = inventoryItemsHeight + GetScreenHeightOffsetHotbar() + (UiState.LayoutMode == InventoryLayout.Creative ? 22.5f : 0f);
        float hotbarItemsWidthOffset = (UiState.LayoutMode is (InventoryLayout.Survival or InventoryLayout.UnknownNotRelevantToThisScreen) or InventoryLayout.Creative) ? 0 : 75f;
        Rect hotbarItemsBox = new(
            screenCenter.X - hotbarItemsWidth / 2 + hotbarItemsWidthOffset,
            screenCenter.Y - hotbarItemsHeight / 2 + hotbarItemsHeightOffset,
            screenCenter.X + hotbarItemsWidth / 2 + hotbarItemsWidthOffset,
            screenCenter.Y + hotbarItemsHeight / 2 + hotbarItemsHeightOffset
        );

        if (inventoryItemsBox.Contains(mousePos)) {
            int columns = 9;
            int rows = 3;
            float cellWidth = inventoryItemsWidth / columns;
            float cellHeight = inventoryItemsHeight / rows;

            int col = (int)((mousePos.X - inventoryItemsBox.X) / cellWidth);
            int row = (int)((mousePos.Y - inventoryItemsBox.Y) / cellHeight);

            if (col >= 0 && col < columns && row >= 0 && row < rows) {
                return 10 + row * columns + col;
            }
        }

        else if (hotbarItemsBox.Contains(mousePos)) {
            int columns = 9;
            float cellWidth = hotbarItemsWidth / columns;

            int col = (int)((mousePos.X - hotbarItemsBox.X) / cellWidth);

            if (col >= 0 && col < columns) {
                return 1 + col;
            }
        }

        return -1;
    }

    public static ItemStack? GetSlot(int slotIndex) {
        return Onix.LocalPlayer?.Inventory.GetItem(slotIndex-1);
    }
}