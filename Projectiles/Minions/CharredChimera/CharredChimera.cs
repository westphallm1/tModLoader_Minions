using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CharredChimera
{
	public class CharredChimeraMinionBuff : MinionBuff
	{
		public CharredChimeraMinionBuff() : base(ProjectileType<CharredChimeraCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Charred Chimera");
			Description.SetDefault("A charred chimera will fight for you!");
		}
	}

	public class CharredChimeraMinionItem : MinionItem<CharredChimeraMinionBuff, CharredChimeraCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Charred Spinal Cord");
			Tooltip.SetDefault("Summons a charred chimera fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 48;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 2, 0);
			Item.rare = ItemRarityID.Yellow;
		}
	}

	public class CharredChimeraMinionHead : GroupAwareMinion
	{

		public override string Texture => "Terraria/Images/Item_0";

		internal override int BuffId => BuffType<CharredChimeraMinionBuff>();

		int speed = 8;
		int inertia = 16;
		int framesSinceLastHit;
		int hitsSinceRetreat;
		Projectile body = default;

		// overwrites default usage in GroupAwareMinion
		float attackingFlag
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 2;
			// ProjectileID.Sets.CountsAsHoming[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minion = false;
			Projectile.minionSlots = 0;
			useBeacon = false;
			Projectile.width = 16;
			Projectile.height = 16;
			attackFrames = 180;
			framesSinceLastHit = 5;
			hitsSinceRetreat = 0;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override Vector2? FindTarget()
		{
			int maxHits = Math.Max(1, 10 - GetActiveMinions().Count());
			if (vectorToIdle.Length() > 300f || attackingFlag == 0 || hitsSinceRetreat > maxHits)
			{
				return null;
			}
			if (SelectedEnemyInRange(300f, 0f, maxRangeFromPlayer: false) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			framesSinceLastHit = 0;
			Projectile.velocity = -Projectile.oldVelocity;
			hitsSinceRetreat++;
		}

		public override Vector2 IdleBehavior()
		{
			body = GetMinionsOfType(ProjectileType<CharredChimeraMinion>()).FirstOrDefault();
			framesSinceLastHit++;
			Projectile.ai[0] = (Projectile.ai[0] + 1) % attackFrames;
			if (body == default)
			{
				return Vector2.Zero;
			}
			else
			{
				return body.Center - Projectile.Center;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			int speed = 16;
			int inertia = 4;
			if (vectorToIdlePosition.Length() < 16)
			{
				// the body is responsible for controlling the heads when they're "attached"
				Projectile.ai[1] = 0;
				hitsSinceRetreat = 0;
				return;
			}
			else if (vectorToIdlePosition.Length() > 300f)
			{
				speed = 20;
				inertia = 8;
			}
			vectorToIdlePosition.Normalize();
			vectorToIdlePosition *= speed;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (framesSinceLastHit < 5)
			{
				return;
			}
			attackingFlag = 1;
			DistanceFromGroup(ref vectorToTargetPosition, closeDistance: 300);
			int speed = this.speed + (vectorToTargetPosition.Length() < 48 ? 4 : 0);
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= speed;
			if (body != default)
			{
				Projectile.spriteDirection = -Math.Sign((body.Center - Projectile.Center).X);
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// no op
		}
	}
	public class CharredChimeraCounterMinion : CounterMinion
	{
		internal override int BuffId => BuffType<CharredChimeraMinionBuff>();
		protected override int MinionType => ProjectileType<CharredChimeraMinion>();
	}
	public class CharredChimeraMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<CharredChimeraMinionBuff>();
		protected int targetedInertia = 15;
		protected int targetedSpeed = 14;
		protected int maxDistanceFromPlayer = 850;
		protected int minDistanceToEnemy = 200;
		protected int animationFrames = 120;
		protected override int dustType => 54;
		public override int CounterType => ProjectileType<CharredChimeraCounterMinion>();

		protected List<Projectile> allHeads = default;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Charred Chimera");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			DrawOriginOffsetX = (56 - 32) / 2;
			DrawOriginOffsetY = (52 - 32) / 2;
			Projectile.tileCollide = false;
			dealsContactDamage = false;
			attackThroughWalls = false;
			frameSpeed = 5;
			animationFrame = 0;
		}

		public override void LoadAssets()
		{
			AddTexture(Texture + "_Ribs");
			AddTexture(Texture + "_Head");
			AddTexture(Texture + "_Heart");
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			float idleAngle = (float)Math.PI * 2 * (animationFrame % animationFrames) / animationFrames;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -5 + 8 * (float)Math.Sin(idleAngle);
			if (!Collision.CanHit(idlePosition, 1, 1, player.Top, 1, 1))
			{
				idlePosition.X = player.Top.X;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);
			int headType = ProjectileType<CharredChimeraMinionHead>();
			int currentHeadCount = player.ownedProjectileCounts[headType];
			if (Main.myPlayer == player.whoAmI)
			{
				for (int i = currentHeadCount; i < EmpowerCount + 1; i++)
				{
					Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Projectile.velocity, headType, Projectile.damage, Projectile.knockBack, player.whoAmI);
				}

				if (currentHeadCount > EmpowerCount + 1)
				{
					allHeads = GetMinionsOfType(ProjectileType<CharredChimeraMinionHead>());
					allHeads[0].Kill(); // get rid of a head if there's too many
				}
			}
			allHeads = GetMinionsOfType(ProjectileType<CharredChimeraMinionHead>());
			TellHeadsToAttack();
			PositionHeads();
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			int inertia = 10;
			int maxSpeed = 16;
			Vector2 speedChange = vectorToIdlePosition - Projectile.velocity;
			if (speedChange.Length() > maxSpeed)
			{
				speedChange.SafeNormalize();
				speedChange *= maxSpeed;
			}
			Projectile.spriteDirection = player.direction;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + speedChange) / inertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int inertia = targetedInertia;
			int maxSpeed = targetedSpeed;
			// move towards the enemy, but don't get too far from the player
			Projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
			if (vectorToTargetPosition.Length() < minDistanceToEnemy)
			{
				vectorToTargetPosition *= -1;
			}
			if (Math.Abs((vectorToTargetPosition.Length() - minDistanceToEnemy)) < 16)
			{
				//projectile.position += vectorToTargetPosition;
				inertia = 40; // don't jitter
			}
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= maxSpeed;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		private static Vector2[,] IdleHeadOffsets = {
			{ Vector2.Zero, default, default, default },
			{ new Vector2(4, 0), new Vector2(-8, 4), default, default },
			{ new Vector2(12, 4), new Vector2(4, -4), new Vector2(-8, 4), default},
			{ new Vector2(8, 0), new Vector2(0, 8), new Vector2(0, -8), new Vector2(-8, 0) }
		};

		private void TellHeadsToAttack()
		{
			if (allHeads.Count == 0)
			{
				return;
			}
			int attackFrames = (int)(Math.Max(20, 60 - 5 * EmpowerCount) * Math.Max(EmpowerCount, 1));
			int attackFrame = animationFrame % attackFrames;
			int interval = (attackFrames / allHeads.Count) % attackFrames;
			for (int i = 0; i < allHeads.Count; i++)
			{
				if (allHeads[i].ai[1] == 0 && attackFrame == i * interval)
				{
					allHeads[i].ai[1] = 2;
				}
				else if (allHeads[i].ai[1] == 2 && attackFrame != i * interval)
				{
					allHeads[i].ai[1] = 0;
				}
			}
		}
		private void PositionHeads()
		{
			var heads = allHeads.Where(h => h.ai[1] != 1).ToList();
			Vector2 spinalCordEndOffset = new Vector2(28 * Projectile.spriteDirection, -4);
			Vector2 headBasePosition = Projectile.Top + spinalCordEndOffset + new Vector2(-13, -14) + Projectile.velocity;
			if (heads.Count() == 0)
			{
				return;
			}
			else if (allHeads.Count() <= IdleHeadOffsets.GetLength(0))
			{
				for (int i = 0; i < heads.Count(); i++)
				{
					Vector2 headOffset = IdleHeadOffsets[allHeads.Count() - 1, i];
					headOffset.X *= Projectile.spriteDirection;
					heads[i].position = headBasePosition + headOffset;
					heads[i].velocity = Vector2.Zero;
					heads[i].spriteDirection = Projectile.spriteDirection;
				}
			}
			else
			{
				float baseIdleAngle = (float)Math.PI * 2 * (animationFrame % animationFrames) / animationFrames;
				float idleAngleStep = (float)(2 * Math.PI) / (allHeads.Count() - 1);
				int headRadius = 4 + 4 * heads.Count();
				for (int i = 0; i < heads.Count() - 1; i++)
				{
					float headAngle = baseIdleAngle + idleAngleStep * i;
					Vector2 headOffset = headRadius * new Vector2((float)Math.Cos(headAngle), (float)Math.Sin(headAngle));
					heads[i].position = headBasePosition + headOffset;
					heads[i].velocity = Vector2.Zero;
					heads[i].spriteDirection = Projectile.spriteDirection;
				}
				heads.Last().position = headBasePosition;
				heads.Last().velocity = Vector2.Zero;
				heads.Last().spriteDirection = Projectile.spriteDirection;
			}
		}

		private void DrawVertibrae(Texture2D headTexture, Color lightColor, Projectile head)
		{
			Vector2 endPosition = Projectile.Top + new Vector2(24 * Projectile.spriteDirection, -4);
			Vector2 center = head.Center;
			ChainDrawer drawer = new ChainDrawer(new Rectangle(6, 36, 12, 14));
			drawer.DrawChain(headTexture, center, endPosition);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			Texture2D bodyTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Texture2D ribsTexture = ExtraTextures[0].Value;
			Texture2D headTexture = ExtraTextures[1].Value;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			int frameHeight = bodyTexture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, bodyTexture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(ribsTexture, pos - Main.screenPosition,
				ribsTexture.Bounds, lightColor, r,
				ribsTexture.Bounds.Center.ToVector2(), 1, effects, 0);
			DrawHeart(lightColor);
			Main.EntitySpriteDraw(bodyTexture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			foreach (Projectile head in allHeads)
			{
				DrawVertibrae(headTexture, lightColor, head);
			}
			return false;
		}

		private void DrawHeart(Color lightColor)
		{
			Vector2 heartCenter = Projectile.Center + new Vector2(16 * Projectile.spriteDirection, 0);
			Texture2D heartTexture = ExtraTextures[2].Value;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = heartTexture.Bounds;
			Vector2 origin = bounds.Center.ToVector2();
			float r = 0;
			int beatFrame = animationFrame % (animationFrames / 2);
			float scale = beatFrame < animationFrames / 8 ?
				1 + (float)(0.25 * Math.Sin(8 * Math.PI * beatFrame / animationFrames)) :
				1 + (float)(0.125 * Math.Sin(Math.PI / 2 + 4 * Math.PI * beatFrame / animationFrames));
			Main.EntitySpriteDraw(heartTexture, heartCenter - Main.screenPosition,
				bounds, lightColor, r,
				origin, scale, effects, 0);
		}

		private void DrawHeads(Color lightColor)
		{
			Texture2D texture = ExtraTextures[1].Value;
			foreach (Projectile head in allHeads)
			{
				float r = head.rotation;
				Vector2 pos = head.Center;
				SpriteEffects effects = head.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
				int frameHeight = texture.Height / Main.projFrames[head.type];
				Rectangle bounds = new Rectangle(0, 0, texture.Width, frameHeight);
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, effects, 0);
				PartyHatSystem.DrawManualHat(head, lightColor);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			DrawHeads(lightColor);
		}

		protected override int ComputeDamage()
		{
			return baseDamage;
		}

		protected override float ComputeSearchDistance()
		{
			return 950f;
		}

		protected override float ComputeInertia()
		{
			return 15;
		}

		protected override float ComputeTargetedSpeed()
		{
			return 14;
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeIdleSpeed() + 3;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 4;
		}
	}
}
