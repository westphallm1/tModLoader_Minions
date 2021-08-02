using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Rats
{
	public class RatsMinionBuff : MinionBuff
	{
		public RatsMinionBuff() : base(ProjectileType<RatsMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Aww, Rats!");
			Description.SetDefault("A group of rats will fight for you!");
		}
	}

	public class RatsMinionItem : MinionItem<RatsMinionBuff, RatsMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Rod of the Ratkeeper");
			Tooltip.SetDefault("Summons a hoarde of rats to fight for you!\nEach rat deals 1/3 of base damage,\nand ignores 10 enemy defense");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 6;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 5, 0);
			Item.rare = ItemRarityID.Blue;
		}

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			base.Shoot(player, source, position, velocity, type, damage, knockback);            // summon 3 rats at a time
			for (int i = 0; i < 3; i++)
			{
				Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<RatsMinion>(), damage, knockback, player.whoAmI);
			}
			return false;
		}
	}

	public class RatsMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<RatsMinionBuff>();

		// which of the 3 rats this is, affects some cosmetic behavior
		private int clusterIdx;

		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (8, 8),
			[GroundAnimationState.JUMPING] = (2, 8),
			[GroundAnimationState.STANDING] = (0, 1),
			[GroundAnimationState.WALKING] = (2, 8),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Rat (Friendly)");
			Main.projFrames[Projectile.type] = 9;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			Projectile.damage = (int)Math.Ceiling(Projectile.damage / 3f);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 8;
			Projectile.height = 16;
			Projectile.minionSlots = 0.333f;
			DrawOffsetX = -6;
			DrawOriginOffsetY = -6;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			// this one likes to jump while attacking
			// different rats like to jump different heights
			vector.Y -= 3 * (clusterIdx % 10);
			if(vectorToTarget is null)
			{
				vector.Y -= 16;
			}
			if (vector.Y < 0 && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 7;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8 && Math.Abs(player.velocity.X) > 4)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (animationFrame - lastHitFrame > 15)
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}

		public override Vector2 IdleBehavior()
		{
			List<Projectile> rats = GetActiveMinions();
			Projectile head;
			if(rats.Count == 0)
			{
				clusterIdx = 0;
				head = Projectile;
			} else
			{
				clusterIdx = rats.IndexOf(Projectile);
				head = rats[0];
			}
			gHelper.SetIsOnGround();
			noLOSPursuitTime = gHelper.isFlying ? 15 : 300;
			Vector2 idlePosition = player.Center;
			// every rat should gather around the first rat
			idlePosition.X += -player.direction * (8 + IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingOnGround, head));
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
			}
			idlePosition.X += (12 + rats.Count/3 ) * (float)Math.Sin(2 * Math.PI * ((groupAnimationFrame % 60) / 60f + clusterIdx/(rats.Count + 1)));
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// manually bypass defense
			// this may not be wholly correct
			int defenseBypass = 10;
			int defense = Math.Min(target.defense, defenseBypass);
			damage += defense / 2;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
			// add poison
			if(Main.rand.Next(0, 10) == 0)
			{
				target.AddBuff(BuffID.Poisoned, 300);
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
		}
	}
}
