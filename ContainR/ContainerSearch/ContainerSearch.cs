using OnixRuntime.Api;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Utils;

namespace ContainR.ContainerSearch {
    public class ContainerSearch {
        public static ContainerSearch Instance { get; private set; } = null!;
        public static bool ShouldOffsetItems;
        public static bool ShouldRenderThings;
        public static bool IsAContainer;
        public static bool IsChestOpen;
        public static bool IsLargeChest;
        public static ItemStack? HeldItem;
        
        public static string? CurrentHoverText { get; set; }
        
        private readonly InputHandler _inputHandler;
        private readonly InventoryProcessor _inventoryProcessor;
        private readonly PositionCalculator _positionCalculator;
        private readonly RenderingEngine _renderingEngine;
        
        public OnixTextbox Textbox { get; private set; } = new(128, "", "");
        private Rect _textboxRect = new(565, 240, 637, 258);
        public ref Rect TextboxRect => ref _textboxRect;

        public List<string> SearchHistory { get; private set; } = [];
        public int HistoryPosition { get; set; } = -1;
        private const int MaxHistorySize = 20;

        public HashSet<string> ProcessedElements { get; private set; } = [];
        public string Str { get; set; } = "";
        public Dictionary<Rect, string> NonEmptySlots { get; private set; } = [];
        
        public Dictionary<Rect, string> CachedNonEmptySlots { get; private set; } = [];
        public bool InventoryCacheValid { get; set; }
        public string LastScreenName { get; set; } = "";

        public ContainerSearch() {
            Instance = this;
            
            _positionCalculator = new PositionCalculator();
            _inventoryProcessor = new InventoryProcessor(this, _positionCalculator);
            _inputHandler = new InputHandler(this);
            _renderingEngine = new RenderingEngine(this, _inventoryProcessor, _positionCalculator);
        }
        
        private HashSet<string> processedElements = [];
        private string str = "";
        private void ProcessElement(GameUIElement element, int depth = 0) {
            string indent = new(' ', depth * 4);
            string elementIdentifier = $"{indent}{element.Name}, {element.Rect.ToString()}, {element.JsonProperties}";
    
            if (!processedElements.Contains(elementIdentifier)) {
                str += $"{elementIdentifier}\n";
                processedElements.Add(elementIdentifier);
            }
    
            if (element.Children.Length > 0) {
                foreach (GameUIElement childElement in element.Children) {
                    ProcessElement(childElement, depth + 1);
                }
            }
        }
        public void Initialize() {
            return;
            if (Onix.Gui.RootUiElement?.Children != null) {
                foreach (GameUIElement gameUiElement in Onix.Gui.RootUiElement.Children) {
                    ProcessElement(gameUiElement, 0);
                }
            }
            Clipboard.SetText(str);
            Console.WriteLine("Copied ui elements to clipboard at time: " + DateTime.Now);
        }

        public bool HandleInput(InputKey key, bool isDown) {
            return _inputHandler.HandleInput(key, isDown);
        }

        public void HandleContainerScreenTick(ContainerScreen container) {
            //Console.WriteLine(container.InventoryLayout);
            ShouldOffsetItems = container.InventoryLayout switch {
                InventoryLayout.Survival or InventoryLayout.None => true,
                InventoryLayout.RecipeBook => false,
                InventoryLayout.Creative => false,
                _ => ShouldOffsetItems
            } || Onix.Gui.ScreenName != "inventory_screen";
            ShouldRenderThings = container.InventoryLayout != InventoryLayout.Creative;
            IsAContainer = container.InventoryLayout != InventoryLayout.UnknownNotRelevantToThisScreen && container.InventoryLayout != InventoryLayout.Creative;
            HeldItem = container.GetItem("cursor_items", 0);
        }

        public void RenderSearch(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi) {
            _renderingEngine.RenderSearch(gfx, delta, screenName, isHudHidden, isClientUi);
        }

        public HashSet<Rect> GetMatchingSearchSlots() {
            return _renderingEngine.GetMatchingSlots();
        }

        public void AddToSearchHistory(string searchText) {
            if (string.IsNullOrWhiteSpace(searchText)) return;
            
            SearchHistory.Remove(searchText);
            
            SearchHistory.Add(searchText);
            
            if (SearchHistory.Count > MaxHistorySize) {
                SearchHistory.RemoveAt(0);
            }
            
            HistoryPosition = -1;
        }

        public void NavigateHistoryUp() {
            if (SearchHistory.Count == 0) return;
            
            if (HistoryPosition == -1) {
                HistoryPosition = SearchHistory.Count - 1;
            } else if (HistoryPosition > 0) {
                HistoryPosition--;
            }
            
            Textbox.Text = SearchHistory[HistoryPosition];
        }

        public void NavigateHistoryDown() {
            if (SearchHistory.Count == 0 || HistoryPosition == -1) return;
            
            if (HistoryPosition < SearchHistory.Count - 1) {
                HistoryPosition++;
                Textbox.Text = SearchHistory[HistoryPosition];
            } else {
                HistoryPosition = -1;
                Textbox.Text = "";
            }
        }
    }
}
