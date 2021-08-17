using AmuletOfManyMinions.Projectiles.Squires.MushroomSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	public class PartyHatConfig
	{
		public Vector2 offset;
		public float rotation;
		public float scale;
		public int spriteDirection;
		public bool doRotate;
		public bool doSpriteDir;

		public PartyHatConfig(Vector2 offset)
		{
			this.offset = offset;
			rotation = 0;
			scale = 1;
			spriteDirection = 1;
			doRotate = true;
			doSpriteDir = true;
		}
	}

	public class PartyHatSystem: GlobalProjectile
	{
		public static Dictionary<int, PartyHatConfig> PreDrawHats;
		public static Dictionary<int, PartyHatConfig> ManualHats;
		public static Dictionary<int, PartyHatConfig> PostDrawHats;

		// Whenever there is a party, or on August 19th (mod's birthday)
		public static bool IsParty => true || BirthdayParty.PartyIsUp || DateTime.Now.Month == 8 && DateTime.Now.Day == 19;

		public override void SetStaticDefaults()
		{
			// load all configurations in a single file to make backporting to 1.3 easier (ugh)
			Main.instance.LoadItem(ItemID.PartyHat);
			PreDrawHats = new Dictionary<int, PartyHatConfig>();
			ManualHats = new Dictionary<int, PartyHatConfig>();
			PostDrawHats = new Dictionary<int, PartyHatConfig>();

			// squires
			ManualHats[ModContent.ProjectileType<MushroomSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16));
		}

		public override void Unload()
		{
			PreDrawHats = null;
			ManualHats = null;
			PostDrawHats = null;
		}

		public static void DrawHat(Projectile projectile, PartyHatConfig config, Color lightColor)
		{
			Texture2D hatTexture = Terraria.GameContent.TextureAssets.Item[ItemID.PartyHat].Value;
			float r = projectile.rotation;
			SpriteEffects effects = projectile.spriteDirection * config.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
			Vector2 baseOffset = effects == 0 ? config.offset : new Vector2(-config.offset.X, config.offset.Y);
			Vector2 offset = config.doRotate ? baseOffset.RotatedBy(r) : baseOffset;
			Vector2 position = projectile.Center + offset;
			Main.EntitySpriteDraw(hatTexture, position - Main.screenPosition,
				hatTexture.Bounds, lightColor, r + config.rotation,
				hatTexture.Bounds.Center(), config.scale, effects, 0);
		}

		// 
		public static void DrawManualHat(Projectile projectile, Color lightColor)
		{
			if(!IsParty || !ManualHats.TryGetValue(projectile.type, out PartyHatConfig hatConfig))
			{
				return;
			}
			DrawHat(projectile, hatConfig, lightColor);
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			if(!IsParty || !PreDrawHats.TryGetValue(projectile.type, out PartyHatConfig hatConfig))
			{
				return base.PreDraw(projectile, ref lightColor);
			}
			DrawHat(projectile, hatConfig, lightColor);
			return base.PreDraw(projectile, ref lightColor);
		}

		public override void PostDraw(Projectile projectile, Color lightColor)
		{
			if(!IsParty || !PostDrawHats.TryGetValue(projectile.type, out PartyHatConfig hatConfig))
			{
				return;
			}
			DrawHat(projectile, hatConfig, lightColor);
		}
	}
}
