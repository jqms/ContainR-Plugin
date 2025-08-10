using OnixRuntime.Api;
using OnixRuntime.Api.UI;

namespace ContainR {
    public static class UiState {
        public static bool HoveringTake = false;
        public static bool HoveringDelete = false;
        public static bool Hovering = false;
        public static InventoryLayout LayoutMode = InventoryLayout.None;
        public static int LastHoveredSlot = -1;
        public static int CurrentHoveredSlot = -1;
    }
}
