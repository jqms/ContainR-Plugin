using System.Runtime.InteropServices;
using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Utils;
using OnixRuntime.Plugin;
using ContainR.ContainerSearch;

namespace ContainR {
    public class ContainR : OnixPluginBase {
        private bool _hasLoaded;
        private bool _firstJoin;
        
        private readonly UiRenderer _uiRenderer = new();
        private readonly ShulkerBox.UiRender _shulkerBoxRenderer = new();
        private readonly ContainerSearch.ContainerSearch _containerSearch = new();
        
        public static ContainR Instance { get; private set; } = null!;
        
        readonly ContainerManager _containerManager = new();
        public ContainerManager ContainerManager => _containerManager;
        public static ContainerManager StaticContainerManager => Instance.ContainerManager;
        public static UiRenderer StaticUiRenderer => Instance._uiRenderer;
        public static ShulkerBox.UiRender StaticShulkerBoxRenderer => Instance._shulkerBoxRenderer;
        public static ContainerSearch.ContainerSearch StaticContainerSearch => Instance._containerSearch;

        public ContainR(OnixPluginInitInfo initInfo) : base(initInfo) {
            Instance = this;
        }

        protected override void OnLoaded() {
            Onix.Events.Rendering.PreRenderScreenGame += OnPreRenderScreen;
            Onix.Events.Rendering.RenderScreenGame += OnRenderScreen;
            Onix.Events.Input.Input += OnInput;
            Onix.Events.Gui.ContainerScreenTick += OnContainerScreenTick;
            Onix.Events.Gui.ContainerScreenTick += _shulkerBoxRenderer.HandleContainerScreenTick;
            Onix.Events.Session.SessionLeft += OnSessionLeft;
            Onix.Events.Session.SessionJoined += OnSessionJoined;
            
            _containerSearch.Initialize();
            
            _hasLoaded = false;
            _firstJoin = false;
        }
        
        private void OnRenderScreen(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi) {
            if (Onix.LocalPlayer is null) return;

            _containerSearch.RenderSearch(gfx, delta, screenName, isHudHidden, isClientUi);

            //_shulkerBoxRenderer.HandleShulkerHover(gfx, delta); // its in the base client now so this is useless, kept the code here for people to reference if they want slot stuff.
        }

        private void OnSessionLeft() {
            _hasLoaded = false;
            _uiRenderer.HasCachedTextures = false;
            _firstJoin = false;
        }
        
        private void OnSessionJoined() {
            _hasLoaded = false;
            _uiRenderer.HasCachedTextures = false;
            _firstJoin = true;
        }

        private bool OnInput(InputKey key, bool isDown) {
            return _containerSearch.HandleInput(key, isDown) || InputHandler.OnInput(key, isDown);
        }
        
        private void OnContainerScreenTick(ContainerScreen container) {
            UiState.LayoutMode = container.InventoryLayout;
            UiState.CurrentHoveredSlot = container.HoveredSlot;
            ContainerManager.HandleContainerScreenTick(container);
            
            _containerSearch.HandleContainerScreenTick(container);
        }

        private void OnPreRenderScreen(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi) {
            _uiRenderer.CacheTexturesIfNeeded(gfx);
            
            // these checks may look useless, but i need them since im killing the screen_background texture
            // which gets reloaded every game relog, so all this is to really make sure it gets killed
            // right after you join the world, not before it
            if (Onix.Gui.ScreenName == "modal_progress_screen" || Onix.Gui.ScreenName == "invalid_screen" || Onix.Gui.ScreenName == "world_loading_progress_screen" || (Onix.Gui.ScreenName == "hud_screen" && _firstJoin)) {
                _hasLoaded = false;
                _firstJoin = false;
                _uiRenderer.HasCachedTextures = false;
            }
            if (!_hasLoaded && Onix.Gui.ScreenName != "modal_progress_screen" && Onix.Gui.ScreenName != "invalid_screen" || Onix.Gui.ScreenName == "world_loading_progress_screen") {
                TexturePath screenBackground = TexturePath.Game("textures/ui/screen_background");
                RawImageData image = RawImageData.Create(1, 1);
                image.SetPixel(0, 0, new ColorF(0, 0, 0, 0));
                gfx.UploadTexture(screenBackground, image);

                _hasLoaded = true;
            }

            if (Onix.LocalPlayer is null || Onix.Gui.ScreenName == "modal_progress_screen" || Onix.Gui.ScreenName == "invalid_screen" || Onix.Gui.ScreenName == "world_loading_progress_screen") return;
            
            if (Onix.Gui.ScreenName != "hud_screen" && Onix.Gui.ScreenName != "chat_screen") {
                _uiRenderer.RenderDarkBackground(gfx);
            }
            
            if (ContainerHelper.IsContainerSupported(screenName)) {
                _uiRenderer.RenderContainerUi(gfx, screenName);
            }
            
            if (Onix.Gui.ScreenName == "inventory_screen") {
                _uiRenderer.RenderDeleteUi(gfx, screenName);
            }
        }

        protected override void OnUnloaded() {
            Onix.Events.Rendering.PreRenderScreenGame -= OnPreRenderScreen;
            Onix.Events.Rendering.RenderScreenGame -= OnRenderScreen;
            Onix.Events.Input.Input -= OnInput;
            Onix.Events.Gui.ContainerScreenTick -= OnContainerScreenTick;
        }
    }
}