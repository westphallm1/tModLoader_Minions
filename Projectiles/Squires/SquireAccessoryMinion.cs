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
			get => (int)Projectile.localAI[1];
			set => Projectile.localAI[1] = value;
		}

		protected abstract bool IsEquipped(SquireModPlayer player);

		protected Projectile squire;
		protected SquireModPlayer squirePlayer;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 2;
			animationFrame = 0;
		}

		public override Vector2 IdleBehavior()
		{
			animationFrame++;
			squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squire = squirePlayer.GetSquire();
			if (squire == null || !IsEquipped(squirePlayer))
			{
				return Projectile.velocity;
			}
			Projectile.timeLeft = 2;
			Vector2 target = squire.Center - Projectile.Center;
			TeleportToPlayer(ref target, 2000f);
			return target;
		}

		public bool SquireAttacking()
		{
			return player.channel && SquireMinionTypes.Contains(player.HeldItem.shoot);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			Projectile.position += vectorToIdlePosition;
		}
	}
}
