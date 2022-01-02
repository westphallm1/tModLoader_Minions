using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SeaSquire
{
	public class SeaSquireMinionBuff : MinionBuff
	{
		public SeaSquireMinionBuff() : base(ProjectileType<SeaSquireMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sea Squire");
			Description.SetDefault("A flying fish will follow your fancies!");
		}
	}

	public class SeaSquireMinionItem : SquireMinionItem<SeaSquireMinionBuff, SeaSquireMinion>
	{
		protected override string SpecialName => "Shark Form";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Sea");
			Tooltip.SetDefault("Summons a squire\nA flying fish squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 7f;
			Item.width = 28;
			Item.height = 32;
			Item.damage = 10;
			Item.value = Item.buyPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Coral, 3).AddIngredient(ItemID.Starfish, 3).AddIngredient(ItemID.Seashell, 3).AddIngredient(ItemID.SharkFin, 1).AddTile(TileID.Anvils).Register();
		}
	}

	public abstract class BaseMinionBubble : ModProjectile
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SeaSquire/SeaSquireBubble";
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.CloneDefaults(ProjectileID.Bubble);
			Projectile.alpha = 240;
			Projectile.timeLeft = 180;
			Projectile.penetrate = 1;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Wet, 300);
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(new LegacySoundStyle(2, 54), Projectile.position);
			for (int i = 0; i < 8; i++)
			{
				int dustCreated = Dust.NewDust(Projectile.Center, 1, 1, 137, Projectile.velocity.X, Projectile.velocity.Y, 0, Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
			}
		}

	}
	public class SeaSquireBubble : BaseMinionBubble
	{
		public override void SetStaticDefaults()
		{
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}
	}

	public class SeaSquireSharkMinion : SquireMinion
	{
		internal int dashDirection = 1;
		private bool isDashing;
		private MotionBlurDrawer blurHelper;
		internal override int BuffId => BuffType<SeaSquireMinionBuff>();
		public SeaSquireSharkMinion() : base(ItemType<SeaSquireMinionItem>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sea Squire Shark");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;
		}
		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			frameSpeed = 10;
			blurHelper = new MotionBlurDrawer(5);
			Projectile.minionSlots = 0;
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			Vector2 target = vectorToTargetPosition;
			if(target.LengthSquared() < 128 * 128)
			{
				isDashing = true;
				for(int i = 0; i < 4; i++)
				{
					Vector2 nextPos = target + dashDirection * 16 * Vector2.UnitX;
					if(Collision.CanHitLine(target, 1, 1, nextPos, 1, 1))
					{
						target = nextPos;
					} else
					{
						break;
					}
				}
				if(target.LengthSquared() < 32 * 32)
				{
					dashDirection *= -1;
				} 
			} else
			{
				isDashing = false;
			}
			base.StandardTargetedMovement(target);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			isDashing = false;
		}

		public override float MaxDistanceFromPlayer() => 280;

		public override float ComputeTargetedSpeed() => isDashing ? 18 : 14;

		public override float ComputeIdleSpeed() => 14;

		public override void AfterMoving()
		{
			if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = 1;
				Projectile.rotation = Projectile.velocity.ToRotation();
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = -1;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
			}
			// left shift old position
			blurHelper.Update(Projectile.Center, isDashing);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			if(isDashing)
			{
				for (int k = 0; k < blurHelper.BlurLength; k++)
				{
					if(!blurHelper.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
					blurPos = blurPos - Main.screenPosition;
					Main.EntitySpriteDraw(texture, blurPos, bounds, blurColor, r, origin, 1, effects, 0);
				}
			}
			// regular version
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, origin, 1, effects, 0);
			return false;
		}
	}

	public class SeaSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<SeaSquireMinionBuff>();
		protected override int AttackFrames => 35;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "Terraria/Images/Item_" + ItemID.Trident;

		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);
		protected override float projectileVelocity => 8;

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;
		public SeaSquireMinion() : base(ItemType<SeaSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sea Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 30;
			Projectile.height = 32;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			// the spear does half damage, this is re-multiplied by 2 to get the bubble damage
			// maybe a little bit iffy
			Projectile.damage = Projectile.damage / 2;
		}

		private void TransformBubbles()
		{
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				int goreIdx = Gore.NewGore(Projectile.position, Vector2.Zero, Mod.Find<ModGore>("SeaSquireBubbleGore").Type, 1f);
				Main.gore[goreIdx].alpha = 128;
				Main.gore[goreIdx].velocity *= 0.25f;
				Main.gore[goreIdx].velocity += offset;
			}
		}

		public override void OnStartUsingSpecial()
		{
			TransformBubbles();
			if(player.whoAmI == Main.myPlayer)
			{
				Projectile p = Projectile.NewProjectileDirect(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.Center, 
					Projectile.velocity, 
					ProjectileType<SeaSquireSharkMinion>(), 
					3 * Projectile.damage, 
					Projectile.knockBack, 
					player.whoAmI);
				p.originalDamage = 3 * Projectile.originalDamage;
			}
		}

		public override void OnStopUsingSpecial()
		{
			TransformBubbles();
			if(player.whoAmI == Main.myPlayer)
			{
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if(p.active && p.owner == player.whoAmI && p.type == ProjectileType<SeaSquireSharkMinion>())
					{
						p.Kill();
						break;
					}
				}
			}
		}

		protected override float WeaponDistanceFromCenter()
		{
			//All of this is based on the weapon sprite and AttackFrames above.
			int reachFrames = AttackFrames / 2; //A spear should spend half the AttackFrames extending, and half retracting by default.
			int spearLength = WeaponTexture.Width(); //A decent aproximation of how long the spear is.
			int spearStart = (spearLength / 3 - 10); //Two thirds of the spear starts behind by default. Subtract to start it further out since this one is puny.
			float spearSpeed = spearLength / reachFrames; //A calculation of how quick the spear should be moving.
			if (attackFrame <= reachFrames)
			{
				return spearSpeed * attackFrame - spearStart;
			}
			else
			{
				return (spearSpeed * reachFrames - spearStart) - spearSpeed * (attackFrame - reachFrames);
			}
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			float bubbleVelOffset = Main.rand.NextFloat() * 2;
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				SoundEngine.PlaySound(new LegacySoundStyle(2, 85), Projectile.position);
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= ModifiedProjectileVelocity() + bubbleVelOffset;
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						Projectile.Center,
						angleVector,
						ProjectileType<SeaSquireBubble>(),
						Projectile.damage * 2,
						Projectile.knockBack / 4,
						Main.myPlayer);
				}
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == ProjectileType<SeaSquireSharkMinion>())
				{
					Projectile.Center = p.Center;
					break;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return !usingSpecial && base.PreDraw(ref lightColor);
		}

		public override void PostDraw(Color lightColor)
		{
			if(!usingSpecial)
			{
				base.PostDraw(lightColor);
			}
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 15;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 25;

		public override float MaxDistanceFromPlayer() => 160;

		public override float ComputeTargetedSpeed() => 8;

		public override float ComputeIdleSpeed() => 8;
	}
}
