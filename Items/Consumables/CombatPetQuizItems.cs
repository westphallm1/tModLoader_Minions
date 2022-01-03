using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Core.Minions.CombatPetsQuiz;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Items.Consumables
{
	class CombatPetQuizItems
	{
	}

	public class CombatPetFriendshipBow: ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Grants you a special combat pet based on your personality!\n" +
				"Your environment may influence the result...");
			DisplayName.SetDefault("Bow of Friendship");
		}

		public static void SetFriendshipBowDefaults(Item Item)
		{
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = new LegacySoundStyle(2, 17);
			Item.consumable = true;
		}

		public override void SetDefaults()
		{
			SetFriendshipBowDefaults(Item);
		}

		public override bool CanUseItem(Player player) => !player.GetModPlayer<CombatPetsQuizModPlayer>().IsTakingQuiz;

		public override bool? UseItem(Player player)
		{
			if(player.whoAmI == Main.myPlayer)
			{
				player.GetModPlayer<CombatPetsQuizModPlayer>().StartPersonalityQuiz();
				Item.stack--;
			}
			return null;
		}
	}

	public class CombatPetTeamworkBow: ModItem
	{
		internal static string FriendshipBowRequirement =
			"You must use the Bow of Friendship before using this item.\n" +
			"Try searching wooden chests near spawn!";
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault(
				"Invites another special combat pet to your team!\n" +
				"It will always be different from the last pet that joined.");
			DisplayName.SetDefault("Bow of Teamwork");
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);
			if(Main.player[Main.myPlayer].GetModPlayer<CombatPetsQuizModPlayer>().HasTakenQuiz)
			{
				return;
			}
			tooltips.Add(new TooltipLine(Mod, "FriendshipBowRequirement", FriendshipBowRequirement)
			{
				overrideColor = Color.Gray
			});
		}

		public override void SetDefaults()
		{
			CombatPetFriendshipBow.SetFriendshipBowDefaults(Item);
		}

		public override bool CanUseItem(Player player) {
			var quizPlayer = player.GetModPlayer<CombatPetsQuizModPlayer>();
			if(quizPlayer.IsTakingQuiz)
			{
				return false;
			}
			if(!quizPlayer.HasTakenQuiz)
			{
				Main.NewText(FriendshipBowRequirement);
				return false;
			}
			return true;
		} 

		public override bool? UseItem(Player player)
		{
			if(player.whoAmI == Main.myPlayer)
			{
				player.GetModPlayer<CombatPetsQuizModPlayer>().StartPartnerQuiz();
				Item.stack--;
			}
			return null;
		}
	}

	public class CombatPetAncientFriendshipBow: ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Grants you a special combat pet of your choosing!");
			DisplayName.SetDefault("Ancient Bow of Friendship");
		}

		public override void SetDefaults()
		{
			CombatPetFriendshipBow.SetFriendshipBowDefaults(Item);
			Item.rare = ItemRarityID.Yellow;
		}

		public override bool CanUseItem(Player player) => !player.GetModPlayer<CombatPetsQuizModPlayer>().IsTakingQuiz;

		public override bool? UseItem(Player player)
		{
			if(player.whoAmI == Main.myPlayer)
			{
				player.GetModPlayer<CombatPetsQuizModPlayer>().StartAnyPartnerQuiz();
				Item.stack--;
			}
			return null;
		}
	}
}
