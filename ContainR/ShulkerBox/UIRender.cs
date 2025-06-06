using System.Reflection;
using OnixRuntime.Api;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.NBT;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.World;

namespace ContainR.ShulkerBox;

public class UiRender {
    private int _hoveredSlotIndex = -1;
    private static ItemStack? _hoveredItemStack;
    private static ItemStack? _previousHoveredItemStack;
    private string _hoveredContainer = "";
    private float _lerpProgress = 0f;
    private ColorF _currentColor = Shulker.ShulkerColors.White;
    private ColorF _targetColor = Shulker.ShulkerColors.White;
    private string _lastShulkerName = "";

    public void HandleContainerScreenTick(ContainerScreen screen) {
        UiState.LayoutMode = screen.InventoryLayout;
        if (Onix.LocalPlayer is null) return;
        _hoveredItemStack = screen.GetItem(screen.HoveredContainer, screen.HoveredSlot);
        _hoveredContainer = screen.HoveredContainer;
    }

    public void HandleShulkerHover(RendererGame gfx, float delta) {
        //gfx.RenderText(new Vec2(0, 20), ColorF.White, (UiState.LayoutMode.ToString()));
        if (Onix.LocalPlayer is null) return;
        _hoveredSlotIndex = Shulker.GetHoveredSlotIndex(Onix.Gui.MousePosition);

        if (_hoveredSlotIndex != -1 && !_hoveredContainer.Contains("recipe")) {
            _previousHoveredItemStack = _hoveredItemStack;
            ItemStack? item =
                ((_hoveredContainer == "inventory_items" || _hoveredContainer == "hotbar_items") &&
                 _hoveredSlotIndex >= 0)
                    ? Shulker.GetSlot(_hoveredSlotIndex)
                    : _hoveredItemStack;
            if (item is not { Item: not null } || !item.Item.Name.Contains("shulker_box") ||
                !Shulker.IsShulkerScreenSupported(Onix.Gui.ScreenName)) return;

            string currentShulkerName = item.Item.Name;

            List<ItemStack>? items = GetDataFromShulkerBox(item);
            if (items != null) {
                Vec2 position = Onix.Gui.MousePosition + new Vec2(14, -70);

                string color = currentShulkerName.Replace("_shulker_box", "");
                ColorF newTargetColor = color switch {
                    "black" => Shulker.ShulkerColors.Black,
                    "blue" => Shulker.ShulkerColors.Blue,
                    "brown" => Shulker.ShulkerColors.Brown,
                    "cyan" => Shulker.ShulkerColors.Cyan,
                    "gray" => Shulker.ShulkerColors.Gray,
                    "green" => Shulker.ShulkerColors.Green,
                    "light_blue" => Shulker.ShulkerColors.LightBlue,
                    "light_gray" => Shulker.ShulkerColors.LightGray,
                    "lime" => Shulker.ShulkerColors.Lime,
                    "magenta" => Shulker.ShulkerColors.Magenta,
                    "orange" => Shulker.ShulkerColors.Orange,
                    "pink" => Shulker.ShulkerColors.Pink,
                    "purple" => Shulker.ShulkerColors.Purple,
                    "red" => Shulker.ShulkerColors.Red,
                    "undyed" => Shulker.ShulkerColors.Undyed,
                    "white" => Shulker.ShulkerColors.White,
                    "yellow" => Shulker.ShulkerColors.Yellow,
                    _ => Shulker.ShulkerColors.White
                };

                bool isNewShulker = currentShulkerName != _lastShulkerName &&
                                    _previousHoveredItemStack is { Item: not null } &&
                                    _previousHoveredItemStack.Item.Name.Contains("shulker_box");

                if (isNewShulker) {
                    _targetColor = newTargetColor;
                    _lerpProgress = 0f;
                    _lastShulkerName = currentShulkerName;
                    _itemAnimations.Clear();
                    for (int i = 0; i < items.Count && i < 27; i++) {
                        _itemAnimations[i] = 0f;
                    }
                }

                _lerpProgress = Math.Min(_lerpProgress + delta * (!(_previousHoveredItemStack is { Item: not null } &&
                                                                    _previousHoveredItemStack.Item.Name.Contains(
                                                                        "shulker_box"))
                    ? 10000f
                    : 1f), 1.0f);

                _currentColor = LerpColor(_currentColor, _targetColor, _lerpProgress);

                RenderShulkerBoxGrid(gfx, position, _currentColor);

                for (int i = 0; i < items.Count && i < 27; i++) {
                    ItemStack itemStack = items[i];
                    int x = i % 9;
                    int y = i / 9;

                    if (isNewShulker || !_itemAnimations.ContainsKey(i)) {
                        _itemAnimations[i] = 0f;
                    }

                    float itemDelay = i * 0.01f;

                    if (_lerpProgress > itemDelay) {
                        float itemAnimationDuration = 0.3f;
                        _itemAnimations[i] = Math.Min(_itemAnimations[i] + delta / itemAnimationDuration, 1.0f);
                    }

                    float scale = _itemAnimations[i] < 1.0f ? EaseOutBack(_itemAnimations[i]) : 1.0f;

                    float slotSize = 18f;
                    Vec2 itemPos = position + new Vec2((x * slotSize) + 1, (y * slotSize) + 1);
                    itemStack.ShowPickup = false;

                    if (_itemAnimations[i] > 0) {
                        float itemSize = 16f;
                        float scaleDifference = 1f - scale;
                        float offsetAmount = (itemSize * scaleDifference) / 2f;
                        Vec2 centeredItemPos = itemPos + new Vec2(offsetAmount, offsetAmount);

                        gfx.RenderItem(centeredItemPos, itemStack, true, scale);
                    }
                }
            }
        }
        else {
            _hoveredItemStack = null;
            _hoveredContainer = "";
            _hoveredSlotIndex = -1;
            _lastShulkerName = "";
            _itemAnimations.Clear();
        }
    }

