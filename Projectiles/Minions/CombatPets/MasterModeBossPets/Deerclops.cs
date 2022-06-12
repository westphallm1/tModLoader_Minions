using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class DeerclopsMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<DeerclopsMinion>() };
		public override string VanillaBuffName => "DeerclopsPet";
		public override int VanillaBuffId => BuffID.DeerclopsPet;
	}

	public class DeerclopsMinionItem : CombatPetMinionItem<DeerclopsMinionBuff, DeerclopsMinion>
	{
		internal override string VanillaItemName => "DeerclopsPetItem";
		internal override int VanillaItemID => ItemID.DeerclopsPetItem;
	}

	public class DeerclopsShadowHand : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.InsanityShadowFriendly;
		int TimeToLive = 60;
		NPC targetNPC;
		Vector2 baseOffset;
		float fadeInFrames = 20;
		float travelDistance;
		float travelVelocity = 8;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = TimeToLive;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void AI()
		{
			if(Projectile.ai[0] == -1)
			{
				Projectile.Kill();
				return;
			}

			if(targetNPC == default)
			{
				targetNPC = Main.npc[(int)Projectile.ai[0]];
				baseOffset = Projectile.Center - targetNPC.Center;
				travelDistance = baseOffset.Length();
				Projectile.spriteDirection = -Math.Sign(baseOffset.X);
				Projectile.rotation = baseOffset.ToRotation() + (Projectile.spriteDirection == 1 ? MathHelper.Pi : 0);
			}

			bool isMoving = TimeToLive - Projectile.timeLeft > 20;
			Projectile.friendly = isMoving;
			if(targetNPC.active)
			{
				Projectile.Center = targetNPC.Center + baseOffset;
				Projectile.velocity = Vector2.Zero;
				Vector2 offset = baseOffset;
				offset.SafeNormalize();
				Projectile.position = targetNPC.Center + travelDistance * offset;
				if(isMoving)
				{
					travelDistance -= travelVelocity;
				} else
				{
					travelDistance += 1;
				}
			} else 			
			{
				Projectile.timeLeft = Math.Min(Projectile.timeLeft, (int)fadeInFrames);
			}
			Projectile.Opacity = isMoving ? Math.Min(1, Projectile.timeLeft/fadeInFrames) : Math.Min(1, (TimeToLive - Projectile.timeLeft)/fadeInFrames);
			if(Projectile.timeLeft < fadeInFrames)
			{
				Projectile.rotation += -Math.Sign(baseOffset.X) * MathHelper.TwoPi / fadeInFrames;
				travelVelocity *= 0.75f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			Rectangle bounds = texture.Bounds;
			float scale = 0.5f;
			Color glowColor = Color.Violet * Projectile.Opacity * 0.5f;
			Color mainColor = Color.Black * Projectile.Opacity;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			OutlineDrawer.DrawOutline(texture, pos, bounds, glowColor, Projectile.rotation, effects);
			Main.EntitySpriteDraw(texture, pos,
				bounds, mainColor, Projectile.rotation, bounds.GetOrigin(), scale, effects, 0);
			return false;
		}
	}

	public class DeerclopsIceBlock : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeerclopsRangedProjectile;
		int TimeToLive = 60;
		float travelDistance;
		float travelVelocity = 8;

		int frame = -1;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = TimeToLive;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft < 50 && Projectile.velocity.Y < 16)
			{
				Projectile.velocity.Y += 0.5f;
			}
			if(frame == -1)
			{
				frame = Main.rand.Next(5);
			}
			Projectile.rotation += Math.Sign(Projectile.velocity.X) * MathHelper.TwoPi / 30;
			Projectile.tileCollide = Projectile.velocity.Y > 0;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.Inflate(24, 24);
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Ice);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameWidth = texture.Width / 3;
			int frameHeight = texture.Height / 4;

			int xOffset = frame % 3;
			int yOffset = 2 + frame / 3;
			Rectangle bounds = new Rectangle(xOffset * frameWidth, yOffset * frameHeight, frameWidth, frameHeight);
			Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, lightColor, Projectile.rotation, origin, 0.75f, 0, 0);
			return false;
		}
	}

	public class DeerclopsMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeerclopsPet;
		internal override int BuffId => BuffType<DeerclopsMinionBuff>();
		internal override int? ProjId => ProjectileType<DeerclopsShadowHand>();

		int attackCycle = 0;

		Vector2 rockStormStart;
		int rockStormDirection;

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(attackCycle % 5 != 3)
			{
				NPC target;
				if(targetNPCIndex is int idx)
				{
					 target = Main.npc[idx];
				} else if (GetClosestEnemyToPosition(Projectile.Center, 2.5f * preferredDistanceFromTarget, false) is NPC anyTarget)
				{
					// manually re-find the nearby enemy if needed, do not know why this is needed
					target = anyTarget;
				} else
				{
					return;
				}
				float offsetRadius = Main.rand.Next(48, 64) + (target.width + target.height) / 4;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					target.Center + Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * offsetRadius,
					Vector2.Zero,
					(int)ProjId,
					(int)(ModifyProjectileDamage(leveledPetPlayer.PetLevelInfo) * Projectile.damage),
					Projectile.knockBack,
					player.whoAmI,
					ai0: target.whoAmI);
			}
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -40);
			ConfigureFrames(17, (0, 0), (5, 12), (4, 4), (13, 16));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int blinkCycle = 115;
			int idleFrame = animationFrame % blinkCycle;
			frameInfo[GroundAnimationState.STANDING] = idleFrame < blinkCycle - 20 ? (0, 0) : (0, 4);
			base.Animate(minFrame, maxFrame);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int framesSinceShoot = animationFrame - lastFiredFrame;
			if(framesSinceShoot == 0)
			{
				attackCycle++;
				if(attackCycle % 5 == 4 && targetNPCIndex is int idx)
				{
					NPC targetNPC = Main.npc[idx];
					rockStormDirection = Math.Sign(vectorToTargetPosition.X);
					rockStormStart = Projectile.Bottom + Vector2.UnitX * rockStormDirection * 32; 
				} else
				{
					rockStormStart = default;
				}
			}
			if(player.whoAmI == Main.myPlayer && attackCycle % 5 == 4 && framesSinceShoot < 30 && framesSinceShoot % 6 == 0 && rockStormStart != default)
			{
				Vector2 launchPos = rockStormStart - new Vector2(0, 6);
				launchPos.X += rockStormDirection * 1.25f * framesSinceShoot + Main.rand.Next(-4, 4);
				Vector2 launchVelocity = new Vector2(rockStormDirection * (0.5f + framesSinceShoot / 8) + Projectile.velocity.X, -6);
				if(Main.rand.NextBool(4))
				{
					launchVelocity.Y -= Main.rand.Next(2);
				}
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					launchPos,
					launchVelocity,
					ProjectileType<DeerclopsIceBlock>(),
					(int)(ModifyProjectileDamage(leveledPetPlayer.PetLevelInfo) * Projectile.damage),
					Projectile.knockBack,
					player.whoAmI);
			}
		}
	}
}
