using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModClient.SummonersShine
{
	internal static class AbigailFlower
	{

		public static int ModSupport_SummonersShine_MourningGloryShot;
		public static void CrossModChanges(int Type)
		{
			if (!ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
				return;
			ModSupport_SummonersShine_MourningGloryShot = summonersShine.Find<ModProjectile>("MourningGloryBolt").Type;
			const int PROJ_STATICS = 1;
			const int ONSPECIALABIL = 4;

			CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
			minionCollection.AddMinionPower(10);
			const int RECHARGE_TIME = 300;
			CrossMod.BakeSummonersShineMinionPower_WithHooks(ItemType<AbigailMinionItem>(), Type, RECHARGE_TIME, minionCollection, AbigailCounterOnSpecialAbility);
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
		public static void DoAI(AbigailCounterMinion minion, Projectile Projectile, Vector2 savedPos)
		{
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
					NPC target = minion.GetClosestEnemyToPosition(Projectile.Center, range);
					if (target != null)
					{
						int attackTarget = minion.GetClosestEnemyToPosition(Projectile.Center, range).whoAmI;//SpecialAbilities.SpecialAbility.RandomMinionTarget(Projectile, range: range);
						int damage = Projectile.damage;
						int count = Math.Max(0, Main.player[Projectile.owner].ownedProjectileCounts[minion.Type] - 1);
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

		//Summoner's Shine content

		const int PROJ_STATICS = 1;
		const int POST_AI = 11;
		public static void Abigail_ApplyStatics(int Type)
		{
			if (!ModLoader.TryGetMod("SummonersShine", out Mod summonersShine)) return;
			summonersShine.Call(PROJ_STATICS, Type, POST_AI, Abigail_PositionFlowers);
		}

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
