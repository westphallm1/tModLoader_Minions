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
	internal enum QuizState
	{
		INTRO,
		QUIZ,
		OUTRO
	}

	internal class CombatPetsQuizModPlayer : ModPlayer
	{
		internal bool IsTakingQuiz { get; private set; }

		internal CombatPetsQuiz CurrentQuiz { get; private set; }
		internal QuizState QuizState { get; private set; }

		internal int dialogLine = 0;

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
			CurrentQuiz = CombatPetsQuiz.MakeQuizWithDominantTrait(preferredType, 6);
			QuizState = QuizState.INTRO;
			dialogLine = 0;
		}

		internal void AdvanceDialog()
		{
			dialogLine++;
			if (EndOfIntro)
			{
				QuizState = QuizState.QUIZ;
			}
			else if (OnLastLine)
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

		internal bool EndOfIntro => QuizState == QuizState.INTRO && dialogLine == IntroLines.Length;

		internal bool OnLastLine => QuizState == QuizState.OUTRO && dialogLine == OutroLines.Length;

		// very hardcoded
		internal bool ShouldShowPortrait => QuizState == QuizState.OUTRO && dialogLine > 1;

		// TODO maybe unique texture instead of resuing the buff
		internal Asset<Texture2D> PortraitTexture => result.PortraitTexture;

		internal string CurrentDialogText()
		{
			return QuizState switch
			{
				QuizState.INTRO => IntroLines[dialogLine],
				QuizState.QUIZ => CurrentQuiz.CurrentQuestion.QuestionText,
				QuizState.OUTRO => string.Format(OutroLines[dialogLine], result?.Description ?? "", result?.PetName ?? ""),
				_ => "",
			};
		}

		internal void AnswerQuestion(int answerIdx)
		{
			CurrentQuiz.AnswerQuestion(answerIdx);
			if(CurrentQuiz.IsComplete())
			{
				QuizState = QuizState.OUTRO;
				dialogLine = 0;
				result = QuizResult.ResultsMap[CurrentQuiz.GetResultType()];
			}
		}

		internal string[] IntroLines =
		{
			"Welcome, to the world of Terraria!",
			"Terraria can be a dangerous place, but having a good friend at your side always makes it better!",
			"To help you and your new friend get to know each other better, here's a few questions!",
			"Be sure to answer sincerely. Let's begin!",
		};

		internal string[] OutroLines =
		{
			"Thank you for answering those questions!",
			"Based on your answers, you appear to be... the {0} type!",
			"A {0} person like yourself would be great friends with... the {1}!",
			"May you and your new friend enjoy many adventures!",
		};
	}
}
