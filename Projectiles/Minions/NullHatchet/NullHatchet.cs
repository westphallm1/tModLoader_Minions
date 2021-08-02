using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.NullHatchet
{
	public class NullHatchetMinionBuff : MinionBuff
	{
		public NullHatchetMinionBuff() : base(ProjectileType<NullHatchetMinion>(), ProjectileType<NullHatchetMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Null Hatchet");
			Description.SetDefault("An ethereal axe will fight for you!");
		}
	}

	public class NullHatchetMinionItem : MinionItem<NullHatchetMinionBuff, NullHatchetMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Null Hatchet");
			Tooltip.SetDefault("Summons an ethereal axe to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 32;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 34;
			Item.height = 34;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			base.Shoot(player, source, position, velocity, type, damage, knockback);
			Projectile.NewProjectile(source, position - new Vector2(5, 0), velocity, Item.shoot, damage, knockback, Main.myPlayer);
			Projectile.NewProjectile(source, position - new Vector2(5, 0), velocity, Item.shoot, damage, knockback, Main.myPlayer);
			return false;
		}
	}


	public class NullHatchetMinion : TeleportingWeaponMinion
	{

		internal override int BuffId => BuffType<NullHatchetMinionBuff>();
		float windUpPerFrame = MathHelper.Pi / 60;
		float swingPerFrame = MathHelper.Pi / 20;
		float initialWindUp = MathHelper.PiOver4;
		private Vector2 swingCenter = default;
		protected override Vector3 lightColor => Color.Red.ToVector3() * 0.75f;
		protected override int maxFramesInAir => 20;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Null Hatchet");
		}


		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = false;
			attackState = AttackState.IDLE;
			Projectile.minionSlots = 1;
			attackFrames = 60;
			attackThroughWalls = true;
			useBeacon = false;
			travelVelocity = 16;
			maxPhaseFrames = 30;
			targetIsDead = false;
		}


		public override bool PreDraw(ref Color lightColor)
		{
			int alpha = 128;
			float phaseLength = maxPhaseFrames / 2;
			if (phaseFrames > 0 && phaseFrames < phaseLength)
			{
				alpha -= (int)(128 * phaseFrames / phaseLength);
			}
			else if (phaseFrames >= phaseLength && phaseFrames < maxPhaseFrames)
			{
				alpha = (int)(128 * (phaseFrames - phaseLength) / phaseLength);
			}
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, alpha);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;


			int height = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * height, texture.Width, height);
			Vector2 origin = bounds.Size() / 2;
			float r = Projectile.spriteDirection == 1 ? Projectile.rotation - MathHelper.PiOver4 : Projectile.rotation + MathHelper.PiOver4;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1, effects, 0);
			return false;
		}
		public override void WindUpBehavior(ref Vector2 vectorToTargetPosition)
		{
			//TODO void knife ai
			//This section might require a slight change of the behavior regarding the teleporting to work properly for MP
			//Randomized stuff should only be decided by the client
			//That would require a change of the ai so it doesnt move for other clients during this phase
			float swingDistance = 80;
			if (Main.myPlayer == player.whoAmI && distanceFromFoe == default)
			{
				distanceFromFoe = swingDistance + Main.rand.Next(-20, 20); ;
				teleportAngle = Main.rand.NextFloat(MathHelper.TwoPi);
				Projectile.netUpdate = true;
				//Don't change position continuously, bandaid fix until a proper way for it to work in MP is figured out
			}
			else if (distanceFromFoe != default)
			{
				int swingFrame = phaseFrames - maxPhaseFrames / 2;
				swingCenter = targetNPC.Center + teleportDirection * distanceFromFoe;
				teleportDirection = teleportAngle.ToRotationVector2();
				// move to fixed position relative to NPC, preDraw will do phase in animation
				float swingAngle = (teleportAngle + MathHelper.Pi + initialWindUp + windUpPerFrame * swingFrame);
				Vector2 swingAngleVector = swingAngle.ToRotationVector2();
				Projectile.rotation = swingAngle + MathHelper.PiOver2;
				Projectile.Center = swingCenter + swingAngleVector * distanceFromFoe;
			}
		}

		public override void SwingBehavior(ref Vector2 vectorToTargetPosition)
		{
			if (framesInAir++ > maxFramesInAir)
			{
				targetNPC = null;
				attackState = AttackState.RETURNING;
				return;
			}

			if (targetNPC != null && !targetIsDead)
			{
				swingCenter = targetNPC.Center + teleportDirection * distanceFromFoe;
			}
			teleportDirection = teleportAngle.ToRotationVector2();
			// move to fixed position relative to NPC, preDraw will do phase in animation
			float swingAngle = (teleportAngle + MathHelper.Pi + initialWindUp + windUpPerFrame * maxPhaseFrames / 2 - swingPerFrame * framesInAir);
			Vector2 swingAngleVector = swingAngle.ToRotationVector2();
			Projectile.rotation = swingAngle + MathHelper.PiOver2;
			Projectile.Center = swingCenter + swingAngleVector * distanceFromFoe;
			if (framesInAir % 3 == 0)
			{
				Dust.NewDust(Projectile.Center, 8, 8, 235);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(teleportAngle);
			writer.Write(distanceFromFoe);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			teleportAngle = reader.ReadSingle();
			distanceFromFoe = reader.ReadSingle();
		}

		internal override void OnAcquireTarget(Vector2 vectorToTargetPosition)
		{
			Projectile.spriteDirection = -1;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (attackState == AttackState.IDLE || phaseFrames < maxPhaseFrames / 2)
			{
				Projectile.spriteDirection = Projectile.Center.X > player.Center.X ? 1 : -1;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
		}
	}
}
