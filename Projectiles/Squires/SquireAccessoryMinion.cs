using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires
{
	public abstract class SquireAccessoryMinion : TransientMinion
	{
		protected int animationFrame;

		protected abstract bool IsEquipped(SquireModPlayer player);

		protected Projectile squire;
		protected SquireModPlayer squirePlayer;
		public override void SetDefaults()
		{
			projectile.friendly = false;
			projectile.tileCollide = false;
			projectile.timeLeft = 2;
			animationFrame = 0;
		}

		public override Vector2 IdleBehavior()
		{
			squirePlayer = player.GetModPlayer<SquireModPlayer>();
			squire = squirePlayer.GetSquire();
			if(squire == null || !IsEquipped(squirePlayer))
			{
				return projectile.velocity;
			}
			projectile.timeLeft = 2;
			animationFrame++;
			return squire.Center  - projectile.position;
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
