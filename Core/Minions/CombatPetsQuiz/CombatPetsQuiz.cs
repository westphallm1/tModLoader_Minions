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
	internal class CombatPetsQuiz
	{
		public List<CombatPetsQuizQuestion> Questions { get; private set; } = new();

		public readonly List<PersonalityType> GivenAnswers = new();

		private int currentIdx = 0;

		public CombatPetsQuizQuestion CurrentQuestion => Questions[currentIdx];

		public void AnswerQuestion(int answerIdx)
		{
			GivenAnswers.Add(CurrentQuestion.AnswerValues[answerIdx]);
			currentIdx++;
		}

		public bool IsComplete() => GivenAnswers.Count == Questions.Count;

		// Not quite sure how this will resolve in the case of a tie
		public PersonalityType GetResultType() =>
			GivenAnswers
				.Select(Type => (Type, GivenAnswers.Where(t => t == Type).Count()))
				.OrderByDescending(t => t.Item2)
				.Select(t => t.Type)
				.FirstOrDefault();

		public static CombatPetsQuiz MakeQuizWithDominantTrait(PersonalityType personalityType, int questionCount)
		{
			var quiz = new CombatPetsQuiz();
			quiz.Questions = DefaultQuestions.Questions
				.OrderBy(q => q.CanGivePointsForType(personalityType) ? 0 : 1)
				.ThenBy(q => Main.rand.Next())
				.Take(questionCount)
				.ToList();
			return quiz;
		}
	}


	// TODO localize
	public enum PersonalityType
	{
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
	}

	internal class DefaultQuestions : ModSystem
	{
		public static CombatPetsQuizQuestion[] Questions { get; private set; }

		public override void Load()
		{
			base.Load();
			Questions = MakeQuestions();
		}

		public override void Unload()
		{
			base.Unload();
			Questions = null;
		}

		// generator function to workaround static load/unload constraints
		private static CombatPetsQuizQuestion[] MakeQuestions() => new CombatPetsQuizQuestion[]
		{
			new("What is your favorite class to play as?",
				("Melee.", HARDY), ("Ranged.", BOLD), ("Mage.", CALM)),

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

			new ("Ok, but what's really your favorite class to play as?",
				("Summoner.", CALM), ("Summoner?", QUIRKY), ("Summoner!", BOLD)),

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
	}

}
