using OnixRuntime.Api.UI;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using System.Text.RegularExpressions;

namespace ContainR.ContainerSearch {
    public class InventoryProcessor {
        private readonly ContainerSearch _containerSearch;
        private readonly PositionCalculator _positionCalculator;

        public InventoryProcessor(ContainerSearch containerSearch, PositionCalculator positionCalculator) {
            _containerSearch = containerSearch;
            _positionCalculator = positionCalculator;
        }

        public void FindPlayerInventorySlots(GameUIElement element) {
            ProcessInventoryRecursive(element);
        }

        public void ProcessInventoryElement(GameUIElement inventoryElement) {
            _containerSearch.NonEmptySlots.Clear();
            if (OnixRuntime.Api.Onix.Gui.RootUiElement != null) {
                ProcessInventoryRecursive(OnixRuntime.Api.Onix.Gui.RootUiElement);
            }
        }
        
        private void ProcessInventoryRecursive(GameUIElement element) {
            if (element.Name == "large_chest_panel_top_half") {
                ContainerSearch.IsChestOpen = true;
                ContainerSearch.IsLargeChest = true;
            } else if (element.Name == "small_chest_panel_top_half") {
                ContainerSearch.IsChestOpen = true;
                ContainerSearch.IsLargeChest = false;
            } else if (element.Name == "chest_panel") {
                ContainerSearch.IsChestOpen = true;
                ContainerSearch.IsLargeChest = false;
            }

            if (element.Name == "hover_text") {
                string jsonProps = element.JsonProperties;
                
                if (jsonProps.Contains("#hover_text")) {
                    Match hoverTextMatch = RegexPatterns.HoverTextRegex().Match(jsonProps);
                    if (hoverTextMatch.Success) {
                        string hoverText = hoverTextMatch.Groups[1].Value;
                        
                        GameUIElement? parentElement = element.Parent;
                        while (parentElement != null) {
                            if (parentElement.JsonProperties.Contains("#collection_name") && parentElement.JsonProperties.Contains("#collection_index")) {
                                Match collectionMatch = RegexPatterns.CollectionNameRegex().Match(parentElement.JsonProperties);
                                Match indexMatch = RegexPatterns.CollectionIndexRegex().Match(parentElement.JsonProperties);
                                
                                if (collectionMatch.Success && indexMatch.Success) {
                                    string collection = collectionMatch.Groups[1].Value;
                                    int index = int.Parse(indexMatch.Groups[1].Value);
                                    
                                    if (collection is "inventory_items") {
                                        GameUIElement? checkParent = parentElement;
                                        while (checkParent != null) {
                                            if (checkParent.Name is "inventory_panel_bottom_half_with_label") {
                                                return;
                                            }
                                            checkParent = checkParent.Parent;
                                        }
                                    }
                                    
                                    Vec2? position = _positionCalculator.CalculateSlotPosition(collection, index);
                                    if (position.HasValue) {
                                        Rect rect = new(position.Value.X, position.Value.Y, position.Value.X + 18, position.Value.Y + 18);
                                        _containerSearch.NonEmptySlots[rect] = hoverText;
                                    }
                                }
                                break;
                            }
                            parentElement = parentElement.Parent;
                        }
                    }
                }
            }

            if (element.Children.Length > 0) {
                foreach (GameUIElement childElement in element.Children) {
                    ProcessInventoryRecursive(childElement);
                }
            }
        }

        public void ProcessElement(GameUIElement element, int depth = 0) {
            string indent = new(' ', depth * 4);
            string elementIdentifier = $"{indent}{element.Name}, {element.Rect.ToString()}, {element.JsonProperties}";
    
            if (!_containerSearch.ProcessedElements.Contains(elementIdentifier)) {
                _containerSearch.Str += $"{elementIdentifier}\n";
                _containerSearch.ProcessedElements.Add(elementIdentifier);
            }
    
            if (element.Children.Length > 0) {
                foreach (GameUIElement childElement in element.Children) {
                    ProcessElement(childElement, depth + 1);
                }
            }
        }

        public void RenderHoverTextDebug(RendererGame gfx, GameUIElement rootElement) {
            ContainerSearch.CurrentHoverText = null;
            ProcessElementForHoverDebug(gfx, rootElement);
        }
        
        private void ProcessElementForHoverDebug(RendererGame gfx, GameUIElement element) {
            if (element.Name == "hover_text" && element.JsonProperties.Contains("#hover_text")) {
                GameUIElement? highlight = element.Parent;
                if (highlight != null && highlight.Name == "highlight" && highlight.JsonProperties.Contains("\"#visible\":true")) {
                    if (highlight.Rect.X != -1 && highlight.Rect.Y != -1 && highlight.Rect.Z != -1 && highlight.Rect.W != -1) {
                        GameUIElement? hover = highlight.Parent;
                        if (hover is { Name: "hover" }) {
                            bool hasWhiteBorder = hover.Children.Any(sibling => sibling.Name == "white_border" && sibling.JsonProperties.Contains("\"#visible\":true"));

                            if (hasWhiteBorder) {
                                Match hoverTextMatch = RegexPatterns.HoverTextRegex().Match(element.JsonProperties);
                                if (hoverTextMatch.Success) {
                                    string hoverText = hoverTextMatch.Groups[1].Value;
                                    
                                    ContainerSearch.CurrentHoverText = hoverText;
                                }
                            }
                        }
                    }
                }
            }
            
            if (element.Children.Length > 0) {
                foreach (GameUIElement childElement in element.Children) {
                    ProcessElementForHoverDebug(gfx, childElement);
                }
            }
        }
    }
}
