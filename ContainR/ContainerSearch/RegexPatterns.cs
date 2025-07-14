using System.Text.RegularExpressions;

namespace ContainR.ContainerSearch {
    public static partial class RegexPatterns {
        [GeneratedRegex(@"#hover_text"":""([^""]+)""")]
        public static partial Regex HoverTextRegex();
        
        [GeneratedRegex(@"#collection_name"":""([^""]+)""")]
        public static partial Regex CollectionNameRegex();
        
        [GeneratedRegex(@"#collection_index"":(\d+)")]
        public static partial Regex CollectionIndexRegex();
        
        [GeneratedRegex(@"§.")]
        public static partial Regex MinecraftFormattingRegex();
    }
}
