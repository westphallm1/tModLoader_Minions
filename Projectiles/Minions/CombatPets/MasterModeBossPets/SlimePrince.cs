using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class SlimePrinceMinionBuff : CombatPetVanillaCloneBuff
	{
		public SlimePrinceMinionBuff() : base(ProjectileType<SlimePrinceMinion>()) { }

		public override int VanillaBuffId => BuffID.KingSlimePet;

		public override string VanillaBuffName => "KingSlimePet";
	}

	public class SlimePrinceMinionItem : CombatPetMinionItem<SlimePrinceMinionBuff, SlimePrinceMinion>
	{
		internal override int VanillaItemID => ItemID.KingSlimePetItem;
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Skeletal;
		internal override string VanillaItemName => "KingSlimePetItem";
	}

	public class SlimePrinceNinjaMinion : CombatPetGroundedMeleeMinion
	{
		internal override int BuffId => BuffType<SlimePrinceMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 11;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 26;
			DrawOffsetX = -2;
			DrawOriginOffsetY = -14;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.STANDING] = (0, 0),
				// other fields set dynamically
			};
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(!gHelper.isFlying && vectorToTarget is Vector2 target && target.Length() < 48)
			{
				frameInfo[GroundAnimationState.WALKING] = (4, 7);
				frameInfo[GroundAnimationState.JUMPING] = (7, 10);
				frameInfo[GroundAnimationState.FLYING] = (7, 10);
			} else
			{
				frameInfo[GroundAnimationState.WALKING] = (0, 4);
				frameInfo[GroundAnimationState.JUMPING] = (10, 10);
				frameInfo[GroundAnimationState.FLYING] = (10, 10);
			}
			base.Animate(minFrame, maxFrame);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(animationFrame > 180)
			{
				Projectile.Kill();
			}
		}

		public override void Kill(int timeLeft)
		{
			float goreVel = 0.25f;
			var source = Projectile.GetSource_Death();
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				if(Main.rand.Next(3) > 0)
				{
					continue;
				}
				int goreIdx = Gore.NewGore(source, Projectile.position, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += offset;
			}
			base.Kill(timeLeft);
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			for(int i = 0; i < 2; i++)
			{
				int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Slime);
				Main.dust[idx].color = Color.LightBlue * 0.75f;
			}
		}
	}

	public class SlimePrinceMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.KingSlimePet;
		internal override int BuffId => BuffType<SlimePrinceMinionBuff>();

		private bool wasFlyingThisFrame = false;

		int lastSpawnedFrame;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.KingSlimePet") + " (AoMM Version)");
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
				var source = Projectile.GetSource_FromThis();
				Gore.NewGore(source, Projectile.Center, Vector2.Zero, GoreID.KingSlimePetCrown);
			}
			wasFlyingThisFrame = gHelper.isFlying;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(gHelper.isFlying) { base.Animate(6, 12); }
			else if(ShouldBounce) { base.Animate(0, 6); }
			else { Projectile.frame = 0; }

			if(gHelper.isFlying && Projectile.velocity.LengthSquared() > 2)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else
			{
				Projectile.rotation = 0;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int projType = ProjectileType<SlimePrinceNinjaMinion>();
			Vector2 launchVel = (-8 * Vector2.UnitY).RotatedByRandom(MathHelper.PiOver4);
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] == 0 &&
				animationFrame - lastSpawnedFrame > 240 && leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal)
			{
				lastSpawnedFrame = animationFrame;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
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
