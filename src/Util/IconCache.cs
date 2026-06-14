using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CasualtiesUnknown.Stats.Util
{
    /// <summary>按 itemId / blockKey 懒加载 Sprite 并缓存；blockKey 前缀 b: 走 tiles[id]，e: 走 Resources.Load 建筑 prefab。</summary>
    internal static class IconCache
    {
        private static readonly Dictionary<string, Sprite> _itemSprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite> _blockSprites = new Dictionary<string, Sprite>();

        internal static Sprite GetItemSprite(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            if (_itemSprites.TryGetValue(itemId, out var s)) return s;
            s = LoadPrefabSprite(itemId);
            _itemSprites[itemId] = s;
            return s;
        }

        internal static Sprite GetBlockSprite(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (_blockSprites.TryGetValue(key, out var s)) return s;
            s = LoadBlockKeySprite(key);
            _blockSprites[key] = s;
            return s;
        }

        /// <summary>把 Sprite 在 atlas 中的子区域映射到 IMGUI Rect 上绘制。</summary>
        internal static void Draw(Rect rect, Sprite s)
        {
            if (s == null) return;
            var tex = s.texture;
            if (tex == null) return;
            var tr = s.textureRect;
            float w = tex.width, h = tex.height;
            if (w <= 0f || h <= 0f) return;
            GUI.DrawTextureWithTexCoords(rect, tex, new Rect(tr.x / w, tr.y / h, tr.width / w, tr.height / h));
        }

        private static Sprite LoadPrefabSprite(string id)
        {
            try
            {
                var prefab = Resources.Load<GameObject>(id);
                if (prefab == null) return null;
                var sr = prefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = prefab.GetComponentInChildren<SpriteRenderer>(includeInactive: true);
                return sr != null ? sr.sprite : null;
            }
            catch { return null; }
        }

        private static Sprite LoadBlockKeySprite(string key)
        {
            if (key.Length > 2 && key[1] == ':')
            {
                string body = key.Substring(2);
                if (key[0] == 'e') return LoadPrefabSprite(body);
                if (key[0] == 'b' && ushort.TryParse(body, out var bid)) return LoadTileSprite(bid);
            }
            if (ushort.TryParse(key, out var legacyBid)) return LoadTileSprite(legacyBid);
            return null;
        }

        private static Sprite LoadTileSprite(ushort bid)
        {
            try
            {
                var w = WorldGeneration.world;
                if (w == null || w.tiles == null || bid >= w.tiles.Length) return null;
                var tile = w.tiles[bid];
                if (tile == null) return null;
                if (tile is Tile t && t.sprite != null) return t.sprite;
                var data = default(TileData);
                tile.GetTileData(Vector3Int.zero, null, ref data);
                return data.sprite;
            }
            catch { return null; }
        }
    }
}
