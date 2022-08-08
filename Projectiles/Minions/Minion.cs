using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.PlayerTargetSelectionTactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions;
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

	public abstract class Minion : ModProjectile, IMinion
	{

		public Player Player { get; set; }

		public int? TargetNPCIndex { get; set; }
		public int TargetNPCCacheFrames { get; set; }


		public bool UseBeacon { get; set; } = true;

		public bool UsingBeacon { get; set; } = false;

		public PlayerTargetSelectionTactic CurrentTactic { get; set; }
		public PlayerTargetSelectionTactic PreviousTactic { get; set; }

		public bool Spawned { get; private set; }

		public abstract int BuffId { get; }

		// keep a local pointer to extra textures
		// for faster retrieval
		// Many unsafe gets to this
		protected List<Asset<Texture2D>> ExtraTextures;

		internal MinionBehavior MinionBehavior;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			if (!Main.dedServ)
			{
				LoadAssets();
			}
			ApplyCrossModChanges();
		}

		public override void SetDefaults()
		{
			MinionBehavior = new(this);
			base.SetDefaults();
			TextureCache.ExtraTextures.TryGetValue(Type, out ExtraTextures);
		}
		public override void AI()
		{
			Player = Main.player[Projectile.owner];
			CheckActive();
			if (!Spawned)
			{
				Spawned = true;
				OnSpawn();
			}
			UsingBeacon = false;
			Behavior();
		}

		public virtual void OnSpawn()
		{

		}

		public virtual void CheckActive()
		{
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (Player.dead || !Player.active)
			{
				Player.ClearBuff(BuffId);
			}
			// give at least one AI cycle to live before killing off
			if (Player.HasBuff(BuffId))
			{
				Projectile.timeLeft = 2;
			}
			else if (Main.projPet[Projectile.type])
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

		public Vector2? PlayerTargetPosition(float maxRange, Vector2? centeredOn = null, float noLOSRange = 0, Vector2? losCenter = null) =>
			MinionBehavior.PlayerTargetPosition(maxRange, centeredOn, noLOSRange, losCenter);

		public Vector2? PlayerAnyTargetPosition(float maxRange, Vector2? centeredOn = null) =>
			MinionBehavior.PlayerAnyTargetPosition(maxRange, centeredOn);

		public Vector2? SelectedEnemyInRange(float maxRange, float noLOSRange = 0, bool maxRangeFromPlayer = true, Vector2? losCenter = null) =>
			MinionBehavior.SelectedEnemyInRange(maxRange, noLOSRange, maxRangeFromPlayer, losCenter);

		// A simpler version of SelectedEnemyInRange that doesn't require any tactics/teams stuff
		public NPC GetClosestEnemyToPosition(Vector2 position, float searchRange, bool requireLOS = true)
			=> MinionBehavior.GetClosestEnemyToPosition(position, searchRange, requireLOS);


		public Vector2? AnyEnemyInRange(float maxRange, Vector2? centeredOn = null, bool noLOS = false)
			=> MinionBehavior.AnyEnemyInRange(maxRange, centeredOn, noLOS);

		public virtual void LoadAssets()
		{
			// todo load assets
		}

		internal void AddTexture(string texturePath)
		{
			if (!TextureCache.ExtraTextures.TryGetValue(Type, out List<Asset<Texture2D>> textures))
			{
				textures = new List<Asset<Texture2D>>();
				TextureCache.ExtraTextures[Type] = textures;
			}
			if (texturePath == null)
			{
				textures.Add(null);
			}
			else
			{
				textures.Add(Request<Texture2D>(texturePath));
			}
		}

		public abstract void Behavior();

		public virtual void ApplyCrossModChanges() { }
	}
}