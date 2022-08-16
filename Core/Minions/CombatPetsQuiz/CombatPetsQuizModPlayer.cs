using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{

	internal class CombatPetsQuizModPlayer : ModPlayer
	{
		internal bool IsTakingQuiz { get; private set; }

		internal CombatPetsQuiz CurrentQuiz { get; private set; }

		const int UsedTypeCacheSize = 3;
		internal PersonalityType[] LastUsedTypes = new PersonalityType[UsedTypeCacheSize];

		private QuizResult result;

		// Make sure this is in the player's inventory at all times while the quiz is active
		// otherwise cancel the quiz
		private int QuizActivatingItemType;

		internal int LatestVersion = 0;

		internal bool HasTakenQuiz => LastUsedTypes.Any(t => t != PersonalityType.NONE);

		internal void StartPersonalityQuiz(int itemType)
		{
			IsTakingQuiz = true;
			QuizActivatingItemType = itemType;
			CurrentQuiz = DefaultPetsQuizData.MakeQuizWithDominantTraits(Player, 6);
		}

		internal void StartPartnerQuiz(int itemType)
		{
			IsTakingQuiz = true;
			QuizActivatingItemType = itemType;
			CurrentQuiz = DefaultPetsQuizData.MakeClassSpecificQuiz(LastUsedTypes);
		}

		internal void StartAnyPartnerQuiz(int itemType)
		{
			IsTakingQuiz = true;
			QuizActivatingItemType = itemType;
			CurrentQuiz = DefaultPetsQuizData.MakeClassSpecificQuiz();
		}

		public override void PostUpdate()
		{
			if(IsTakingQuiz && !HasQuizItemInInventory())
			{
				IsTakingQuiz = false;
			}
		}

		/**
		 * Ensure the player has kept the quiz item in their inventory
		 * (to prevent duplication). Cancel the quiz otherwise
		 */
		private bool HasQuizItemInInventory()
		{
			for(int i = 0; i < Player.inventory.Length; i++)
			{
				Item item = Player.inventory[i];
				if(!item.IsAir && item.type == QuizActivatingItemType)
				{
					return true;
				}
			}
			return false;
		}

		private void ConsumeQuizActivatingItem()
		{
			for(int i = 0; i < Player.inventory.Length; i++)
			{
				Item item = Player.inventory[i];
				if(!item.IsAir && item.type == QuizActivatingItemType)
				{
					item.stack--;
					return;
				}
			}
		}

		internal void AdvanceDialog()
		{
			CurrentQuiz.AdvanceDialog();
			if(CurrentQuiz.OnLastLine)
			{
				FinishQuiz();
			}
		}

		internal void FinishQuiz()
		{
			IsTakingQuiz = false;
			LeveledCombatPetModPlayer petPlayer = Player.GetModPlayer<LeveledCombatPetModPlayer>();
			petPlayer.TemporarilyUnflagPetBuff(result.BuffType);
			var source = Player.GetSource_Misc("PlayerDropItemCheck");
			if (CurrentQuiz.ExtraResultItemID != ItemID.None)
			{
				Player.QuickSpawnItem(source, CurrentQuiz.ExtraResultItemID);
			}
			Player.QuickSpawnItem(source, result.ItemType);
			Player.AddBuff(result.BuffType, 2);
			// shift out the oldest personality quiz result, then save this answer
			for(int i = LastUsedTypes.Length -2; i >= 0; i--)
			{
				LastUsedTypes[i + 1] = LastUsedTypes[i];
			}
			LastUsedTypes[0] = CurrentQuiz.GetResultType();
			ConsumeQuizActivatingItem();
		}

		public override void SaveData(TagCompound tag)
		{
			TagCompound quizTag = new TagCompound
			{
				["v"] = LatestVersion,
				["lastUsedTypes"] = LastUsedTypes.Select(t => (int)t).ToArray()
			};
			tag.Add("quiz", quizTag);
		}

		public override void LoadData(TagCompound tag)
		{
			TagCompound quizTag = tag.Get<TagCompound>("quiz");
			int version = quizTag.GetInt("v");
			if(version == 0 && quizTag.ContainsKey("lastUsedTypes"))
			{
				LastUsedTypes = quizTag.GetIntArray("lastUsedTypes").Select(v=>(PersonalityType)v).ToArray();
			}
			base.LoadData(tag);
		}

		internal bool ShouldShowPortrait => CurrentQuiz.ShouldShowPortrait;

		// TODO maybe unique texture instead of resuing the buff
		internal Asset<Texture2D> PortraitTexture => CurrentQuiz.Result.PortraitTexture;

		internal void AnswerQuestion(int answerIdx)
		{
			CurrentQuiz.AnswerQuestion(answerIdx);
			if(CurrentQuiz.IsComplete())
			{
				CurrentQuiz.ComputeResult();
				result = QuizResult.ResultsMap[CurrentQuiz.GetResultType()];
			}
		}
	}
}
