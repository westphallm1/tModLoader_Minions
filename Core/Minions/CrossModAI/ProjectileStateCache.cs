using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	/// <summary>
	/// Helper class for storing and retrieving the state of a projectile
	/// Used for overwriting base AI initiated changes to a projectile while
	/// cross-mod AI is interactive, and vice versa while cross-mod AI is inactive
	/// </summary>
	internal class ProjectileStateCache
	{
		// store the initial state, in case we need to roll back
		internal Vector2 InitialPosition { get; private set; }
		internal Vector2 InitialVelocity { get; private set; }
		internal bool InitialTileCollide { get; private set; }

		internal Vector2? Position { get; private set; }
		internal Vector2? Velocity { get; private set; }

		internal Vector2? PlayerPosition { get; private set; }
		internal Vector2? PlayerVelocity { get; private set; }

		internal bool? IsFriendly { get; private set; }

		internal bool? TileCollide { get; private set; }

		// this one is a bit contentious, appears to get set erroneously
		internal float? GfxOffY { get; private set; }

		public void ClearProjectile()
		{
			Position = null;
			Velocity = null;
			TileCollide = null;
			GfxOffY = null;
			IsFriendly = null;
		}

		public void Clear()
		{
			ClearProjectile();
			PlayerPosition = null;
			PlayerVelocity = null;
		}

		public void Cache(Projectile proj)
		{
			Position ??= proj.position;
			Velocity ??= proj.velocity;
			TileCollide ??= proj.tileCollide;
			GfxOffY ??= proj.gfxOffY;
			IsFriendly ??= proj.friendly;
			PlayerPosition ??= Main.player[proj.owner].position;
			PlayerVelocity ??= Main.player[proj.owner].velocity;
		}

		public void Rollback(Projectile proj)
		{
			if(Position != default && Velocity != default)
			{
				proj.position = InitialPosition;
				proj.velocity = InitialVelocity;
				proj.tileCollide = InitialTileCollide;
				ClearProjectile();
			}
			if(PlayerPosition is Vector2 playerPosition && PlayerVelocity is Vector2 playerVelocity)
			{
				Main.player[proj.owner].position = playerPosition;
				Main.player[proj.owner].velocity = playerVelocity;
				Clear();
			}
		}

		public void CacheInitial(Projectile proj)
		{
			InitialPosition = proj.position;
			InitialVelocity = proj.velocity;
			InitialTileCollide = proj.tileCollide;
		}

		public void Uncache(Projectile proj)
		{
			proj.position = Position ?? proj.position;
			proj.velocity = Velocity ?? proj.velocity;
			proj.tileCollide = TileCollide ?? proj.tileCollide;
			proj.gfxOffY = GfxOffY ?? proj.gfxOffY;
			proj.friendly = IsFriendly ?? proj.friendly;
			Main.player[proj.owner].position = PlayerPosition ?? Main.player[proj.owner].position;
			Main.player[proj.owner].velocity = PlayerVelocity ?? Main.player[proj.owner].velocity;
			Clear();
		}
	}

	/// <summary>
	/// Helper class for storing and retrieving the defaults of a projectile
	/// Used for overwriting SetDefaults values while cross-mod AI is 
	/// active, and restoring those values while cross-mod AI is inactive
	/// </summary>
	internal class ProjectileDefaultsCache
	{
		public bool IsMinion { get; private set; }
		public bool IsFriendly { get; private set; }
		public DamageClass DamageType { get; private set; }
		public bool UsesLocalImmunity { get; private set; }
		public bool UsesIdStaticImmunity { get; private set; }
		public int LocalHitCooldown { get; private set; }

		public ProjectileDefaultsCache(Projectile projectile)
		{
			IsMinion = projectile.minion;
			IsFriendly = projectile.friendly;
			DamageType = projectile.DamageType;
			UsesLocalImmunity = projectile.usesLocalNPCImmunity;
			UsesIdStaticImmunity = projectile.usesIDStaticNPCImmunity;
			LocalHitCooldown = projectile.localNPCHitCooldown;
		}

		internal void RestoreDefaults(Projectile projectile)
		{
			projectile.minion = IsMinion;
			projectile.friendly = IsFriendly;
			projectile.DamageType = DamageType;
			projectile.usesLocalNPCImmunity = UsesLocalImmunity;
			projectile.usesIDStaticNPCImmunity = UsesIdStaticImmunity;
			projectile.localNPCHitCooldown = LocalHitCooldown;
		}
	}

}
