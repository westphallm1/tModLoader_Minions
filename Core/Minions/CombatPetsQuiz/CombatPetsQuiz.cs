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
using Terraria.Localization;

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

		internal LocalizedText[] IntroLines = Array.Empty<LocalizedText>();
		internal LocalizedText[] OutroLines = Array.Empty<LocalizedText>();

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
			Result = DefaultPetsQuizData.ResultsMap[GetResultType()];
			CurrentState = QuizState.OUTRO;
			dialogIdx = 0;
		}

		internal bool EndOfIntro => CurrentState == QuizState.INTRO && dialogIdx == IntroLines.Length;

		internal bool OnLastLine => CurrentState == QuizState.OUTRO && dialogIdx == OutroLines.Length;

		// Tenuous condition, check if any dialog shown so far says the name of the minion
		internal bool ShouldShowPortrait => CurrentState == QuizState.OUTRO && OutroLines.Take(dialogIdx+1).Any(l=>l.ToString().Contains("{1}"));

		internal string CurrentDialogText => CurrentState switch
		{
			QuizState.INTRO => IntroLines[dialogIdx].ToString(),
			QuizState.QUIZ => CurrentQuestion.QuestionText.ToString(),
			QuizState.OUTRO => string.Format(OutroLines[dialogIdx].Format(Result?.Description.ToString() ?? "", Result?.PetName.ToString() ?? "")),
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

	internal class QuizResult
	{
		// TODO these should probably be drawn directly from the ModItem. Not sure what the best way to reverse-lookup is
		internal Asset<Texture2D> PortraitTexture { get; set; }

		internal LocalizedText Description { get; private set; }
		internal LocalizedText PetName { get; private set; }

		internal int ItemType { get; private set; }
		internal int BuffType { get; private set; }

		internal QuizResult(PersonalityType personalityType, int itemType, int buffType)
		{
			string category = $"CombatPetQuiz.";
			Mod mod = GetInstance<AmuletOfManyMinions>();
			Description = DefaultPetsQuizData.PersonalityTypeNames[personalityType];
			PetName = Language.GetOrRegister(mod.GetLocalizationKey($"{category}QuizResults.{personalityType}"));
			ItemType = itemType;
			BuffType = buffType;
		}

		internal static Dictionary<PersonalityType, QuizResult> MakeResultsMap()
		{
			var dict = new Dictionary<PersonalityType, QuizResult>()
			{
				[CALM] = new QuizResult(CALM, ItemType<PlantPupMinionItem>(), BuffType<PlantPupMinionBuff>()),
				[HARDY] = new QuizResult(HARDY, ItemType<TruffleTurtleMinionItem>(), BuffType<TruffleTurtleMinionBuff>()),
				[QUIRKY] = new QuizResult(QUIRKY, ItemType<AxolotlMinionItem>(), BuffType<AxolotlMinionBuff>()),
				[JOLLY] = new QuizResult(JOLLY, ItemType<LilGatorMinionItem>(), BuffType<LilGatorMinionBuff>()),
				[QUIET] = new QuizResult(QUIET, ItemType<SmolederMinionItem>(), BuffType<SmolederMinionBuff>()),
				[BOLD] = new QuizResult(BOLD, ItemType<CinderHenMinionItem>(), BuffType<CinderHenMinionBuff>()),
				[HASTY] = new QuizResult(HASTY, ItemType<WyvernFlyMinionItem>(), BuffType<WyvernFlyMinionBuff>()),
				[RELAXED] = new QuizResult(RELAXED, ItemType<CloudiphantMinionItem>(), BuffType<CloudiphantMinionBuff>()),
			};
			return dict;
		}
	}

	internal class CombatPetsQuizQuestion
	{
		public LocalizedText QuestionText { get; private set; }
		public LocalizedText[] AnswerTexts { get; private set; }
		public PersonalityType[] AnswerValues { get; private set; }

		public CombatPetsQuizQuestion(string keyCategory, params (LocalizedText, PersonalityType)[] answers)
		{
			//Goal is to only register/detect localizations for the given PersonalityTypes in the specific order
			//Other personalities will not be detected
			//Possible to supply own LocalizedText, skipping registration of question and answers

			Init(keyCategory, null, answers);
		}
		public CombatPetsQuizQuestion(string keyCategory, LocalizedText question, params (LocalizedText, PersonalityType)[] answers)
		{
			Init(keyCategory, question, answers);
		}

		public CombatPetsQuizQuestion(string keyCategory, params PersonalityType[] answers)
		{
			var array = new (LocalizedText, PersonalityType)[answers.Length];
			for (int i = 0; i < answers.Length; i++)
			{
				//null to force localization registration
				array[i] = (null, answers[i]);
			}

			Init(keyCategory, null, array);
		}

		private void Init(string keyCategory, LocalizedText question, params (LocalizedText, PersonalityType)[] answers)
		{
			Mod mod = GetInstance<AmuletOfManyMinions>();
			QuestionText = question ?? Language.GetOrRegister(mod.GetLocalizationKey($"{keyCategory}Text"));

			AnswerTexts = new LocalizedText[answers.Length];
			AnswerValues = new PersonalityType[answers.Length];
			for (int i = 0; i < answers.Length; i++)
			{
				var type = answers[i].Item2;
				var text = answers[i].Item1 ?? Language.GetOrRegister(mod.GetLocalizationKey($"{keyCategory}Answers.{type}"));
				AnswerTexts[i] = text;
				AnswerValues[i] = type;
			}
		}

		public bool CanGivePointsForType(PersonalityType type) => AnswerValues.Contains(type);

		internal Func<int, CombatPetsQuizQuestion> AddFollowUpQuestion;
	}
}
