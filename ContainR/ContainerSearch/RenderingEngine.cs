using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;
using OnixRuntime.Api;
using OnixRuntime.Api.Inputs;
using System.Text.RegularExpressions;
using OnixRuntime.Api.OnixClient;
using OnixRuntime.Api.OnixClient.Settings;

namespace ContainR.ContainerSearch {
    public class RenderingEngine(
        ContainerSearch containerSearch,
        InventoryProcessor inventoryProcessor,
        PositionCalculator positionCalculator) {
        
        private static readonly ColorF ExactMatchColor = new(0, 255, 0, 0.0f); // 0 opacity rn cuz i didnt want it anymore
        private static readonly ColorF FuzzyMatchColor = new(255, 255, 0, 0.0f); // 0 opacity rn cuz i didnt want it anymore
        private static readonly ColorF RegexMatchColor = new(0, 150, 255, 0.0f); // 0 opacity rn cuz i didnt want it anymore
        private static readonly ColorF LevenshteinMatchColor = new(255, 150, 0, 0.0f); // 0 opacity rn cuz i didnt want it anymore
        private static readonly ColorF NonMatchFadeColor = new(0, 0, 0, 0.5f);

        private static readonly float lerpSpeed = 10f;
        private static Vec2 mouseScrollDelta = new(0, 0);
        private static Vec2 currentOffset = new(0, 0);

        public void restoreScrollDelta() {
            mouseScrollDelta = new Vec2(0, 0);
            UiState.LastHoveredSlot = UiState.CurrentHoveredSlot;
        }

        public bool TooltipOnInput(InputKey key, bool isDown) {
            bool isScrollableTooltipEnabled = false;
            OnixModuleList modules = Onix.Client.Modules;
            OnixModule betterTooltips = modules.GetModule("module.better_tooltips.name")!;
            if (betterTooltips.Enabled) {
                foreach (OnixSetting setting in betterTooltips.Settings) {
                    if (setting.SaveName == "module.better_tooltips.setting.scrollable_tooltips.name") {
                        OnixSettingBool scrollableTooltipSetting = (OnixSettingBool)setting;
                        isScrollableTooltipEnabled = scrollableTooltipSetting.Value;
                        break;
                    }
                }
            }
            
            if (ContainerSearch.IsCtrlDown && isScrollableTooltipEnabled) {
                if (key == InputKey.Type.Scroll) {
                    float delta = isDown ? 10f : -10f;
                    if (MouseData.ShiftDown)
                        mouseScrollDelta.X += delta;
                    else
                        mouseScrollDelta.Y += delta;
                    return true;
                }
            }

            if (key == InputKey.Type.Ctrl && !isDown) restoreScrollDelta();
            if (key == InputKey.Type.MMB && isDown) restoreScrollDelta();
            return false;
        }
        
        private float Lerp(float start, float end, float t) {
            return start + (end - start) * t;
        }

        public void RenderSearch(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi) {
            //gfx.RenderText(new Vec2(0,0), ColorF.White, Onix.Gui.MousePosition.ToString());
            
            if (Onix.Gui.RootUiElement != null) {
                inventoryProcessor.RenderHoverTextDebug(gfx, Onix.Gui.RootUiElement);
            }

            if (screenName != "inventory_screen" || UiState.LastHoveredSlot != UiState.CurrentHoveredSlot) {
                restoreScrollDelta();
            }
            
            containerSearch.LastScreenName = screenName;
            
            ContainerSearch.IsChestOpen = false;
            ContainerSearch.IsLargeChest = false;
            if (screenName.Contains("chest_screen") || screenName.Contains("shulker") || screenName.Contains("barrel")) {
                ContainerSearch.IsChestOpen = true;
                ContainerSearch.IsLargeChest = screenName == "large_chest_screen";
            }
            
            ContainerSearch.IsAContainer = screenName == "inventory_screen" || screenName.Contains("chest_screen") || screenName == "container_screen" || screenName.Contains("shulker") || screenName.Contains("barrel");
            if (ContainerSearch.IsAContainer && ContainerSearch.ShouldRenderThings) {
                UpdateTextboxPosition();

                TextureManager.EditBoxIndentNineSlice.Render(gfx, containerSearch.TextboxRect, 1.0f);
                gfx.FlushMesh();

                if (containerSearch.TextboxRect.Contains(Onix.Gui.MousePosition)) {
                    TextureManager.EditBoxIndentHoverNineSlice.Render(gfx, containerSearch.TextboxRect, 1.0f);
                    gfx.FlushMesh();
                }

                containerSearch.Textbox.Render(containerSearch.TextboxRect, new ColorF(255, 255, 255),
                    new ColorF(0, 0, 0, 0.0f), new ColorF(0, 0, 0, 0f),
                    OnixTextbox.CursorVisibility.BlinkingWhenFocused);
                UpdateInventoryCache();
                RenderSlotHighlights(gfx);

                Vec2 tooltipPos = new(Onix.Gui.MousePosition.X + 14, Onix.Gui.MousePosition.Y - 6f);
                const float padding2 = 4f;
                string displayText = (ContainerSearch.CurrentHoverText ?? "").Replace("\\n", "\n");
                if (displayText != "" && !string.IsNullOrEmpty(displayText)) {
                    gfx.FontType = FontType.Mojangles;
                    float lerpFactor = MathF.Min(lerpSpeed * delta, 1.0f);
                    currentOffset.X = Lerp(currentOffset.X, mouseScrollDelta.X, lerpFactor);
                    currentOffset.Y = Lerp(currentOffset.Y, mouseScrollDelta.Y, lerpFactor);
                    tooltipPos.X += currentOffset.X;
                    tooltipPos.Y += currentOffset.Y;
                    Vec2 textSize = gfx.MeasureText(displayText);
                    TextureManager.PurpleBorderNineSlice.Render(gfx, new Rect(tooltipPos.X - padding2, tooltipPos.Y - padding2, tooltipPos.X + textSize.X + padding2, tooltipPos.Y + textSize.Y + padding2), 1.0f);
                    Vec2 textPos = new(tooltipPos.X, tooltipPos.Y);
                    textPos.X += 0.5f;
                    textPos.Y += 0.5f;
                    gfx.RenderText(textPos, ColorF.White, displayText);
                    gfx.FontType = FontType.UserPreference;
                }
            }
        }

        private void UpdateTextboxPosition() {
            Vec2 screenSize = Onix.Gui.ScreenSize;
            Vec2 screenCenter = new(screenSize.X / 2, screenSize.Y / 2);
            
            const float textboxWidth = 72f;
            const float textboxHeight = 16f;
            
            if (ContainerSearch.IsChestOpen) {
                Vec2 chestTextboxOffset = ContainerSearch.IsLargeChest ? new Vec2(-6f, -106f) : new Vec2(-6f, -79f); 
                    
                Vec2 textboxPos = screenCenter + chestTextboxOffset;
                containerSearch.TextboxRect = new Rect(
                    textboxPos.X, 
                    textboxPos.Y, 
                    textboxPos.X + textboxWidth, 
                    textboxPos.Y + textboxHeight
                );
            } else {
                Vec2 inventoryTextboxOffset = new(85f, -21f);
                
                if (ContainerSearch.ShouldOffsetItems) {
                    inventoryTextboxOffset.X -= 75f;
                }
                
                Vec2 textboxPos = screenCenter + inventoryTextboxOffset;
                containerSearch.TextboxRect = new Rect(
                    textboxPos.X, 
                    textboxPos.Y, 
                    textboxPos.X + textboxWidth, 
                    textboxPos.Y + textboxHeight
                );
            }
        }

        private void UpdateInventoryCache() {
            if (!containerSearch.InventoryCacheValid) {
                containerSearch.NonEmptySlots.Clear();
                
                if (Onix.Gui.RootUiElement?.Children != null) {
                    foreach (GameUIElement gameUiElement in Onix.Gui.RootUiElement.Children) {
                        inventoryProcessor.FindPlayerInventorySlots(gameUiElement);
                    }
                }
                
                containerSearch.CachedNonEmptySlots.Clear();
                foreach (KeyValuePair<Rect, string> slot in containerSearch.NonEmptySlots) {
                    containerSearch.CachedNonEmptySlots[slot.Key] = slot.Value;
                }
                containerSearch.InventoryCacheValid = true;
            } else {
                containerSearch.NonEmptySlots.Clear();
                foreach (KeyValuePair<Rect, string> slot in containerSearch.CachedNonEmptySlots) {
                    containerSearch.NonEmptySlots[slot.Key] = slot.Value;
                }
            }
        }
        
        ColorF DarkenColor(ColorF color, float amount) {
            return new ColorF(
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount),
                color.A
            );
        }

