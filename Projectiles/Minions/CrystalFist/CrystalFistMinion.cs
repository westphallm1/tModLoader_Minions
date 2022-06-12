using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CrystalFist
{
	public class CrystalFistMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CrystalFistMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Fist");
			Description.SetDefault("A crystal fist will fight for you!");
		}
	}

	public class CrystalFistMinionItem : MinionItem<CrystalFistMinionBuff, CrystalFistMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Fist Staff");
			Tooltip.SetDefault("Summons a crystal fist to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 42;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 16;
			Item.height = 16;
			Item.value = Item.buyPrice(0, 12, 0, 0);
			Item.rare = ItemRarityID.Pink;
		}
		public override void ApplyCrossModChanges()
		{
			CrossMod.WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, CrossMod.SummonersShineDefaultSpecialWhitelistType.MELEE);
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

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.SoulofMight, 10).AddIngredient(ItemID.CrystalShard, 10).AddTile(TileID.MythrilAnvil).Register();
		}
	}


	public class CrystalFistMinion : GroupAwareMinion
	{

		internal override int BuffId => BuffType<CrystalFistMinionBuff>();
		private int framesInAir;
		private float idleAngle;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crystal Fist");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 20;
			Projectile.tileCollide = false;
			attackState = AttackState.IDLE;
			Projectile.minionSlots = 0.5f;
			attackThroughWalls = false;
			useBeacon = false;
			attackFrames = 30;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.tileCollide)
			{
				attackState = AttackState.RETURNING;
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
				player.ownedProjectileCounts[ProjectileType<CrystalFistHeadMinion>()] == 0)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, ProjectileType<CrystalFistHeadMinion>(), 0, 0, Main.myPlayer);
			}
			Projectile head = GetHead(ProjectileType<CrystalFistHeadMinion>());
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
			idlePosition.X += 2 + 45 * (float)Math.Sin(idleAngle);
			idlePosition.Y += 2 + 45 * (float)Math.Cos(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			Projectile head = GetHead(ProjectileType<CrystalFistHeadMinion>());
			if (head == default)
			{
				// the head got despawned, wait for it to respawn
				return null;
			}
			if (FindTargetInTurnOrder(600f, head.Center) is Vector2 target)
			{
				return target;
			}
			else
			{
				return null;
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesInAir = Math.Max(framesInAir, 12); // force a return shortly after hitting a target
			Dust.NewDust(Projectile.position, 16, 16, DustID.PinkCrystalShard, Projectile.velocity.X / 2, Projectile.velocity.Y / 2);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int speed = 16;
			if (oldVectorToTarget == null && vectorToTarget is Vector2 target)
			{
				target.Y -= Math.Abs(target.X) / 10; // add a bit of vertical increase to target
				target.SafeNormalize();
				target *= speed;
				framesInAir = 0;
				Projectile.velocity = target;
			}
			Projectile.spriteDirection = 1;
			Projectile.rotation = (float)(Math.PI + Projectile.velocity.ToRotation());
			if (framesInAir++ > 15)
			{
				attackState = AttackState.RETURNING;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// attack should continue until attack timer is up
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
				Projectile head = GetHead(ProjectileType<CrystalFistHeadMinion>());
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
			Lighting.AddLight(Projectile.position, Color.Pink.ToVector3() * 0.75f);
		}
	}
}
