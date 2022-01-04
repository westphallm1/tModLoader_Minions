using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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

	internal class CombatPetsQuizModPlayer : ModPlayer
	{
		internal bool IsTakingQuiz { get; private set; }

		internal CombatPetsQuiz CurrentQuiz { get; private set; }

		const int UsedTypeCacheSize = 3;
		internal PersonalityType[] LastUsedTypes = new PersonalityType[UsedTypeCacheSize];

		private QuizResult result;

		internal int LatestVersion = 0;

		internal bool HasTakenQuiz => LastUsedTypes.Any(t => t != PersonalityType.NONE);

		// TODO can't seem to get the SetStaticDefaults hook on ModSystem working, do it here instead
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			QuizResult.ResultsMap = QuizResult.MakeResultsMap();
			if(Main.dedServ)
			{
				return;
			}
			string TextureBasePath = GetType().Namespace.Replace('.', '/') + "/Portrait_";
			foreach(var personalityType in QuizResult.ResultsMap.Keys)
			{
				string TexturePath = TextureBasePath + Enum.GetName(personalityType);
				QuizResult.ResultsMap[personalityType].PortraitTexture = ModContent.Request<Texture2D>(TexturePath);
			}
		}

		internal void StartPersonalityQuiz()
		{
			IsTakingQuiz = true;
			CurrentQuiz = DefaultPetsQuizData.MakeQuizWithDominantTraits(Player, 6);
		}

		internal void StartPartnerQuiz()
		{
			IsTakingQuiz = true;
			CurrentQuiz = DefaultPetsQuizData.MakeClassSpecificQuiz(LastUsedTypes);
		}

		internal void StartAnyPartnerQuiz()
		{
			IsTakingQuiz = true;
			CurrentQuiz = DefaultPetsQuizData.MakeClassSpecificQuiz();
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
