using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Minions.FlyingSword
{
	public class FlyingSwordMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<FlyingSwordMinion>(), ProjectileType<FlyingSwordMinion>() };
	}

	public class FlyingSwordMinionItem : MinionItem<FlyingSwordMinionBuff, FlyingSwordMinion>
	{
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.MELEE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 48;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 50;
			Item.height = 50;
			Item.value = Item.buyPrice(0, 12, 0, 0);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.SoulofFlight, 10).AddIngredient(ItemID.HallowedBar, 12).AddTile(TileID.MythrilAnvil).Register();
		}
	}
	public class FlyingSwordMinion : GroupAwareMinion
	{
		public override int BuffId => BuffType<FlyingSwordMinionBuff>();

		int hitCount = 0;
		int maxHitCount = 8;
		int framesInAir = 0;
		int maxFramesInAir = 90;
		int enemyHitFrame = 0;


		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = false;
			AttackState = AttackState.IDLE;
			attackFrames = 120;
		}

		public override void OnHitTarget(NPC target)
		{
			if (hitCount++ >= maxHitCount)
			{
				AttackState = AttackState.RETURNING;
			}
			enemyHitFrame = framesInAir;
			Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, DustID.Platinum);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = IdleLocationSets.GetProjectilesInSet(IdleLocationSets.trailingInAir, Projectile.owner);
			int minionCount = minions.Count;
			int order = minions.IndexOf(Projectile);
			float idleAngle = (float)(2 * Math.PI * order) / minionCount;
			if (minions.Count > 0)
			{
				idleAngle += (2 * (float)Math.PI * GroupAnimationFrame) / GroupAnimationFrames;
			}
			Vector2 idlePosition = Player.Center;
			idlePosition.X += -Player.direction * IdleLocationSets.GetXOffsetInSet(minions, Projectile);
			idlePosition.Y += -35 + 5 * (float)Math.Sin(idleAngle);
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition = Player.Center;
				idlePosition.X += 30 * -Player.direction;
				idlePosition.Y += -35;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(950f, Projectile.Center) is Vector2 target)
			{
				return target;
			}
			else
			{
				return null;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// alway clamp to the idle position
			int inertia = 5;
			int speed = 15;
			framesInAir++;
			if ((enemyHitFrame == 0 || enemyHitFrame + 9 < framesInAir) && vectorToTargetPosition.Length() > 8)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
				Projectile.rotation += (float)Math.PI / 9;
			}
			else
			{
				if (Projectile.velocity == Vector2.Zero)
				{
					Projectile.velocity = Vector2.One;
				}
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= speed; // travel straight away from the impact
			}
			if (framesInAir >= maxFramesInAir)
			{
				AttackState = AttackState.RETURNING;
			}
			Lighting.AddLight(Projectile.position, Color.LightGray.ToVector3() * 0.5f);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// alway clamp to the idle position
			int inertia = 5;
			int maxSpeed = 32;
			if (vectorToIdlePosition.Length() < 32)
			{
				// return to the attacking state after getting back home
				AttackState = AttackState.IDLE;
				hitCount = 0;
				framesInAir = 0;
				enemyHitFrame = 0;
			}
			Vector2 speedChange = vectorToIdlePosition - Projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + speedChange) / inertia;

			float intendedRotation = Projectile.velocity.X * 0.05f;
			Projectile.rotation = intendedRotation;
		}
	}
}
