using AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.Core.Minions.CombatPetsQuiz.PersonalityType;
using Terraria.ID;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{
	internal enum QuizState
	{
		INTRO,
		QUIZ,
		OUTRO
	}

	internal class CombatPetsQuiz
	{
		public List<CombatPetsQuizQuestion> Questions { get; set; } = new();

		public readonly List<PersonalityType> GivenAnswers = new();

		private int currentQuestionIdx = 0;
		private int dialogIdx = 0;

		internal string[] IntroLines = Array.Empty<string>();
		internal string[] OutroLines = Array.Empty<string>();

		internal QuizState CurrentState { get; private set; } = QuizState.INTRO;
		internal QuizResult Result { get; private set; }

		internal int ExtraResultItemID { get; set; } = ItemID.None;


		public CombatPetsQuizQuestion CurrentQuestion => Questions[currentQuestionIdx];

		public void AnswerQuestion(int answerIdx)
		{
			GivenAnswers.Add(CurrentQuestion.AnswerValues[answerIdx]);
			if(CurrentQuestion.AddFollowUpQuestion?.Invoke(answerIdx) is CombatPetsQuizQuestion followUp)
			{
				Questions.Insert(currentQuestionIdx + 1, followUp);
			}
			currentQuestionIdx++;
		}

		public bool IsComplete() => GivenAnswers.Count == Questions.Count;

		// Not quite sure how this will resolve in the case of a tie
		public PersonalityType GetResultType() =>
			GivenAnswers
				.Where(t=> t != NONE)
				.Select(Type => (Type, GivenAnswers.Where(t => t == Type).Count()))
				.OrderByDescending(t => t.Item2)
				.Select(t => t.Type)
				.FirstOrDefault();

		public void ComputeResult()
		{
			Result = QuizResult.ResultsMap[GetResultType()];
			CurrentState = QuizState.OUTRO;
			dialogIdx = 0;
		}

		internal bool EndOfIntro => CurrentState == QuizState.INTRO && dialogIdx == IntroLines.Length;

		internal bool OnLastLine => CurrentState == QuizState.OUTRO && dialogIdx == OutroLines.Length;

		// Tenuous condition, check if any dialog shown so far says the name of the minion
		internal bool ShouldShowPortrait => CurrentState == QuizState.OUTRO && OutroLines.Take(dialogIdx+1).Any(l=>l.Contains("{1}"));

		internal string CurrentDialogText => CurrentState switch
		{
			QuizState.INTRO => IntroLines[dialogIdx],
			QuizState.QUIZ => CurrentQuestion.QuestionText,
			QuizState.OUTRO => string.Format(OutroLines[dialogIdx], Result?.Description ?? "", Result?.PetName ?? ""),
			_ => "",
		};

		internal void AdvanceDialog()
		{
			dialogIdx ++;
			if (EndOfIntro)
			{
				CurrentState = QuizState.QUIZ;
			}
		}

	}


	// TODO localize
	public enum PersonalityType
	{
		NONE, // Some questions don't directly give points
		CALM, // Plant Pup
		HARDY, // Truffle Turtle 
		QUIRKY, // Axolotl
		JOLLY, // Lil Gator
		QUIET, // Smoleder
		BOLD, // Cinder Hen
		HASTY, // Wyvernfly
		RELAXED // Cloudiphant
	}

	internal class QuizResult : ModSystem
	{
		internal string Description { get; private set; }
		// TODO these should probably be drawn directly from the ModItem. Not sure what the best way to reverse-lookup is
		internal Asset<Texture2D> PortraitTexture { get; set; }
		internal string PetName { get; private set; }

		internal int ItemType { get; private set; }
		internal int BuffType { get; private set; }

		internal QuizResult(string description, string petName, int itemType, int buffType)
		{
			Description = description;
			PetName = petName;
			ItemType = itemType;
			BuffType = buffType;
		}

		public override void Unload()
		{
			base.Unload();
			ResultsMap = null;
		}


		// TODO mod load hook
		internal static Dictionary<PersonalityType, QuizResult> ResultsMap;
		
		
		internal static Dictionary<PersonalityType, QuizResult> MakeResultsMap() => new()
		{
			[CALM] = new QuizResult("calm", "Plant Pup", ItemType<PlantPupMinionItem>(), BuffType<PlantPupMinionBuff>()),
			[HARDY] = new QuizResult("hardy", "Truffle Turtle", ItemType<TruffleTurtleMinionItem>(), BuffType<TruffleTurtleMinionBuff>()),
			[QUIRKY] = new QuizResult("quirky", "Axolittl", ItemType<AxolotlMinionItem>(), BuffType<AxolotlMinionBuff>()),
			[JOLLY] = new QuizResult("jolly", "Lil Gator", ItemType<LilGatorMinionItem>(), BuffType<LilGatorMinionBuff>()),
			[QUIET] = new QuizResult("quiet", "Smoleder", ItemType<SmolederMinionItem>(), BuffType<SmolederMinionBuff>()),
			[BOLD] = new QuizResult("bold", "Cinder Hen", ItemType<CinderHenMinionItem>(), BuffType<CinderHenMinionBuff>()),
			[HASTY] = new QuizResult("hasty", "Wyvernfly", ItemType<WyvernFlyMinionItem>(), BuffType<WyvernFlyMinionBuff>()),
			[RELAXED] = new QuizResult("relaxed", "Cloudiphant", ItemType<CloudiphantMinionItem>(), BuffType<CloudiphantMinionBuff>()),
		};
	}

	internal class CombatPetsQuizQuestion
	{
		public string QuestionText { get; private set; }
		public string[] AnswerTexts { get; private set; }
		public PersonalityType[] AnswerValues { get; private set; }

		public CombatPetsQuizQuestion(string questionText, params (string, PersonalityType)[] answers)
		{
			QuestionText = questionText;
			AnswerTexts = new string[answers.Length];
			AnswerValues = new PersonalityType[answers.Length];
			for(int i = 0; i < answers.Length; i++)
			{
				AnswerTexts[i] = answers[i].Item1;
				AnswerValues[i] = answers[i].Item2;
			}
		}

		public bool CanGivePointsForType(PersonalityType type) => AnswerValues.Contains(type);

		internal Func<int, CombatPetsQuizQuestion> AddFollowUpQuestion;
	}

}
