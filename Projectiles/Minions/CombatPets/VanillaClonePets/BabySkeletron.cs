using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabySkeletronHeadMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabySkeletronHeadMinionBuff() : base(ProjectileType<BabySkeletronHeadMinion>()) { }
		public override string VanillaBuffName => "BabySkeletronHead";
		public override int VanillaBuffId => BuffID.BabySkeletronHead;
	}

	public class BabySkeletronHeadMinionItem : CombatPetMinionItem<BabySkeletronHeadMinionBuff, BabySkeletronHeadMinion>
	{
		internal override string VanillaItemName => "BoneKey";
		internal override int VanillaItemID => ItemID.BoneKey;
	}

	public class BabySkeletronHeadMinion : CombatPetHoverDasherMinion
	{
		internal override int BuffId => BuffType<BabySkeletronHeadMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabySkeletronHead;

		internal override bool DoBumblingMovement => true;

		internal static int FallDuration = 90;
		internal int fallStartFrame = -FallDuration;
		internal bool IsFalling => animationFrame - fallStartFrame < FallDuration;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			attackThroughWalls = true;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(IsFalling)
			{
				return;
			} else if(vectorToTarget is null)
			{
				Projectile.rotation = 0.05f * Projectile.velocity.X;
			} else
			{
				Projectile.rotation += MathHelper.TwoPi / 15 * Math.Sign(Projectile.velocity.X);
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			Projectile.friendly &= !IsFalling;
		}

		public override void OnHitTarget(NPC target)
		{
			// start falling, approximately
			if(player.whoAmI != Main.myPlayer)
			{
				StartFalling();
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			StartFalling();
		}
		private void StartFalling()
		{
			fallStartFrame = animationFrame;
			float xVel = -4 * Math.Sign(Projectile.velocity.X);
			float yVel = -4;
			Projectile.velocity = new Vector2(xVel, yVel);
		}

		private void DoFallingMovement()
		{
			Projectile.velocity.X *= 0.9f;
			if(Projectile.velocity.Y < 16)
			{
				Projectile.velocity.Y += 0.5f;
			}
			if(animationFrame - fallStartFrame >= FallDuration - 1 || Projectile.Center.X  - player.Center.X > 600)
			{
				// go somewhere off screen before we start falling, synced MP
				Vector2 spawnOffset = default;
				if(Main.rand.NextBool())
				{
					spawnOffset.X = Math.Sign(Main.rand.NextFloat() - 0.5f) * (32 + Main.screenWidth / 2);
					spawnOffset.Y = Main.rand.Next(Main.screenHeight) - Main.screenHeight/2;
				} else
				{
					spawnOffset.Y = Math.Sign(Main.rand.NextFloat() - 0.5f) * (32 + Main.screenHeight/ 2);
					spawnOffset.X = Main.rand.Next(Main.screenWidth) - Main.screenWidth/2;
				}
				fallStartFrame = animationFrame - FallDuration - 1;
				Projectile.position = player.Center + spawnOffset;
				Projectile.velocity = Vector2.Zero;
				if(player.whoAmI == Main.myPlayer)
				{
					Projectile.netUpdate = true; // lazy networking implementation here
				}
			}
		}


		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			 // lots more knockback and damage, but applied after initial damage reduction from defense
			knockback += 5;
			damage *= 8;
			base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(IsFalling)
			{
				DoFallingMovement();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(IsFalling)
			{
				DoFallingMovement();
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor = new Color(
				Math.Max(lightColor.R, (byte)25),
				Math.Max(lightColor.G, (byte)25),
				Math.Max(lightColor.B, (byte)25));
			return base.PreDraw(ref lightColor);
		}

		public override Vector2? FindTarget()
		{
			float searchRange = targetSearchDistance;
			if (PlayerTargetPosition(searchRange, player.Center, 0.67f * searchRange, losCenter: player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchRange, 0.67f * searchRange, losCenter: player.Center) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}

	}
}
