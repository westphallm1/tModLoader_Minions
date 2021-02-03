using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Terraria;

namespace AmuletOfManyMinions.Projectiles.Squires
{
	// Uses localAi[1] for animation frame
	public abstract class SquireAccessoryMinion : TransientMinion
	{
		new protected int animationFrame
		{
			get => (int)projectile.localAI[1];
			set => projectile.localAI[1] = value;
		}

		protected abstract bool IsEquipped(SquireModPlayer player);

		protected Projectile squire;
		protected SquireModPlayer squirePlayer;

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.friendly = false;
			projectile.tileCollide = false;
			projectile.timeLeft = 2;
			animationFrame = 0;
		}

		public override Vector2 IdleBehavior()
		{
			animationFrame++;
			squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squire = squirePlayer.GetSquire();
			if (squire == null || !IsEquipped(squirePlayer))
			{
				return projectile.velocity;
			}
			projectile.timeLeft = 2;
			Vector2 target = squire.Center - projectile.Center;
			TeleportToPlayer(ref target, 2000f);
			return target;
		}

		public bool SquireAttacking()
		{
			return player.channel && SquireMinionTypes.Contains(player.HeldItem.shoot);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			projectile.position += vectorToIdlePosition;
		}
	}
}
