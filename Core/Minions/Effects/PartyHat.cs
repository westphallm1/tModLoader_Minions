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
using AmuletOfManyMinions.Projectiles.Minions.PricklyPear;
using AmuletOfManyMinions.Projectiles.Minions.Rats;
using AmuletOfManyMinions.Projectiles.Minions.Slimecart;
using AmuletOfManyMinions.Projectiles.Minions.TumbleSheep;
using AmuletOfManyMinions.Projectiles.Squires.AdamantiteSquire;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.CrimsonSquire;
using AmuletOfManyMinions.Projectiles.Squires.DemonSquire;
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
using static Terraria.ModLoader.ModContent;

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

		public PartyHatConfig(Vector2 offset, float scale = 1f)
		{
			this.offset = offset;
			rotation = 0;
			this.scale = scale;
			spriteDirection = 1;
			doRotate = true;
			doSpriteDir = true;
		}
	}

	[Autoload(true, Side = ModSide.Client)]
	public class PartyHatSystem: GlobalProjectile
	{
		public static Dictionary<int, PartyHatConfig> ManualHats;
		public static Dictionary<int, PartyHatConfig> PostDrawHats;

		// Whenever there is a party, or on August 19th (mod's birthday)
		public static bool IsParty => BirthdayParty.PartyIsUp || DateTime.Now.Month == 8 && DateTime.Now.Day == 19;

		public override void SetStaticDefaults()
		{
			// load all configurations in a single file to make backporting to 1.3 easier (ugh)
			PostDrawHats = new Dictionary<int, PartyHatConfig>
			{
				/////////////
				// minions //
				/////////////
				[ProjectileType<MeteorFistHead>()] = new PartyHatConfig(new Vector2(-2, -18)) { spriteDirection = -1 },
				[ProjectileType<BombBuddyMinion>()] = new PartyHatConfig(new Vector2(-4, -14)),
				[ProjectileType<PricklyPearMinion>()] = new PartyHatConfig(new Vector2(6, -6), 0.75f),
				[ProjectileType<SlimecartMinion>()] = new PartyHatConfig(new Vector2(-2, -28)) { doRotate = false },
				[ProjectileType<TumbleSheepMinion>()] = new PartyHatConfig(new Vector2(8, -13)),
				[ProjectileType<RatsMinion>()] = new PartyHatConfig(new Vector2(4, -4), 0.5f),
				// doesn't flip appropriately on sprite direction change for some reason
				// PostDrawHats[ModContent.ProjectileType<BalloonMonkeyMinion>()] = new PartyHatConfig(new Vector2(4, -20));
				[ProjectileType<BeeQueenMinion>()] = new PartyHatConfig(new Vector2(-4, -22)) { spriteDirection = -1 },
				[ProjectileType<ExciteSkullMinion>()] = new PartyHatConfig(new Vector2(-2, -28)) { doRotate = false },
				[ProjectileType<GoblinGunnerMinion>()] = new PartyHatConfig(new Vector2(-4, -20)) { spriteDirection = -1 },
				[ProjectileType<CrystalFistHeadMinion>()] = new PartyHatConfig(new Vector2(-2, -26)) { spriteDirection = -1 },
				[ProjectileType<NecromancerMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<GoblinTechnomancerMinion>()] = new PartyHatConfig(new Vector2(-2, -20)),
				[ProjectileType<EclipseHeraldMinion>()] = new PartyHatConfig(new Vector2(8, -24))
			};
			ManualHats = new Dictionary<int, PartyHatConfig>
			{
				// manually draw a hat on each head
				[ProjectileType<CharredChimeraMinionHead>()] = new PartyHatConfig(new Vector2(-4, -16)),

				/////////////
				// squires //
				/////////////
				[ProjectileType<MushroomSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16)),
				[ProjectileType<VikingSquireMinion>()] = new PartyHatConfig(new Vector2(-2, -22)),
				[ProjectileType<SeaSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -20)),
				[ProjectileType<AncientCobaltSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<PumpkinSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<SkywareSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16)),
				[ProjectileType<GuideSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -20)),
				[ProjectileType<GuideVoodooSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -20)),
				[ProjectileType<ShadowSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<CrimsonSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<BoneSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<DemonSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<AdamantiteSquireMinion>()] = new PartyHatConfig(new Vector2(0, -18)),
				[ProjectileType<TitaniumSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<SqueyereMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				[ProjectileType<ArmoredBoneSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18)),
				// glitchy, maybe debug later
				// ManualHats[ModContent.ProjectileType<GoldenRogueSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -18));
				[ProjectileType<StardustSquireMinion>()] = new PartyHatConfig(new Vector2(-4, -16))
			};
		}

		public override void Unload()
		{
			ManualHats = null;
			PostDrawHats = null;
		}

		public static void DrawHat(Projectile projectile, PartyHatConfig config, Color lightColor)
		{
			Main.instance.LoadItem(ItemID.PartyHat);
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
