using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class SkeletronJrMinionBuff : CombatPetVanillaCloneBuff
	{
		public SkeletronJrMinionBuff() : base(ProjectileType<SkeletronJrMinion>()) { }

		public override int VanillaBuffId => BuffID.SkeletronPet;

		public override string VanillaBuffName => "SkeletronPet";
	}

	public class SkeletronJrMinionItem : CombatPetMinionItem<SkeletronJrMinionBuff, SkeletronJrMinion>
	{
		internal override int VanillaItemID => ItemID.SkeletronPetItem;

		internal override string VanillaItemName => "SkeletronPetItem";
	}

	internal struct SkeletronHand
	{
		// do this rather than velocity, may or may not make sense
		internal Vector2 Position { get; set; }
		internal Vector2 TargetPosition { get; set; }
		internal int Frame { get; set; }
		internal int SpriteDirection { get; set; }
		internal float Rotation { get; set; }

		internal void UpdatePosition(int maxVelocity)
		{
			Vector2 target = TargetPosition - Position;
			if(target.LengthSquared() > maxVelocity * maxVelocity)
			{
				target.Normalize();
				target *= maxVelocity;
			}
			Position += target;
		}

		internal void Draw(Vector2 center, Texture2D texture, int frameHeight, Color lightColor)
		{
			Vector2 handPosition = Position + center;
			SpriteEffects effects = SpriteDirection == 1  ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = new Rectangle(0, Frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
			Main.EntitySpriteDraw(texture, handPosition - Main.screenPosition,
				bounds, lightColor, Rotation, origin, 1, effects, 0);
		}
	}
	
	public abstract class SkeletronCombatPet : CombatPetHoverShooterMinion
	{
		internal override int? FiredProjectileId => null;


		internal SkeletronHand[] hands;

		internal int attackCycle;

		internal override int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(20, 45 - 4 * info.Level);
		internal override bool DoBumblingMovement => attackCycle > 4;

		public override void SetDefaults()
		{
			base.SetDefaults();
			hands = new SkeletronHand[2];
			circleHelper.idleBumbleFrames = 90;
			circleHelper.idleBumbleRadius = 96;
			hsHelper.targetInnerRadius = 64;
			hsHelper.targetOuterRadius = 96;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 vectorToIdle = base.IdleBehavior();
			dealsContactDamage = true;
			return vectorToIdle;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1  ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// body
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);

			for(int i = 0; i < hands.Length; i++)
			{
				hands[i].Draw(Projectile.Center, texture, frameHeight, lightColor);
			}
			return false;
		}

		internal override void AfterFiringProjectile()
		{
			IncrementAttackCyle();
		}

		public override void OnHitTarget(NPC target)
		{
			base.OnHitTarget(target);
			if(DoBumblingMovement)
			{
				IncrementAttackCyle();
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(framesSinceHadTarget > 30)
			{
				attackCycle = 0;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		private void IncrementAttackCyle()
		{
			attackCycle = (attackCycle +1) % 9;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			for(int i = 0; i < hands.Length; i++)
			{
				UpdateHand(ref hands[i], i);
				hands[i].UpdatePosition(8);
			}
		}

		internal abstract void UpdateHand(ref SkeletronHand hand, int handIdx);

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(DoBumblingMovement)
			{
				return base.Colliding(projHitbox, targetHitbox);
			}
			targetHitbox.Inflate(16, 16);
			for(int i = 0; i < hands.Length; i++)
			{
				Vector2 handPos = hands[i].Position + Projectile.Center;
				if(targetHitbox.Contains(handPos.ToPoint()))
				{
					return true;
				}
			}
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			bool isSpinning = DoBumblingMovement && vectorToTarget is Vector2 target &&
				target.LengthSquared() < 2 * hsHelper.targetOuterRadius * hsHelper.targetOuterRadius;
			if(isSpinning)
			{
				Projectile.frame = 6;
				Projectile.rotation += Math.Sign(Projectile.velocity.X) * MathHelper.TwoPi / 15;
			} else
			{
				base.Animate(0, 6);
				Projectile.rotation = Projectile.velocity.X * 0.05f;
			}
		}
	}


	public class SkeletronJrMinion : SkeletronCombatPet
	{
		internal override int BuffId => BuffType<SkeletronJrMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SkeletronPet;
		internal override int? FiredProjectileId => null;


		internal int handFrames = 4;
		internal int firstHandFrame = 7;

		internal override int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(20, 45 - 4 * info.Level);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.SkeletronJr"));
			Main.projFrames[Projectile.type] = 10;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		internal override void UpdateHand(ref SkeletronHand hand, int handIdx)
		{
			Vector2 offset;
			int shootFrame = animationFrame - hsHelper.lastShootFrame;
			if(attackCycle > 4 || handIdx != attackCycle % 2 || vectorToTarget is not Vector2 target || shootFrame > attackFrames)
			{
				// very hacky way to get -1 and 1
				Vector2 baseOffset = 32 * Vector2.UnitX * Math.Sign(handIdx - 0.5f);
				float cycleAngle = MathHelper.TwoPi * animationFrame / 120 + handIdx * MathHelper.Pi;
				Vector2 cycleOffset = 8 * cycleAngle.ToRotationVector2();
				offset = baseOffset + cycleOffset;
			} else
			{
				float attackFraction = MathF.Sin(MathHelper.Pi * shootFrame / attackFrames);
				offset = target * attackFraction;
			}
			int handFrame = (handIdx + animationFrame / 10) % 4;
			handFrame = handFrame == 3 ? 1 : handFrame;
			handFrame += firstHandFrame;
			hand.TargetPosition = offset;
			hand.Frame = handFrame;
			hand.SpriteDirection = handIdx == 0 ? 1 : -1;
		}
	}
}
