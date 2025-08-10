using OnixRuntime.Api.Inputs;
using OnixRuntime.Api;

namespace ContainR.ContainerSearch {
    public class InputHandler(ContainerSearch containerSearch) {
        public bool _ctrlPressed;
        
        public bool HandleInput(InputKey key, bool isDown) {
            if (key == InputKey.Type.Ctrl || key == InputKey.Type.LCtrl || key == InputKey.Type.RCtrl) {
                _ctrlPressed = isDown;
                ContainerSearch.IsCtrlDown = isDown;
            }

            if (containerSearch.LastScreenName != "hud_screen") {
                containerSearch.InventoryCacheValid = false;
            }

            if (ContainerSearch.IsAContainer && ContainerSearch.ShouldRenderThings) {
                if (key == InputKey.Type.F && isDown && _ctrlPressed) {
                    if (containerSearch.Textbox.IsFocused) {
                        containerSearch.AddToSearchHistory(containerSearch.Textbox.Text);
                        containerSearch.Textbox.IsFocused = false;
                        containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, 0);
                    } else {
                        containerSearch.Textbox.IsFocused = true;
                        containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, containerSearch.Textbox.Text.Length);
                    }
                    return true;
                }

                if (containerSearch.Textbox.IsFocused && key == InputKey.Type.Escape && isDown == false) {
                    containerSearch.Textbox.IsFocused = false;
                    containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, 0);
                    return true;
                }

                if (containerSearch.Textbox.IsFocused && isDown) {
                    if (key == InputKey.Type.Up) {
                        containerSearch.NavigateHistoryUp();
                        return true;
                    }
                    if (key == InputKey.Type.Down) {
                        containerSearch.NavigateHistoryDown();
                        return true;
                    }
                    if (key == InputKey.Type.Enter) {
                        containerSearch.AddToSearchHistory(containerSearch.Textbox.Text);
                        containerSearch.Textbox.IsFocused = false;
                        containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, 0);
                        return true;
                    }
                }

                if (key == InputKey.Type.LMB && isDown) {
                    if (containerSearch.TextboxRect.Contains(Onix.Gui.MousePosition)) {
                        containerSearch.Textbox.IsFocused = true;
                        return true;
                    }
                    else if (containerSearch.Textbox.IsFocused) {
                        containerSearch.AddToSearchHistory(containerSearch.Textbox.Text);
                        containerSearch.Textbox.IsFocused = false;
                        containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, 0);
                    }
                }
                if (key == InputKey.Type.LMB) {
                    if (containerSearch.Textbox.IsFocused && !containerSearch.TextboxRect.Contains(Onix.Gui.MousePosition)) {
                        containerSearch.AddToSearchHistory(containerSearch.Textbox.Text);
                        containerSearch.Textbox.IsFocused = false;
                        containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, 0);
                    }
                }
                
                if (key == InputKey.Type.MouseButton5 && isDown) {
                    if (containerSearch.Textbox.IsFocused) {
                        containerSearch.AddToSearchHistory(containerSearch.Textbox.Text);
                        containerSearch.Textbox.IsFocused = false;
                        containerSearch.Textbox.Selection = new OnixTextbox.TextSelection(0, 0);
                        return true;
                    }
                }
            }

            return containerSearch.Textbox.IsFocused;
        }
    }
}
