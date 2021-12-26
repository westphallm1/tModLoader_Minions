using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static AmuletOfManyMinions.Core.Minions.CombatPetsQuiz.PersonalityType;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{
	internal class CombatPetsQuiz
	{
		public List<CombatPetsQuizQuestion> Questions { get; private set; } = new();

		private readonly List<PersonalityType> GivenAnswers = new();

		private int currentIdx = 0;

		public CombatPetsQuizQuestion CurrentQuestion => Questions[currentIdx];

		public void AnswerQuestion(int answerIdx)
		{
			if(answerIdx < Questions.Count)
			{
				GivenAnswers.Add(CurrentQuestion.AnswerValues[answerIdx]);
				currentIdx++;
			}
		}

		public bool IsComplete() => GivenAnswers.Count == Questions.Count;

		// Not quite sure how this will resolve in the case of a tie
		public PersonalityType GetResultType() =>
			GivenAnswers.GroupBy(a => a).OrderByDescending(g => g.Count()).Select(g=>g.Key).FirstOrDefault();

		public static CombatPetsQuiz MakeQuizWithDominantTrait(PersonalityType personalityType, int questionCount)
		{
			var quiz = new CombatPetsQuiz();
			// first, make half of the quiz the questions that can give points to the dominant type
			var questionsWithPoints =
				DefaultQuestions.Questions.OrderBy(q => q.CanGivePointsForType(personalityType) ? 0 : 1).Take(questionCount / 2);

			// Then, select the rest of the quiz from the remaining questions at random
			var otherQuestions = 
				DefaultQuestions.Questions
				.OrderBy(q => (q.CanGivePointsForType(personalityType) ? 0 : 1))
				.ThenBy(q=>Main.rand.Next())
				.Skip(questionCount/2)
				.Take(questionCount);

			quiz.Questions = Enumerable.Concat(questionsWithPoints, otherQuestions)
				.OrderBy(q => Main.rand.Next())
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
		BOLD, // Cinder Chicken
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
