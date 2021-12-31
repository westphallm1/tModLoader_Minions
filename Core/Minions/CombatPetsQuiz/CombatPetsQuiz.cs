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
		public List<CombatPetsQuizQuestion> Questions { get; private set; } = new();

		public readonly List<PersonalityType> GivenAnswers = new();

		private int currentQuestionIdx = 0;
		private int dialogIdx = 0;

		internal string[] IntroLines = Array.Empty<string>();
		internal string[] OutroLines = Array.Empty<string>();

		internal QuizState CurrentState { get; private set; } = QuizState.INTRO;
		internal QuizResult Result { get; private set; }


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

		public static CombatPetsQuiz MakeQuizWithDominantTrait(PersonalityType personalityType, int questionCount)
		{
			var quiz = new CombatPetsQuiz();
			quiz.Questions = DefaultQuestions.BasicQuestions
				.OrderBy(q => q.CanGivePointsForType(personalityType) ? 0 : 1)
				.ThenBy(q => Main.rand.Next())
				.Take(questionCount)
				.ToList();

			quiz.IntroLines = new string[]
			{
				"Welcome, to the world of Terraria!",
				"Terraria can be a dangerous place, but having a good friend at your side always makes it better!",
				"To help you and your new friend get to know each other better, here's a few questions!",
				"Be sure to answer sincerely. Let's begin!",
			};

			quiz.OutroLines = new string[]
			{
				"Thank you for answering those questions!",
				"Based on your answers, you appear to be... the {0} type!",
				"A {0} person like yourself would be great friends with... the {1}!",
				"May you and your new friend enjoy many adventures!",
			};
			return quiz;
		}

		public static CombatPetsQuiz MakeClassSpecificQuiz(params PersonalityType[] disallowedTypes)
		{
			var quiz = new CombatPetsQuiz();
			string[] allAnswerTexts =
			{
				"A friend with a fiery passion!",
				"A friend who can go with the flow!",
				"A friend who's down to earth!"
			};
			List<int> usedIndices = DefaultQuestions.ClassSpecificQuestions
				.Select((q, idx) => (Question: q, Idx: idx))
				.Where(q => !q.Question.AnswerValues.Any(a => disallowedTypes.Contains(a)))
				.Select(q => q.Idx)
				.ToList();


			CombatPetsQuizQuestion question = new (
				"What sort of friend would you like to have join you on your journey",
				usedIndices.Select(idx => (allAnswerTexts[idx], NONE)).ToArray())
			{
				AddFollowUpQuestion = idx => DefaultQuestions.ClassSpecificQuestions[usedIndices[idx]]
			};

			quiz.Questions = new List<CombatPetsQuizQuestion>() { question };

			quiz.IntroLines = new string[]
			{
				"Welcome back, to the world of Terraria!",
				"I hope you and your friends have been enjoying your adventures!",
				"Your exploits are known far and wide, and a new friend wants to join you!",
			};

			quiz.OutroLines = new string[]
			{
				"Ah, a {0} friend will be great to have!",
				"The {0} {1} is excited to join your team!",
				"May you and your new friend enjoy many adventures!",
			};
			return quiz;
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


		// TODO mod load hook
		internal static Dictionary<PersonalityType, QuizResult> ResultsMap = new Dictionary<PersonalityType, QuizResult>
		{
			[CALM] = new QuizResult("calm", "Plant Pup", ItemType<PlantPupMinionItem>(), BuffType<PlantPupMinionBuff>()),
			[HARDY] = new QuizResult("hardy", "Truffle Turtle", ItemType<TruffleTurtleMinionItem>(), BuffType<TruffleTurtleMinionBuff>()),
			[QUIRKY] = new QuizResult("quirky", "Axolittl", ItemType<AxolotlMinionItem>(), BuffType<AxolotlMinionBuff>()),
			[JOLLY] = new QuizResult("jolly", "Lil Gator", ItemType<LilGatorMinionItem>(), BuffType<LilGatorMinionBuff>()),
			[QUIET] = new QuizResult("quiet", "Smoleder", ItemType<SmolederMinionItem>(), BuffType<SmolederMinionBuff>()),
			[BOLD] = new QuizResult("bold", "Cinder Hen", ItemType<CinderHenMinionItem>(), BuffType<CinderHenMinionBuff>()),
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

	internal class DefaultQuestions : ModSystem
	{
		public static CombatPetsQuizQuestion[] BasicQuestions { get; private set; }

		public static CombatPetsQuizQuestion[] ClassSpecificQuestions { get; private set; }

		public override void Load()
		{
			base.Load();
			BasicQuestions = MakeQuestions();
			ClassSpecificQuestions = MakeClassSpecificQuestions();
		}

		public override void Unload()
		{
			base.Unload();
			BasicQuestions = null;
			ClassSpecificQuestions = null;
		}

		// generator function to workaround static load/unload constraints
		private static CombatPetsQuizQuestion[] MakeQuestions() => new CombatPetsQuizQuestion[]
		{
			new("What is your favorite class to play as?",
				("Melee.", HARDY), ("Ranged.", BOLD), ("Mage.", CALM))
			{
				AddFollowUpQuestion = idx => 
				new ("Ok, but what's really your favorite class to play as?",
					("Summoner.", CALM), ("Summoner?", QUIRKY), ("Summoner!", BOLD)),
			},

			new("You accidentally clicked Reforge one too many times, now your Legendary Terra Blade is Broken! What do you do?",
				("Cry.", QUIET), 
				("Keep mashing 'Reforge'.", JOLLY), 
				("Install Consistent Reforging!", QUIRKY)),

			new ("The Clothier lost his hat, and claims the Guide stole it! How do you help out?",
				 ("Let them fight it out.", QUIET),
				 ("Confront the Guide directly.", BOLD),
				 ("Try to mediate.", CALM)),

			new ("You see an innocent Bunny chased by a group of Goblin Scouts! Do you intervene?",
				("Yes.", BOLD),
				("No.", CALM),
				("Chase the bunny too!", JOLLY)),

			new ("You just started a new world. How do you build a house for the Guide?",
				("A lovely cabin.", HARDY),
				("Prison cube.", QUIET),
				("No house at all.", QUIRKY)),


			new ("Re-Logic just nerfed your favorite weapon! How do you adapt?",
				("Keep using it.", HARDY), ("Try a new one!", JOLLY)),

			new ("Do you prefer building or adventuring?",
				("Building.", QUIET), ("Adventuring.", BOLD), ("Fishing!", QUIRKY)),

			new ("After looting a chest, do you take the chest with you?",
				("Yes.", HARDY), ("No.", QUIRKY)),

			new ("While exploring a cave, you see a treasure chest sitting out in the open.",
				("Yay, treasure!", JOLLY), ("It's a mimic!", HARDY), ("It's a trapped chest!", QUIET)),

			new ("What's your favorite color?",
				("Red.", BOLD), ("Green.", CALM), ("Blue.", JOLLY)),

			new ("How well organized are your chests?",
				("Not at all.", BOLD), ("A little bit.", QUIRKY), ("Well organized.", QUIET)),
		};

		private static CombatPetsQuizQuestion[] MakeClassSpecificQuestions() => new CombatPetsQuizQuestion[]
		{
			new ("A fiery friend is a great choice! How should their disposition be?",
				("Outgoing and bold!", BOLD), ("Reserved and quiet.", QUIET)),

			new ("An easygoing friend is a great choice! How should their disposition be?",
				("Jolly and carefree!", JOLLY), ("Quirky and eccentric!", QUIRKY)),

			new ("A down to earth friend is a great choice! How should their disposition be?",
				("Hardy and resillient!", HARDY), ("Calm and collected.", CALM)),
		};
	}

}
