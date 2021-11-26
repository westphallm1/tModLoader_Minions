using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class SlimePrincessMinionBuff : CombatPetVanillaCloneBuff
	{
		public SlimePrincessMinionBuff() : base(ProjectileType<SlimePrincessMinion>()) { }

		public override int VanillaBuffId => BuffID.QueenSlimePet;

		public override string VanillaBuffName => "QueenSlimePet";
	}

	public class SlimePrincessMinionItem : CombatPetMinionItem<SlimePrincessMinionBuff, SlimePrincessMinion>
	{
		internal override int VanillaItemID => ItemID.QueenSlimePetItem;
		internal override int AttackPatternUpdateTier => 4;

		internal override string VanillaItemName => "QueenSlimePetItem";
	}

	public class SlimePrincessHelperSlimeMinion : CombatPetSlimeMinion
	{
		internal override int BuffId => BuffType<SlimePrincessMinionBuff>();
		internal int spriteType = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 12;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 20;
			DrawOffsetX = (Projectile.width - 44) / 2;
			DrawOriginOffsetY = 0;
		}

		public override void OnSpawn()
		{
			spriteType = Main.rand.Next(2);
			SpawnDust();
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = 6 * spriteType + (gHelper.isFlying ? 2 : 0);
			maxFrame = 6 * spriteType + (gHelper.isFlying ? 6 : 2);
			base.Animate(minFrame, maxFrame);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1, effects, 0);
			return false;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(animationFrame > 180)
			{
				Projectile.Kill();
				SpawnDust();
			}
		}

		private void SpawnDust()
		{
			for(int i = 0; i < 2; i++)
			{
				int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkSlime);
			}
		}
	}

	public class SlimePrincessMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenSlimePet;
		internal override int BuffId => BuffType<SlimePrincessMinionBuff>();

		private bool wasFlyingThisFrame = false;

		int lastSpawnedFrame;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 12;
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 30;
			Projectile.height = 30;
			DrawOriginOffsetY = -4;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(!wasFlyingThisFrame && gHelper.isFlying)
			{
				Gore.NewGore(Projectile.Center, Vector2.Zero, GoreID.QueenSlimePetCrown);
			}
			wasFlyingThisFrame = gHelper.isFlying;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(gHelper.isFlying) { base.Animate(6, 12); }
			else if(ShouldBounce) { base.Animate(0, 6); }
			else { Projectile.frame = 0; }

			if(gHelper.isFlying)
			{
				Projectile.rotation = Projectile.velocity.X * 0.05f;
			} else
			{
				Projectile.rotation = 0;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int projType = ProjectileType<SlimePrincessHelperSlimeMinion>();
			Vector2 launchVel = (-8 * Vector2.UnitY).RotatedByRandom(MathHelper.PiOver4);
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] == 0 && 
				animationFrame - lastSpawnedFrame > 240 && leveledPetPlayer.PetLevel >= 4)
			{
				lastSpawnedFrame = animationFrame;
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.Center,
					launchVel,
					projType,
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
			}
		}
	}
}