    private readonly Dictionary<int, float> _itemAnimations = new();

    private float EaseOutBack(float x) {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * (float)Math.Pow(x - 1, 3) + c1 * (float)Math.Pow(x - 1, 2);
    }

    private ColorF LerpColor(ColorF a, ColorF b, float t) {
        return new ColorF(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            a.A + (b.A - a.A) * t
        );
    }

    public void RenderShulkerBoxGrid(RendererGame gfx, Vec2 position, ColorF tint = default) {
        const int slotsX = 9;
        const int slotsY = 3;
        const float slotSize = 18f;

        Vec2 gridSize = new(slotsX * slotSize, slotsY * slotSize);

        const float padding = 4f;
        Vec2 backgroundSize = new(gridSize.X + (padding * 2), gridSize.Y + (padding * 2));

        Vec2 backgroundTopLeft = position - new Vec2(padding);

        Rect backgroundRect = Rect.FromSize(backgroundTopLeft.X, backgroundTopLeft.Y, backgroundSize.X, backgroundSize.Y);

        TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, backgroundRect, tint, TextureManager.DialogBackgroundTexture.Uv);

        for (int y = 0; y < slotsY; y++) {
            for (int x = 0; x < slotsX; x++) {
                Vec2 slotPos = position + new Vec2(x * slotSize, y * slotSize);
                Rect slotRect = new Rect(slotPos, slotPos + new Vec2(slotSize, slotSize));

                gfx.RenderTexture(slotRect, TextureManager.SlotTexture.Path, tint, TextureManager.SlotTexture.Uv);
            }
        }
    }

    public List<ItemStack>? GetDataFromShulkerBox(ItemStack? shulkerItem) {
        if (shulkerItem is not { Item: not null } || !shulkerItem.Item.Name.Contains("shulker_box")) {
            return null;
        }

        if (shulkerItem.Nbt == null) {
            return null;
        }

        Dictionary<string, NbtTag> tag = shulkerItem.Nbt.Value;
        List<ItemStack> result = [];

        if (tag.TryGetValue("Items", out NbtTag? itemsTag)) {
            if (itemsTag is ArrayListTag itemsArrayTag) {
                foreach (var itemTag in itemsArrayTag.Value) {
                    if (itemTag is ObjectTag itemObj) {
                        ItemStack itemStack = new(itemObj);
                        result.Add(itemStack);
                    }
                }
            }
            else {
                return null;
            }
        } else {
            return null;
        }
        return result;
    }

    public static void RenderAllSlotPositions(RendererGame gfx) {
        Vec2 screenSize = Onix.Gui.ScreenSize;
        Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
        string currentScreen = Onix.Gui.ScreenName;
        
        float inventoryItemsWidthOffset = UiState.LayoutMode == InventoryLayout.Survival ? 0f : 75f;

        float inventoryItemsWidth = 162;
        float inventoryItemsHeight = 54;
        float inventoryItemsHeightOffset = Shulker.GetScreenHeightOffset();
        Rect inventoryItemsBox = new(
            screenCenter.X - inventoryItemsWidth / 2 + inventoryItemsWidthOffset,
            screenCenter.Y - inventoryItemsHeight / 2 + inventoryItemsHeightOffset,
            screenCenter.X + inventoryItemsWidth / 2 + inventoryItemsWidthOffset,
            screenCenter.Y + inventoryItemsHeight / 2 + inventoryItemsHeightOffset
        );

        float hotbarItemsWidth = 162;
        float hotbarItemsHeight = 18;
        float hotbarItemsHeightOffset = inventoryItemsHeight + Shulker.GetScreenHeightOffsetHotbar();
        float hotbarItemsWidthOffset = UiState.LayoutMode == InventoryLayout.Survival ? 0f : 75f;
        Rect hotbarItemsBox = new(
            screenCenter.X - hotbarItemsWidth / 2 + hotbarItemsWidthOffset,
            screenCenter.Y - hotbarItemsHeight / 2 + hotbarItemsHeightOffset,
            screenCenter.X + hotbarItemsWidth / 2 + hotbarItemsWidthOffset,
            screenCenter.Y + hotbarItemsHeight / 2 + hotbarItemsHeightOffset
        );

        gfx.DrawRectangle(inventoryItemsBox, new ColorF(0, 255, 255, 100), 1);
        gfx.DrawRectangle(hotbarItemsBox, new ColorF(0, 255, 255, 100), 1);

        int columns = 9;
        int rows = 3;
        float cellWidth = inventoryItemsWidth / columns;
        float cellHeight = inventoryItemsHeight / rows;

        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                float x = inventoryItemsBox.X + col * cellWidth;
                float y = inventoryItemsBox.Y + row * cellHeight;

                int slotIndex = 10 + row * columns + col;

                Rect slotRect = new(x, y, x + cellWidth, y + cellHeight);
                gfx.DrawRectangle(slotRect, new ColorF(0, 200, 200, 50), 1);

                Vec2 textPos = new(x + cellWidth / 2, y + cellHeight / 2);
                gfx.RenderText(textPos, new ColorF(0, 255, 255, 200), slotIndex.ToString());
            }
        }

        int selectedSlot = Onix.LocalPlayer?.SelectedSlot ?? 0;

        columns = 9;
        cellWidth = hotbarItemsWidth / columns;

        for (int col = 0; col < columns; col++) {
            float x = hotbarItemsBox.X + col * cellWidth;
            float y = hotbarItemsBox.Y;

            int slotIndex = 1 + col;

            Rect slotRect = new(x, y, x + cellWidth, y + hotbarItemsHeight);

            if (col == selectedSlot) {
                gfx.FillRectangle(slotRect, new ColorF(255, 255, 0, 70));
                gfx.DrawRectangle(slotRect, new ColorF(255, 255, 0, 150), 2);
            }
            else {
                gfx.DrawRectangle(slotRect, new ColorF(0, 200, 200, 50), 1);
            }

            Vec2 textPos = new(x + cellWidth / 2, y + hotbarItemsHeight / 2);
            gfx.RenderText(textPos, new ColorF(0, 255, 255, 200), slotIndex.ToString());
        }

        if (Shulker.IsShulkerScreenSupported(currentScreen)) {
            var (containerSize, containerOffset) = Containers.GetContainerInfo(currentScreen);

            if (containerSize.Z > 0 && containerSize.W > 0) {
                const float slotSize = 18f;

                Vec2 containerPos = screenCenter + containerOffset;
                float containerWidth = containerSize.Z * slotSize;
                float containerHeight = containerSize.W * slotSize;

                Rect containerRect = new(
                    containerPos.X,
                    containerPos.Y,
                    containerPos.X + containerWidth,
                    containerPos.Y + containerHeight
                );

                gfx.DrawRectangle(containerRect, new ColorF(255, 200, 0, 100), 1);

                if (currentScreen == Containers.Furnaces.Furnace ||
                    currentScreen == Containers.Furnaces.BlastFurnace ||
                    currentScreen == Containers.Furnaces.Smoker) {

                    Rect ingredientRect = new(
                        containerPos.X + Containers.Furnaces.IngredientSize.X * slotSize,
                        containerPos.Y + Containers.Furnaces.IngredientSize.Y * slotSize,
                        containerPos.X + (Containers.Furnaces.IngredientSize.X + 1) * slotSize,
                        containerPos.Y + (Containers.Furnaces.IngredientSize.Y + 1) * slotSize
                    );
                    gfx.DrawRectangle(ingredientRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 ingredientTextPos = new(ingredientRect.X + slotSize / 2, ingredientRect.Y + slotSize / 2);
                    gfx.RenderText(ingredientTextPos, new ColorF(255, 200, 0, 200), "C0");
                    
                    Rect fuelRect = new(
                        containerPos.X + Containers.Furnaces.FuelSize.X * slotSize,
                        containerPos.Y + Containers.Furnaces.FuelSize.Y * slotSize,
                        containerPos.X + (Containers.Furnaces.FuelSize.X + 1) * slotSize,
                        containerPos.Y + (Containers.Furnaces.FuelSize.Y + 1) * slotSize
                    );
                    gfx.DrawRectangle(fuelRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 fuelTextPos = new(fuelRect.X + slotSize / 2, fuelRect.Y + slotSize / 2);
                    gfx.RenderText(fuelTextPos, new ColorF(255, 200, 0, 200), "C1");
                    
                    Rect resultRect = new(
                        containerPos.X + Containers.Furnaces.ResultSize.X * slotSize,
                        containerPos.Y + Containers.Furnaces.ResultSize.Y * slotSize,
                        containerPos.X + (Containers.Furnaces.ResultSize.X + 1) * slotSize,
                        containerPos.Y + (Containers.Furnaces.ResultSize.Y + 1) * slotSize
                    );
                    gfx.DrawRectangle(resultRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 resultTextPos = new(resultRect.X + slotSize / 2, resultRect.Y + slotSize / 2);
                    gfx.RenderText(resultTextPos, new ColorF(255, 200, 0, 200), "C2");
                } else if (currentScreen == Containers.Anvils.Anvil) {
                    Rect inputRect = new(
                        containerPos.X + Containers.Anvils.InputItem.X * slotSize,
                        containerPos.Y + Containers.Anvils.InputItem.Y * slotSize,
                        containerPos.X + (Containers.Anvils.InputItem.Z + 1) * slotSize,
                        containerPos.Y + (Containers.Anvils.InputItem.W + 1) * slotSize
                    );
                    gfx.DrawRectangle(inputRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 inputTextPos = new(inputRect.X + slotSize / 2, inputRect.Y + slotSize / 2);
                    gfx.RenderText(inputTextPos, new ColorF(255, 200, 0, 200), "C0");

                    Rect materialRect = new(
                        containerPos.X + Containers.Anvils.MaterialItem.X * slotSize,
                        containerPos.Y + Containers.Anvils.MaterialItem.Y * slotSize,
                        containerPos.X + (Containers.Anvils.MaterialItem.Z + 1) * slotSize,
                        containerPos.Y + (Containers.Anvils.MaterialItem.W + 1) * slotSize
                    );
                    gfx.DrawRectangle(materialRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 materialTextPos = new(materialRect.X + slotSize / 2, materialRect.Y + slotSize / 2);
                    gfx.RenderText(materialTextPos, new ColorF(255, 200, 0, 200), "C1");

                    Rect resultRect = new(
                        containerPos.X + Containers.Anvils.ResultItem.X * slotSize,
                        containerPos.Y + Containers.Anvils.ResultItem.Y * slotSize,
                        containerPos.X + (Containers.Anvils.ResultItem.Z + 1) * slotSize,
                        containerPos.Y + (Containers.Anvils.ResultItem.W + 1) * slotSize
                    );
                    gfx.DrawRectangle(resultRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 resultTextPos = new(resultRect.X + slotSize / 2, resultRect.Y + slotSize / 2);
                    gfx.RenderText(resultTextPos, new ColorF(255, 200, 0, 200), "C2");
                } 
                else if (currentScreen == Containers.Enchanting.EnchantingTable) {
                    Rect enchantRect = new(
                        containerPos.X + Containers.Enchanting.ContainerSize.X * slotSize,
                        containerPos.Y + Containers.Enchanting.ContainerSize.Y * slotSize,
                        containerPos.X + (Containers.Enchanting.ContainerSize.Z + 1) * slotSize,
                        containerPos.Y + (Containers.Enchanting.ContainerSize.W + 1) * slotSize
                    );
                    gfx.DrawRectangle(enchantRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 enchantTextPos = new(enchantRect.X + slotSize / 2, enchantRect.Y + slotSize / 2);
                    gfx.RenderText(enchantTextPos, new ColorF(255, 200, 0, 200), "C0");
                }
                else if (currentScreen == Containers.Hoppers.Hopper) {
                    Rect hopperRect = new(
                        containerPos.X + Containers.Hoppers.ContainerSize.X * slotSize,
                        containerPos.Y + Containers.Hoppers.ContainerSize.Y * slotSize,
                        containerPos.X + (Containers.Hoppers.ContainerSize.Z + 1) * slotSize,
                        containerPos.Y + (Containers.Hoppers.ContainerSize.W + 1) * slotSize
                    );
                    gfx.DrawRectangle(hopperRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 hopperTextPos = new(hopperRect.X + slotSize / 2, hopperRect.Y + slotSize / 2);
                    gfx.RenderText(hopperTextPos, new ColorF(255, 200, 0, 200), "C0");
                } 
                else if (currentScreen == Containers.CraftingGrid.CraftingTable) {
                    Rect craftingRect = new(
                        containerPos.X + Containers.CraftingGrid.ContainerSize.X * slotSize,
                        containerPos.Y + Containers.CraftingGrid.ContainerSize.Y * slotSize,
                        containerPos.X + (Containers.CraftingGrid.ContainerSize.Z + 1) * slotSize,
                        containerPos.Y + (Containers.CraftingGrid.ContainerSize.W + 1) * slotSize
                    );
                    gfx.DrawRectangle(craftingRect, new ColorF(255, 200, 0, 80), 1);
                    Vec2 craftingTextPos = new(craftingRect.X + slotSize / 2, craftingRect.Y + slotSize / 2);
                    gfx.RenderText(craftingTextPos, new ColorF(255, 200, 0, 200), "C0");
                }
                else if (currentScreen == Containers.ThreeByThree.Dispenser ||
                         currentScreen == Containers.ThreeByThree.Dropper) {
                    for (int row = 0; row < containerSize.W; row++) {
                        for (int col = 0; col < containerSize.Z; col++) {
                            float x = containerPos.X + col * slotSize;
                            float y = containerPos.Y + row * slotSize;

                            int slotIndex = row * (int)containerSize.Z + col;

                            Rect slotRect = new(x, y, x + slotSize, y + slotSize);
                            gfx.DrawRectangle(slotRect, new ColorF(255, 200, 0, 50), 1);

                            Vec2 textPos = new(x + slotSize / 2, y + slotSize / 2);
                            gfx.RenderText(textPos, new ColorF(255, 200, 0, 200), $"C{slotIndex}");
                        }
                    }
                }
                else {
                    for (int row = 0; row < containerSize.W; row++) {
                        for (int col = 0; col < containerSize.Z; col++) {
                            float x = containerPos.X + col * slotSize;
                            float y = containerPos.Y + row * slotSize;

                            int slotIndex = row * (int)containerSize.Z + col;

                            Rect slotRect = new(x, y, x + slotSize, y + slotSize);
                            gfx.DrawRectangle(slotRect, new ColorF(255, 200, 0, 50), 1);

                            Vec2 textPos = new(x + slotSize / 2, y + slotSize / 2);
                            gfx.RenderText(textPos, new ColorF(255, 200, 0, 200), $"C{slotIndex}");
                        }
                    }
                }

                UpdateContainerSizes(currentScreen, containerRect);
            }
        }

        int hoveredSlotIndex = Shulker.GetHoveredSlotIndex(Onix.Gui.MousePosition);

        if (hoveredSlotIndex != -1) {
            Rect highlightRect;

            if (hoveredSlotIndex > 0) {
                if (hoveredSlotIndex >= 10) {
                    int inventoryIndex = hoveredSlotIndex - 10;
                    int row = inventoryIndex / 9;
                    int col = inventoryIndex % 9;

                    float x = inventoryItemsBox.X + col * cellWidth;
                    float y = inventoryItemsBox.Y + row * cellHeight;

                    highlightRect = new(x, y, x + cellWidth, y + cellHeight);
                }
                else {
                    int col = hoveredSlotIndex - 1;

                    float x = hotbarItemsBox.X + col * cellWidth;
                    float y = hotbarItemsBox.Y;

                    highlightRect = new(x, y, x + cellWidth, y + hotbarItemsHeight);
                }

                gfx.DrawRectangle(highlightRect, new ColorF(0, 255, 0, 150), 2);
                
                Vec2 infoPos = new(screenSize.X / 2, screenSize.Y - 30);
                gfx.RenderText(infoPos, new ColorF(0, 255, 255, 255), $"Hovered Slot: {hoveredSlotIndex}");
                
                ItemStack? item = Shulker.GetSlot(hoveredSlotIndex);
                if (item != null && item.Item != null) {
                    Vec2 itemInfoPos = new(screenSize.X / 2, screenSize.Y - 15);
                    gfx.RenderText(itemInfoPos, new ColorF(0, 255, 255, 255),
                        $"Item: {item.Item.Name} (x{item.Count})");
                }
            }
            else if (hoveredSlotIndex < -1) {
                int containerSlotIndex = -2 - hoveredSlotIndex;
                var (containerSize, containerOffset) = Containers.GetContainerInfo(currentScreen);
                const float slotSize = 18f;
                Vec2 containerPos = screenCenter + containerOffset;

                Rect containerHighlightRect
                    ;
                
                if (currentScreen == Containers.Furnaces.Furnace ||
                    currentScreen == Containers.Furnaces.BlastFurnace ||
                    currentScreen == Containers.Furnaces.Smoker) {

                    if (containerSlotIndex == 0) {
                        containerHighlightRect
                            = new(
                            containerPos.X + Containers.Furnaces.IngredientSize.X * slotSize,
                            containerPos.Y + Containers.Furnaces.IngredientSize.Y * slotSize,
                            containerPos.X + (Containers.Furnaces.IngredientSize.X + 1) * slotSize,
                            containerPos.Y + (Containers.Furnaces.IngredientSize.Y + 1) * slotSize
                        );
                    }
                    else if (containerSlotIndex == 1) {
                        containerHighlightRect
                            = new(
                            containerPos.X + Containers.Furnaces.FuelSize.X * slotSize,
                            containerPos.Y + Containers.Furnaces.FuelSize.Y * slotSize,
                            containerPos.X + (Containers.Furnaces.FuelSize.X + 1) * slotSize,
                            containerPos.Y + (Containers.Furnaces.FuelSize.Y + 1) * slotSize
                        );
                    }
                    else {
                        containerHighlightRect
                            = new(
                            containerPos.X + Containers.Furnaces.ResultSize.X * slotSize,
                            containerPos.Y + Containers.Furnaces.ResultSize.Y * slotSize,
                            containerPos.X + (Containers.Furnaces.ResultSize.X + 1) * slotSize,
                            containerPos.Y + (Containers.Furnaces.ResultSize.Y + 1) * slotSize
                        );
                    }
                }
                else {
                    int col = containerSlotIndex % (int)containerSize.Z;
                    int row = containerSlotIndex / (int)containerSize.Z;

                    containerHighlightRect
                        = new(
                        containerPos.X + col * slotSize,
                        containerPos.Y + row * slotSize,
                        containerPos.X + (col + 1) * slotSize,
                        containerPos.Y + (row + 1) * slotSize
                    );
                }

                gfx.DrawRectangle(containerHighlightRect
                    , new ColorF(255, 140, 0, 150),
                    2);


                Vec2 infoPos = new(screenSize.X / 2, screenSize.Y - 30);
                gfx.RenderText(infoPos, new ColorF(255, 200, 0, 255), $"Hovered Container Slot: C{containerSlotIndex}");
            }
        }

        Vec2 selectedInfoPos = new(screenSize.X / 2, screenSize.Y - 45);
        int selectedSlotIndexInUI = selectedSlot + 1;
        gfx.RenderText(selectedInfoPos, new ColorF(255, 255, 0, 255), $"Selected Slot: {selectedSlotIndexInUI}");
    }

    private static void UpdateContainerSizes(string screenName, Rect containerRect) {
        Vec4 size = new Vec4(
            containerRect.X,
            containerRect.Y,
            containerRect.Z - containerRect.X,
            containerRect.W - containerRect.Y
        );

        Vec2 screenCenter = new(Onix.Gui.ScreenSize.X / 2, Onix.Gui.ScreenSize.Y / 2);
        Vec2 offset = new(containerRect.X - screenCenter.X, containerRect.Y - screenCenter.Y);

        string sizeInfo = $"Container: {screenName}, Size: {size}, Offset: {offset}";
        //Console.WriteLine(sizeInfo);
    }
}