        private void RenderSlotHighlights(RendererGame gfx) {
            if (containerSearch.Textbox.IsEmpty) return;
            
            Dictionary<Rect, string> matchingSlots = [];
            int totalMatches = 0;
            int totalSlots = containerSearch.NonEmptySlots.Count;
            
            string searchText = containerSearch.Textbox.Text;
            bool isRegexSearch = searchText.StartsWith("r?");
            bool isFuzzySearch = searchText.StartsWith("f?");
            bool isLevenshteinSearch = searchText.StartsWith("l?");
            
            string actualSearchText;
            if (isRegexSearch || isFuzzySearch || isLevenshteinSearch) {
                actualSearchText = searchText[2..];
            } else {
                actualSearchText = searchText;
            }
            
            Regex? regex = null;
            if (isRegexSearch && !string.IsNullOrEmpty(actualSearchText)) {
                try {
                    regex = new Regex(actualSearchText, RegexOptions.IgnoreCase);
                } catch {
                    isRegexSearch = false;
                }
            }
            
            foreach (KeyValuePair<Rect, string> slot in containerSearch.NonEmptySlots) {
                Rect pos = slot.Key;
                string itemName = slot.Value;
                if (ContainerSearch.ShouldOffsetItems && containerSearch.LastScreenName == "inventory_screen") {
                    pos.X -= 75;
                    pos.Z -= 75;
                }
                
                string matchType = "";
                bool isMatch;
                
                if (isRegexSearch && regex != null) {
                    isMatch = regex.IsMatch(itemName);
                    if (isMatch) matchType = "regex";
                } else if (isFuzzySearch) {
                    isMatch = IsFuzzyMatch(itemName, actualSearchText);
                    if (isMatch) matchType = "fuzzy";
                } else if (isLevenshteinSearch) {
                    isMatch = IsLevenshteinMatch(itemName, actualSearchText);
                    if (isMatch) matchType = "levenshtein";
                } else {
                    isMatch = itemName.Contains(actualSearchText, StringComparison.CurrentCultureIgnoreCase);
                    if (isMatch) matchType = "exact";
                }
                
                if (isMatch) {
                    matchingSlots[pos] = matchType;
                    totalMatches++;
                }
            }
            
            List<(string collection, Rect pos)> allSlots = GenerateAllSlotPositions();
            
            foreach ((Rect pos, string _) in matchingSlots) {
                Rect highlightRect = new(pos.X, pos.Y, pos.Z - 1, pos.W - 1);

                ColorF highlightColor = new(0, 0, 0, 0);
                
                gfx.FillRectangle(highlightRect, highlightColor);
            }
            
            foreach ((string _, Rect pos) in allSlots) {
                if (!matchingSlots.ContainsKey(pos)) {
                    Rect highlightRect = new(pos.X, pos.Y,pos.Z - 2, pos.W - 2);

                    gfx.FillRectangle(highlightRect, NonMatchFadeColor);
                }
            }
            
            if (!string.IsNullOrEmpty(actualSearchText)) {
                string countText = $"{totalMatches}/{totalSlots} items";
                
                gfx.FontType = FontType.Mojangles;
                Vec2 textSize = gfx.MeasureText(countText);
                const float padding = 4f;
                Vec2 backgroundSize = new(textSize.X + (padding * 2), textSize.Y + (padding * 2));
                
                Vec2 screenSize = Onix.Gui.ScreenSize;
                float screenCenterX = screenSize.X / 2f;
                float counterCenterX = screenCenterX - backgroundSize.X / 2f;
                float heightOffset = Onix.Gui.ScreenName != "inventory_screen" ? 20.5f : 100.5f;
                Vec2 countPos = new(screenCenterX - textSize.X / 2f, containerSearch.TextboxRect.Y - heightOffset);
                Rect backgroundRect = Rect.FromSize(counterCenterX, containerSearch.TextboxRect.Y - (heightOffset + 0.5f) - padding, backgroundSize.X, backgroundSize.Y);
                
                TextureManager.DialogBackgroundOpaqueNineSlice.Render(gfx, backgroundRect, 1f, TextureManager.DialogBackgroundTexture.Uv);
                
                ColorF textColor = totalMatches == totalSlots ? new ColorF(85, 255, 85) : totalMatches > 0 ? new ColorF(0, 170, 0) : new ColorF(170, 0, 0);
                Vec2 shadowOffset = new(1f, 1f);
                Vec2 shadowPos = countPos + shadowOffset;
                //gfx.RenderText(shadowPos, DarkenColor(textColor, 50f), countText);
                gfx.RenderText(countPos, textColor, countText);
                gfx.FontType = FontType.UserPreference;
            }
        }

