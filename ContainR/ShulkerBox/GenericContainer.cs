using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;

namespace ContainR.ShulkerBox;

public class Containers {
    public static class Generic {
        public static readonly string ShulkerBox = "shulker_box_screen";
        public static readonly string EnderChest = "ender_chest_screen";
        public static readonly string SmallChest = "small_chest_screen";
        public static readonly string Barrel = "barrel_screen";
        public static readonly Vec4 ContainerSize = new(0, 0, 9, 3);
        public static readonly Vec2 ContainerOffset = new(-81, -62f);
    }
    
    public static class LargeChests {
        public static readonly string LargeChest = "large_chest_screen";
        public static readonly Vec4 ContainerSize = new(0, 0, 9, 6);
        public static readonly Vec2 ContainerOffset = new(-81, -89);
    }
    
    public static class ThreeByThree {
        public static readonly string Dispenser = "dispenser_screen";
        public static readonly string Dropper = "dropper_screen";
        public static readonly Vec4 ContainerSize = new(0, 0, 3, 3);
        public static readonly Vec2 ContainerOffset = new(-27, -67.5f);
    }

    public static class CraftingGrid {
        public static readonly string CraftingTable = "crafting_screen";
        public static readonly Vec4 ContainerSize = new(0, 0, 3, 3);
        public static Vec2 ContainerOffset = new(-59f, -67f);
    }

    public static class Furnaces {
        public static readonly string Furnace = "furnace_screen";
        public static readonly string BlastFurnace = "blast_furnace_screen";
        public static readonly string Smoker = "smoker_screen";
        public static readonly Vec4 IngredientSize = new(0, 0, 1, 1);
        public static readonly Vec4 FuelSize = new(0, 2.125f, 1, 1);
        public static readonly Vec4 ResultSize = new(3.45f, 1, 2, 1);
        public static readonly Vec2 ContainerOffset = new(-38, -68.5f);
    }
    
    public static class Hoppers {
        public static readonly string Hopper = "hopper_screen";
        public static readonly Vec4 ContainerSize = new(0, 0, 5, 1);
        public static readonly Vec2 ContainerOffset = new(-45, -48.5f);
    }
    
    public static class Anvils {
        public static readonly string Anvil = "anvil_screen";
        public static readonly Vec4 InputItem = new(0, 0.025f, 1, 1);
        public static readonly Vec4 MaterialItem = new(3.025f, 0.025f, 1, 1);
        public static readonly Vec4 ResultItem = new(8.525f, 0, 1, 1);
        public static readonly Vec2 ContainerOffset = new(-67.5f, -36f);
    }
    
    public static class Enchanting {
        public static readonly string EnchantingTable = "enchanting_screen";
        public static readonly Vec4 ContainerSize = new(0, 1f, 1, 1);
        public static readonly Vec2 ContainerOffset = new(-75, -35.5f);
    }
    
    public static (Vec4 size, Vec2 offset) GetContainerInfo(string screenName) {
        if (screenName == Generic.ShulkerBox || 
            screenName == Generic.EnderChest || 
            screenName == Generic.SmallChest || 
            screenName == Generic.Barrel) {
            return (Generic.ContainerSize, Generic.ContainerOffset);
        }
        
        if (screenName == LargeChests.LargeChest) {
            return (LargeChests.ContainerSize, LargeChests.ContainerOffset);
        }
        
        if (screenName == ThreeByThree.Dispenser || 
            screenName == ThreeByThree.Dropper) {
            return (ThreeByThree.ContainerSize, ThreeByThree.ContainerOffset);
        }

        if (screenName == CraftingGrid.CraftingTable) {
            if (UiState.LayoutMode == InventoryLayout.RecipeBook) CraftingGrid.ContainerOffset.X = 16f;
            else CraftingGrid.ContainerOffset.X = -59f;
            return (CraftingGrid.ContainerSize, CraftingGrid.ContainerOffset);
        }
        
        if (screenName == Hoppers.Hopper) {
            return (Hoppers.ContainerSize, Hoppers.ContainerOffset);
        }
        
        if (screenName == Furnaces.Furnace || 
            screenName == Furnaces.BlastFurnace || 
            screenName == Furnaces.Smoker) {
            return (Furnaces.IngredientSize, Furnaces.ContainerOffset);
        }
        
        if (screenName == Anvils.Anvil) {
            return (Anvils.InputItem, Anvils.ContainerOffset);
        }
        
        if (screenName == Enchanting.EnchantingTable) {
            return (Enchanting.ContainerSize, Enchanting.ContainerOffset);
        }
        
        return (new Vec4(0, 0, 0, 0), Vec2.Zero);
    }
}