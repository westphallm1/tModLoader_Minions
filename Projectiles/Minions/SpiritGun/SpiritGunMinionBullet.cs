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
			get => projectile.ai[0] != 0;
			set => projectile.ai[0] = value ? 1 : 0;
		}

		internal override int BuffId => -1;

		bool lookingForTarget;
		const int speed = 26;
		Vector2 velocity = default;
		Vector2 vectorToTarget = default;
		NPC targetNPC = null;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.width = 24;
			projectile.height = 2;
			projectile.friendly = true;
			projectile.penetrate = 3;
			projectile.tileCollide = true;
			projectile.timeLeft = 120;
			hitTarget = false;
			lookingForTarget = false;
		}

		private void LookForTarget()
		{
			if ((PlayerTargetPosition(600) ?? SelectedEnemyInRange(600)) is Vector2 target && targetNPCIndex is int targetIdx)
			{
				velocity = target - projectile.Center;
				vectorToTarget = velocity;
				lookingForTarget = false;
				velocity.SafeNormalize();
				velocity *= speed;
				targetNPC = Main.npc[targetIdx];
			}
		}

		public override void AI()
		{
			player = Main.player[projectile.owner];
			if (velocity == default)
			{
				velocity = projectile.velocity;
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
			projectile.rotation = velocity.ToRotation();
			projectile.velocity = velocity;
			Lighting.AddLight(projectile.position, Color.LightCyan.ToVector3());
			// MP-safe collision check
			if (targetNPC != null && targetNPC.active &&
				Vector2.DistanceSquared(targetNPC.Center, projectile.Center) < projectile.velocity.LengthSquared())
			{
				hitTarget = true;
			}
		}


		public override void Behavior()
		{
			// no-op
		}
	}
}