        public HashSet<Rect> GetMatchingSlots() {
            if (containerSearch.Textbox.IsEmpty) return [];
            
            HashSet<Rect> matchingSlots = [];
            
            string searchText = containerSearch.Textbox.Text;
            bool isRegexSearch = searchText.StartsWith("r?");
            bool isFuzzySearch = searchText.StartsWith("f?");
            bool isLevenshteinSearch = searchText.StartsWith("l?");
            
            string actualSearchText;
            if (isRegexSearch || isFuzzySearch || isLevenshteinSearch) {
                actualSearchText = searchText.Substring(2);
            } else {
                actualSearchText = searchText;
            }
            
            Regex? regex = null;
            if (isRegexSearch && !string.IsNullOrEmpty(actualSearchText)) {
                try {
                    regex = new Regex(actualSearchText, RegexOptions.IgnoreCase);
                } catch {
                    isRegexSearch = false;
                }
            }
            
            foreach (KeyValuePair<Rect, string> slot in containerSearch.NonEmptySlots) {
                Rect pos = slot.Key;
                string itemName = slot.Value;
                if (ContainerSearch.ShouldOffsetItems && containerSearch.LastScreenName == "inventory_screen") {
                    pos.X -= 75;
                    pos.Z -= 75;
                }
                
                bool isMatch;
                if (isRegexSearch && regex != null) {
                    isMatch = regex.IsMatch(itemName);
                } else if (isFuzzySearch) {
                    isMatch = IsFuzzyMatch(itemName, actualSearchText);
                } else if (isLevenshteinSearch) {
                    isMatch = IsLevenshteinMatch(itemName, actualSearchText);
                } else {
                    isMatch = itemName.Contains(actualSearchText, StringComparison.CurrentCultureIgnoreCase);
                }
                
                if (isMatch) {
                    matchingSlots.Add(pos);
                }
            }
            
            return matchingSlots;
        }
        
