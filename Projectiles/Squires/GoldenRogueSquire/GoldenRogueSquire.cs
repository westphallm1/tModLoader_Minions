using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire
{
	public class GoldenRogueSquireMinionBuff : MinionBuff
	{
		public GoldenRogueSquireMinionBuff() : base(ProjectileType<GoldenRogueSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Golden Rogue Squire");
			Description.SetDefault("A golden rogue squire will follow your orders!");
		}
	}
	public class GoldenRogueSquireMinionItem : SquireMinionItem<GoldenRogueSquireMinionBuff, GoldenRogueSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Rogue Crest");
			Tooltip.SetDefault("Summons a squire\nA golden rogue squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 4.0f;
			item.width = 24;
			item.height = 38;
			item.damage = 24;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
	}

	public class GoldenDaggerCloud : ModProjectile
	{

		const int TimeToLive = 180;

		public override string Texture => "Terraria/Projectile_" + ProjectileID.MagicDagger;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.width = 12;
			projectile.height = 12;
			projectile.timeLeft = TimeToLive;
			projectile.friendly = false;
			projectile.tileCollide = false;
			projectile.minion = true;
		}

		// ai is wholly controlled by golden rogue squire, but die if squire does
		public override void AI()
		{
			base.AI();
			if(Main.player[projectile.owner].ownedProjectileCounts[ProjectileType<GoldenRogueSquireMinion>()] == 0)
			{
				projectile.Kill();
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			Rectangle bounds = texture.Bounds;
			Vector2 origin = texture.Bounds.Center.ToVector2();
			float spawnPercent = Math.Min(1f, (TimeToLive - projectile.timeLeft) / 5);
			Color color = Color.White * spawnPercent;
			float scale = 1;
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, color, projectile.rotation,
				origin, scale, 0, 0);
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// manually bypass defense
			// this may not be wholly correct
			int defenseBypass = 10;
			int defense = Math.Min(target.defense, defenseBypass);
			damage += defense / 2;
		}
	}

	public class GoldenDagger : ModProjectile
	{

		const int TimeToLive = 60;
		const int TimeLeftToStartFalling = TimeToLive - 15;

		public override string Texture => "Terraria/Projectile_" + ProjectileID.MagicDagger;

		private Vector2 baseVelocity;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.width = 12;
			projectile.height = 12;
			projectile.timeLeft = TimeToLive;
			projectile.friendly = true;
			projectile.tileCollide = true;
			projectile.minion = true;
			baseVelocity = default;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			Rectangle bounds = texture.Bounds;
			Vector2 origin = texture.Bounds.Center.ToVector2();
			Color color = Color.White;
			float scale = 1;
			if (projectile.timeLeft < TimeLeftToStartFalling)
			{
				color.A = 64;
				scale = projectile.timeLeft / (float)TimeLeftToStartFalling;
			}
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, color, projectile.rotation,
				origin, scale, 0, 0);
			return false;
		}
		public override void AI()
		{
			Projectile parent = Main.projectile[(int)projectile.ai[0]];
			if (baseVelocity == default)
			{
				baseVelocity = projectile.velocity;
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			if (parent.active && projectile.timeLeft > TimeLeftToStartFalling)
			{
				projectile.velocity = parent.velocity + baseVelocity;
			}
			else
			{
				projectile.velocity.Y = Math.Min(projectile.velocity.Y + 0.5f, 16);
				projectile.rotation += 0.15f;
				projectile.velocity.X *= 0.99f;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// manually bypass defense
			// this may not be wholly correct
			int defenseBypass = 20;
			int defense = Math.Min(target.defense, defenseBypass);
			damage += defense / 2;
		}
	}

	public class GoldenRogueSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<GoldenRogueSquireMinionBuff>();
		protected override int AttackFrames => 15;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/GoldenWings";

		protected override string WeaponTexturePath => null;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(-4, 4);

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override int SpecialDuration => 3 * 60;
		protected override int SpecialCooldown => 7 * 60;

		private int daggerSpeed = 10;
		private float daggerSpread = 2.25f;
		private int knifeIdx;
		protected NPC targetNPC = default;
		private bool didTeleport;
		private int travelDir;
		private float npcRadius;
		private int maxKnifeCount = 8;
		private int knivesPerRow = 8;

		public GoldenRogueSquireMinion() : base(ItemType<GoldenRogueSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Rogue Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 32;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// glowy golden wings
			return base.PreDraw(spriteBatch, Color.White);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == player.whoAmI)
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

						Projectile.NewProjectile(projectile.Center,
							velocity,
							ProjectileType<GoldenDagger>(),
							projectile.damage,
							projectile.knockBack,
							Main.myPlayer,
							ai0: projectile.whoAmI);
					}
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D glow = GetTexture(Texture + "_Glow");
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = glow.Bounds;
			Vector2 origin = bounds.Center.ToVector2();
			spriteBatch.Draw(glow, pos - Main.screenPosition,
				bounds, Color.White, r, origin, 1, effects, 0);
			if (attackFrame < 10)
			{
				// only draw arm at start of attack
				base.PostDraw(spriteBatch, lightColor);
			}
			// draw a spinning reticle as a visual indicator for the special
			if(player.whoAmI == Main.myPlayer && usingSpecial)
			{
				Texture2D reticle = GetTexture(Texture + "_Reticle");
				bounds = reticle.Bounds;
				origin = bounds.Center.ToVector2();
				r = MathHelper.TwoPi * animationFrame / 120;
				float scale = 1f + 0.2f * (float)Math.Sin(r);
				pos = targetNPC == default ? Main.MouseScreen + 8 * Vector2.One : targetNPC.Center - Main.screenPosition;
				spriteBatch.Draw(reticle, pos, bounds, Color.White, r, origin, scale, effects, 0);
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
			projectile.tileCollide = false;
			projectile.spriteDirection = travelDir;
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
			projectile.Center = targetNPC.Center + offset;
		}

		private void ManageKnifeCloud()
		{
			int cloudSize = player.ownedProjectileCounts[ProjectileType<GoldenDaggerCloud>()];
			if(Main.myPlayer == player.whoAmI && specialFrame % 3 == 0 && cloudSize < maxKnifeCount)
			{
				Projectile.NewProjectile(projectile.Center,
					Vector2.Zero,
					ProjectileType<GoldenDaggerCloud>(),
					projectile.damage,
					projectile.knockBack,
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
				if(p.active && p.type == ProjectileType<GoldenDaggerCloud>() && p.owner == player.whoAmI && p.ai[0] > -1)
				{
					int ai0 = (int)p.ai[0];
					int knifeRow = ai0 / knivesPerRow;
					int knifeIdx = ai0 % knivesPerRow;
					float angleOffset = MathHelper.PiOver4 - knifeIdx * MathHelper.PiOver2 / knivesPerRow;
					float animationSin = (float)Math.Sin(MathHelper.TwoPi * animationFrame / 30);
					angleOffset *= 1 + 0.2f * animationSin * (knifeRow == 0 ? 1 : -1);
					Vector2 baseOffset = (projectile.Center - targetNPC.Center).RotatedBy(angleOffset);
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
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				if(Main.rand.Next(3) > 0)
				{
					continue;
				}
				int goreIdx = Gore.NewGore(projectile.position, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += offset;
			}
		}

		private void LaunchKnives()
		{
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.type == ProjectileType<GoldenDaggerCloud>() && p.owner == player.whoAmI)
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
				if(p.active && p.type == ProjectileType<GoldenDaggerCloud>() && p.owner == player.whoAmI)
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
				projectile.position += vectorToIdle;
				didTeleport = true;
			}
			targetNPC = default;
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override float MaxDistanceFromPlayer() => usingSpecial ? 1400 : 232;

		public override float ComputeTargetedSpeed() => 11;

		public override float ComputeIdleSpeed() => 11;

		protected override float WeaponDistanceFromCenter() => 12;

		public override void AfterMoving()
		{
			base.AfterMoving();
			TeleportDust();
		}
	}
}

