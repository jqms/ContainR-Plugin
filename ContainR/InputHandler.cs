using OnixRuntime.Api;
using OnixRuntime.Api.Inputs;

namespace ContainR {
    public abstract class InputHandler {
        public static bool OnInput(InputKey key, bool isDown) {
            if (key == InputKey.Type.LMB) {
                MouseData.LmbDown = isDown;
            }
            if (key == InputKey.Type.RMB) {
                MouseData.RmbDown = isDown;
            }
            if (key == InputKey.Type.MMB) {
                MouseData.MmbDown = isDown;
            }
            if (key == InputKey.Type.Shift) {
                MouseData.ShiftDown = isDown;
            }
            
            if ((key == InputKey.Type.LMB || key == InputKey.Type.RMB) && (ContainerHelper.IsContainerSupported(Onix.Gui.ScreenName) || UiState.HoveringDelete)) {
                if (UiState.Hovering) {
                    return true;
                }
            } else {
                UiState.Hovering = false;
                UiState.HoveringTake = false;
                UiState.HoveringDelete = false;
            }

            return false;
        }
    }
}
