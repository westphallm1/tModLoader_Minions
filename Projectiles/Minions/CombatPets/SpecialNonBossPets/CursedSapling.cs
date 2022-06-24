using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Items.Armor;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class CursedSaplingMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CursedSaplingMinion>() };
		public override string VanillaBuffName => "CursedSapling";
		public override int VanillaBuffId => BuffID.CursedSapling;
	}

	public class CursedSaplingMinionItem : CombatPetMinionItem<CursedSaplingMinionBuff, CursedSaplingMinion>
	{
		internal override string VanillaItemName => "CursedSapling";
		internal override int VanillaItemID => ItemID.CursedSapling;
	}

	public class CursedSaplingBranchProjectile : ModProjectile
	{
		int TimeToLive = 60;

		int frame = -1;
		SpriteEffects effects;
		float maxScale;
		float scale;
		Vector2 growthDirection;
		internal static int SpikeMaxLength = 200;

		int animationFrame => TimeToLive - Projectile.timeLeft;


		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.timeLeft = TimeToLive;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if(frame == -1)
			{
				frame = Main.rand.Next(4);
				effects = Main.rand.NextBool() ? SpriteEffects.FlipVertically : SpriteEffects.None;
				// use initial velocity to transmit basic state in an MP-safe way
				growthDirection = Projectile.velocity;
				maxScale = Projectile.velocity.Length();
				growthDirection.SafeNormalize();
				Projectile.rotation = growthDirection.ToRotation();
				Projectile.velocity = Vector2.Zero;
			}
			scale = maxScale * Math.Min(1, animationFrame / 8f);
		}


		public override void Kill(int timeLeft)
		{
			//
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for(int i = 0; i < scale * SpikeMaxLength; i+= 16) 
			{ 
				if(targetHitbox.Contains((Projectile.Center + growthDirection * i).ToPoint()))
				{
					return true;
				}
			}
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			float brightness = Projectile.timeLeft < 12 ?
				Projectile.timeLeft / 12f : Math.Min(1, animationFrame / 8f);
			Vector2 centerOffset = texture.Width * 0.5f * scale * growthDirection;

			Rectangle bounds = new(0, frame * frameHeight, texture.Width, frameHeight);
			Main.EntitySpriteDraw(texture, Projectile.Center + centerOffset - Main.screenPosition,
				bounds, lightColor * brightness, Projectile.rotation, bounds.GetOrigin(), scale, effects, 0);
			return false;
		}
	}

	public class CursedSaplingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedSapling;
		internal override int BuffId => BuffType<CursedSaplingMinionBuff>();
		internal override int? ProjId => ProjectileType<CursedSaplingBranchProjectile>();
		internal int SpikePlacementTries = 8;
		internal override Vector2 LaunchPos => Projectile.Top;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -8, -32, -1);
			ConfigureFrames(10, (0, 0), (3, 6), (1, 1), (7, 10));
		}

		private (Vector2, Vector2) ChooseSpikeSpawnLocation()
		{
			NPC target = Main.npc[(int)targetNPCIndex];
			float minSpikeScale = 0.67f;
			float maxSearchRange = CursedSaplingBranchProjectile.SpikeMaxLength * minSpikeScale * 0.85f;
			int searchStep = 16;
			Vector2 searchDir = default;
			for(int attempt = 0; attempt < SpikePlacementTries; attempt++)
			{
				searchDir = Main.rand.NextVector2Unit();
				for (int i = 0; i < maxSearchRange; i += searchStep)
				{
					Vector2 current = target.Center + searchDir * i;
					Vector2 next = current + searchDir * searchStep;
					if(!Collision.CanHitLine(current, 1, 1, next, 1, 1))
					{
						// we've found a wall to cling to, return it
						return (next, -searchDir * Main.rand.NextFloat(minSpikeScale, 1f));
					}
				}
			}
			return (target.Center + searchDir * maxSearchRange, -searchDir * Main.rand.NextFloat(minSpikeScale, 1f));
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(targetNPCIndex == null)
			{
				return;
			}
			var (position, direction) = ChooseSpikeSpawnLocation();

			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				position,
				direction,
				(int)ProjId,
				(int)(ModifyProjectileDamage(leveledPetPlayer.PetLevelInfo) * Projectile.damage),
				Projectile.knockBack,
				player.whoAmI,
				ai0: ai0 ?? Projectile.whoAmI);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(gHelper.isFlying)
			{
				Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);
				for(int i = 0; i < 2; i++)
				{
					int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100);
					Main.dust[dustId].position.X -= 2f;
					Main.dust[dustId].position.Y += 2f;
					Main.dust[dustId].scale += Main.rand.NextFloat(0.5f);
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].velocity.Y -= 2f;
				}
			}
		}
	}
}
