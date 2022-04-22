using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
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
	public class MiniPrimeMinionBuff : CombatPetVanillaCloneBuff
	{
		public MiniPrimeMinionBuff() : base(ProjectileType<MiniPrimeMinion>()) { }

		public override int VanillaBuffId => BuffID.SkeletronPrimePet;

		public override string VanillaBuffName => "SkeletronPrimePet";
	}

	public class MiniPrimeMinionItem : CombatPetMinionItem<MiniPrimeMinionBuff, MiniPrimeMinion>
	{
		internal override int VanillaItemID => ItemID.SkeletronPrimePetItem;

		internal override string VanillaItemName => "SkeletronPrimePetItem";
	}


	public class MiniPrimeMinion : SkeletronCombatPet
	{
		internal override int BuffId => BuffType<MiniPrimeMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SkeletronPrimePet;
		internal override int? FiredProjectileId => null;


		internal int handFrames = 4;
		internal int firstHandFrame = 7;

		internal override int GetAttackFrames(ICombatPetLevelInfo info) => Math.Max(20, 45 - 4 * info.Level);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.MiniPrime"));
			Main.projFrames[Projectile.type] = 11;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			hands = new SkeletronHand[4];
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int framesSinceShoot = animationFrame - hsHelper.lastShootFrame;
			bool isLaserFrame = attackCycle == 2 && (framesSinceShoot == attackFrames / 4 || framesSinceShoot == 3 * attackFrames / 4);
			bool isBombFrame = attackCycle == 4 && framesSinceShoot == attackFrames / 4;
			bool shouldShoot = player.whoAmI == Main.myPlayer && (isLaserFrame || isBombFrame);
			if(shouldShoot)
			{
				Vector2 target = vectorToTargetPosition;
				target.SafeNormalize();
				Vector2 spawnPos;
				int damage;
				int projId;
				if(isLaserFrame)
				{
					target *= 12;
					spawnPos = Projectile.Center + hands[1].Position;
					damage = Projectile.damage;
					projId = ProjectileType<MiniTwinsLaser>();
					SoundEngine.PlaySound(new LegacySoundStyle(2, 10).WithVolume(0.5f), Projectile.Center);
				} else 
				{
					target *= 8;
					spawnPos = Projectile.Center + hands[3].Position;
					damage = 3 * Projectile.damage / 2;
					projId = ProjectileType<PirateCannonball>();
					SoundEngine.PlaySound(new LegacySoundStyle(2, 11).WithVolume(0.5f), Projectile.Center);
				}
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					spawnPos,
					VaryLaunchVelocity(target),
					projId,
					damage,
					Projectile.knockBack,
					player.whoAmI);

			}
		}
		internal override void UpdateHand(ref SkeletronHand hand, int handIdx)
		{
			// very hacky way to get -1 and 1
			Vector2 offset;
			int shootFrame = animationFrame - hsHelper.lastShootFrame;
			if(attackCycle > 4 || handIdx != attackCycle - 1 || vectorToTarget is not Vector2 target || shootFrame > attackFrames)
			{
				Vector2 baseOffset = 32 * Vector2.UnitX * (handIdx % 2 == 0 ? -1 : 1);
				baseOffset += 16 * Vector2.UnitY * (handIdx > 1 ? -1 : 1);
				float cycleAngle = MathHelper.TwoPi * animationFrame / 120 + handIdx * MathHelper.Pi/2;
				Vector2 cycleOffset = 8 * cycleAngle.ToRotationVector2();
				offset = baseOffset + cycleOffset;
				hand.Rotation = 0;
				hand.SpriteDirection = forwardDir * Math.Sign(Projectile.velocity.X);
			} else
			{
				float attackFraction = MathF.Sin(MathHelper.Pi * shootFrame / attackFrames);
				if(handIdx == 0 || handIdx == 2)
				{
					offset = target * attackFraction;
				} else
				{
					target.SafeNormalize();
					offset = target * Math.Min(32, 64 * attackFraction);
				}
				hand.SpriteDirection = forwardDir;
				hand.Rotation = target.ToRotation();
			}
			int handFrame = handIdx + firstHandFrame;
			hand.TargetPosition = offset;
			hand.Frame = handFrame;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			for(int i = 0; i < hands.Length; i++)
			{
				hands[i].SpriteDirection = Projectile.spriteDirection;
			}
		}
	}

}
