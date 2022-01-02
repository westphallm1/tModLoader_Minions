using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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

	public static class ModProjectileExtensions
	{
		public static void ClientSideNPCHitCheck(this ModProjectile modProjectile)
		{
			if(modProjectile.Projectile.owner == Main.myPlayer || 
				Minion.GetClosestEnemyToPosition(modProjectile.Projectile.Center, 128, requireLOS: false) is not NPC npc)
			{
				return;
			}
			if(modProjectile.Projectile.Hitbox.Intersects(npc.Hitbox))
			{
				modProjectile.OnHitNPC(npc, 0, 0, false);
			}
		}
	}

	public abstract class Minion : ModProjectile
	{
		public readonly float PI = (float)Math.PI;

		public Player player;

		internal int? targetNPCIndex;
		protected int targetNPCCacheFrames;


		protected bool useBeacon = true;

		protected bool usingBeacon = false;

		protected PlayerTargetSelectionTactic currentTactic;
		protected PlayerTargetSelectionTactic previousTactic;

		public bool Spawned { get; private set; }

		internal abstract int BuffId { get; }

		// keep a local pointer to extra textures
		// for faster retrieval
		// Many unsafe gets to this
		protected List<Asset<Texture2D>> ExtraTextures;


		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			if(!Main.dedServ)
			{
				LoadAssets();
			}
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			TextureCache.ExtraTextures.TryGetValue(Type, out ExtraTextures);
		}
		public override void AI()
		{
			player = Main.player[Projectile.owner];
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
			// give at least one AI cycle to live before killing off
			if (player.HasBuff(BuffId))
			{
				Projectile.timeLeft = 2;
			} else if (Main.projPet[Projectile.type])
			{
				Projectile.Kill(); // pets don't die naturally for some reason
			}
		}

		/// <summary>
		/// Whether or not a specific NPC is a viable attack target for this minion.
		/// By default, ignore target dummies and critters
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public virtual bool ShouldIgnoreNPC(NPC npc)
		{
			return !npc.CanBeChasedBy() || !npc.active;
		}

		public Vector2? PlayerTargetPosition(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, Vector2? losCenter = null)
		{
			MinionTacticsPlayer tacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			if(tacticsPlayer.IgnoreVanillaMinionTarget > 0 && tacticsPlayer.SelectedTactic != TargetSelectionTacticHandler.GetTactic<ClosestEnemyToMinion>())
			{
				return null;
			}
			Vector2 center = centeredOn ?? Projectile.Center;
			Vector2 losCenterVector = losCenter ?? Projectile.Center;
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				if(ShouldIgnoreNPC(npc))
				{
					return null;
				}
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
			Vector2 center = centeredOn ?? Projectile.Center;
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

		public Vector2? SelectedEnemyInRange(float maxRange, float noLOSRange = 0, bool maxRangeFromPlayer = true, Vector2? losCenter = null)
		{
			Vector2 losCenterVector = losCenter ?? Projectile.Center;
			MinionTacticsPlayer tacticsPlayer = player.GetModPlayer<MinionTacticsPlayer>();
			MinionPathfindingPlayer pathfindingPlayer = player.GetModPlayer<MinionPathfindingPlayer>();

			// Make sure not to cache the target if the target selection tactic changes
			currentTactic = tacticsPlayer.GetTacticForMinion(this);
			bool tacticDidChange = currentTactic != previousTactic;
			previousTactic = currentTactic;
			
			// to cut back on Line-of-Sight computations, always chase the same NPC for some number of frames once one has been found
			if(!tacticDidChange && targetNPCIndex is int idx && Main.npc[idx].active && targetNPCCacheFrames++ < currentTactic.TargetCacheFrames)
			{
				return Main.npc[idx].Center;
			}
			Vector2 rangeCheckCenter;
			BlockAwarePathfinder pathfinder = pathfindingPlayer.GetPathfinder(this);
			Vector2 waypointPos = pathfindingPlayer.GetWaypointPosition(this);
			if(!maxRangeFromPlayer)
			{
				rangeCheckCenter = Projectile.Center;
			} else if (!pathfinder.InProgress() && pathfinder.searchSucceeded && waypointPos != default)
			{
				rangeCheckCenter = waypointPos;
			} else
			{
				rangeCheckCenter = player.Center;
			}
			List<NPC> possibleTargets = new List<NPC>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (ShouldIgnoreNPC(npc))
				{
					continue;
				}
				bool inRange = Vector2.DistanceSquared(npc.Center, rangeCheckCenter) < maxRange * maxRange;
				bool inNoLOSRange = Vector2.DistanceSquared(npc.Center, player.Center) < noLOSRange * noLOSRange;
				bool lineOfSight = inNoLOSRange || (inRange && Collision.CanHitLine(losCenterVector, 1, 1, npc.position, npc.width, npc.height));
				if (inNoLOSRange || (lineOfSight && inRange))
				{
					possibleTargets.Add(npc);
				}
			}
			int tacticsGroup = tacticsPlayer.GetGroupForMinion(this);
			NPC chosen = currentTactic.ChooseTargetNPC(Projectile, tacticsGroup, possibleTargets);
			if(chosen != default)
			{
				targetNPCIndex = chosen.whoAmI;
				targetNPCCacheFrames = 0;
				return chosen.Center;
			} 
			else
			{
				return null;
			}
		}

		// A simpler version of SelectedEnemyInRange that doesn't require any tactics/teams stuff
		public NPC GetClosestEnemyToPosition(Vector2 position, float searchRange, bool requireLOS = true)
		{
			return GetClosestEnemyToPosition(position, searchRange, ShouldIgnoreNPC, requireLOS);
		}

		public static bool GenericIgnoreNPC(NPC npc) => !npc.CanBeChasedBy();

		public static NPC GetClosestEnemyToPosition(Vector2 position, float searchRange,  Func<NPC, bool> shouldIgnore = null, bool requireLOS = true)
		{
			float minDist = float.MaxValue;
			NPC closest = null;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (shouldIgnore?.Invoke(npc) ?? (!npc.active || !npc.CanBeChasedBy()))
				{
					continue;
				}
				float distanceSq = Vector2.DistanceSquared(npc.Center, position);
				bool inRange =  distanceSq < searchRange * searchRange;
				bool lineOfSight = (!requireLOS) || (inRange && Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height));
				if (lineOfSight && inRange && distanceSq < minDist)
				{
					minDist = distanceSq;
					closest = npc;
				}
			}
			return closest;
		}

		public Vector2? AnyEnemyInRange(float maxRange, Vector2? centeredOn = null, bool noLOS = false)
		{
			Vector2 center = centeredOn ?? Projectile.Center;
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

		public virtual void LoadAssets()
		{
			// todo load assets
		}

		internal void AddTexture(string texturePath)
		{
			if(!TextureCache.ExtraTextures.TryGetValue(Type, out List<Asset<Texture2D>> textures))
			{
				textures = new List<Asset<Texture2D>>();
				TextureCache.ExtraTextures[Type] = textures;
			}
			if(texturePath == null)
			{
				textures.Add(null);
			} else
			{
				textures.Add(Request<Texture2D>(texturePath));
			}
		}

		public abstract void Behavior();
	}
}