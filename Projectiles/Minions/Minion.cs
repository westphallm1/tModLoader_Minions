using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public static class Vector2Extensions
	{
		// prevent 
		public static void SafeNormalize(this ref Vector2 vec)
		{
			if (vec != Vector2.Zero)
			{
				vec.Normalize();
			}
		}
	}
	public abstract class Minion : ModProjectile
	{
		public readonly float PI = (float)Math.PI;

		public Player player;

		protected int? targetNPCIndex;
		protected int targetNPCCacheFrames;


		protected bool useBeacon = true;

		protected bool usingBeacon = false;

		public bool Spawned { get; private set; }

		protected abstract int BuffId { get; }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}
		public override void AI()
		{
			player = Main.player[projectile.owner];
			CheckActive();
			if (!Spawned)
			{
				Spawned = true;
				OnSpawn();
			}
			usingBeacon = false;
			Behavior();
		}

		public virtual void OnSpawn()
		{

		}

		public virtual void CheckActive()
		{
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active)
			{
				player.ClearBuff(BuffId);
			}
			if (player.HasBuff(BuffId))
			{
				projectile.timeLeft = 2;
			}
		}

		public Vector2? PlayerTargetPosition(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, Vector2? losCenter = null)
		{
			MinionTacticsPlayer tacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			if(tacticsPlayer.IgnoreVanillaMinionTarget > 0 && tacticsPlayer.SelectedTactic != TargetSelectionTacticHandler.GetTactic<ClosestEnemyToMinion>())
			{
				return null;
			}
			Vector2 center = centeredOn ?? projectile.Center;
			Vector2 losCenterVector = losCenter ?? projectile.Center;
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
				if (distance < noLOSRange || (distance < maxRange &&
					Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height)))
				{
					targetNPCIndex = player.MinionAttackTargetNPC;
					return npc.Center;
				}
			}
			return null;
		}

		public Vector2? PlayerAnyTargetPosition(float maxRange, Vector2? centeredOn = null)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float distance = Vector2.Distance(npc.Center, center);
				bool lineOfSight = Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1);
				if (distance < maxRange && lineOfSight)
				{
					targetNPCIndex = player.MinionAttackTargetNPC;
					return npc.Center;
				}
			}
			return null;
		}

		public Vector2? SelectedEnemyInRange(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, bool maxRangeFromPlayer = true, Vector2? losCenter = null)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			Vector2 losCenterVector = losCenter ?? projectile.Center;
			PlayerTargetSelectionTactic tactic = player.GetModPlayer<MinionTacticsPlayer>().PlayerTactic;
			// to cut back on Line-of-Sight computations, always chase the same NPC for some number of frames once one has been found
			if(targetNPCIndex is int idx && Main.npc[idx].active && targetNPCCacheFrames++ < tactic.TargetCacheFrames)
			{
				return Main.npc[idx].Center;
			}
			List<NPC> possibleTargets = new List<NPC>();
			bool anyInRange = false;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy() || !npc.active)
				{
					continue;
				}
				bool inRange = Vector2.DistanceSquared(npc.Center, maxRangeFromPlayer ? player.Center : projectile.Center) < maxRange * maxRange;
				bool inNoLOSRange = Vector2.DistanceSquared(npc.Center, player.Center) < noLOSRange * noLOSRange;
				bool lineOfSight = inNoLOSRange || (inRange && Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height));
				if (inNoLOSRange || (lineOfSight && inRange))
				{
					possibleTargets.Add(npc);
				}
				anyInRange |= inRange;
			}
			NPC chosen = tactic.ChooseTargetFromList(projectile, possibleTargets);
			if(chosen != default)
			{
				targetNPCIndex = chosen.whoAmI;
				targetNPCCacheFrames = 0;
				return chosen.Center;
			} else if (useBeacon && anyInRange)
			{
				usingBeacon = true;
				return BeaconPosition(center, maxRange, noLOSRange);
			}
			else
			{
				return null;
			}
		}

		public Vector2? BeaconPosition(Vector2 center, float maxRange, float noLOSRange = 0)
		{
			int type = MinionWaypoint.Type;
			// should automatically fall through to here if can't hit target
			if (player.ownedProjectileCounts[type] == 0)
			{
				return null;
			}
			Vector2? waypointCenter = null;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == Main.myPlayer && p.type == type)
				{
					Vector2 target = p.position;
					float distance = Vector2.Distance(target, center);
					if (distance < noLOSRange || (distance < maxRange &&
						Collision.CanHitLine(projectile.Center, 1, 1, target, 1, 1)))
					{
						waypointCenter = target;
						break;
					}
				}
			}
			// try again with the beacon position as the central search point
			if (waypointCenter is Vector2 wCenter && AnyEnemyInRange(maxRange, wCenter) is Vector2 anyTarget)
			{
				DrawDirectionDust(wCenter, anyTarget);
				return wCenter;
			}
			else
			{
				return null;
			}
		}

		private int directionFrame = 0;
		protected void DrawDirectionDust(Vector2 waypointCenter, Vector2 anyTarget)
		{
			if ((directionFrame++) % 30 != 0 || player.GetModPlayer<MinionSpawningItemPlayer>().didDrawDustThisFrame)
			{
				return;
			}
			player.GetModPlayer<MinionSpawningItemPlayer>().didDrawDustThisFrame = true;
			int lineLength = 64;
			Vector2 fromVector = projectile.Center - waypointCenter;
			Vector2 toVector = anyTarget - waypointCenter;
			fromVector.SafeNormalize();
			toVector.SafeNormalize();
			for (int i = 12; i < lineLength; i += 2)
			{
				float scale = 1.5f - 0.015f * i;
				Dust.NewDust(waypointCenter + fromVector * i, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: scale);
				Dust.NewDust(waypointCenter + toVector * i, 1, 1, DustType<MinionWaypointDust>(), newColor: new Color(0.5f, 1, 0.5f), Scale: scale);

			}

		}

		public Vector2? AnyEnemyInRange(float maxRange, Vector2? centeredOn = null, bool noLOS = false)
		{
			Vector2 center = centeredOn ?? projectile.Center;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy())
				{
					continue;
				}
				// 
				bool inRange = Vector2.Distance(center, npc.Center) < maxRange;
				bool lineOfSight = noLOS || (inRange && Collision.CanHitLine(center, 1, 1, npc.Center, 1, 1));
				if (lineOfSight && inRange)
				{
					targetNPCIndex = npc.whoAmI;
					return npc.Center;
				}
			}
			return null;
		}

		public abstract void Behavior();
	}
}