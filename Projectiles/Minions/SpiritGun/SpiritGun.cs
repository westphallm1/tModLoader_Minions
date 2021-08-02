using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Minions.SpiritGun
{
	public class SpiritGunMinionBuff : MinionBuff
	{
		public SpiritGunMinionBuff() : base(ProjectileType<SpiritGunCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Revolver");
			Description.SetDefault("A group of sentient bullets will fight for you!");
		}
	}

	public class SpiritGunMinionItem : MinionItem<SpiritGunMinionBuff, SpiritGunCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Revolver");
			Tooltip.SetDefault("Summons sentient bullets to fight for you.\nMake sure they don't get hungry...");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 36;
			Item.height = 22;
			Item.damage = 53;
			Item.value = Item.buyPrice(0, 20, 0, 0);
			Item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.SpectreBar, 12).AddIngredient(ItemID.IllegalGunParts, 1).AddTile(TileID.MythrilAnvil).Register();
		}
	}

	public class SpiritGunCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<SpiritGunMinionBuff>();
		protected override int MinionType => ProjectileType<SpiritGunMinion>();
	}
	/// <summary>
	/// Uses ai[1] to track fired vs unfired bullets
	/// </summary>
	public class SpiritGunMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<SpiritGunMinionBuff>();
		private int framesSinceLastHit;
		private const int AnimationFrames = 120;
		private Queue<Vector2> activeTargetVectors;
		private bool isReloading;
		protected override int dustType => 137;

		private float unfiredShots
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		protected override int CounterType => ProjectileType<SpiritGunCounterMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Revolver");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 36;
			Projectile.height = 22;
			Projectile.tileCollide = false;
			animationFrame = 0;
			framesSinceLastHit = 0;
			Projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
			activeTargetVectors = new Queue<Vector2>();
		}

		private Color ShadowColor(Color original)
		{
			return new Color(original.R / 2, original.G / 2, original.B / 2);
		}

		private Vector2 GetSpiritLocation(int index)
		{
			float r = (float)(2 * Math.PI * animationFrame) / AnimationFrames;
			Vector2 pos = Projectile.Center;
			float r1 = r + 2 * (float)Math.PI * index / (EmpowerCount + 1);
			Vector2 pos1 = pos + new Vector2((float)Math.Cos(r1), (float)Math.Sin(r1)) * 32;
			return pos1;

		}

		private void SpiritDust(int index)
		{
			Dust.NewDust(GetSpiritLocation(index), 10, 14, 137);
		}
		private void DrawSpirits(Color lightColor)
		{
			Rectangle bounds = new Rectangle(0, Projectile.height, 10, 14);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			for (int i = 0; i < unfiredShots; i++)
			{
				Main.EntitySpriteDraw(texture, GetSpiritLocation(i) - Main.screenPosition,
					bounds, lightColor, 0,
					origin, 1, 0, 0);
			}
			foreach (Vector2 active in activeTargetVectors)
			{
				Lighting.AddLight(active, Color.LightCyan.ToVector3() * 0.75f);
				Main.EntitySpriteDraw(texture, active - Main.screenPosition,
					bounds, Color.LightCyan, 0,
					origin, 1, 0, 0);
			}
		}


		private void DrawShadows(Color lightColor)
		{
			Vector2 pos = Projectile.Center;
			Rectangle bounds = new Rectangle(0, 0, Projectile.width, Projectile.height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Color shadowColor = ShadowColor(lightColor);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// echo 1
			float offset = 2f * (float)Math.Sin(Math.PI * (animationFrame % 60) / 30);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Vector2.One * offset,
				bounds, shadowColor, Projectile.rotation, origin, 1, effects, 0);
			// echo 2
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition - Vector2.One * offset,
				bounds, shadowColor, Projectile.rotation, origin, 1, effects, 0);

		}

		public override bool PreDraw(ref Color lightColor)
		{
			animationFrame %= AnimationFrames;
			DrawSpirits(lightColor);
			DrawShadows(lightColor);
			return true;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			unfiredShots = 1;
		}

		public override void OnEmpower()
		{
			base.OnEmpower();
			Projectile.ai[1] += 1;
		}
		public override Vector2 IdleBehavior()
		{
			// this is a little messy
			int reloadSpeed = Math.Max(3, 30 / Math.Max((int)EmpowerCount, 1));
			int reloadDelay = Math.Max(10, 45 - 4 * (int)EmpowerCount);
			framesSinceLastHit++;
			bool timeBetweenShots = framesSinceLastHit > 2 * reloadDelay;
			bool timeSinceReload = isReloading && framesSinceLastHit > reloadDelay;
			if ((timeBetweenShots || timeSinceReload) && framesSinceLastHit % reloadSpeed == 0)
			{
				if (unfiredShots == EmpowerCount + 1)
				{
					isReloading = false;
					activeTargetVectors.Clear();
				}
				else if (unfiredShots < EmpowerCount + 1)
				{
					unfiredShots += 1;
					// the count can sometimes get desynced in multiplayer since ai is synced and 
					// the target queue isn't, this is a lazy failsafe against that
					if (activeTargetVectors.Count > 0)
					{
						Vector2 returned = activeTargetVectors.Dequeue();
						Dust.NewDust(returned, 4, 10, 137);
					}
				}
			}
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -32;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Center.X;
				idlePosition.Y = player.Center.Y - 24;
			}
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			Lighting.AddLight(Projectile.position, Color.LightCyan.ToVector3() * 0.75f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToIdle);
			if (framesSinceLastHit > 15 && unfiredShots > 0 && !isReloading)
			{
				vectorToTargetPosition = VaryLaunchVelocity(vectorToTargetPosition);
				Vector2 pos = Projectile.Center;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						pos,
						vectorToTargetPosition,
						ProjectileType<SpiritGunMinionBullet>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item97, pos);
				// TODO handle flipping and stuff
				Projectile.rotation = vectorToTargetPosition.ToRotation();
				unfiredShots -= 1;
				activeTargetVectors.Enqueue(Projectile.Center + vectorToTargetPosition);
				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(Projectile.Center + vectorToTargetPosition, 4, 10, 137);
				}
				framesSinceLastHit = 0;
				if (unfiredShots == 0)
				{
					isReloading = true;
				}
			}
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 5) * (int)EmpowerCount;
		}


		private Vector2? GetAnyTargetVector(Vector2 center)
		{
			float searchDistance = 600;
			if (PlayerAnyTargetPosition(searchDistance, center) is Vector2 target)
			{
				return target;
			}
			else if (AnyEnemyInRange(searchDistance, center) is Vector2 target2)
			{
				return target2;
			}
			else
			{
				return null;
			}
		}

		private Vector2? TrickshotAngle(int frame)
		{
			// search a fraction of possible positions each frame
			float angle = 2 * frame * (float)Math.PI / AnimationFrames;
			float distance = ComputeSearchDistance() / (1 + frame % 8);
			Vector2 target = Projectile.Center + distance * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			if (!Collision.CanHitLine(Projectile.Center, 1, 1, target, 1, 1))
			{
				return null;
			}
			if (GetAnyTargetVector(target) != null)
			{
				return target - Projectile.Center;
			}
			else
			{
				return null;
			}
		}
		public override Vector2? FindTarget()
		{
			if (framesSinceLastHit > 15 && !isReloading && TrickshotAngle(animationFrame) is Vector2 trickshot2)
			{
				return trickshot2;
			}
			return null;
		}

		protected override float ComputeSearchDistance()
		{
			return 600;
		}

		protected override float ComputeInertia()
		{
			return 5;
		}

		protected override float ComputeTargetedSpeed()
		{
			return 16;
		}

		protected override float ComputeIdleSpeed()
		{
			return 16;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (framesSinceLastHit > 30)
			{
				if (Math.Abs(Projectile.velocity.X) > 2)
				{
					Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
				}
				Projectile.rotation = 0.025f * Projectile.velocity.X;
			}
			else
			{
				Projectile.spriteDirection = 1;
			}
		}
	}
}
