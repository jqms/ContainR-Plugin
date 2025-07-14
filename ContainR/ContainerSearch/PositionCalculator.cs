using OnixRuntime.Api.Maths;

namespace ContainR.ContainerSearch {
    public class PositionCalculator {
        public Vec2? CalculateSlotPosition(string collection, int index) {
            float inventoryBaseX = 474.0f;
            float inventoryBaseY = 265.0f;
            float hotbarBaseX = 474.0f;
            float hotbarBaseY = 322.0f;
            float largeChestBaseX = 399.0f;
            float largeChestBaseY = 173.0f;
            float smallChestBaseX = 399.0f;
            float smallChestBaseY = 200.0f;
            
            switch (collection) {
                case "inventory_items": {
                    int row = index / 9;
                    int col = index % 9;
                    return new Vec2(inventoryBaseX + (col * 18), inventoryBaseY + (row * 18));
                }
                case "hotbar_items":
                    return new Vec2(hotbarBaseX + (index * 18), hotbarBaseY);
                case "container_items": {
                    if (ContainerSearch.IsLargeChest) {
                        int row = index / 9;
                        int col = index % 9;
                        return new Vec2(largeChestBaseX + (col * 18), largeChestBaseY + (row * 18));
                    } else {
                        int row = index / 9;
                        int col = index % 9;
                        return new Vec2(smallChestBaseX + (col * 18), smallChestBaseY + (row * 18));
                    }
                }
                default:
                    return null;
            }
        }

        public string GetCollectionForPosition(Rect pos) {
            float inventoryBaseX = 474.0f;
            float inventoryBaseY = 265.0f;
            float hotbarBaseX = 474.0f;
            float hotbarBaseY = 322.0f;
            float largeChestBaseX = 399.0f;
            float largeChestBaseY = 173.0f;
            float smallChestBaseX = 399.0f;
            float smallChestBaseY = 200.0f;
            
            if (Math.Abs(pos.Y - hotbarBaseY) < 1 && 
                pos.X >= hotbarBaseX && pos.X < hotbarBaseX + (9 * 18)) {
                return "hotbar_items";
            }
            
            if (ContainerSearch.IsChestOpen) {
                if (ContainerSearch.IsLargeChest) {
                    if (pos.Y >= largeChestBaseY && pos.Y < largeChestBaseY + (6 * 18) &&
                        pos.X >= largeChestBaseX && pos.X < largeChestBaseX + (9 * 18)) {
                        return "container_items";
                    }
                } else {
                    if (pos.Y >= smallChestBaseY && pos.Y < smallChestBaseY + (3 * 18) &&
                        pos.X >= smallChestBaseX && pos.X < smallChestBaseX + (9 * 18)) {
                        return "container_items";
                    }
                }
                
                if (pos.Y >= inventoryBaseY && pos.Y < inventoryBaseY + (3 * 18) &&
                    pos.X >= inventoryBaseX && pos.X < inventoryBaseX + (9 * 18)) {
                    return "skip_inventory";
                }
            } else {
                if (pos.Y >= inventoryBaseY && pos.Y < inventoryBaseY + (3 * 18) &&
                    pos.X >= inventoryBaseX && pos.X < inventoryBaseX + (9 * 18)) {
                    return "inventory_items";
                }
            }
            
            return "unknown";
        }
    }
}
