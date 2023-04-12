using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd;
using Microsoft.Xna.Framework;
using Mono.Cecil;
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
	internal static class Imp
	{
		public static int ModSupport_SummonersShine_ImpSuperFireball;
		public static void CrossModChanges(int Type)
		{
			if (!ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
				return;
			ModSupport_SummonersShine_ImpSuperFireball = summonersShine.Find<ModProjectile>("ImpSuperFireball").Type;
			const int PROJ_STATICS = 1;
			const int ONSPECIALABIL = 4;

			CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
			minionCollection.AddMinionPower(5);
			const int RECHARGE_TIME = 300;
			CrossMod.BakeSummonersShineMinionPower_WithHooks(ItemType<ImpMinionItem>(), Type, RECHARGE_TIME, minionCollection, ImpOnSpecialAbility, SpecialAbilityFindMinions: PreAbility_Imp);
		}
		internal static void SetDeaults_Imp(Projectiles.Minions.VanillaClones.ImpMinion Imp)
		{

			Imp.hsHelper.CustomFireProjectile = (i, j, k) => ImpOnShootProjectile(Imp, i, j, k);
		}
		public static void ImpOnShootProjectile(ImpMinion minion, Vector2 lineOfFire, int projId, float ai0)
		{
			if (!CrossMod.GetSummonersShineIsCastingSpecialAbility(minion.Projectile, ItemType<ImpMinionItem>()))
			{
				minion.hsHelper.FireProjectile(lineOfFire, projId, ai0);
				return;
			}
			Projectile.NewProjectile(
				minion.Projectile.GetSource_FromThis(),
				minion.Projectile.Center,
				minion.Behavior.VaryLaunchVelocity(lineOfFire),
				ModSupport_SummonersShine_ImpSuperFireball,
				minion.Projectile.damage,
				minion.Projectile.knockBack,
				minion.Projectile.owner,
				ai0: ai0,
				CrossMod.ReplaceValueWithSummonersShineMinionPower(5, minion.Projectile, 0));
		}
		public static void ImpOnSpecialAbility(Projectile projectile, Entity _target, int specialType, bool fromServer)
		{
			ModLoader.TryGetMod("SummonersShine", out Mod summonersShine);
			const int SET_PROJDATA = 5;
			const int ENERGY = 2;
			const int SPECIALTIME = 5;
			const int REGENMULT = 1;
			const int SPECIALCASTTARGET = 6;
			summonersShine.Call(SET_PROJDATA, projectile, ENERGY, (float)0);
			summonersShine.Call(SET_PROJDATA, projectile, SPECIALTIME, 0);
			summonersShine.Call(SET_PROJDATA, projectile, REGENMULT, (float)0);
			summonersShine.Call(SET_PROJDATA, projectile, SPECIALCASTTARGET, (NPC)_target);

			//instantly attac after X duration, stop retreating

			//projectile.ai[1] = 90 - specialType;
			//projectile.ai[0] = 0;

			ImpMinion modProj = projectile.ModProjectile as ImpMinion;
			if (modProj != null)
			{
				modProj.hsHelper.lastShootFrame = modProj.hsHelper.Behavior.AnimationFrame - modProj.hsHelper.attackFrames + specialType;
			}
		}
		public static List<Projectile> PreAbility_Imp(Player player, int itemType, List<Projectile> valid)
		{
			ModLoader.TryGetMod("SummonersShine", out Mod summonersShine);
			if (valid.Count > 1)
			{
				int count = 0;
				int max = valid.Count - 1;
				valid.ForEach(i =>
				{
					const int SET_PROJDATA = 5;
					const int ABILTYPE = 0;
					summonersShine.Call(SET_PROJDATA, i, ABILTYPE, (int)(count * 30 / max)); //stagger the initial shooty
					count++;
				});
			}
			return valid;
		}

		public static Vector2 ImpTargetedMovement(Vector2 vectorToTargetPosition, Projectile projectile, ImpMinion imp, out int TempTarget)
		{
			TempTarget = -1;
			if (CrossMod.GetSummonersShineIsCastingSpecialAbility(projectile, ItemType<ImpMinionItem>()))
			{
				if (ImpPreSpecialTeleportShoot(projectile, imp.hsHelper))
				{
					Mod summonersShine = ModLoader.GetMod("SummonersShine");
					const int GET_PROJDATA = 7;
					const int SPECIALCASTTARGET = 6;
					NPC npc = (NPC)summonersShine.Call(GET_PROJDATA, projectile, SPECIALCASTTARGET);
					TempTarget = npc.whoAmI;
					return npc.Center - projectile.Center;
				}
			}
			return vectorToTargetPosition;
		}
		public static bool ImpPreSpecialTeleportShoot(Projectile projectile, HoverShooterHelper hoverShooterClass)
		{
			Mod summonersShine = ModLoader.GetMod("SummonersShine");
			const int USEFUL_FUNCS = 10;
			const int GET_MINION_POWER = 3;
			const int INCREMENT_SPECIAL_TIMER = 9;
			const int SET_PROJDATA = 5;
			const int GET_PROJDATA = 7;
			const int SPECIALCASTTARGET = 6;
			//if (ModUtils.IsCastingSpecialAbility(projData, projFuncs.SourceItem))
			//{
			Player player = Main.player[projectile.owner];
			summonersShine.Call(USEFUL_FUNCS, INCREMENT_SPECIAL_TIMER, projectile, 420, (float)1);
			//projectile.IncrementSpecialAbilityTimer(projFuncs, projData, 420);

			//if (projData.specialCastTarget == null || projData.specialCastTarget.active == false || projData.specialCastTarget.DistanceSQ(player.Center) > 1000 * 1000)
			//{
			NPC npc = (NPC)summonersShine.Call(GET_PROJDATA, projectile, SPECIALCASTTARGET);
			if (npc == null || npc.active == false || npc.DistanceSQ(player.Center) > 1000 * 1000)
			{
				int newtarg = -1;
				projectile.Minion_FindTargetInRange(1400, ref newtarg, false);
				if (newtarg != -1)
					npc = Main.npc[newtarg];
				//projData.specialCastTarget = Main.npc[newtarg];
				else
					npc = null;
				//projData.specialCastTarget = null;
				summonersShine.Call(SET_PROJDATA, projectile, SPECIALCASTTARGET, npc);
			}
			//if (projData.specialCastTarget == null)
			if (npc == null)
				return false;

			bool? doAttack = hoverShooterClass.ExtraAttackConditionsMet?.Invoke();
			if ((doAttack is null || doAttack == true) && hoverShooterClass.Behavior.AnimationFrame - hoverShooterClass.lastShootFrame >= hoverShooterClass.attackFrames)
			//if (projectile.ai[1] > 88 || projectile.ai[1] == 0) //should shoot
			{
				Vector2 projOriginalPos = projectile.Center;

				NPC origTarget = npc;
				projectile.Center = origTarget.Center;
				if (!origTarget.CanBeChasedBy(projectile) || Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
				{

					projectile.Center = projOriginalPos;

					int targ = -1;
					projectile.Minion_FindTargetInRange(1400, ref targ, false);
					if (targ == -1)
						return false;
					origTarget = Main.npc[targ];
					projectile.Center = origTarget.Center;
				}
				Vector2 targetPos = origTarget.Center;

				int attackTarget = -1;

				//trick to exclude special cast target
				bool immune = origTarget.chaseable;
				origTarget.chaseable = false;
				projectile.Minion_FindTargetInRange(1400, ref attackTarget, false);
				origTarget.chaseable = immune;

				Vector2 line;
				float dist = Main.rand.NextFloat(8, 24);
				bool failed = false;
				if (attackTarget != -1)
				{
					NPC target = Main.npc[attackTarget];
					//SingleThreadExploitation.impTarget = player.MinionAttackTargetNPC;
					player.MinionAttackTargetNPC = attackTarget;

					line = targetPos - target.Center;
					line.Normalize();
					line *= dist;
				}
				else
				{
					line = new Vector2(0, dist).RotatedBy(Main.rand.NextFloat(-MathF.PI, MathF.PI));
					//SingleThreadExploitation.impTarget = player.MinionAttackTargetNPC;
					player.MinionAttackTargetNPC = attackTarget;
					failed = true;
				}

				line *= 16;
				/*
				bool tooCramped = true;
				Vector2 endPos = Vector2.Zero;
				int give = 48;
				for (int i = 0; i < 16; i++) {
					endPos = ModUtils.GetLineCollision(targetPos, line, out tooCramped, give);
					if (!tooCramped)
					{
						NPC targ = projData.specialCastTarget;
						tooCramped = !Collision.CanHitLine(projectile.position, projectile.width, projectile.height, targ.position, targ.width, targ.height);
					}
					if (!tooCramped)
						break;
					else
					{
						line = new Vector2(0, dist).RotatedBy(Main.rand.NextFloat(-MathF.PI, MathF.PI)) * 16;
					}
				}
				if (tooCramped)
				{
					projectile.Center = projOriginalPos;
					return true;
				}*/


				Vector2 endPos = FurthestCanHitLine(targetPos, line);
				bool tooFar = FindImpCenterDiffSqr(player.Center, player.direction, endPos, projectile) > 1000 * 1000;

				if (failed)
				{
					for (int x = 0; x < 16; x++)
					{
						if (!tooFar && (endPos - targetPos).LengthSquared() > dist * dist * 64)
						{
							break;
						}
						dist = Main.rand.NextFloat(8, 24);
						line = new Vector2(0, dist).RotatedBy(Main.rand.NextFloat(-MathF.PI, MathF.PI)) * 16;
						endPos = FurthestCanHitLine(targetPos, line);
						tooFar = FindImpCenterDiffSqr(player.Center, player.direction, endPos, projectile) > 1000 * 1000;
					}
				}

				if (tooFar)
				{
					projectile.Center = projOriginalPos;
					return false;
				}

				//insta shoot
				//projectile.ai[1] = 90;
				//projectile.ai[0] = 0;

				projectile.velocity = Vector2.Zero;
				//projData.lastRelativeVelocity = Vector2.Zero; //no need

				projectile.Center = projOriginalPos;
				ImpDespawnEffect(projectile, player);
				projectile.Center = endPos;
				ImpDespawnEffect(projectile, player);

				//shoot here
				return true;
			}
			/*else
			{
				//SingleThreadExploitation.impTarget = player.MinionAttackTargetNPC;
				//player.MinionAttackTargetNPC = projData.specialCastTarget.whoAmI;
			}*/
			return false;
		}
		public static void ImpDespawnEffect(Projectile projectile, Player player)
		{
			DrawImpTeleportSigil(projectile.Center, i => {
				Vector2 difference = i.position - projectile.Center;
				i.velocity = difference * 0.01f;
				i.noGravity = true;
				i.shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
				i.scale = 1.5f;
			});
		}

		static float FindImpCenterDiffSqr(Vector2 center, int playerDir, Vector2 projCenter, Projectile projectile)
		{
			Vector2 rv = center - projCenter;

			int projPos = 1;

			for (int m = 0; m < projectile.whoAmI; m++)
			{
				if (Main.projectile[m].active && Main.projectile[m].owner == projectile.owner && Main.projectile[m].type == projectile.type)
				{
					projPos++;
				}
			}

			rv.X -= (float)(10 * playerDir);
			rv.X -= (float)(projPos * 40 * playerDir);
			rv.Y -= 10f;

			return rv.LengthSquared();
		}
		static int[] ImpSigilParticles = {
			DustID.SolarFlare,
			DustID.DemonTorch,
			DustID.Torch,
		};
		static void DrawImpSigil(Vector2 center, Action<Dust> OnCreate)
		{
			float angle = Main.rand.NextFloat(6.283f);
			DrawArcWithParticles(center, angle, Main.rand.NextFloat(4.398f, 6.6f), Main.rand.NextFloat(32, 48), Main.rand.NextFloat(32, 48), ImpSigilParticles, 48, OnCreate);
			int maxSmallCircles = Main.rand.Next(2, 6);
			for (int x = 0; x < maxSmallCircles; x++)
			{
				angle = Main.rand.NextFloat(6.283f);
				Vector2 pos = center + new Vector2(0, Main.rand.NextFloat(32, 48)).RotatedBy(Main.rand.NextFloat(6.283f));
				DrawArcWithParticles(pos, angle, Main.rand.NextFloat(4.398f, 6.6f), Main.rand.NextFloat(16, 32), Main.rand.NextFloat(16, 32), ImpSigilParticles, 24, OnCreate);
			}
			int maxLines = Main.rand.Next(3, 7);
			Vector2 newPos = center + new Vector2(0, Main.rand.NextFloat(0, 80)).RotatedBy(Main.rand.NextFloat(6.283f));
			for (int x = 0; x < maxLines; x++)
			{
				Vector2 pos = center + new Vector2(0, Main.rand.NextFloat(0, 80)).RotatedBy(Main.rand.NextFloat(6.283f));
				DrawLineWithParticles(pos, newPos, ImpSigilParticles, 15, OnCreate);
				newPos = pos;
			}
		}
		static void DrawImpTeleportSigil(Vector2 center, Action<Dust> OnCreate)
		{
			float angle = Main.rand.NextFloat(6.283f);
			DrawArcWithParticles(center, angle, Main.rand.NextFloat(4.398f, 6.6f), Main.rand.NextFloat(12, 24), Main.rand.NextFloat(12, 24), ImpSigilParticles, 16, OnCreate);
			int maxLines = Main.rand.Next(2, 4);
			Vector2 newPos = center + new Vector2(0, Main.rand.NextFloat(0, 30)).RotatedBy(Main.rand.NextFloat(6.283f));
			for (int x = 0; x < maxLines; x++)
			{
				Vector2 pos = center + new Vector2(0, Main.rand.NextFloat(0, 30)).RotatedBy(Main.rand.NextFloat(6.283f));
				DrawLineWithParticles(pos, newPos, ImpSigilParticles, 15, OnCreate);
				newPos = pos;
			}
		}
		public static void DrawLineWithParticles(Vector2 start, Vector2 end, int[] particleIDs, int particleCount, Action<Dust> OnCreate = null)
		{
			for (int x = 0; x < particleCount; x++)
			{
				float lerpVal = particleCount != 0 ? x / (float)(particleCount - 1) : 0;
				Vector2 result = Vector2.Lerp(start, end, lerpVal);
				Dust particle = Main.dust[Dust.NewDust(result, 0, 0, particleIDs[Main.rand.Next(particleIDs.Length)])];
				particle.velocity *= 0.01f;
				particle.noGravity = true;
				if (OnCreate != null)
				{
					OnCreate(particle);
				}
			}
		}
		public static void DrawArcWithParticles(Vector2 center, float startAngle, float angleToTurn, float startLength, float endLength, int[] particleIDs, int particleCount, Action<Dust> OnCreate = null)
		{
			if (particleCount <= 1)
			{
				return;
			}
			for (int x = 0; x < particleCount; x++)
			{
				float ratio = (float)x / (particleCount - 1);
				float len = (endLength - startLength) * ratio + startLength;
				Vector2 result = center + new Vector2(0, len).RotatedBy(startAngle + angleToTurn * ratio);
				Dust particle = Main.dust[Dust.NewDust(result, 0, 0, particleIDs[Main.rand.Next(particleIDs.Length)])];
				if (OnCreate != null)
				{
					OnCreate(particle);
				}
			}
		}
		public static Vector2 FurthestCanHitLine(Vector2 start, Vector2 line)
		{
			//ripped from Collision.CanHitLine
			int center1x = (int)((start.X) / 16f);
			int center1y = (int)((start.Y) / 16f);
			int center2x = (int)((start + line).X / 16f);
			int center2y = (int)((start + line).Y / 16f);

			Vector2 farthestCanHit = start;

			Math.Clamp(center1x, 1, Main.maxTilesX - 1);
			Math.Clamp(center1y, 1, Main.maxTilesY - 1);
			Math.Clamp(center2x, 1, Main.maxTilesX - 1);
			Math.Clamp(center2y, 1, Main.maxTilesY - 1);
			float xDiff = (float)Math.Abs(center1x - center2x);
			float yDiff = (float)Math.Abs(center1y - center2y);
			//same location
			if (xDiff == 0f && yDiff == 0f)
			{
				return farthestCanHit;
			}
			float xOverY = 1f;
			float yOverX = 1f;
			if (xDiff == 0f || yDiff == 0f)
			{
				if (xDiff == 0f)
				{
					xOverY = 0f;
				}
				if (yDiff == 0f)
				{
					yOverX = 0f;
				}
			}
			else if (xDiff > yDiff)
			{
				xOverY = xDiff / yDiff;
			}
			else
			{
				yOverX = yDiff / xDiff;
			}
			float num9 = 0f;
			float num10 = 0f;
			int centerOneUnderCenter2 = 1;
			if (center1y < center2y)
			{
				centerOneUnderCenter2 = 2;
			}
			int xDiffInt = (int)xDiff;
			int yDiffInt = (int)yDiff;
			int stepX = Math.Sign(center2x - center1x);
			int stepY = Math.Sign(center2y - center1y);
			bool flag = false;
			bool flag2 = false;

			try
			{
				for (; ; )
				{
					if (centerOneUnderCenter2 != 1)
					{
						if (centerOneUnderCenter2 == 2)
						{
							num9 += xOverY;
							int num16 = (int)num9;
							num9 %= 1f;
							for (int i = 0; i < num16; i++)
							{
								if (Main.tile[center1x, center1y - 1] == null
									|| Main.tile[center1x, center1y] == null
									|| Main.tile[center1x, center1y + 1] == null)
								{
									return farthestCanHit;
								}
								Tile tile = Main.tile[center1x, center1y - 1];
								Tile tile2 = Main.tile[center1x, center1y + 1];
								Tile tile3 = Main.tile[center1x, center1y];
								if ((!tile.IsActuated && tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || (!tile2.IsActuated && tile2.HasTile && Main.tileSolid[tile2.TileType] && !Main.tileSolidTop[tile2.TileType]) || (!tile3.IsActuated && tile3.HasTile && Main.tileSolid[tile3.TileType] && !Main.tileSolidTop[tile3.TileType]))
								{
									return farthestCanHit;
								}
								if (xDiffInt == 0 && yDiffInt == 0)
								{
									flag = true;
									break;
								}
								farthestCanHit = new Vector2(center1x, center1y) * 16;
								center1x += stepX;
								xDiffInt--;
								if (xDiffInt == 0 && yDiffInt == 0 && num16 == 1)
								{
									flag2 = true;
								}
							}
							if (yDiffInt != 0)
							{
								centerOneUnderCenter2 = 1;
							}
						}
					}
					else
					{
						num10 += yOverX;
						int numSteps = (int)num10;
						num10 %= 1f;
						for (int j = 0; j < numSteps; j++)
						{
							if (Main.tile[center1x - 1, center1y] == null
								|| Main.tile[center1x, center1y] == null
								|| Main.tile[center1x + 1, center1y] == null)
							{
								return farthestCanHit;
							}
							Tile tile4 = Main.tile[center1x - 1, center1y];
							Tile tile5 = Main.tile[center1x + 1, center1y];
							Tile tile6 = Main.tile[center1x, center1y];
							if ((!tile4.IsActuated && tile4.HasTile && Main.tileSolid[tile4.TileType] && !Main.tileSolidTop[tile4.TileType]) || (!tile5.IsActuated && tile5.HasTile && Main.tileSolid[tile5.TileType] && !Main.tileSolidTop[tile5.TileType]) || (!tile6.IsActuated && tile6.HasTile && Main.tileSolid[tile6.TileType] && !Main.tileSolidTop[tile6.TileType]))
							{
								return farthestCanHit;
							}
							if (xDiffInt == 0 && yDiffInt == 0)
							{
								flag = true;
								break;
							}
							farthestCanHit = new Vector2(center1x, center1y) * 16;
							center1y += stepY;
							yDiffInt--;
							if (xDiffInt == 0 && yDiffInt == 0 && numSteps == 1)
							{
								flag2 = true;
							}
						}
						if (xDiffInt != 0)
						{
							centerOneUnderCenter2 = 2;
						}
					}
					if (Main.tile[center1x, center1y] == null)
					{
						return farthestCanHit;
					}
					Tile tile7 = Main.tile[center1x, center1y];
					if (!tile7.IsActuated && tile7.HasTile && Main.tileSolid[tile7.TileType] && !Main.tileSolidTop[tile7.TileType])
					{
						return farthestCanHit;
					}
					if (flag || flag2)
					{
						farthestCanHit = start + line;
						return farthestCanHit;
					}
				}
			}
			catch
			{
				return farthestCanHit;
			}
		}
	}
}
