using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core
{
	/**
	 * Giant static class that stores references to every texture
	 * used by the mod. Textures are loaded/unloaded by the class that uses them
	 */
	internal class TextureCache : ModSystem
	{
		internal static Dictionary<int, List<Asset<Texture2D>>> ExtraTextures;

		public override void Load()
		{
			base.Load();
			ExtraTextures = new Dictionary<int, List<Asset<Texture2D>>>();
		}

		public override void Unload()
		{
			// Hopefully this GC's everything
			ExtraTextures = null;
		}

	}
}
