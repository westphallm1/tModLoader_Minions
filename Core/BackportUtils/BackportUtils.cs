using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.BackportUtils
{
	public abstract class BackportModProjectile : ModProjectile
	{
		public Projectile Projectile { get => projectile; }

		public int DrawOffsetX { get => drawOffsetX; set => drawOffsetX = value; }
		public int DrawOriginOffsetY { get => drawOriginOffsetY; set => drawOriginOffsetY = value; }


		// no choice but a loose approximation here
		public void SetOriginalDamage(int damage)
		{
			Player owner = Main.player[Projectile.owner];
			Projectile.damage = (int)(damage * (owner.minionDamageMult + owner.minionDamage - 1f));
		}
	}

	public abstract class BackportModItem : ModItem
	{
		public Item Item { get => item; }


		public RecipeChain CreateRecipe(int resultCount)
		{
			RecipeChain chain = new RecipeChain(this, resultCount);
			return chain;
		}
	}

	public abstract class BackportModPlayer: ModPlayer
	{
		public Player Player { get => player; }
	}


	public abstract class BackportModBuff : ModBuff
	{
		public virtual string Texture => "";
		public override bool Autoload(ref string name, ref string texture)
		{
			if(Texture != "")
			{
				texture = Texture;
			}
			return base.Autoload(ref name, ref texture);
		}

		public virtual void SetStaticDefaults()
		{
			// no-op
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			SetStaticDefaults();
		}
	} 


	public class SoundEngine
	{
		public static void PlaySound(LegacySoundStyle type, Vector2 position)
		{
			Main.PlaySound(type, position);
		}

		public static void PlaySound(int type, int x, int y, int style, float volumeScale, float pitchOffset)
		{
			Main.PlaySound(type, x, y, style, volumeScale, pitchOffset);
		}
	}

	public class RecipeChain
	{
		internal ModRecipe Recipe;

		public RecipeChain(ModItem result, int stack)
		{
			Recipe = new ModRecipe(result.mod);
			Recipe.SetResult(result, stack);
		}

		public RecipeChain AddIngredient(int itemId, int stack)
		{
			Recipe.AddIngredient(itemId, stack);
			return this;
		}

		public RecipeChain AddRecipeGroup(string groupName, int stack)
		{
			Recipe.AddRecipeGroup(groupName, stack);
			return this;
		}

		public RecipeChain AddTile(int tileId)
		{
			Recipe.AddTile(tileId);
			return this;
		}

		public void Register()
		{
			Recipe.AddRecipe();
		}
	}
}
