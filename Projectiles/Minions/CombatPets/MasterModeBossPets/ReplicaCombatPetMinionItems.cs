using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent.Events;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public abstract class ReplicaCombatPetMinionItem<TBuff, TProj> : CombatPetMinionItem<TBuff, TProj> where TBuff: ModBuff where TProj: Minion
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(gold: 50);
			Item.master = false;
			Item.masterOnly = false;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			byte oldA = drawColor.A;
			drawColor *= 0.75f;
			drawColor.A = oldA;
			Texture2D texture = Terraria.GameContent.TextureAssets.Item[Type].Value;
			spriteBatch.Draw(texture, position, frame, drawColor, 0, origin, scale, SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			byte oldA = lightColor.A;
			lightColor *= 0.75f;
			lightColor.A = oldA;
			Texture2D texture = Terraria.GameContent.TextureAssets.Item[Type].Value;
			spriteBatch.Draw(texture, Item.position - Main.screenPosition, texture.Bounds, lightColor, 0, Vector2.Zero, scale, SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		public override void AddRecipes()
		{
			// no recipe for these, sold by travelling merchant
		}
	}

	public class AlienSkaterReplicaMinionItem : ReplicaCombatPetMinionItem<AlienSkaterMinionBuff, AlienSkaterMinion>
	{
		internal override int VanillaItemID => ItemID.MartianPetItem;
		internal override string VanillaItemName => "MartianPetItem";
	}

	public class BabyOgreReplicaMinionItem : ReplicaCombatPetMinionItem<BabyOgreMinionBuff, BabyOgreMinion>
	{
		internal override int VanillaItemID => ItemID.DD2OgrePetItem;
		internal override string VanillaItemName => "DD2OgrePetItem";
	}

	public class DestroyerLiteReplicaMinionItem : ReplicaCombatPetMinionItem<DestroyerLiteMinionBuff, DestroyerLiteMinion>
	{
		internal override int VanillaItemID => ItemID.DestroyerPetItem;
		internal override int AttackPatternUpdateTier => 5;
		internal override string VanillaItemName => "DestroyerPetItem";
	}

	public class EaterOfWormsReplicaMinionItem : ReplicaCombatPetMinionItem<EaterOfWormsMinionBuff, EaterOfWormsMinion>
	{
		internal override int VanillaItemID => ItemID.EaterOfWorldsPetItem;
		internal override string VanillaItemName => "EaterOfWorldsPetItem";
	}

	public class EverscreamReplicaSaplingMinionItem : ReplicaCombatPetMinionItem<EverscreamSaplingMinionBuff, EverscreamSaplingMinion>
	{
		internal override int VanillaItemID => ItemID.EverscreamPetItem;
		internal override string VanillaItemName => "EverscreamPetItem";
	}

	public class HoneyBeeReplicaMinionItem : ReplicaCombatPetMinionItem<HoneyBeeMinionBuff, HoneyBeeMinion>
	{
		internal override int VanillaItemID => ItemID.QueenBeePetItem;
		internal override string VanillaItemName => "QueenBeePetItem";
	}

	public class IceQueenReplicaMinionItem : ReplicaCombatPetMinionItem<IceQueenMinionBuff, IceQueenMinion>
	{
		internal override int VanillaItemID => ItemID.IceQueenPetItem;
		internal override string VanillaItemName => "IceQueenPetItem";
	}

	public class ItsyBetsyReplicaMinionItem : ReplicaCombatPetMinionItem<ItsyBetsyMinionBuff, ItsyBetsyMinion>
	{
		internal override int VanillaItemID => ItemID.DD2BetsyPetItem;
		internal override string VanillaItemName => "DD2BetsyPetItem";
	}

	public class MiniPrimeReplicaMinionItem : ReplicaCombatPetMinionItem<MiniPrimeMinionBuff, MiniPrimeMinion>
	{
		internal override int VanillaItemID => ItemID.SkeletronPrimePetItem;
		internal override string VanillaItemName => "SkeletronPrimePetItem";
	}
	
	public class MoonlingReplicaMinionItem : ReplicaCombatPetMinionItem<MoonlingMinionBuff, MoonlingMinion>
	{
		internal override int VanillaItemID => ItemID.MoonLordPetItem;
		internal override string VanillaItemName => "MoonLordPetItem";
	}

	public class PhantasmalDragonReplicaMinionItem : ReplicaCombatPetMinionItem<PhantasmalDragonMinionBuff, PhantasmalDragonMinion>
	{
		internal override int VanillaItemID => ItemID.LunaticCultistPetItem;
		internal override int AttackPatternUpdateTier => 5;
		internal override string VanillaItemName => "LunaticCultistPetItem";
	}

	public class PlanteraSeedlingReplicaMinionItem : ReplicaCombatPetMinionItem<PlanteraSeedlingMinionBuff, PlanteraSeedlingMinion>
	{
		internal override int VanillaItemID => ItemID.PlanteraPetItem;
		internal override int AttackPatternUpdateTier => 5;
		internal override string VanillaItemName => "PlanteraPetItem";
	}
	public class RezAndSpazReplicaMinionItem : ReplicaCombatPetMinionItem<RezAndSpazMinionBuff, RezMinion>
	{
		internal override int VanillaItemID => ItemID.TwinsPetItem;
		internal override int AttackPatternUpdateTier => 5;
		internal override string VanillaItemName => "TwinsPetItem";
	}

	public class SkeletronJrReplicaMinionItem : ReplicaCombatPetMinionItem<SkeletronJrMinionBuff, SkeletronJrMinion>
	{
		internal override int VanillaItemID => ItemID.SkeletronPetItem;
		internal override string VanillaItemName => "SkeletronPetItem";
	}

	public class SlimePrinceReplicaMinionItem : ReplicaCombatPetMinionItem<SlimePrinceMinionBuff, SlimePrinceMinion>
	{
		internal override int VanillaItemID => ItemID.KingSlimePetItem;
		internal override int AttackPatternUpdateTier => 4;
		internal override string VanillaItemName => "KingSlimePetItem";
	}
	public class SlimePrincessReplicaMinionItem : ReplicaCombatPetMinionItem<SlimePrincessMinionBuff, SlimePrincessMinion>
	{
		internal override int VanillaItemID => ItemID.QueenSlimePetItem;
		internal override int AttackPatternUpdateTier => 4;
		internal override string VanillaItemName => "QueenSlimePetItem";
	}
	public class SpiderBrainReplicaMinionItem : ReplicaCombatPetMinionItem<SpiderBrainMinionBuff, SpiderBrainMinion>
	{
		internal override int VanillaItemID => ItemID.BrainOfCthulhuPetItem;
		internal override int AttackPatternUpdateTier => 4;
		internal override string VanillaItemName => "BrainOfCthulhuPetItem";
	}
	public class SuspiciousEyeReplicaMinionItem : ReplicaCombatPetMinionItem<SuspiciousEyeMinionBuff, SuspiciousEyeMinion>
	{
		internal override int VanillaItemID => ItemID.EyeOfCthulhuPetItem;
		internal override int AttackPatternUpdateTier => 4;
		internal override string VanillaItemName => "EyeOfCthulhuPetItem";
	}
	public class TinyFishronReplicaMinionItem : ReplicaCombatPetMinionItem<TinyFishronMinionBuff, TinyFishronMinion>
	{
		internal override int VanillaItemID => ItemID.DukeFishronPetItem;
		internal override int AttackPatternUpdateTier => 5;
		internal override string VanillaItemName => "DukeFishronPetItem";
	}

	public class DeerclopsReplicaMinionItem : ReplicaCombatPetMinionItem<DeerclopsMinionBuff, DeerclopsMinion>
	{
		internal override int VanillaItemID => ItemID.DeerclopsPetItem;
		internal override string VanillaItemName => "DeerclopsPetItem";
	}

	public class ReplicaMinionItemTravellingMerchantNPC: GlobalNPC
	{
		static (int, Func<bool>)[] ReplicaSellConditions;

		public override void SetStaticDefaults()
		{
			ReplicaSellConditions = new (int, Func<bool>)[]
			{
				// main bosses
				(ItemType<SlimePrinceReplicaMinionItem>(), ()=>NPC.downedBoss2),
				(ItemType<SuspiciousEyeReplicaMinionItem>(), ()=>NPC.downedBoss1),
				(ItemType<EaterOfWormsReplicaMinionItem>(), ()=>NPC.downedBoss2 && !WorldGen.crimson),
				(ItemType<SpiderBrainReplicaMinionItem>(), ()=>NPC.downedBoss2 && WorldGen.crimson),
				(ItemType<HoneyBeeReplicaMinionItem>(), ()=>NPC.downedQueenBee),
				(ItemType<DeerclopsReplicaMinionItem>(), ()=>NPC.downedDeerclops),
				(ItemType<SkeletronJrReplicaMinionItem>(), ()=>NPC.downedBoss3),
				(ItemType<SlimePrincessReplicaMinionItem>(), ()=>NPC.downedQueenSlime),
				(ItemType<DestroyerLiteReplicaMinionItem>(), ()=>NPC.downedMechBoss1),
				(ItemType<RezAndSpazReplicaMinionItem>(), ()=>NPC.downedMechBoss2),
				(ItemType<MiniPrimeReplicaMinionItem>(), ()=>NPC.downedMechBoss3),
				(ItemType<PlanteraSeedlingReplicaMinionItem>(), ()=>NPC.downedPlantBoss),
				(ItemType<TinyFishronReplicaMinionItem>(), ()=>NPC.downedFishron),
				(ItemType<PhantasmalDragonReplicaMinionItem>(), ()=>NPC.downedAncientCultist),
				(ItemType<MoonlingReplicaMinionItem>(), ()=>NPC.downedMoonlord),

				// event bosses
				(ItemType<AlienSkaterReplicaMinionItem>(), ()=>NPC.downedMartians),
				(ItemType<IceQueenReplicaMinionItem>(), ()=>NPC.downedChristmasIceQueen),
				(ItemType<EverscreamReplicaSaplingMinionItem>(), ()=>NPC.downedChristmasTree),
				(ItemType<BabyOgreReplicaMinionItem>(), ()=>DD2Event.DownedInvasionT2),
				(ItemType<ItsyBetsyReplicaMinionItem>(), ()=>DD2Event.DownedInvasionT3),
			};
		}

		public override void Unload()
		{
			ReplicaSellConditions = null;
		}

		public override void SetupTravelShop(int[] shop, ref int nextSlot)
		{
			base.SetupTravelShop(shop, ref nextSlot);
			List<int> possibleItemIds = ReplicaSellConditions.Where(sc => sc.Item2.Invoke()).Select(sc => sc.Item1).ToList();
			if(possibleItemIds.Count == 0)
			{
				return;
			}
			// 15% chance to spawn one item, + 3% for each additional defeated boss, max 45%
			float spawnChance = Math.Min(0.45f, 0.15f + 0.03f * possibleItemIds.Count);
			if(Main.rand.NextFloat() < spawnChance)
			{
				if(possibleItemIds.Count == 0)
				{
					return;
				}
				int index = Main.rand.Next(possibleItemIds.Count - 1);
				shop[nextSlot++] = possibleItemIds[index];
				// also occassionally spawn a second one to be nice
				if(Main.rand.NextBool())
				{
					possibleItemIds.RemoveAt(index);
					index = Main.rand.Next(possibleItemIds.Count - 1);
					shop[nextSlot++] = possibleItemIds[index];
				}

			}
		}

	}
}
