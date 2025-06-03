using OnixRuntime.Api;
using OnixRuntime.Api.UI;

namespace ContainR {
    public class ContainerManager {
            static ShulkerBox.UiRender _shulkerBoxRenderer = new ShulkerBox.UiRender();
        public static void HandleContainerScreenTick(ContainerScreen container) {
            //ContainerHelper.HandleDelete(container, 0);
            if (MouseData.MmbDown) {
    
            }

            if (!UiState.Hovering || !(MouseData.LmbDown || MouseData.RmbDown)) return;
            if (ContainerHelper.IsContainerSupported(Onix.Gui.ScreenName)) {
                if (UiState.HoveringTake) {
                    ContainerHelper.HandleTake(container);
                }
                else {
                    ContainerHelper.HandleGive(container);
                }
            }

            if (UiState.HoveringDelete) {
                if (MouseData.ShiftDown) {
                    if (MouseData.RmbDown) {
                        ContainerHelper.HandleDeleteInventory(container);
                    } else if (MouseData.LmbDown) {
                        for (int i = 0; i < 9; i++) {
                            ContainerHelper.HandleDelete(container, i);
                        }
                    }
                }
                else {
                    if (!MouseData.LmbDown) return;
                    ContainerHelper.HandleDeleteHeld(container, 0);
                }
            }
        }
    }
}
