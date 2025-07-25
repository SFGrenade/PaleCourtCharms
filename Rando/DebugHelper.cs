using System.Linq;
using ItemChanger;
using ItemChanger.Tags;
using UnityEngine;

namespace PaleCourtCharms
{
    internal static class ChainDebugHelper
    {
        private static float timer = 0f;
        private const float interval = 2f;

        public static void OnHeroUpdate()
        {
            timer += Time.deltaTime;
            if (timer < interval) return;
            timer = 0f;

            var crest = Finder.GetItemInternal(ItemChanger.ItemNames.Defenders_Crest);
            if (crest == null)
            {
                Modding.Logger.Log("[PC_Charms][ChainDebug] Crest not found.");
                return;
            }

            if (crest.tags == null || crest.tags.Count == 0)
            {
                Modding.Logger.Log("[PC_Charms][ChainDebug] Crest has no tags.");
                return;
            }

            var chainTags = crest.tags.OfType<ItemChainTag>().ToList();
            if (chainTags.Count == 0)
            {
                Modding.Logger.Log("[PC_Charms][ChainDebug] Crest has tags, but no chain tags.");
            }
            else
            {
                foreach (var tag in chainTags)
                {
                    Modding.Logger.Log($"[PC_Charms][ChainDebug] Crest chain tag → successor: {tag.successor}, pred: {tag.predecessor ?? "null"}");
                }
            }
        }
    }
}
