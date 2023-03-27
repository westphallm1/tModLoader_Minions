using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.CrossModClient.SummonersShine
{
	internal class Bubble : ILoadable
	{

		public static Texture2D SummonersShineThoughtBubble;
		public void Load(Mod mod)
		{
			SummonersShineThoughtBubble = ModContent.Request<Texture2D>("AmuletOfManyMinions/ThoughtBubble", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		}
		public void Unload()
		{
			SummonersShineThoughtBubble = null;
		}
		public static Tuple<Texture2D, Rectangle> SummonersShine_GetEnergyThoughtTexture(int itemID, int frame)
		{
			int offsetNum;
			if (itemID == ModContent.ItemType<AbigailMinionItem>())
			{
				offsetNum = 21;
			}
			else
			{
				return null;
			}

			Texture2D value = SummonersShineThoughtBubble;
			Rectangle rectangle = value.Frame(6, 46, frame, offsetNum, 0, 0);
			return new Tuple<Texture2D, Rectangle>(value, rectangle);
		}
	}
}
