using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Items.Materials;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.GuideSquire;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using ReLogic.Content;

namespace AmuletOfManyMinions.Projectiles.Squires.WoFSquire
{
	public class GuideVoodooSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<GuideVoodooSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Squire");
			Description.SetDefault("You can guide the Guide!");
		}
	}
	public class WoFSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<WoFSquireMinion>() };
		public override void SetStaticDefaults()
		{
			//TODO 1.4 did not call base before
			base.SetStaticDefaults();
			DisplayName.SetDefault("Wall of Flesh Squire");
			Description.SetDefault("You can guide the Wall of Flesh!");
			CrossMod.HookBuffToItemCrossMod(Type, ItemType<GuideVoodooSquireMinionItem>());
		}
		public override void Update(Player player, ref int buffIndex)
		{
			// don't keep self active just because minion is alive
			if (player.ownedProjectileCounts[ProjectileType<WoFSquireMinion>()] == 0)
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class GuideVoodooSquireMinionItem : SquireMinionItem<GuideVoodooSquireMinionBuff, GuideVoodooSquireMinion>
	{
		protected override string SpecialName => "Brutal Dash";
		protected override string SpecialDescription => 
			"Dashes across the whole screen horizontally,\n" +
			"hitting everything in its path";
		private int wofType => ProjectileType<WoFSquireMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("True Guide Voodoo Doll");
			Tooltip.SetDefault("Summons a squire\nClick and hold to guide its attacks!\n'You are a *REALLY* terrible person'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 5f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 140;
			Item.value = Item.sellPrice(0, 0, 1, 0);
			Item.rare = ItemRarityID.Red;
		}

		public override bool CanShoot(Player player)
		{
			if (player.ownedProjectileCounts[wofType] > 0)
			{
				return false;
			}
			return base.CanShoot(player);
		}

		public override bool CanUseItem(Player player)
		{
			base.CanUseItem(player);
			if (player.ownedProjectileCounts[Item.shoot] > 0 || player.ownedProjectileCounts[wofType] > 0)
			{
				Item.UseSound = null;
				Item.noUseGraphic = true;
				Item.useStyle = ItemUseStyleID.Shoot;
			}
			else
			{
				Item.useStyle = ItemUseStyleID.HoldUp;
				Item.noUseGraphic = false;
				Item.UseSound = SoundID.Item44;
			}
			return true;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.GuideVoodooDoll, 1).AddIngredient(ItemType<GuideSquireMinionItem>(), 1).AddIngredient(ItemType<GuideHair>(), 1).AddTile(TileID.LunarCraftingStation).Register();
		}
	}

	public class WoFSquireMinion : SquireMinion
	{
		internal override int BuffId => BuffType<WoFSquireMinionBuff>();
		protected override int ItemType => ItemType<GuideVoodooSquireMinionItem>();
		int dashDirection = 1;
		bool isDashing;

		private Vector2?[] laserTargets;
		private int[] laserFrames;

		static int SpecialChargeTime = 60;

		static int SpecialXRange = 1000;

		static int SpecialLoopSpeed = 40;

		static int SpecialLoopCount = 1;

		// used for tracking special state
		Vector2 specialStartPos;
		int chargeDirection;

		protected override SoundStyle? SpecialStartSound => SoundID.Roar;
		protected override int SpecialDuration => SpecialLoopCount * SpecialLoopSpeed + SpecialLoopSpeed / 2 + SpecialChargeTime;
		protected override int SpecialCooldown => 6 * 60;

		private MotionBlurDrawer blurDrawer;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Squire of Flesh");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
		}
		public override void LoadAssets()
		{
			AddTexture(Texture + "_Shockwave");
			AddTexture(Texture + "_Eye");
			AddTexture(Texture + "_Clingers");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			frameSpeed = 7;
			laserTargets = new Vector2?[2];
			laserFrames = new int[2];
			isDashing = false;
			blurDrawer = new MotionBlurDrawer(5);
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.75f);
			// use very small hit cooldown during special, since the hitbox moves so quickly
			baseLocalIFrames = usingSpecial ? 2 : 10;
			return base.IdleBehavior() + new Vector2(-player.direction * 8, 0);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			isDashing = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			int maxDashDistance = 80;
			float dashCheckDistance = 1.25f * maxDashDistance;
			float searchDistance = 2 * dashCheckDistance;
			int collisionStep = 8;
			isDashing = false;
			Vector2? _nearestNPCVector = AnyEnemyInRange(searchDistance, Projectile.Center);
			if (_nearestNPCVector is Vector2 nearestNPCVector)
			{
				vectorToTargetPosition = nearestNPCVector - Projectile.Center;
			}
			if (_nearestNPCVector != null)
			{
				if (vectorToTargetPosition.Length() < dashCheckDistance)
				{
					isDashing = true;
					for (int i = 0; i < maxDashDistance; i += collisionStep)
					{
						vectorToTargetPosition.X += collisionStep * dashDirection;
						if (!Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, Projectile.position + vectorToTargetPosition, 1, 1))
						{
							break;
						}
					}
					if (vectorToTargetPosition.Length() < 16f)
					{
						dashDirection *= -1;
					}
				}
			}
			base.StandardTargetedMovement(vectorToTargetPosition);
		}

		public override void OnStartUsingSpecial()
		{
			specialStartPos = Projectile.Center;
		}
		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			float targetX;
			if(specialFrame < SpecialChargeTime / 2)
			{
				isDashing = false;
				targetX = MathHelper.Lerp(specialStartPos.X, player.Center.X, 2f * specialFrame / SpecialChargeTime );
				Projectile.spriteDirection = Math.Sign(targetX - player.Center.X);
			} else if (specialFrame < SpecialChargeTime)
			{
				targetX = player.Center.X;
				chargeDirection = Math.Sign(syncedMouseWorld.X - targetX);
				Projectile.spriteDirection = chargeDirection;
			} else
			{
				isDashing = true;
				int dashFrame = (specialFrame - SpecialChargeTime) % SpecialLoopSpeed;
				int xPerFrame =  chargeDirection * SpecialXRange / SpecialLoopSpeed;
				targetX = player.Center.X + 2 * dashFrame * xPerFrame +
					(dashFrame < SpecialLoopSpeed / 2 ? 0 : -2 * chargeDirection * SpecialXRange); 
			}
			float maxYSpeed = 20;
			float yInertia = 8;
			float targetY = vectorToTargetPosition.Y;
			float yVelMagnitude = Math.Sign(targetY) * Math.Min(maxYSpeed, Math.Abs(targetY));
			float yVel = (Projectile.velocity.Y * (yInertia - 1) + yVelMagnitude) / yInertia;
			Projectile.Center = new Vector2(targetX, Projectile.Center.Y);
			Projectile.velocity = Vector2.UnitY * yVel;

			Projectile.tileCollide = false;
		}

		public override void AfterMoving()
		{
			blurDrawer.Update(Projectile.Center, isDashing);
			// also interpolate extra blurs while using special
			if(isDashing && usingSpecial)
			{
				AddShockwaveDust();
			}
		}

		public override void OnStopUsingSpecial()
		{
			for(int i = 0; i < 10; i++)
			{
				AddShockwaveDust(1f);
			}
			returningToPlayer = true;
		}

		private void AddShockwaveDust(float speedMult = 0.25f)
		{
			int boxWidth =  SpecialXRange / SpecialLoopSpeed;
			int boxHeight = 60;
			Vector2 startPos = Projectile.Center + new Vector2(chargeDirection == -1 ? boxWidth : 0, -boxHeight / 2);
			for(int i = 0; i < 2; i++)
			{
				int dustCreated = Dust.NewDust(startPos, boxWidth, boxHeight, 60, boxHeight * speedMult * chargeDirection, 0, 50, Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].velocity *= 0.8f;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(usingSpecial && specialFrame < SpecialChargeTime)
			{
				return false;
			} else if(usingSpecial && isDashing)
			{
				projHitbox.Inflate(128, 64);
				projHitbox.X += -chargeDirection * 64;
				return projHitbox.Intersects(targetHitbox);
			}
			return base.Colliding(projHitbox, targetHitbox);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (isDashing)
			{
				DrawAferImage(ref lightColor);
			}
			if (vectorToTarget != null)
			{
				DrawClingers(ref lightColor);
			}
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			DrawEyes(ref lightColor);
			if(usingSpecial && isDashing)
			{
				DrawSonicBoom(Projectile.Center);
			}
			return false;
		}

		private void DrawSonicBoom(Vector2 center)
		{
			Vector2 offset = new Vector2(96, 0) * chargeDirection;
			float animSin = (float)Math.Sin(MathHelper.TwoPi * animationFrame / 15);
			float scale = 1.1f + 0.1f * animSin;
			// stamp out an "aura" around the main shockwave
			// I'm pretty sure this effect is accomplished via shaders in vanilla, but..
			float offsetScale = 2.5f + 1.5f * animSin;
			for (int i = 0; i < 4; i++)
			{
				// thanks to direwolf420 for this outline effect
				int x = i / 2 % 2 == 0 ? -1 : 1;
				int y = i % 2 == 0 ? -1 : 1;
				Vector2 outlineOffset = new Vector2(x, y) * offsetScale;
				DrawShockwave(center + offset + outlineOffset, Color.Crimson * 0.5f, scale);
			}
			DrawShockwave(center + offset, Color.White * 0.5f, scale);
		}

		private void DrawShockwave(Vector2 center, Color color, float scale)
		{
			float r = Projectile.rotation + Projectile.spriteDirection * MathHelper.PiOver2;
			Texture2D shockwaveTexture = ExtraTextures[0].Value;
			Main.EntitySpriteDraw(shockwaveTexture, center - Main.screenPosition,
				shockwaveTexture.Frame(1, 1), color, r,
				shockwaveTexture.Frame(1, 1).Center.ToVector2(), scale, 0, 0);
		}



		private Vector2 EyePos(int idx)
		{
			Vector2 pos = Projectile.Center;
			pos.X += 8 * Projectile.spriteDirection;
			pos.Y += idx == 0 ? 13 : -15;
			return pos;
		}

		private void DrawEyes(ref Color lightColor)
		{
			Texture2D texture = ExtraTextures[1].Value;
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			for (int i = 0; i < 2; i++)
			{
				float r;
				if (laserTargets[i] is Vector2 target && animationFrame - laserFrames[i] < 10)
				{
					r = target.ToRotation();
				}
				else if (Projectile.spriteDirection == 1)
				{
					r = MathHelper.Pi;
				}
				else
				{
					r = 0;
				}
				Main.EntitySpriteDraw(texture, EyePos(i) - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, 0, 0);
			}
		}

		private void DrawAferImage(ref Color lightColor)
		{
			float r = Projectile.rotation;
			SpriteEffects effects = Projectile.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			for(int i = 0; i < blurDrawer.BlurLength; i++)
			{
				if(!blurDrawer.GetBlurPosAndColor(i, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
				Main.EntitySpriteDraw(texture, blurPos - Main.screenPosition,
					bounds, blurColor, r, origin, 1, effects, 0);
				if(usingSpecial && isDashing)
				{
					float scale = (blurDrawer.BlurLength - i) / (float)(blurDrawer.BlurLength + 1);
					Vector2 offset = new Vector2(96, 0) * chargeDirection;
					DrawShockwave(blurPos + offset, Color.Crimson * scale, scale);
				}
			}
		}

		private void DrawClingers(ref Color lightColor)
		{
			Texture2D texture = ExtraTextures[2].Value;
			int nFrames = 2;
			int frameHeight = texture.Height / nFrames;
			float r = Projectile.spriteDirection == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
			for (int i = 0; i < 3; i++)
			{
				int frame = animationFrame % 60 < 30 ? 0 : 1;
				if (i == 1)
				{
					frame = frame == 1 ? 0 : 1; // flip the frame for the middle clinger
				}
				Rectangle bounds = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				Vector2 pos = Projectile.Center;
				float yOffset = 15 + 4 * (float)Math.Sin(2 * Math.PI * animationFrame / 60f);
				if (i == 0)
				{
					pos.Y -= yOffset;
				}
				else if (i == 2)
				{
					pos.Y += yOffset;
				}
				pos.X += (float)(Projectile.spriteDirection * 20 + 4 * Math.Sin(2 * Math.PI * (animationFrame / 60f + i / 3f)));
				Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, 0, 0);
			}

		}

		public override void Kill(int timeLeft)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				Vector2 goreVelocity = Projectile.velocity;
				goreVelocity.Normalize();
				goreVelocity *= 4f;
				var source = Projectile.GetSource_Death();
				int gore1 = Gore.NewGore(source, Projectile.position, goreVelocity, Mod.Find<ModGore>("WoFEyeGore").Type, 1f);
				int gore2 = Gore.NewGore(source, Projectile.position, goreVelocity, Mod.Find<ModGore>("WoFEyeGore").Type, 1f);
				int gore3 = Gore.NewGore(source, Projectile.position, goreVelocity, Mod.Find<ModGore>("WoFHammerGore").Type, 1f);
				foreach (int gore in new int[] { gore1, gore2, gore3 })
				{
					Main.gore[gore].timeLeft = 180; // make it last not as long
					Main.gore[gore].alpha = 128; // make it transparent
				}
			}
			for (int i = 0; i < 8; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y);
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(usingSpecial)
			{
				// set in SpecialTargetedMovement
			} else if (vectorToIdle.Length() < 32)
			{
				Projectile.spriteDirection = player.direction;
			}
			else if (vectorToTarget is Vector2 target)
			{
				if (target.Length() < 64f && !isDashing)
				{
					Projectile.spriteDirection = player.Center.X - Projectile.Center.X > 0 ? -1 : 1;
				}
				else if (relativeVelocity.X > 2)
				{
					Projectile.spriteDirection = 1;
				}
				else if (relativeVelocity.X < -2)
				{
					Projectile.spriteDirection = -1;
				}
			}
			base.Animate(minFrame, maxFrame);
		}


		public override float ComputeIdleSpeed() => 18;

		// needs to slow down a little so dash is visible
		public override float ComputeTargetedSpeed() => isDashing ? 16 / player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier : 18;

		public override float ComputeInertia() => isDashing ? 4 : base.ComputeInertia();

		public override float MaxDistanceFromPlayer() => usingSpecial ? 2 * SpecialXRange : 700f;
	}

	public class GuideVoodooSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<GuideVoodooSquireMinionBuff>();
		protected override int ItemType => ItemType<GuideVoodooSquireMinionItem>();
		protected override int AttackFrames => 40;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";
		protected override string WeaponTexturePath => null;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override bool attackSpeedCanBeModified => false;

		protected int mockHealth;

		protected int baseDamage;
		protected float baseKnockback;

		protected static int MockMaxHealth = 3;

		protected int knockbackCounter = 0;

		protected override SoundStyle? SpecialStartSound => SoundID.PlayerHit with { Variants = stackalloc int[] { 0 } }; //Works too: new SoundStyle("Terraria/Sounds/Player_Hit_0");

		protected override SoundStyle? attackSound => null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Guide Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}
		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture("Terraria/Images/HealthBar1");
			AddTexture("Terraria/Images/HealthBar2");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
			mockHealth = MockMaxHealth;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			baseDamage = Projectile.originalDamage;
			baseKnockback = Projectile.knockBack;
			Projectile.damage = 1;
			Projectile.originalDamage = 1;
			Projectile.knockBack = 0;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (knockbackCounter < 0)
			{
				Vector2 kbDirection = target.Center - Projectile.Center;
				kbDirection.Normalize();
				kbDirection *= -3.5f;
				Projectile.velocity = target.velocity + kbDirection;
				if (target.CanBeChasedBy())
				{
					mockHealth = Math.Max(0, mockHealth - 1);
				}
				knockbackCounter = 12;
			}
			if (mockHealth == 0)
			{
				Projectile.Kill();
			}
		}

		public override void OnStartUsingSpecial()
		{
			mockHealth = Math.Max(0, mockHealth - 1);
			if(mockHealth == 0)
			{
				Projectile.Kill();
			}
		}

		private byte getGradient(byte b1, byte b2, float weight)
		{
			return (byte)(b2 + (b1 - b2) * (weight));
		}
		public override void PostDraw(Color lightColor)
		{
			if (mockHealth != MockMaxHealth)
			{
				float widthFraction = (new float[] { 0.05f, 0.33f, 0.67f })[mockHealth];

				Color maxHealthColor = new Color(77, 230, 0);
				Color halfHealthColor = new Color(230, 230, 0);
				Color zeroHealthColor = new Color(230, 37, 0);
				byte r, g, b;
				if (widthFraction > 0.5f)
				{
					float weight = 2 * (widthFraction - 0.5f);
					r = getGradient(maxHealthColor.R, halfHealthColor.R, weight);
					g = getGradient(maxHealthColor.G, halfHealthColor.G, weight);
					b = getGradient(maxHealthColor.B, halfHealthColor.B, weight);
				}
				else
				{
					float weight = 2 * (widthFraction);
					r = getGradient(halfHealthColor.R, zeroHealthColor.R, weight);
					g = getGradient(halfHealthColor.G, zeroHealthColor.G, weight);
					b = getGradient(halfHealthColor.B, zeroHealthColor.B, weight);
				}
				Color drawColor = new Color(r, g, b);
				Texture2D healthBar = ExtraTextures[2].Value;
				Texture2D healthBarBack = ExtraTextures[3].Value;
				Rectangle bounds = new Rectangle(0, 0, (int)(healthBar.Width * widthFraction), healthBar.Height);
				Vector2 origin = healthBar.Bounds.Center.ToVector2();
				Vector2 pos = Projectile.Bottom + new Vector2(0, 8);
				Main.EntitySpriteDraw(healthBarBack, pos - Main.screenPosition,
					healthBarBack.Bounds, drawColor, 0,
					healthBarBack.Bounds.Center.ToVector2(), 1, 0, 0);
				Main.EntitySpriteDraw(healthBar, pos - Main.screenPosition,
					bounds, drawColor, 0,
					origin, 1, 0, 0);
			}
			base.PostDraw(lightColor);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (knockbackCounter-- < 0)
			{
				base.StandardTargetedMovement(vectorToTargetPosition);
			}
			else
			{
				Projectile.velocity *= 0.99f;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (knockbackCounter-- < 0)
			{
				base.IdleMovement(vectorToIdlePosition);
			}
			else
			{
				Projectile.velocity *= 0.99f;
			}
		}

		public override void Kill(int timeLeft)
		{
			if (mockHealth == 0)
			{
				if (Main.netMode != NetmodeID.Server)
				{
					Vector2 goreVelocity = Projectile.velocity;
					goreVelocity.Normalize();
					goreVelocity *= 4f;
					var source = Projectile.GetSource_Death();
					int gore1 = Gore.NewGore(source, Projectile.position, goreVelocity, Mod.Find<ModGore>("GuideGore").Type, 1f);
					int gore2 = Gore.NewGore(source, Projectile.position, goreVelocity, Mod.Find<ModGore>("GuideBodyGore").Type, 1f);
					int gore3 = Gore.NewGore(source, Projectile.position, goreVelocity, Mod.Find<ModGore>("GuideLegsGore").Type, 1f);
					foreach (int gore in new int[] { gore1, gore2, gore3 })
					{
						Main.gore[gore].timeLeft = 180; // make it last not as long
						Main.gore[gore].alpha = 128; // make it transparent
					}
				}
				for (int i = 0; i < 6; i++)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y);
				}

				if (player.whoAmI == Main.myPlayer)
				{
					player.AddBuff(BuffType<WoFSquireMinionBuff>(), 60 * 20); // evolved form lasts 20 seconds
					Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ProjectileType<WoFSquireMinion>(), baseDamage, baseKnockback, player.whoAmI);
					p.originalDamage = baseDamage;
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 6;

		public override float ComputeIdleSpeed() => 15;

		public override float ComputeTargetedSpeed() => 15;

		public override float MaxDistanceFromPlayer() => 600f;
	}
}