        private bool IsFuzzyMatch(string itemName, string searchText) {
            if (string.IsNullOrEmpty(searchText)) return true;
            if (string.IsNullOrEmpty(itemName)) return false;
            
            string normalizedItem = NormalizeFuzzyText(itemName.ToLowerInvariant());
            string normalizedSearch = NormalizeFuzzyText(searchText.ToLowerInvariant());
            
            int searchIndex = 0;
            for (int i = 0; i < normalizedItem.Length && searchIndex < normalizedSearch.Length; i++) {
                if (normalizedItem[i] == normalizedSearch[searchIndex]) {
                    searchIndex++;
                }
            }
            
            return searchIndex == normalizedSearch.Length;
        }
        
        private string NormalizeFuzzyText(string text) {
            return text
                .Replace("protection", "prot")
                .Replace("sharpness", "sharp")
                .Replace("efficiency", "eff")
                .Replace("unbreaking", "unb")
                .Replace("fortune", "fort")
                .Replace("looting", "loot")
                .Replace("feather falling", "ff")
                .Replace("fire protection", "fire prot")
                .Replace("blast protection", "blast prot")
                .Replace("projectile protection", "proj prot")
                .Replace("thorns", "thorn")
                .Replace("respiration", "resp")
                .Replace("aqua affinity", "aqua")
                .Replace("depth strider", "depth")
                .Replace("frost walker", "frost")
                .Replace("soul speed", "soul")
                .Replace("swift sneak", "swift")
                .Replace("mending", "mend")
                .Replace("infinity", "inf")
                .Replace("flame", "flame")
                .Replace("power", "pow")
                .Replace("punch", "punch")
                .Replace("loyalty", "loyal")
                .Replace("impaling", "imp")
                .Replace("riptide", "rip")
                .Replace("channeling", "channel")
                .Replace("multishot", "multi")
                .Replace("piercing", "pier")
                .Replace("quick charge", "quick")
                .Replace("silk touch", "silk")
                .Replace("knockback", "kb")
                .Replace("fire aspect", "fire")
                .Replace("sweeping edge", "sweep")
                .Replace("bane of arthropods", "bane")
                .Replace("smite", "smite")
                .Replace("v", "5")
                .Replace("iv", "4")
                .Replace("iii", "3")
                .Replace("ii", "2")
                .Replace("i", "1")
                .Replace(" ", "");
        }
        
