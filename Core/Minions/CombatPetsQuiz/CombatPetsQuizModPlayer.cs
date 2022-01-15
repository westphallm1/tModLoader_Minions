using AmuletOfManyMinions.Core.BackportUtils;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{

	internal class CombatPetsQuizModPlayer : BackportModPlayer
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

		// TODO can't seem to get the SetStaticDefaults hook on ModSystem working, do it here instead
		public static void SetStaticDefaults()
		{
			QuizResult.ResultsMap = QuizResult.MakeResultsMap();
			if(Main.dedServ)
			{
				return;
			}
			string TextureBasePath = typeof(CombatPetsQuizModPlayer).Namespace.Replace('.', '/') + "/Portrait_";
			foreach(var personalityType in QuizResult.ResultsMap.Keys)
			{
				string TexturePath = TextureBasePath + Enum.GetName(typeof(PersonalityType), personalityType);
				QuizResult.ResultsMap[personalityType].PortraitTexture = ModContent.GetTexture(TexturePath);
			}
		}

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
			if(CurrentQuiz.ExtraResultItemID != ItemID.None)
			{
				Player.QuickSpawnItem(CurrentQuiz.ExtraResultItemID);
			}
			Player.QuickSpawnItem(result.ItemType);
			Player.AddBuff(result.BuffType, 2);
			// shift out the oldest personality quiz result, then save this answer
			for(int i = LastUsedTypes.Length -2; i >= 0; i--)
			{
				LastUsedTypes[i + 1] = LastUsedTypes[i];
			}
			LastUsedTypes[0] = CurrentQuiz.GetResultType();
			ConsumeQuizActivatingItem();
		}

		public override TagCompound Save()
		{
			TagCompound baseTag = new TagCompound();
			TagCompound quizTag = new TagCompound
			{
				["v"] = LatestVersion,
				["lastUsedTypes"] = LastUsedTypes.Select(t => (int)t).ToArray()
			};
			baseTag.Add("quiz", quizTag);
			return baseTag;
		}

		public override void Load(TagCompound tag)
		{
			TagCompound quizTag = tag.Get<TagCompound>("quiz");
			int version = quizTag.GetInt("v");
			if(version == 0 && quizTag.ContainsKey("lastUsedTypes"))
			{
				LastUsedTypes = quizTag.GetIntArray("lastUsedTypes").Select(v=>(PersonalityType)v).ToArray();
			}
		}

		internal bool ShouldShowPortrait => CurrentQuiz.ShouldShowPortrait;

		// TODO maybe unique texture instead of resuing the buff
		internal Texture2D PortraitTexture => CurrentQuiz.Result.PortraitTexture;

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
