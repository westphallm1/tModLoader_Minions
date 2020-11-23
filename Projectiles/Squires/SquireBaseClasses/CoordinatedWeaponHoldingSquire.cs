using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses
{
	/// <summary>
	/// Uses ai[0] for attackSequence (by passing projectile.identity)
	/// </summary>
	public abstract class CoordinatedWeaponHoldingSquire<T> : WeaponHoldingSquire<T> where T : ModBuff
	{

		public int attackSequence = 0;
		public virtual int AttackSequenceLength => 4;
		public virtual bool IsBoss => false;

		protected CoordinatedWeaponHoldingSquire(int itemID) : base(itemID) { }

		protected virtual bool IsMyTurn()
		{
			if (IsBoss)
			{
				return attackSequence < AttackSequenceLength / 2;
			}
			else
			{
				return attackSequence >= AttackSequenceLength / 2;
			}
		}

		protected override bool IsAttacking()
		{
			return base.IsAttacking() && IsMyTurn();
		}

		public override Vector2? FindTarget()
		{
			Vector2? vector2Target = base.FindTarget();
			if (vector2Target != null)
			{
				if (IsBoss && attackFrame == ModifiedAttackFrames - 1)
				{
					attackSequence = (attackSequence + 1) % AttackSequenceLength;
					projectile.ai[0] = attackSequence;
				}
				if (!IsBoss)
				{
					attackSequence = (int)Main.projectile[(int)projectile.ai[0]].ai[0];
				}
				// default frame increment path gets blocked, need to recreate here
				if (!IsAttacking())
				{
					attackFrame = (attackFrame + 1) % ModifiedAttackFrames;
				}
				if (!IsMyTurn())
				{
					IdleMovement(vectorToIdle);
					return null;
				}
			}
			return vector2Target;
		}


	}
}