        private bool IsLevenshteinMatch(string itemName, string searchText) {
            if (string.IsNullOrEmpty(searchText)) return true;
            if (string.IsNullOrEmpty(itemName)) return false;
            
            string[] itemWords = itemName.ToLowerInvariant().Split([' ', '_', '-'], StringSplitOptions.RemoveEmptyEntries);
            string searchLower = searchText.ToLowerInvariant();

            int maxDistance = searchLower.Length > 4 ? 2 : 1;
            
            foreach (string word in itemWords) {
                if (LevenshteinDistance(word, searchLower) <= maxDistance) {
                    return true;
                }
                
                if (word.Contains(searchLower)) {
                    return true;
                }
            }
            
            return false;
        }
        
        private int LevenshteinDistance(string source, string target) {
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;
            
            int[,] distance = new int[source.Length + 1, target.Length + 1];
            
            for (int i = 0; i <= source.Length; i++) {
                distance[i, 0] = i;
            }
            for (int j = 0; j <= target.Length; j++) {
                distance[0, j] = j;
            }
            
            for (int i = 1; i <= source.Length; i++) {
                for (int j = 1; j <= target.Length; j++) {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    
                    distance[i, j] = Math.Min(
                        Math.Min(
                            distance[i - 1, j] + 1,
                            distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost
                    );
                }
            }
            
            return distance[source.Length, target.Length];
        }
        
        private List<(string collection, Rect pos)> GenerateAllSlotPositions() {
            List<(string collection, Rect pos)> allSlots = [];

            if (Onix.Gui.ScreenName == "inventory_screen") {
                for (int i = 0; i < 27; i++) {
                    Vec2? position = positionCalculator.CalculateSlotPosition("inventory_items", i);
                    if (position.HasValue) {
                        Rect pos = new(position.Value.X, position.Value.Y, position.Value.X + 18,
                            position.Value.Y + 18);
                        if (ContainerSearch.ShouldOffsetItems) {
                            pos.X -= 75;
                            pos.Z -= 75;
                        }

                        allSlots.Add(("inventory_items", pos));
                    }
                }

                for (int i = 0; i < 9; i++) {
                    Vec2? position = positionCalculator.CalculateSlotPosition("hotbar_items", i);
                    if (position.HasValue) {
                        Rect pos = new(position.Value.X, position.Value.Y, position.Value.X + 18,
                            position.Value.Y + 18);
                        if (ContainerSearch.ShouldOffsetItems) {
                            pos.X -= 75;
                            pos.Z -= 75;
                        }

                        allSlots.Add(("hotbar_items", pos));
                    }
                }
            }

            if (ContainerSearch.IsChestOpen) {
                int containerSlots = ContainerSearch.IsLargeChest ? 54 : 27;
                for (int i = 0; i < containerSlots; i++) {
                    Vec2? position = positionCalculator.CalculateSlotPosition("container_items", i);
                    if (position.HasValue) {
                        Rect pos = new(position.Value.X, position.Value.Y, position.Value.X + 18, position.Value.Y + 18);
                        allSlots.Add(("container_items", pos));
                    }
                }
            }
            
            return allSlots;
        }
    }
}
