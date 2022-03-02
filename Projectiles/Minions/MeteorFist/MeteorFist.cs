using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.MeteorFist
{
	public class MeteorFistMinionBuff : MinionBuff
	{
		public MeteorFistMinionBuff() : base(ProjectileType<MeteorFistMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Meteor Fist");
			Description.SetDefault("A meteor fist will fight for you!");
		}
	}

	public class MeteorFistMinionItem : MinionItem<MeteorFistMinionBuff, MeteorFistMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Meteor Fist");
			Tooltip.SetDefault("Summons a meteor fist to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 12;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 26;
			Item.height = 26;
			Item.value = Item.buyPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Orange;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.MeteoriteBar, 14).AddTile(TileID.Anvils).Register();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);
			var p1 = Projectile.NewProjectileDirect(source, position + new Vector2(5, 0), velocity, Item.shoot, damage, knockback, Main.myPlayer);
			p1.originalDamage = damage;
			var p2 = Projectile.NewProjectileDirect(source, position - new Vector2(5, 0), velocity, Item.shoot, damage, knockback, Main.myPlayer);
			p2.originalDamage = damage;
			return false;
		}
	}


	public class MeteorFistMinion : GroupAwareMinion
	{

		internal override int BuffId => BuffType<MeteorFistMinionBuff>();
		private int framesInAir;
		private float idleAngle;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Meteor Fist");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = false;
			attackState = AttackState.IDLE;
			Projectile.minionSlots = 0.5f;
			attackFrames = 30;
			useBeacon = false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.tileCollide)
			{
				attackState = AttackState.RETURNING;
				Projectile.tileCollide = false;
			}
			return base.OnTileCollide(oldVelocity);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = GetActiveMinions();
			Projectile leader = GetFirstMinion(minions);
			if (Main.myPlayer == player.whoAmI &&
				leader.minionPos == Projectile.minionPos &&
				player.ownedProjectileCounts[ProjectileType<MeteorFistHead>()] == 0)
			{
				Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.position, Vector2.Zero, ProjectileType<MeteorFistHead>(), 0, 0, Main.myPlayer);
			}
			Projectile head = GetHead(ProjectileType<MeteorFistHead>());
			if (head == default)
			{
				// the head got despawned, wait for it to respawn
				return Vector2.Zero;
			}
			Vector2 idlePosition = head.Center;
			int minionCount = minions.Count;
			int order = minions.IndexOf(Projectile);
			idleAngle = (float)(2 * Math.PI * order) / minionCount;
			idleAngle += Projectile.spriteDirection * 2 * (float)Math.PI * groupAnimationFrame / groupAnimationFrames;
			idlePosition.X += 2 + 30 * (float)Math.Sin(idleAngle);
			idlePosition.Y += 2 + 30 * (float)Math.Cos(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			Projectile head = GetHead(ProjectileType<MeteorFistHead>());
			if (head == default)
			{
				// the head got despawned, wait for it to respawn
				return null;
			}
			if (FindTargetInTurnOrder(400f, head.Center) is Vector2 target)
			{
				return target;
			}
			else
			{
				return null;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			framesInAir = Math.Max(framesInAir, 12); // force a return shortly after hitting a target
			Dust.NewDust(Projectile.position, 16, 16, 6, Projectile.velocity.X / 2, Projectile.velocity.Y / 2);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int speed = 16;
			Projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
			if (oldVectorToTarget == null && vectorToTarget is Vector2 target)
			{
				target.Y -= Math.Abs(target.X) / 10; // add a bit of vertical increase to target
				target.SafeNormalize();
				target *= speed;
				framesInAir = 0;
				Projectile.velocity = target;
				Projectile.rotation = (float)(Math.Atan2(Projectile.velocity.Y, Projectile.spriteDirection * Projectile.velocity.X));
			}
			if (framesInAir++ > 15)
			{
				attackState = AttackState.RETURNING;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// attack should continue until ground is hit
			if (attackState == AttackState.ATTACKING)
			{
				TargetedMovement(Vector2.Zero);
				return;
			}
			Projectile.rotation = (float)Math.PI;
			// alway clamp to the idle position
			Projectile.tileCollide = false;
			int inertia = 2;
			int maxSpeed = 20;
			if (vectorToIdlePosition.Length() < 32)
			{
				attackState = AttackState.IDLE;
				Projectile head = GetHead(ProjectileType<MeteorFistHead>());
				if (head != default)
				{
					// the head got despawned, wait for it to respawn
					Projectile.spriteDirection = head.spriteDirection;
				}
				Projectile.position += vectorToIdlePosition;
				Projectile.velocity = Vector2.Zero;
			}
			else
			{
				Vector2 speedChange = vectorToIdlePosition - Projectile.velocity;
				if (speedChange.Length() > maxSpeed)
				{
					speedChange.SafeNormalize();
					speedChange *= maxSpeed;
				}
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + speedChange) / inertia;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Lighting.AddLight(Projectile.position, Color.Red.ToVector3() * 0.75f);
		}
	}
}
