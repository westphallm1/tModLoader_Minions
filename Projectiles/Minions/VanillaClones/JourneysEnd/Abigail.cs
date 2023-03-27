using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using static AmuletOfManyMinions.Items.Accessories.MinionSpawningItemPlayer;
using System;
using Terraria.Audio;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Core;
using Terraria.Graphics.Shaders;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class AbigailMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.AbigailMinion;

		internal override int[] ProjectileTypes => new int[] { ProjectileType<AbigailCounterMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.AbigailMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.AbigailMinion"));
		}

	}

	public class AbigailMinionItem : VanillaCloneMinionItem<AbigailMinionBuff, AbigailCounterMinion>
	{
		internal override int VanillaItemID => ItemID.AbigailsFlower;

		internal override string VanillaItemName => "AbigailsFlower";
	}

	public class AbigailCounterMinion : CounterMinion
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/VanillaClones/JourneysEnd/MourningGlory";
		public override int BuffId => BuffType<AbigailMinionBuff>();
		protected override int MinionType => ProjectileType<AbigailMinion>();

		static int ModSupport_SummonersShine_MourningGloryShot;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.scale = 2;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.alpha = 100;
		}

		public override void ApplyCrossModChanges()
		{
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
			{
				const int PROJ_STATICS = 1;
				const int ONSPECIALABIL = 4;
				ModSupport_SummonersShine_MourningGloryShot = summonersShine.Find<ModProjectile>("MourningGloryBolt").Type;

				CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
				minionCollection.AddMinionPower(10);
				const int RECHARGE_TIME = 300;
				CrossMod.BakeSummonersShineMinionPower_WithHooks(ItemType<AbigailMinionItem>(), Type, RECHARGE_TIME, minionCollection, AbigailCounterOnSpecialAbility);
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			return CrossMod.SummonersShineLoaded;
		}
		public static void AbigailCounterOnSpecialAbility(Projectile projectile, Entity _target, int specialType, bool fromServer)
		{
			ModLoader.TryGetMod("SummonersShine", out Mod summonersShine);
			const int SET_PROJDATA = 5;
			const int ENERGY = 2;
			const int SPECIALTIME = 5;
			const int REGENMULT = 1;
			summonersShine.Call(SET_PROJDATA, projectile, ENERGY, (float)0);
			summonersShine.Call(SET_PROJDATA, projectile, SPECIALTIME, 0);
			summonersShine.Call(SET_PROJDATA, projectile, REGENMULT, (float)0);
			projectile.ai[0] = Main.rand.Next(0, 25);
		}
		public override void DoAI()
		{
			if (!CrossMod.SummonersShineLoaded)
			{
				base.DoAI();
				return;
			}

			Vector2 savedPos = Projectile.position;
			base.DoAI();
			Projectile.position = savedPos;

			Player player = Main.player[Projectile.owner];
			/*
            if (player.dead)
            {
                player.abigailMinion = false;
            }
            if (player.abigailMinion)
            {
                projectile.timeLeft = 2;
            }*/
			//Projectile.RefreshMinionTimer(projFuncs, player);
			Projectile.frameCounter++;
			int frame = Projectile.frameCounter / 6;
			if (frame == 7)
			{
				Projectile.frameCounter = 0;
				frame = 0;
			}
			if (frame > 3)
			{
				frame = 6 - frame;
			}
			Projectile.frame = frame;

			Lighting.AddLight(Projectile.Center, TorchID.Bone);

			if (CrossMod.GetSummonersShineIsCastingSpecialAbility(Projectile, ItemType<AbigailMinionItem>()) && Main.myPlayer == Projectile.owner)
			{
				Mod summonersShine = ModLoader.GetMod("SummonersShine");
				const int USEFUL_FUNCS = 10;
				const int GET_MINION_POWER = 3;
				const int INCREMENT_SPECIAL_TIMER = 9;
				if (Projectile.ai[0] <= 0)
				{
					
					float range = (float)summonersShine.Call(USEFUL_FUNCS, GET_MINION_POWER, Projectile, 0) * 16;
					NPC target = GetClosestEnemyToPosition(Projectile.Center, range);
					if (target != null)
					{
						int attackTarget = GetClosestEnemyToPosition(Projectile.Center, range).whoAmI;//SpecialAbilities.SpecialAbility.RandomMinionTarget(Projectile, range: range);
						int damage = Projectile.damage;
						int count = Math.Max(0, Main.player[Projectile.owner].ownedProjectileCounts[Type] - 1);
						float mult = 0.55f;
						if (Main.hardMode)
						{
							mult = 1.3f;
						}
						damage = (int)((float)damage * (1f + (float)count * mult));
						Projectile bolt = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, -6)), ModSupport_SummonersShine_MourningGloryShot, damage, Projectile.knockBack, Projectile.owner, attackTarget * 4 /* Global Entity ID system */);
						bolt.originalDamage = (int)range;
						bolt.netUpdate = true;
					}
					Projectile.ai[0] = 60 + Main.rand.NextFloat(-10, 10);
				}
				Projectile.ai[0]--;
				summonersShine.Call(USEFUL_FUNCS, INCREMENT_SPECIAL_TIMER, Projectile, 300, (float)1);
				//ModUtils.IncrementSpecialAbilityTimer(projectile, projFuncs, projData, 300);
			}

			if (Main.rand.NextBool(150))
				for (int x = 0; x <= Main.rand.Next(3); x++)
					Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), 0, 0, DustID.SteampunkSteam, newColor: Color.GhostWhite, Alpha: 50).shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);

		}
	}

	public class AbigailMinion : EmpoweredMinion
	{
		public override int BuffId => BuffType<AbigailMinionBuff>();
		public override int CounterType => ProjectileType<AbigailCounterMinion>();
		protected override int dustType => 6;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.AbigailMinion;

		internal bool isCloseToTarget = false;
		internal HoverShooterHelper hsHelper;
		internal int stayInPlaceFrames = 0;
		internal int attackRadius = 148;
		internal int damageRadius = 80;
		internal bool IsAttacking => VectorToTarget is Vector2 target && target.LengthSquared() < attackRadius * attackRadius;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.AbigailMinion"));
			Main.projFrames[Projectile.type] = 13;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public override void LoadAssets()
		{
			Main.instance.LoadProjectile(ProjectileID.MedusaHeadRay);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = true;
			AttackThroughWalls = false;
			Projectile.width = 32;
			Projectile.height = 32;
			FrameSpeed = 8;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			Projectile.localNPCHitCooldown = 20;
			hsHelper = new HoverShooterHelper(this, default)
			{
				attackFrames = 30,
				projectileVelocity = 14,
				targetShootProximityRadius = attackRadius,
				targetInnerRadius = 48,
				targetOuterRadius = 64,
			};
		}

		public override void ApplyCrossModChanges()
		{
			const int PROJ_STATICS = 1;
			const int POST_AI = 11;
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine)) {
				summonersShine.Call(PROJ_STATICS, Type, POST_AI, Abigail_PositionFlowers);
			}
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = Player.Top;
			idlePosition.X += -Player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -32;
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition.X = Player.Top.X;
				idlePosition.Y = Player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			projHitbox = new Rectangle(
				(int)Projectile.Center.X - damageRadius, (int)Projectile.Center.Y - damageRadius,
				2 * damageRadius, 2 * damageRadius);
			return Vector2.DistanceSquared(Projectile.Center, targetHitbox.Center.ToVector2()) < damageRadius * damageRadius
			    || projHitbox.Intersects(targetHitbox);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			hsHelper.TargetedMovement(vectorToTargetPosition);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			isCloseToTarget = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		protected override int ComputeDamage() => baseDamage + baseDamage * (int)((EmpowerCount - 1) * (Main.hardMode ? 1.3f : 0.55f));

		protected override float ComputeSearchDistance() => 800;

		protected override float ComputeInertia() => 11;

		protected override float ComputeTargetedSpeed() => Math.Min(13, 9 + EmpowerCount);

		protected override float ComputeIdleSpeed() => ComputeTargetedSpeed() + 3;

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			if(IsAttacking)
			{
				minFrame = 9;
				maxFrame = 13;
			} else
			{
				minFrame = 0;
				maxFrame = 8;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(IsAttacking)
			{
				Projectile.spriteDirection = Math.Sign(((Vector2)VectorToTarget).X);
			}
			else if(Math.Abs(Projectile.velocity.X) > 1)
			{
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
		}


		private void DrawLightRays(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.MedusaHeadRay].Value;
			Rectangle bounds = texture.Bounds;
			float baseAngle = MathHelper.TwoPi * AnimationFrame / 180;
			int rayCount = 7;
			for(int i = 0; i < rayCount; i++)
			{
				float localAngle = baseAngle + MathHelper.TwoPi * i / rayCount;
				float localIntensity = MathF.Sin(1.75f * localAngle);
				float scale = 0.5f + 0.25f * localIntensity;
				float brightness = 0.65f + 0.25f * localIntensity;
				Vector2 drawOffset = localAngle.ToRotationVector2() * scale * bounds.Height / 2;
				Main.EntitySpriteDraw(texture, Projectile.Center + drawOffset - Main.screenPosition,
					bounds, lightColor.MultiplyRGB(Color.LightCoral) * brightness, localAngle + MathHelper.PiOver2,
					bounds.GetOrigin(), scale, 0, 0);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// attacking light rays
			if(IsAttacking)
			{
				DrawLightRays(ref lightColor);
			}

			// body
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new (0, Projectile.frame * frameHeight, texture.Width/4, frameHeight);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, lightColor * 0.75f, Projectile.rotation,
				bounds.GetOrigin(), 1, effects, 0);

			// flower
			Color flowerColor = MinionColors[((EmpowerCount - 1)/ 3) % MinionColors.Length];
			Vector2 flowerOffset = Projectile.frame == 9 ? new(0, -6) : Projectile.frame == 10 ? new(0, 8) : default;
			bounds = new ((1 + (EmpowerCount - 1) % 3) * texture.Width/4, 0, texture.Width/4, frameHeight);
			Main.EntitySpriteDraw(texture, Projectile.Center + flowerOffset - Main.screenPosition,
				bounds, flowerColor.MultiplyRGB(lightColor), Projectile.rotation,
				bounds.GetOrigin(), 1, effects, 0);
			return false;
		}

		//Summoner's Shine content

		static void Abigail_PositionFlowers(Projectile projectile)
		{
			const int minFlowersPerRow_c = 1;
			const int addFlowersPerRow_k = 8;
			const float addFlowersPerRow_k_x2 = 2f / addFlowersPerRow_k;
			const float twoc_k_div_k = (2 * minFlowersPerRow_c - addFlowersPerRow_k) / (2f * addFlowersPerRow_k);
			const float twoc_k_div_k_sqr = twoc_k_div_k * twoc_k_div_k;

			int maxIndex = 0;
			List<Projectile> allFlowers = new();
			for (int i = 0; i < 1000; i++)
			{
				Projectile flower = Main.projectile[i];
				if (flower.type == ProjectileType<AbigailCounterMinion>() && flower.active && flower.owner == projectile.owner)
				{
					allFlowers.Add(flower);
					maxIndex++;
				}
			}

			int maxHeight = (int)Math.Floor(Math.Sqrt(twoc_k_div_k_sqr + (maxIndex - 0.5f) * addFlowersPerRow_k_x2) - twoc_k_div_k);
			int width = addFlowersPerRow_k * maxHeight + minFlowersPerRow_c;
			int flowerCapacity = (maxHeight + 1) * (width + minFlowersPerRow_c) / 2;

			int diff = flowerCapacity - maxIndex;
			int heightToDecreAdd = 0;
			int extraAdd = 0;
			if (diff > 0 && maxHeight > 1)
			{
				int lastFlowerCapacity = flowerCapacity - (maxHeight) * addFlowersPerRow_k - minFlowersPerRow_c;
				int extras = maxIndex - lastFlowerCapacity;
				heightToDecreAdd = extras % maxHeight;
				extraAdd = (extras - heightToDecreAdd) / maxHeight + 1;
				if (heightToDecreAdd == 0)
					extraAdd -= 1;
				width -= addFlowersPerRow_k;
			}
			maxHeight -= 1;

			int remainder = maxIndex;
			int index = 0;
			int height = 0;
			int storedRemaining = remainder;
			int dir = -projectile.spriteDirection;
			int basewidth = 32;
			if (maxIndex < 8)
			{
				basewidth = 8 + 3 * maxIndex;
			}
			allFlowers.ForEach(i =>
			{
				int workingWidth = width + extraAdd;
				int sentwidth;
				if (height == maxHeight && workingWidth > storedRemaining)
					sentwidth = storedRemaining;
				else
					sentwidth = workingWidth;
				Abigail_TeleportFlower(projectile, i, index, sentwidth, height, dir, basewidth);
				index++;
				remainder--;
				if (index >= workingWidth)
				{
					height++;
					width -= addFlowersPerRow_k;
					index = 0;
					if (diff > 0 && height == heightToDecreAdd)
						extraAdd--;
					storedRemaining = remainder;
				}
			});
		}
		static void Abigail_TeleportFlower(Projectile abi, Projectile flower, int pos, int width, int height, int dir, int baseWidth)
		{
			const float extraWidthPerStack = 16f;

			Vector2 initialDisp = new Vector2(0, -6);
			Vector2 layerDisp = new Vector2(0, -12);
			Vector2 abiHead = abi.Top + initialDisp;
			Vector2 disp = abiHead + layerDisp * height;

			float posinrow;
			if (width > 1)
			{
				posinrow = (float)(pos) / (width - 1);
				if (width < 8)
				{
					float diff = width / 8f;
					posinrow *= diff;
					posinrow += (8 - width) / 16f;
				}
			}
			else
				posinrow = 0.5f;

			float rad = extraWidthPerStack * (height) + baseWidth;
			Vector2 circle = new Vector2(rad, rad).RotatedBy((posinrow) * -Math.PI * 1.5f);
			circle.X *= dir;
			disp += circle;

			flower.Center = disp;
		}
	}
}
