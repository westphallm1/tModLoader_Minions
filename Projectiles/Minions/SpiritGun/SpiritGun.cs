using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.SpiritGun
{
	public class SpiritGunMinionBuff : MinionBuff
	{
		public SpiritGunMinionBuff() : base(ProjectileType<SpiritGunCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
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
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 36;
			item.height = 22;
			item.damage = 53;
			item.value = Item.buyPrice(0, 20, 0, 0);
			item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SpectreBar, 12);
			recipe.AddIngredient(ItemID.IllegalGunParts, 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
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
			get => projectile.ai[1];
			set => projectile.ai[1] = value;
		}

		protected override int CounterType => ProjectileType<SpiritGunCounterMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Revolver");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 2;
			IdleLocationSets.trailingInAir.Add(projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 36;
			projectile.height = 22;
			projectile.tileCollide = false;
			animationFrame = 0;
			framesSinceLastHit = 0;
			projectile.friendly = true;
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
			Vector2 pos = projectile.Center;
			float r1 = r + 2 * (float)Math.PI * index / (EmpowerCount + 1);
			Vector2 pos1 = pos + new Vector2((float)Math.Cos(r1), (float)Math.Sin(r1)) * 32;
			return pos1;

		}

		private void SpiritDust(int index)
		{
			Dust.NewDust(GetSpiritLocation(index), 10, 14, 137);
		}
		private void DrawSpirits(SpriteBatch spriteBatch, Color lightColor)
		{
			Rectangle bounds = new Rectangle(0, projectile.height, 10, 14);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Texture2D texture = Main.projectileTexture[projectile.type];
			for (int i = 0; i < unfiredShots; i++)
			{
				spriteBatch.Draw(texture, GetSpiritLocation(i) - Main.screenPosition,
					bounds, lightColor, 0,
					origin, 1, 0, 0);
			}
			foreach (Vector2 active in activeTargetVectors)
			{
				Lighting.AddLight(active, Color.LightCyan.ToVector3() * 0.75f);
				spriteBatch.Draw(texture, active - Main.screenPosition,
					bounds, Color.LightCyan, 0,
					origin, 1, 0, 0);
			}
		}


		private void DrawShadows(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 pos = projectile.Center;
			Rectangle bounds = new Rectangle(0, 0, projectile.width, projectile.height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Color shadowColor = ShadowColor(lightColor);
			Texture2D texture = Main.projectileTexture[projectile.type];
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// echo 1
			float offset = 2f * (float)Math.Sin(Math.PI * (animationFrame % 60) / 30);
			spriteBatch.Draw(texture, pos - Main.screenPosition + Vector2.One * offset,
				bounds, shadowColor, projectile.rotation, origin, 1, effects, 0);
			// echo 2
			spriteBatch.Draw(texture, pos - Main.screenPosition - Vector2.One * offset,
				bounds, shadowColor, projectile.rotation, origin, 1, effects, 0);

		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			animationFrame %= AnimationFrames;
			DrawSpirits(spriteBatch, lightColor);
			DrawShadows(spriteBatch, lightColor);
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
			projectile.ai[1] += 1;
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
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, projectile);
			idlePosition.Y += -32;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Center.X;
				idlePosition.Y = player.Center.Y - 24;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			Lighting.AddLight(projectile.position, Color.LightCyan.ToVector3() * 0.75f);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IdleMovement(vectorToIdle);
			if (framesSinceLastHit > 15 && unfiredShots > 0 && !isReloading)
			{
				vectorToTargetPosition = VaryLaunchVelocity(vectorToTargetPosition);
				Vector2 pos = projectile.Center;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						pos,
						vectorToTargetPosition,
						ProjectileType<SpiritGunMinionBullet>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
				Main.PlaySound(SoundID.Item97, pos);
				// TODO handle flipping and stuff
				projectile.rotation = vectorToTargetPosition.ToRotation();
				unfiredShots -= 1;
				activeTargetVectors.Enqueue(projectile.Center + vectorToTargetPosition);
				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(projectile.Center + vectorToTargetPosition, 4, 10, 137);
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
			Vector2 target = projectile.Center + distance * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			if (!Collision.CanHitLine(projectile.Center, 1, 1, target, 1, 1))
			{
				return null;
			}
			if (GetAnyTargetVector(target) != null)
			{
				return target - projectile.Center;
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
				if (Math.Abs(projectile.velocity.X) > 2)
				{
					projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
				}
				projectile.rotation = 0.025f * projectile.velocity.X;
			}
			else
			{
				projectile.spriteDirection = 1;
			}
		}
	}
}
