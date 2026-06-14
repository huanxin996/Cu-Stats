namespace CasualtiesUnknown.Stats.Util
{
    /// <summary>统计列表 id → 本地化显示名的解析；解不出回落原 id。Block 键前缀 b: 走 BlockInfo.name，e: 走 Locale.GetBuilding。</summary>
    internal static class NameResolver
    {
        internal static string Item(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            try { var n = Locale.GetItem(id); return string.IsNullOrEmpty(n) ? id : n; }
            catch { return id; }
        }

        internal static string Block(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            if (key.Length > 2 && key[1] == ':')
            {
                string body = key.Substring(2);
                if (key[0] == 'e')
                {
                    try { var n = Locale.GetBuilding(body); return string.IsNullOrEmpty(n) ? body : n; }
                    catch { return body; }
                }
                if (key[0] == 'b' && ushort.TryParse(body, out var bid))
                {
                    return BlockNameById(bid, body);
                }
            }
            if (ushort.TryParse(key, out var legacyBid)) return BlockNameById(legacyBid, key);
            return key;
        }

        private static string BlockNameById(ushort bid, string fallback)
        {
            try
            {
                var w = WorldGeneration.world;
                if (w == null) return fallback;
                var info = w.GetBlockInfo(bid);
                return info != null && !string.IsNullOrEmpty(info.name) ? info.name : ("#" + bid);
            }
            catch { return "#" + bid; }
        }
    }
}
