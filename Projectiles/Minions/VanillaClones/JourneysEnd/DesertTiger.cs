using static AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses.CombatPetConvenienceMethods;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Items.Accessories;
using System;
using Terraria.Audio;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using System.Collections.Generic;
using System.Linq;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class DesertTigerMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.StormTiger;

		internal override int[] ProjectileTypes => new int[] { ProjectileType<DesertTigerCounterMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.StormTiger") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.StormTiger"));
		}

	}

	public class DesertTigerMinionItem : VanillaCloneMinionItem<DesertTigerMinionBuff, DesertTigerCounterMinion>
	{
		internal override int VanillaItemID => ItemID.StormTigerStaff;

		internal override string VanillaItemName => "StormTigerStaff";
	}
	
	public class DesertTigerCounterMinion : CounterMinion
	{
		public override int BuffId => BuffType<DesertTigerMinionBuff>();
		protected override int MinionType => ProjectileType<DesertTigerMinion>();
	}

	/// <summary>
	/// Uses ai[0] for search range
	/// </summary>
	public class DesertTigerDashMinion : BumblingTransientMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StormTigerAttack;

		protected override float inertia => 1;
		protected override float idleSpeed => 22;
		protected override int timeToLive => 400;
		internal override bool tileCollide => false;
		protected override float distanceToBumbleBack => 2000f; // don't bumble back

		protected List<NPC> targetList;
		protected Vector2 returnTo;

		private MotionBlurDrawer blurDrawer;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			AttackThroughWalls = true;
			blurDrawer = new MotionBlurDrawer(5);
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			targetList = new();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy() &&
					Vector2.DistanceSquared(Projectile.Center, npc.Center) < Projectile.ai[0] * Projectile.ai[0])
				{
					targetList.Add(npc);
				}
			}
			targetList = targetList.OrderBy(t => Vector2.DistanceSquared(t.Center, Projectile.Center)).ToList();
			maxSpeed = idleSpeed;
		}

		public override Vector2 IdleBehavior()
		{
			if(returnTo == default)
			{
				returnTo = Projectile.Center;
			}
			targetList = targetList.Where(npc => npc.active).ToList();
			if(targetList.Count == 0 && Vector2.DistanceSquared(returnTo, Projectile.Center) < 24 * 24)
			{
				// move the main minion back to wherever the dash attack ended
				int minionType = ProjectileType<DesertTigerMinion>();
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if(p.active && p.owner == Player.whoAmI && p.type == minionType)
					{
						p.Center = Projectile.Center;
						break;
					}
				}
				SpawnDust(4, 1);
				Projectile.Kill();
			}
			return returnTo - Projectile.Center;
		}

		public override Vector2? FindTarget()
		{
			if(targetList.Count > 0)
			{
				TargetNPCIndex = targetList[0].whoAmI;
				return targetList[0].Center - Projectile.Center;
			} else
			{
				return null;
			}
		}

		protected override void Move(Vector2 vector2Target, bool isIdle = false)
		{
			if(targetList.Count > 0)
			{
				base.Move(vector2Target, false);
			} else
			{
				base.Move(VectorToIdle, true);
			}
			if(Main.rand.NextBool())
			{
				SpawnDust(1, 0);
			}
			blurDrawer.Update(Projectile.Center, true);
			Projectile.rotation += MathHelper.TwoPi / 15 *  Math.Sign(Projectile.velocity.X);
		}

		public override void OnHitTarget(NPC target)
		{
			// remove enemy from list after hit
			targetList = targetList.Where(npc => npc.whoAmI != target.whoAmI).ToList();
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
			targetList = targetList.Where(npc => npc.whoAmI != target.whoAmI).ToList();
		}

		public void SpawnDust(int count, float velocityMult)
		{
			for (float i = 0; i < count; i++)
			{
				Vector2 velocity = velocityMult * Projectile.velocity;
				int dustCreated = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 169, velocity.X, velocity.Y);
				Main.dust[dustCreated].noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			Rectangle bounds = new(0, 0, texture.Width, texture.Height);
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Color outlineColor = Color.Gold * 0.5f;
			SpriteEffects effects = 0;
			// motion blur
			blurDrawer.DrawBlur(texture, outlineColor * 0.5f, bounds, Projectile.rotation);
			// glowy outline
			OutlineDrawer.DrawOutline(texture, pos, bounds, outlineColor, Projectile.rotation, effects);
			// main entity
			Main.EntitySpriteDraw(texture, pos,
				bounds, Color.White, Projectile.rotation, bounds.GetOrigin(), 1, effects, 0);

			return false;
		}
	}

	public class DesertTigerMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StormTigerTier1;
		public override int BuffId => BuffType<DesertTigerMinionBuff>();

		private MotionBlurDrawer blurDrawer;

		// Current tangle of class hierarchies doesn't support a ground based empowered minion
		// it's easier to make a ground based minion empowered than it is to make an empowered
		// minion ground based
		private int EmpowerCount { get; set; }

		private Asset<Texture2D> currentTexture;
		private int dashCooldown;
		private int lastDashFrame = -7 * 60;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(this, 30, 30, 0, 0);
			xMaxSpeed = 12;
			idleInertia = 8;
			Projectile.localNPCHitCooldown = 6;
			blurDrawer = new MotionBlurDrawer(5);
		}

		public override void LoadAssets()
		{
			Main.instance.LoadProjectile(ProjectileID.StormTigerTier2);
			Main.instance.LoadProjectile(ProjectileID.StormTigerTier3);
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			UpdateEmpowerCount();

			if(AnimationFrame - lastDashFrame > dashCooldown && AnimationFrame % 10 == 0)
			{
				DashIfEnemiesOnscreen();
			}

			if(Player.ownedProjectileCounts[ProjectileType<DesertTigerDashMinion>()] > 0)
			{
				Projectile.Center = Player.Center;
				Projectile.velocity = Vector2.Zero;
				return Vector2.Zero;
			}

			if(gHelper.isFlying)
			{
				float idleAngle = MathHelper.TwoPi * AnimationFrame / 90f;
				target = Player.Center + 42 * idleAngle.ToRotationVector2() - Projectile.Center;
			}
			return target;
		}

		private void UpdateEmpowerCount()
		{
			int counterType = ProjectileType<DesertTigerCounterMinion>();
			EmpowerCount = Player.ownedProjectileCounts[counterType];
			// update damage to scale based on number of counters
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Player.whoAmI && p.type == counterType)
				{
					Projectile.originalDamage = (int)(p.originalDamage * (0.6f + 0.4f * EmpowerCount));
					break;
				}
			}
			xMaxSpeed = Math.Min(17, 12 + 2 * EmpowerCount / 3);
			maxSpeed = xMaxSpeed + 2;
			dashCooldown = Math.Max(4 * 60, (6 - EmpowerCount / 3) * 60);
			searchDistance = Math.Min(1250, 850 + 50 * EmpowerCount);
			UpdateEmpoweredAppearance();
		}

		private void DashIfEnemiesOnscreen()
		{
			if(Projectile.owner == Main.myPlayer && AnyEnemyInRange(searchDistance, Projectile.Center, true) != null)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ProjectileType<DesertTigerDashMinion>(),
					3 * Projectile.damage / 2,
					Projectile.knockBack,
					Projectile.owner,
					ai0: searchDistance);
				lastDashFrame = AnimationFrame;
			}
		}

		private void UpdateEmpoweredAppearance()
		{
			if (EmpowerCount > 6)
			{
				DrawOriginOffsetY = -4;
				ConfigureFrames(12, (0, 0), (1, 9), (1, 1), (10, 10));
				currentTexture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.StormTigerTier3];
			}
			else if (EmpowerCount > 3)
			{
				DrawOriginOffsetY = -4;
				ConfigureFrames(12, (0, 0), (1, 9), (1, 1), (10, 10));
				currentTexture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.StormTigerTier2];
			} else
			{
				DrawOriginOffsetY = 0;
				ConfigureFrames(10, (0, 0), (1, 7), (4, 4), (8, 8));
				currentTexture = Terraria.GameContent.TextureAssets.Projectile[Type];
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			var animState = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if(animState == GroundAnimationState.FLYING)
			{
				Projectile.rotation = Math.Sign(Projectile.velocity.X) * MathHelper.TwoPi * AnimationFrame / 15;
			} else
			{
				Projectile.rotation = 0;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if(Player.ownedProjectileCounts[ProjectileType<DesertTigerDashMinion>()] > 0)
			{
				return false;
			}
			Texture2D texture = currentTexture.Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			Rectangle bounds = new(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 offset = new(DrawOffsetX, DrawOriginOffsetY);
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// motion blur
			blurDrawer.DrawBlur(texture, lightColor * 0.5f, bounds, Projectile.rotation);

			Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, bounds, lightColor, 
				Projectile.rotation, bounds.GetOrigin(), 1, effects, 0);
			return false;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			DoDefaultGroundedMovement(vector);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			Projectile.friendly &= Player.ownedProjectileCounts[ProjectileType<DesertTigerDashMinion>()] == 0;
			blurDrawer.Update(Projectile.Center, Projectile.velocity.LengthSquared() > 2);
		}
	}
}
