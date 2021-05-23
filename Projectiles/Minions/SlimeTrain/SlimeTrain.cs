using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.SlimeTrain
{
	public class SlimeTrainMinionBuff : MinionBuff
	{
		public SlimeTrainMinionBuff() : base(ProjectileType<SlimeTrainCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Copter-X");
			Description.SetDefault("A flexible helicopter will fight for you!");
		}
	}

	public class SlimeTrainMinionItem : MinionItem<SlimeTrainMinionBuff, SlimeTrainCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Staff of the Celestial Steam Train");
			Tooltip.SetDefault("Summons a flexible helicopter to fight for you!");

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.damage = 43;
			item.height = 34;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.MechanicalLens, 1);
			recipe.AddIngredient(ItemID.HallowedBar, 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class SlimeTrainCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<SlimeTrainMinionBuff>();
		protected override int MinionType => ProjectileType<SlimeTrainMinion>();
	}
	public class SlimeTrainMinion : WormMinion
	{
		internal override int BuffId => BuffType<SlimeTrainMinionBuff>();
		protected override int CounterType => ProjectileType<SlimeTrainCounterMinion>();
		protected override int dustType => DustType<StarDust>();

		private int nSlimes = 7;
		private int SlimeFrameTop(int i) => 40 * (i % nSlimes) + 4;
		private int YFrameTop => 40 * projectile.frame + 4;
		private int FrameHeight => 34;
		
		// radius to circle while summoning a sub projectile
		private int spawnRadius = 120;

		private int SubProjectileType; 
		private Projectile currentMarker = null;

		private Texture2D SlimeTexture;

		// 0 or Pi depending on approach direction
		private float startAngle = 0;
		// 1 for CW, -1 for CCW, depending on approach direction
		private int rotationDir = 1;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Celestial Steam Train");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			attackThroughWalls = true;
			frameSpeed = 8;
			if(SlimeTexture == null)
			{
				SlimeTexture = GetTexture(Texture + "_Slimes");
			}
			SubProjectileType = ProjectileType<SlimeTrainMarkerProjectile>();
		}
		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			int radius = Math.Abs(player.velocity.X) < 4 ? 120 : 24;
			float idleAngle = 2 * PI * groupAnimationFrame / groupAnimationFrames;
			idlePosition.X += radius * (float)Math.Cos(idleAngle);
			idlePosition.Y += radius * (float)Math.Sin(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.75f);
			// find the SlimeTrainMarker that's still in its spawn animation, if any
			currentMarker = null;
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == SubProjectileType && 
					(int)p.localAI[0] < SlimeTrainMarkerProjectile.SetupTime)
				{
					currentMarker = p;
					break;
				}
			}
			return vectorToIdlePosition;
		}

		public override bool ShouldIgnoreNPC(NPC npc)
		{
			// ignore any npc with a marker actively placed on it
			return base.ShouldIgnoreNPC(npc) || Main.projectile.Any(p =>
				p.active && p.owner == player.whoAmI &&
				p.type == SubProjectileType && (int)p.ai[0] == npc.whoAmI);
		}

		protected override void DrawHead()
		{
			Rectangle slime = new Rectangle(0, SlimeFrameTop(0), 52, FrameHeight);
			texture = SlimeTexture;
			AddSprite(2, slime);
			Rectangle head = new Rectangle(70, YFrameTop, 52, FrameHeight);
			texture = Main.projectileTexture[projectile.type];
			AddSprite(2, head);
		}

		protected override void DrawBody()
		{
			Rectangle body;
			for (int i = 0; i < GetSegmentCount() + 1; i++)
			{
				if (i == 0)
				{
					body = new Rectangle(36, YFrameTop, 30, FrameHeight);
				}
				else
				{
					Rectangle slime = new Rectangle(0, SlimeFrameTop(i), 30, FrameHeight);
					texture = SlimeTexture;
					AddSprite(48+30 * i, slime);
					body = new Rectangle(2, YFrameTop, 30, FrameHeight);
				}
				texture = Main.projectileTexture[projectile.type];
				AddSprite(48 + 30 * i, body);
			}
		}

		protected override void DrawTail()
		{
			// no tail, maybe should add a caboose? 
			lightColor = Color.White;
			lightColor.A = 128;
		}

		protected override float ComputeSearchDistance()
		{
			return 1400 + 50 * GetSegmentCount();
		}

		protected override float ComputeInertia()
		{
			return Math.Max(12, 22 - GetSegmentCount());
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(22, 14 + GetSegmentCount());
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 4;
		}

		private void DoMarkerSpawnMovement()
		{
			int markerFrame = (int)currentMarker.localAI[0];
			int npcIdx = (int)currentMarker.ai[0];
			int targetSize = (int)(Main.npc[npcIdx].Size.X + Main.npc[npcIdx].Size.Y) / 2;
			float idleAngle = startAngle + rotationDir * 2 * PI * markerFrame / SlimeTrainMarkerProjectile.SetupTime;
			Vector2 targetPosition = currentMarker.Center;
			targetPosition.X += (targetSize + spawnRadius) * (float)Math.Cos(idleAngle);
			targetPosition.Y += (targetSize + spawnRadius) * (float)Math.Sin(idleAngle);
			projectile.velocity = targetPosition - projectile.position;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(currentMarker != null)
			{
				DoMarkerSpawnMovement();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(currentMarker != null)
			{
				DoMarkerSpawnMovement();
			} else
			{
				Vector2 startOffset = vectorToTargetPosition;
				if(targetNPCIndex is int idx)
				{
					int TargetSize = (int)(Main.npc[idx].Size.X + Main.npc[idx].Size.Y) / 2;
					int approachDir = -Math.Sign(vectorToTargetPosition.X);
					startOffset += new Vector2(approachDir * (TargetSize + spawnRadius), 0);
					if(player.whoAmI == Main.myPlayer && startOffset.LengthSquared() < 16 * 16)
					{
						startAngle = approachDir == 1 ?  0 : MathHelper.Pi;
						rotationDir = approachDir * Math.Sign(projectile.velocity.Y);

						Projectile.NewProjectile(
							projectile.Center,
							Vector2.Zero,
							SubProjectileType,
							projectile.damage,
							projectile.knockBack,
							Main.myPlayer,
							ai0: idx,
							ai1: EmpowerCount);
					}
				}
				base.TargetedMovement(startOffset);
			}
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, searchDistance, losCenter: player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, player.Center, searchDistance, losCenter: player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			projectile.friendly = false;
		}
		protected override void AddSprite(float dist, Rectangle bounds, Color c = default)
		{
			Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r,
				origin, 1, GetEffects(r), 0);
			if (Main.rand.Next(30) == 0)
			{
				int dustId = Dust.NewDust(pos, bounds.Width, bounds.Height, dustType, 0f, 0f, 0, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
				Main.dust[dustId].velocity *= 0.25f;
			}
		}
	}
}
