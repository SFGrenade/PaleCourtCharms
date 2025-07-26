using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PaleCourtCharms
{
    internal static class EmbeddedSprites
    {
        private static readonly Dictionary<string, Sprite> cache = new();

        public static Sprite Get(string key, float ppu = 100f)
        {
            if (cache.TryGetValue(key, out var s)) return s;

            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream($"{asm.GetName().Name}.assets.{key}.png");

            if (stream == null)
                throw new FileNotFoundException($"Sprite resource not found: {key}.png");
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, data, true);
            tex.filterMode = FilterMode.Bilinear;
            s = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                ppu);
            cache[key] = s;
            return s;
        }
    }
}