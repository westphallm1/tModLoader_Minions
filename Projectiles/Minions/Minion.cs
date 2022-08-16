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

		internal MinionBehavior MinionBehavior;

		public Player Player => MinionBehavior.Player;

		public int? TargetNPCIndex { get => MinionBehavior.TargetNPCIndex; set => MinionBehavior.TargetNPCIndex = value; }
		public int TargetNPCCacheFrames { get => MinionBehavior.TargetNPCCacheFrames; set => MinionBehavior.TargetNPCCacheFrames = value; }


		public bool UseBeacon { get => MinionBehavior.UseBeacon; set => MinionBehavior.UseBeacon = value; }

		public bool UsingBeacon { get => MinionBehavior.UsingBeacon; set => MinionBehavior.UsingBeacon = value; }

		public PlayerTargetSelectionTactic CurrentTactic { get => MinionBehavior.CurrentTactic; set => MinionBehavior.CurrentTactic = value; }
		public PlayerTargetSelectionTactic PreviousTactic { get => MinionBehavior.PreviousTactic; set => MinionBehavior.PreviousTactic = value; }

		public bool Spawned { get => MinionBehavior.Spawned; private set => MinionBehavior.Spawned = value; }

		public abstract int BuffId { get; }

		// keep a local pointer to extra textures
		// for faster retrieval
		// Many unsafe gets to this
		protected List<Asset<Texture2D>> ExtraTextures;


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
			if (!CheckActive())
			{
				//Projectile despawned, don't continue with AI
				return;
			}
			if (!Spawned)
			{
				Spawned = true;
				OnSpawn();
			}
			UsingBeacon = false;
			DoAI();
		}

		public virtual void OnSpawn()
		{

		}

		/// <summary>
		/// Returns false if the projectile was manually despawned
		/// </summary>
		public virtual bool CheckActive()
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
				return false;
			}

			return true;
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

		public abstract void DoAI();

		public virtual void ApplyCrossModChanges() { }
	}
}