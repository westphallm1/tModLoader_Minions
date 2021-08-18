using AmuletOfManyMinions.Projectiles.Minions.BeeQueen;
using AmuletOfManyMinions.Projectiles.Minions.BombBuddy;
using AmuletOfManyMinions.Projectiles.Minions.CharredChimera;
using AmuletOfManyMinions.Projectiles.Minions.CrystalFist;
using AmuletOfManyMinions.Projectiles.Minions.EclipseHerald;
using AmuletOfManyMinions.Projectiles.Minions.ExciteSkull;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Minions.GoblinTechnomancer;
using AmuletOfManyMinions.Projectiles.Minions.MeteorFist;
using AmuletOfManyMinions.Projectiles.Minions.Necromancer;
using AmuletOfManyMinions.Projectiles.Minions.PaperSurfer;
using AmuletOfManyMinions.Projectiles.Minions.PricklyPear;
using AmuletOfManyMinions.Projectiles.Minions.Slimecart;
using AmuletOfManyMinions.Projectiles.Minions.TumbleSheep;
using AmuletOfManyMinions.Projectiles.Squires.AdamantiteSquire;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.CrimsonSquire;
using AmuletOfManyMinions.Projectiles.Squires.DemonSquire;
using AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire;
using AmuletOfManyMinions.Projectiles.Squires.GuideSquire;
using AmuletOfManyMinions.Projectiles.Squires.MushroomSquire;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using AmuletOfManyMinions.Projectiles.Squires.ShadowSquire;
using AmuletOfManyMinions.Projectiles.Squires.SkywareSquire;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.StardustSquire;
using AmuletOfManyMinions.Projectiles.Squires.TitaniumSquire;
using AmuletOfManyMinions.Projectiles.Squires.VikingSquire;
using AmuletOfManyMinions.Projectiles.Squires.WoFSquire;
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
		public PartyHatConfig(Vector2 offset, float scale)
		{
			this.offset = offset;
			this.scale = scale;
			rotation = 0;
			spriteDirection = 1;
			doRotate = true;
			doSpriteDir = true;
		}
	}

	public class PartyHatSystem: GlobalProjectile
	{
		public static Dictionary<int, PartyHatConfig> ManualHats;
		public static Dictionary<int, PartyHatConfig> PostDrawHats;


		// Whenever there is a party, or on August 19th (mod's birthday)
		public static bool IsParty => true || BirthdayParty.PartyIsUp || DateTime.Now.Month == 8 && DateTime.Now.Day == 19;


		public static void SetStaticDefaults()
		{
			Main.itemTexture[ItemID.PartyHat] = ModContent.GetTexture("Terraria/Item_" + ItemID.PartyHat);
			// load all configurations in a single file to make backporting to 1.3 easier (ugh)
			ManualHats = new Dictionary<int, PartyHatConfig>();
			PostDrawHats = new Dictionary<int, PartyHatConfig>();

			/////////////
			// minions //
			/////////////
			PostDrawHats[ModContent.ProjectileType<MeteorFistHead>()] = new PartyHatConfig(new Vector2(-2, -18)) { spriteDirection = -1 };
			PostDrawHats[ModContent.ProjectileType<BombBuddyMinion>()] = new PartyHatConfig(new Vector2(-4, -14));
			PostDrawHats[ModContent.ProjectileType<PricklyPearMinion>()] = new PartyHatConfig(new Vector2(6, -6), 0.75f);
			PostDrawHats[ModContent.ProjectileType<SlimecartMinion>()] = new PartyHatConfig(new Vector2(-2, -28)) { doRotate = false };
			PostDrawHats[ModContent.ProjectileType<TumbleSheepMinion>()] = new PartyHatConfig(new Vector2(8, -14));
			// head moves in each frame
			// PostDrawHats[ModContent.ProjectileType<PaperSurferMinion>()] = new PartyHatConfig(new Vector2(2, -16), 0.75f) { spriteDirection = -1 };
			PostDrawHats[ModContent.ProjectileType<BeeQueenMinion>()] = new PartyHatConfig(new Vector2(-4, -22)) { spriteDirection = -1 };
			PostDrawHats[ModContent.ProjectileType<ExciteSkullMinion>()] = new PartyHatConfig(new Vector2(-2, -28)) { doRotate = false };
			PostDrawHats[ModContent.ProjectileType<GoblinGunnerMinion>()] = new PartyHatConfig(new Vector2(-4, -20)) { spriteDirection = -1 };
			PostDrawHats[ModContent.ProjectileType<CrystalFistHeadMinion>()] = new PartyHatConfig(new Vector2(-2, -26)) { spriteDirection = -1 };
			PostDrawHats[ModContent.ProjectileType<NecromancerMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			PostDrawHats[ModContent.ProjectileType<GoblinTechnomancerMinion>()] = new PartyHatConfig(new Vector2(-2, -20));
			PostDrawHats[ModContent.ProjectileType<EclipseHeraldMinion>()] = new PartyHatConfig(new Vector2(8, -24));
			// manuall draw a hat on each head
			ManualHats[ModContent.ProjectileType<CharredChimeraMinionHead>()] = new PartyHatConfig(new Vector2(-4, -16));

			/////////////
			// squires //
			/////////////
			ManualHats[ModContent.ProjectileType<MushroomSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16));
			ManualHats[ModContent.ProjectileType<VikingSquireMinion>()] = new PartyHatConfig(new Vector2(-2, -22));
			ManualHats[ModContent.ProjectileType<SeaSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -20));
			ManualHats[ModContent.ProjectileType<AncientCobaltSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<PumpkinSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<SkywareSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16));
			ManualHats[ModContent.ProjectileType<GuideSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -20));
			ManualHats[ModContent.ProjectileType<GuideVoodooSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -20));
			ManualHats[ModContent.ProjectileType<ShadowSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<CrimsonSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<BoneSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<DemonSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<AdamantiteSquireMinion>()] = new PartyHatConfig(new Vector2(0, -18));
			ManualHats[ModContent.ProjectileType<TitaniumSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<SqueyereMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<ArmoredBoneSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			// glitchy, maybe debug later
			// ManualHats[ModContent.ProjectileType<GoldenRogueSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
			ManualHats[ModContent.ProjectileType<StardustSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16));
		}

		public static void Unload()
		{
			ManualHats = null;
			PostDrawHats = null;
		}

		public static void DrawHat(Projectile projectile, SpriteBatch spriteBatch, PartyHatConfig config, Color lightColor)
		{
			Texture2D hatTexture = Main.itemTexture[ItemID.PartyHat];
			float r = config.doRotate ? projectile.rotation : 0;
			SpriteEffects effects = projectile.spriteDirection * config.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
			Vector2 baseOffset = effects == 0 ? config.offset : new Vector2(-config.offset.X, config.offset.Y);
			Vector2 offset = baseOffset.RotatedBy(r);
			Vector2 position = projectile.Center + offset;
			spriteBatch.Draw(hatTexture, position - Main.screenPosition,
				hatTexture.Bounds, lightColor, r + config.rotation,
				hatTexture.Bounds.Center(), config.scale, effects, 0);
		}

		// 
		public static void DrawManualHat(Projectile projectile,  SpriteBatch spriteBatch, Color lightColor)
		{
			if(!IsParty || !ManualHats.TryGetValue(projectile.type, out PartyHatConfig hatConfig))
			{
				return;
			}
			DrawHat(projectile, spriteBatch, hatConfig, lightColor);
		}

		public override void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			if(!IsParty || !PostDrawHats.TryGetValue(projectile.type, out PartyHatConfig hatConfig))
			{
				return;
			}
			DrawHat(projectile, spriteBatch, hatConfig, lightColor);
		}
	}
}
