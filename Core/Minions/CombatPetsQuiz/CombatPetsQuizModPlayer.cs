using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{

	internal class CombatPetsQuizModPlayer : ModPlayer
	{
		internal bool IsTakingQuiz { get; private set; }

		internal CombatPetsQuiz CurrentQuiz { get; private set; }

		private QuizResult result;

		public override void OnEnterWorld(Player player)
		{
			base.OnEnterWorld(player);
			StartQuiz((PersonalityType)Main.rand.Next((int)PersonalityType.BOLD));
		}

		public override void SetStaticDefaults()
		{
			// TODO this is an odd place to autoload portrait textures
			base.SetStaticDefaults();
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

		internal void StartQuiz(PersonalityType preferredType)
		{
			IsTakingQuiz = true;
			// CurrentQuiz = CombatPetsQuiz.MakeQuizWithDominantTrait(preferredType, 6);
			CurrentQuiz = DefaultPetsQuizData.MakeClassSpecificQuiz(PersonalityType.QUIRKY);
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
			Player.QuickSpawnItem(result.ItemType);
			Player.AddBuff(result.BuffType, 2);
		}

		// very hardcoded
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
