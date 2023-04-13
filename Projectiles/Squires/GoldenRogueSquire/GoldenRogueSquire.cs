using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire
{
	public class GoldenRogueSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<GoldenRogueSquireMinion>() };
	}
	public class GoldenRogueSquireMinionItem : SquireMinionItem<GoldenRogueSquireMinionBuff, GoldenRogueSquireMinion>
	{
		protected override string SpecialName => "Cloud of Knives";
		protected override string SpecialDescription =>
			"Teleports to the enemy nearest the cursor\n" +
			"and throws a barrage of knives at them";
		
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGEDNOINSTASTRIKE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 4.0f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 34;
			Item.value = Item.buyPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Yellow;
		}
	}

	public class GoldenDaggerCloud : ModProjectile
	{

		const int TimeToLive = 180;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicDagger;
		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			//Projectile.minion = true; //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
		}

		// ai is wholly controlled by golden rogue squire, but die if squire does
		public override void AI()
		{
			base.AI();
			if(Main.player[Projectile.owner].ownedProjectileCounts[ProjectileType<GoldenRogueSquireMinion>()] == 0)
			{
				Projectile.Kill();
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rectangle bounds = texture.Bounds;
			float spawnPercent = Math.Min(1f, (TimeToLive - Projectile.timeLeft) / 5);
			Color color = Color.White * spawnPercent;
			float scale = 1;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, color, Projectile.rotation,
				bounds.GetOrigin(), scale, 0, 0);
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.ArmorPenetration += 20;
		}
	}

	public class GoldenDagger : ModProjectile
	{

		const int TimeToLive = 30;
		const int TimeLeftToStartFalling = TimeToLive - 15;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicDagger;

		private Vector2 baseVelocity;
		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 2;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			//Projectile.minion = true; //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
			Projectile.usesLocalNPCImmunity = true;
			baseVelocity = default;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rectangle bounds = texture.Bounds;
			Color color = Color.White;
			float scale = 1;
			if (Projectile.timeLeft < TimeLeftToStartFalling)
			{
				color.A = 64;
				scale = Projectile.timeLeft / (float)TimeLeftToStartFalling;
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, color, Projectile.rotation, bounds.GetOrigin(), scale, 0, 0);
			return false;
		}
		public override void AI()
		{
			Projectile parent = Main.projectile[(int)Projectile.ai[0]];
			if (baseVelocity == default)
			{
				baseVelocity = Projectile.velocity;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			if (parent.active && Projectile.timeLeft > TimeLeftToStartFalling)
			{
				Projectile.velocity = parent.velocity + baseVelocity;
			}
			else
			{
				Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.5f, 16);
				Projectile.rotation += 0.15f;
				Projectile.velocity.X *= 0.99f;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.ArmorPenetration += 20;
		}
	}

	public class GoldenRogueSquireMinion : WeaponHoldingSquire
	{
		public override int BuffId => BuffType<GoldenRogueSquireMinionBuff>();
		protected override int ItemType => ItemType<GoldenRogueSquireMinionItem>();
		protected override int AttackFrames => 12;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/GoldenWings";

		protected override string WeaponTexturePath => null;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(-4, 4);

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override int SpecialDuration => 3 * 60;
		protected override int SpecialCooldown => 10 * 60;

		private int daggerSpeed = 10;
		private float daggerSpread = 2.25f;
		private int knifeIdx;
		protected NPC targetNPC = default;
		private bool didTeleport;
		private int travelDir;
		private float npcRadius;
		private int maxKnifeCount = 8;
		private int knivesPerRow = 8;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}
		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture(Texture + "_Glow");
			AddTexture(Texture + "_Reticle");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 32;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// glowy golden wings
			lightColor = Color.White;
			return base.PreDraw(ref lightColor);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
					vector2Mouse *= daggerSpeed;
					Vector2 tangent = new Vector2(vector2Mouse.Y, -vector2Mouse.X);
					tangent.Normalize();
					tangent *= daggerSpread;
					Vector2[] velocities =
					{
						vector2Mouse - tangent,
						vector2Mouse,
						vector2Mouse + tangent
					};
					foreach (Vector2 velocity in velocities)
					{

						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(), 
							Projectile.Center,
							velocity,
							ProjectileType<GoldenDagger>(),
							Projectile.damage,
							Projectile.knockBack,
							Main.myPlayer,
							ai0: Projectile.whoAmI);
					}
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D glow = ExtraTextures[2].Value;
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = glow.Bounds;
			Main.EntitySpriteDraw(glow, pos - Main.screenPosition,
				bounds, Color.White, r, bounds.GetOrigin(), 1, effects, 0);
			if (attackFrame < 10)
			{
				// only draw arm at start of attack
				base.PostDraw(lightColor);
			}
			// draw a spinning reticle as a visual indicator for the special
			if(Player.whoAmI == Main.myPlayer && usingSpecial)
			{
				Texture2D reticle = ExtraTextures[3].Value;
				bounds = reticle.Bounds;
				r = MathHelper.TwoPi * AnimationFrame / 120;
				float scale = 1f + 0.2f * (float)Math.Sin(r);
				pos = targetNPC == default ? Main.MouseScreen + 8 * Vector2.One : targetNPC.Center - Main.screenPosition;
				Main.EntitySpriteDraw(reticle, pos, bounds, Color.White, r, bounds.GetOrigin(), scale, effects, 0);
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(targetNPC != default && !targetNPC.active)
			{
				ClearKnives();
				targetNPC = default;
			}
			if(targetNPC == default && GetClosestEnemyToPosition(syncedMouseWorld, 200f, false) is NPC target)
			{
				targetNPC = target;
				// try to teleport behind the enemy
				travelDir = Math.Sign((syncedMouseWorld - targetNPC.Center).X);
			    npcRadius = Math.Max(64, (targetNPC.width + targetNPC.height) / 2);
				didTeleport = true;
			}
            if(targetNPC == default)
			{
				base.StandardTargetedMovement(vectorToTargetPosition);
				return;
			}
			HoverByTargetNPC();
			ManageKnifeCloud();
		}

		private void HoverByTargetNPC()
		{
			Projectile.tileCollide = false;
			Projectile.spriteDirection = travelDir;
			Vector2 offset = syncedMouseWorld - targetNPC.Center;
			offset.Y *= 0.5f;
			if(Math.Abs(offset.Y) > npcRadius)
			{
				offset.Y = Math.Sign(offset.Y) * npcRadius;
			}
			if(Math.Sign(offset.X) != travelDir)
			{
				offset.X *= -1;
			}
			offset.SafeNormalize();
			offset *= npcRadius;
			Projectile.Center = targetNPC.Center + offset;
		}

		private void ManageKnifeCloud()
		{
			int cloudSize = Player.ownedProjectileCounts[ProjectileType<GoldenDaggerCloud>()];
			if(Main.myPlayer == Player.whoAmI && specialFrame % 3 == 0 && cloudSize < maxKnifeCount)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					Vector2.Zero,
					ProjectileType<GoldenDaggerCloud>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: knifeIdx);
				knifeIdx++;
			} 
			PositionKnives();
			if (cloudSize == maxKnifeCount)
			{
				LaunchKnives();
			}
		}

		private void PositionKnives()
		{
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.type == ProjectileType<GoldenDaggerCloud>() && p.owner == Player.whoAmI && p.ai[0] > -1)
				{
					int ai0 = (int)p.ai[0];
					int knifeRow = ai0 / knivesPerRow;
					int knifeIdx = ai0 % knivesPerRow;
					float angleOffset = MathHelper.PiOver4 - knifeIdx * MathHelper.PiOver2 / knivesPerRow;
					float animationSin = (float)Math.Sin(MathHelper.TwoPi * AnimationFrame / 30);
					angleOffset *= 1 + 0.2f * animationSin * (knifeRow == 0 ? 1 : -1);
					Vector2 baseOffset = (Projectile.Center - targetNPC.Center).RotatedBy(angleOffset);
					baseOffset.SafeNormalize();
					baseOffset *= (npcRadius + (32 + 6 * animationSin)* (1+knifeRow));
					p.rotation = baseOffset.ToRotation() - MathHelper.PiOver2;
					p.position = targetNPC.Center + baseOffset;
				}
			}
		}

		private void TeleportDust()
		{
			if(!didTeleport)
			{
				return;
			}
			didTeleport = false;
			float goreVel = 0.25f;
			var source = Projectile.GetSource_FromThis();
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				if(Main.rand.Next(3) > 0)
				{
					continue;
				}
				int goreIdx = Gore.NewGore(source, Projectile.position, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += offset;
			}
		}

		private void LaunchKnives()
		{
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.type == ProjectileType<GoldenDaggerCloud>() && p.owner == Player.whoAmI)
				{
					p.ai[0] = -1;
					p.timeLeft = Math.Min(p.timeLeft, 15);
					p.friendly = true;
					Vector2 velocity = (p.rotation - MathHelper.PiOver2).ToRotationVector2();
					velocity.SafeNormalize();
					velocity *= 20;
					p.velocity = velocity;
				}
			}
			knifeIdx = 0;
		}

		private void ClearKnives()
		{
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.type == ProjectileType<GoldenDaggerCloud>() && p.owner == Player.whoAmI)
				{
					p.Kill();
				}
			}
			knifeIdx = 0;
		}

		public override void OnStopUsingSpecial()
		{
			knifeIdx = 0;
			if(targetNPC != default)
			{
				LaunchKnives();
				// teleport back to player
				Projectile.position += VectorToIdle;
				didTeleport = true;
			}
			targetNPC = default;
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override float MaxDistanceFromPlayer() => usingSpecial ? 1400 : 300;

		public override float ComputeTargetedSpeed() => 14;

		public override float ComputeIdleSpeed() => 14;

		protected override float WeaponDistanceFromCenter() => 12;

		public override void AfterMoving()
		{
			base.AfterMoving();
			TeleportDust();
		}
	}
}

