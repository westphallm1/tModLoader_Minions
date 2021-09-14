using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires
{

	public class SquireMinionTypes : ModSystem
	{
		public static HashSet<int> squireTypes;

		public override void OnModLoad()
		{
			squireTypes = new HashSet<int>();
		}

		public override void Unload()
		{
			squireTypes = null;
		}

		public static void Add(int squireType)
		{
			squireTypes.Add(squireType);
		}

		public static bool Contains(int squireType)
		{
			return squireTypes.Contains(squireType);
		}
	}

	public abstract class SquireMinion : SimpleMinion
	{
		protected int itemType;


		protected Vector2 relativeVelocity = Vector2.Zero;

		protected virtual float IdleDistanceMulitplier => 1.5f;

		protected bool returningToPlayer = false;

		protected int baseLocalIFrames;

		protected virtual bool travelRangeCanBeModified => true;

		protected virtual bool attackSpeedCanBeModified => true;

		protected virtual float projectileVelocity => default;

		// state tracking variables for special attacks
		protected bool usingSpecial;

		protected int specialStartFrame;
		internal Vector2 syncedMouseWorld;

		protected virtual int SpecialDuration => 30;
		protected virtual int SpecialCooldown => 6 * 60;
		protected int specialFrame => animationFrame - specialStartFrame;

		protected bool SpecialOnCooldown => player.HasBuff(ModContent.BuffType<SquireCooldownBuff>());

		public virtual int CooldownDoneDust => 15;

		protected virtual LegacySoundStyle SpecialStartSound => new LegacySoundStyle(2, 43);

		public SquireMinion(int itemID)
		{
			itemType = itemID;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireMinionTypes.Add(Projectile.type);
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = false;

			ProjectileID.Sets.CountsAsHoming[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = false;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			useBeacon = false;
			usesTactics = false;
			Projectile.minionSlots = 0;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			baseLocalIFrames = Projectile.localNPCHitCooldown;
		}

		public override bool? CanCutTiles()
		{
			return true;
		}

		public override Vector2? FindTarget()
		{
			// move towards the mouse if player is holding and clicking
			if (returningToPlayer || Vector2.Distance(Projectile.Center, player.Center) > IdleDistanceMulitplier * ModifiedMaxDistance())
			{
				returningToPlayer = true;
				return null; // force back into non-attacking mode if too far from player
			}
			if (player.HeldItem.type == itemType && (usingSpecial || (player.channel && player.altFunctionUse != 2)))
			{
				MousePlayer mPlayer = player.GetModPlayer<MousePlayer>();
				mPlayer.SetMousePosition();
				Vector2? _mouseWorld = mPlayer.GetMousePosition();
				if (_mouseWorld is Vector2 mouseWorld)
				{
					syncedMouseWorld = mouseWorld;
					Vector2 targetFromPlayer = mouseWorld - player.Center;
					if (targetFromPlayer.Length() < ModifiedMaxDistance())
					{
						return mouseWorld - Projectile.Center;
					}
					targetFromPlayer.Normalize();
					targetFromPlayer *= ModifiedMaxDistance();
					return player.Center + targetFromPlayer - Projectile.Center;
				}
			}
			return null;
		}

		public override Vector2 IdleBehavior()
		{
			// hover behind the player
			Vector2 idlePosition = player.Top;
			idlePosition.X += 24 * -player.direction;
			idlePosition.Y += -8;
			// not sure what side effects changing this each frame might have
			if (attackSpeedCanBeModified)
			{
				Projectile.localNPCHitCooldown = (int)(baseLocalIFrames * player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier);
			}
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Center.X;
				idlePosition.Y = player.Center.Y - 24;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			CheckSpecialUsage();
			Main.NewText(Projectile.originalDamage);
			return vectorToIdlePosition;
		}

		protected void CheckSpecialUsage()
		{
			// switch from "not using special" to "using special"
			int cooldownBuffType = ModContent.BuffType<SquireCooldownBuff>();
			if(player.whoAmI == Main.myPlayer && !usingSpecial && !SpecialOnCooldown && player.channel && Main.mouseRight)
			{
				StartSpecial();
			} else if (usingSpecial && specialFrame >= SpecialDuration)
			{
				usingSpecial = false;
				OnStopUsingSpecial();
			} else if (player.whoAmI == Main.myPlayer 
				&& SpecialOnCooldown && player.buffTime[player.FindBuffIndex(cooldownBuffType)] == 1)
			{
				// a little dust animation to indicate special can be used again
				for(int i = 0; i < 3; i++)
				{
					int dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, CooldownDoneDust, 0, 0);
					Main.dust[dustIdx].noGravity = true;
					Main.dust[dustIdx].noLight = true;
				}
				// maybe using the mana refill sound isn't the best idea
				SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
			}
		}
		
		// little bit weird to put this in the squire itself rather than the modplayer, but so it goes
		public void StartSpecial(bool fromSync = false)
		{
			int cooldownBuffType = ModContent.BuffType<SquireCooldownBuff>();
			player.AddBuff(cooldownBuffType, SpecialCooldown + SpecialDuration);
			// this is a bit hacky, but this code only runs on the first squire, so
			// manually propegate the special to all squires the player owns
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.owner == player.whoAmI && SquireMinionTypes.Contains(p.type))
				{
					((SquireMinion)p.ModProjectile).SetSpecialStartFrame();
				}
			}
			if(!fromSync)
			{
				new SquireSpecialStartPacket(player).Send();
			}
		}

		public void SetSpecialStartFrame()
		{
			usingSpecial = true;
			specialStartFrame = animationFrame;
			if(SpecialStartSound != null)
			{
				SoundEngine.PlaySound(SpecialStartSound, Projectile.Center);
			}
			OnStartUsingSpecial();
		}

		public virtual void OnStartUsingSpecial()
		{
			// default no-op, mostly used for visual effects
		}

		public virtual void OnStopUsingSpecial()
		{
			// default no-op, mostly used for visual effects
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// always clamp to the idle position
			float inertia = ComputeInertia();
			float maxSpeed = ModifiedIdleSpeed();
			Vector2 speedChange = vectorToIdlePosition - Projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			else
			{
				returningToPlayer = false;
			}
			relativeVelocity = (relativeVelocity * (inertia - 1) + speedChange) / inertia;
			Projectile.velocity = player.velocity + relativeVelocity;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(usingSpecial)
			{
				SpecialTargetedMovement(vectorToTargetPosition);
			} else
			{
				StandardTargetedMovement(vectorToTargetPosition);
			}
		}

		public virtual void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (vectorToTargetPosition.Length() < 8 && relativeVelocity.Length() < 4)
			{
				relativeVelocity = Vector2.Zero;
				Projectile.velocity = player.velocity;
				Vector2 newPosition = Projectile.position + vectorToTargetPosition;
				if (!Collision.SolidCollision(newPosition, Projectile.width, Projectile.height))
				{
					Projectile.position = newPosition;
				}
				return;
			}
			else if (relativeVelocity.Length() > vectorToTargetPosition.Length() / 3)
			{
				relativeVelocity *= 0.9f;
			}
			float inertia = ComputeInertia();
			float speed = ModifiedTargetedSpeed();
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			relativeVelocity = (relativeVelocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			Projectile.velocity = player.velocity + relativeVelocity;
		}

		public virtual void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			// by default, don't do anything special
			StandardTargetedMovement(vectorToTargetPosition);
		}

		public float ModifiedTargetedSpeed() => ComputeTargetedSpeed() * player.GetModPlayer<SquireModPlayer>().squireTravelSpeedMultiplier;
		public float ModifiedIdleSpeed() => ComputeIdleSpeed() * player.GetModPlayer<SquireModPlayer>().squireTravelSpeedMultiplier;

		public float ModifiedMaxDistance() => MaxDistanceFromPlayer() + (travelRangeCanBeModified ? player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus : 0);

		// increase projectile velocity based on max travel distance, since projectile shooting squires
		// can't take advantage of it
		// 15 blocks extra range doubles projectile speed
		protected float ModifiedProjectileVelocity() => projectileVelocity * (1 + player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus / 240f);

		public virtual float ComputeInertia() => 12;

		public virtual float ComputeIdleSpeed() => 8;

		public virtual float ComputeTargetedSpeed() => 8;


		public virtual float MaxDistanceFromPlayer() => 80;

	}
}
