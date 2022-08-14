using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AmuletOfManyMinions.Projectiles.Minions.SpiritGun
{
	// <summary>
	// Uses ai[0] to track whether we've hit a target yet
	// </summary>
	class SpiritGunMinionBullet : Minion
	{
		bool hitTarget
		{
			get => Projectile.ai[0] != 0;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		public override int BuffId => -1;

		bool lookingForTarget;
		const int speed = 26;
		Vector2 velocity = default;
		Vector2 vectorToTarget = default;
		NPC targetNPC = null;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 2;
			Projectile.friendly = true;
			Projectile.penetrate = 3;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 120;
			hitTarget = false;
			lookingForTarget = false;
		}

		private void LookForTarget()
		{
			if ((PlayerTargetPosition(600) ?? SelectedEnemyInRange(600)) is Vector2 target && TargetNPCIndex is int targetIdx)
			{
				velocity = target - Projectile.Center;
				vectorToTarget = velocity;
				lookingForTarget = false;
				velocity.SafeNormalize();
				velocity *= speed;
				targetNPC = Main.npc[targetIdx];
			}
		}

		public override void AI()
		{
			Player = Main.player[Projectile.owner];
			if (velocity == default)
			{
				velocity = Projectile.velocity;
				vectorToTarget = velocity;
				velocity.SafeNormalize();
				velocity *= speed;
			}
			if (hitTarget)
			{
				return;
			}
			if (vectorToTarget.LengthSquared() < speed * speed)
			{
				lookingForTarget = true;
			}
			if (lookingForTarget)
			{
				LookForTarget();
			}
			vectorToTarget -= velocity;
			Projectile.rotation = velocity.ToRotation();
			Projectile.velocity = velocity;
			Lighting.AddLight(Projectile.position, Color.LightCyan.ToVector3());
			// MP-safe collision check
			if (targetNPC != null && targetNPC.active &&
				Vector2.DistanceSquared(targetNPC.Center, Projectile.Center) < Projectile.velocity.LengthSquared())
			{
				hitTarget = true;
			}
		}


		public override void DoAI()
		{
			// no-op
		}
	}
}
