using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Maths;

namespace ContainR {
    public static class ContainerHelper {
        public static bool IsContainerSupported(string containerName) {
            return containerName switch {
                "small_chest_screen" => true,
                "large_chest_screen" => true,
                "shulker_box_screen" => true,
                "barrel_screen" => true,
                "furnace_screen" => true,
                "blast_furnace_screen" => true,
                "smoker_screen" => true,
                "dispenser_screen" => true,
                "dropper_screen" => true,
                "hopper_screen" => true,
                "enchanting_screen" => true,
                "anvil_screen" => true,
                "cartography_table_screen" => true,
                "loom_screen" => true,
                "stonecutter_screen" => true,
                _ => false
            };
        }
        
        public static void HandleGive(ContainerScreen container) {
            ItemStack item = container.GetItem("cursor_items", 0);
            if (item.IsEmpty) return;

            bool foundMatching = false;
            string[] sourcesContainers = ["inventory_items", "hotbar_items"];
            foreach (string sourceContainer in sourcesContainers) {
                for (int i = 0; i < 36; i++) {
                    ItemStack itemInContainer = container.GetItem(sourceContainer, i);
                    if (itemInContainer.IsEmpty) continue;
                    if (itemInContainer.Item?.NameFull == item.Item?.NameFull) {
                        container.AutoPlace(sourceContainer, i);
                        foundMatching = true;
                    }
                }
            }
            
            if (!foundMatching) {
                container.AutoPlace("cursor_items", 0);
            }
        }

        public static void HandleTake(ContainerScreen container) {
            if (MouseData.ShiftDown && !ContainR.StaticContainerSearch.Textbox.IsEmpty) {
                HandleTakeAllHighlighted(container);
                return;
            }
            
            ItemStack item = container.GetItem("cursor_items", 0);
            if (item.IsEmpty) return;
            
            bool foundMatching = false;
            const string targetContainer = "container_items";
            for (int i = 0; i < 54; i++) {
                ItemStack itemInContainer = container.GetItem(targetContainer, i);
                if (itemInContainer.IsEmpty) continue;
                if (itemInContainer.Item?.NameFull == item.Item?.NameFull) {
                    container.AutoPlace(targetContainer, i);
                    foundMatching = true;
                }
            }
            
            if (!foundMatching) {
                container.AutoPlace("cursor_items", 0);
            }
        }
        
        public static void HandleTakeAllHighlighted(ContainerScreen container) {
            HashSet<Rect> matchingSlots = ContainR.StaticContainerSearch.GetMatchingSearchSlots();
            if (matchingSlots.Count == 0) return;
            
            const string targetContainer = "container_items";
            int maxSlots = ContainerSearch.ContainerSearch.IsLargeChest ? 54 : 27;
            
            var positionCalculator = new ContainerSearch.PositionCalculator();
            
            for (int i = 0; i < maxSlots; i++) {
                Vec2? slotPosition = positionCalculator.CalculateSlotPosition(targetContainer, i);
                if (!slotPosition.HasValue) continue;
                
                Rect slotRect = new(slotPosition.Value.X, slotPosition.Value.Y, slotPosition.Value.X + 18, slotPosition.Value.Y + 18);
                
                if (matchingSlots.Contains(slotRect)) {
                    ItemStack itemInContainer = container.GetItem(targetContainer, i);
                    if (!itemInContainer.IsEmpty) {
                        container.AutoPlace(targetContainer, i);
                    }
                }
            }
        }
        
        public static void HandleDelete(ContainerScreen container, int slot, bool ignoreHotbar = false, bool ignoreInventory = true) {
            if (Onix.Gui.ScreenName != "inventory_screen" || Onix.LocalPlayer?.GameMode != GameMode.Creative) return;

            string[] sourcesContainers = ["inventory_items", "hotbar_items"];
            if (ignoreHotbar) {
                sourcesContainers = ["inventory_items"];
            } else if (ignoreInventory) {
                sourcesContainers = ["hotbar_items"];
            }

            foreach (string sourceContainer in sourcesContainers) {
                ItemStack item = container.GetItem(sourceContainer, slot);
                if (item.IsEmpty) continue;
                container.TakeAll(sourceContainer, slot);
                container.PlaceAll("recipe_items", slot);
            }
        }

        public static void HandleDeleteHeld(ContainerScreen container, int slot) {
            if (Onix.Gui.ScreenName != "inventory_screen" || Onix.LocalPlayer?.GameMode != GameMode.Creative) return;

            ItemStack item = container.GetItem("cursor_items", 0);
            if (item.IsEmpty) return;
            container.TakeAll("cursor_items", 0);
            container.PlaceAll("recipe_items", slot);
        }

        public static void HandleDeleteInventory(ContainerScreen container) {
            if (Onix.Gui.ScreenName != "inventory_screen" || Onix.LocalPlayer?.GameMode != GameMode.Creative) return;
            
            string[] sourcesContainers = ["inventory_items", "hotbar_items", "cursor_items"];
            for (int i = 0; i < 36; i++) {
                foreach (string sourceContainer in sourcesContainers) {
                    ItemStack item = container.GetItem(sourceContainer, i);
                    if (item.IsEmpty) continue;
                    container.TakeAll(sourceContainer, i);
                    container.PlaceAll("recipe_items", i);
                }
            }
        }
    }
}
