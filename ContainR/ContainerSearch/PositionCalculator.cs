using OnixRuntime.Api.Maths;
using OnixRuntime.Api;

namespace ContainR.ContainerSearch {
    public class PositionCalculator {
        public Vec2? CalculateSlotPosition(string collection, int index) {
            Vec2 screenSize = Onix.Gui.ScreenSize;
            Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
            
            Vec2 inventoryOffset = new(-5, 4f);
            Vec2 hotbarOffset = new(-5f, 61f);
            Vec2 largeChestOffset = new(-80f, -88f);
            Vec2 smallChestOffset = new(-80f, -61f);
            
            const float slotSize = 18f;
            
            switch (collection) {
                case "inventory_items": {
                    int row = index / 9;
                    int col = index % 9;
                    Vec2 basePos = screenCenter + inventoryOffset;
                    return new Vec2(basePos.X + (col * slotSize), basePos.Y + (row * slotSize));
                }
                case "hotbar_items": {
                    Vec2 basePos = screenCenter + hotbarOffset;
                    return new Vec2(basePos.X + (index * slotSize), basePos.Y);
                }
                case "container_items": {
                    Vec2 basePos = ContainerSearch.IsLargeChest ? 
                        screenCenter + largeChestOffset : 
                        screenCenter + smallChestOffset;
                    int row = index / 9;
                    int col = index % 9;
                    return new Vec2(basePos.X + (col * slotSize), basePos.Y + (row * slotSize));
                }
                default:
                    return null;
            }
        }

        public string GetCollectionForPosition(Rect pos) {
            Vec2 screenSize = Onix.Gui.ScreenSize;
            Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
            
            Vec2 inventoryOffset = new(-15f, 23f);
            Vec2 hotbarOffset = new(-15f, 80f);
            Vec2 largeChestOffset = new(-90f, -69f);
            Vec2 smallChestOffset = new(-90f, -42f);
            
            const float slotSize = 18f;
            
            Vec2 hotbarBase = screenCenter + hotbarOffset;
            Vec2 inventoryBase = screenCenter + inventoryOffset;
            
            if (Math.Abs(pos.Y - hotbarBase.Y) < 1 && 
                pos.X >= hotbarBase.X && pos.X < hotbarBase.X + (9 * slotSize)) {
                return "hotbar_items";
            }
            
            if (ContainerSearch.IsChestOpen) {
                Vec2 containerBase = ContainerSearch.IsLargeChest ? 
                    screenCenter + largeChestOffset : 
                    screenCenter + smallChestOffset;
                    
                int containerRows = ContainerSearch.IsLargeChest ? 6 : 3;
                if (pos.Y >= containerBase.Y && pos.Y < containerBase.Y + (containerRows * slotSize) &&
                    pos.X >= containerBase.X && pos.X < containerBase.X + (9 * slotSize)) {
                    return "container_items";
                }
                
                if (pos.Y >= inventoryBase.Y && pos.Y < inventoryBase.Y + (3 * slotSize) &&
                    pos.X >= inventoryBase.X && pos.X < inventoryBase.X + (9 * slotSize)) {
                    return "skip_inventory";
                }
            } else {
                if (pos.Y >= inventoryBase.Y && pos.Y < inventoryBase.Y + (3 * slotSize) &&
                    pos.X >= inventoryBase.X && pos.X < inventoryBase.X + (9 * slotSize)) {
                    return "inventory_items";
                }
            }
            
            return "unknown";
        }
    }
}
