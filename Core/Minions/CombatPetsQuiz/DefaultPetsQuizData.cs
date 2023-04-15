using AmuletOfManyMinions.Items.Accessories.CombatPetAccessories;
using AmuletOfManyMinions.Items.Materials;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static AmuletOfManyMinions.Core.Minions.CombatPetsQuiz.PersonalityType;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{
	internal class DefaultPetsQuizData : ModSystem
	{
		public static CombatPetsQuizQuestion[] BasicQuestions { get; private set; }

		public static CombatPetsQuizQuestion[] ClassSpecificQuestions { get; private set; }

		internal static Dictionary<PersonalityType, QuizResult> ResultsMap { get; private set; }

		public override void Load()
		{
			base.Load();
			BasicQuestions = MakeQuestions();
			ClassSpecificQuestions = MakeClassSpecificQuestions();
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ResultsMap = QuizResult.MakeResultsMap();
			if (Main.dedServ)
			{
				return;
			}
			string TextureBasePath = GetType().Namespace.Replace('.', '/') + "/Portrait_";
			foreach (var personalityType in ResultsMap.Keys)
			{
				string TexturePath = TextureBasePath + Enum.GetName(personalityType);
				ResultsMap[personalityType].PortraitTexture = ModContent.Request<Texture2D>(TexturePath);
			}
		}

		public override void Unload()
		{
			base.Unload();
			BasicQuestions = null;
			ClassSpecificQuestions = null;
			ResultsMap = null;
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

			new("You accidentally clicked Reforge one too many times, now your Legendary Zenith is Broken! What do you do?",
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
				("A lovely cabin.", CALM),
				("A prison cube.", QUIET),
				("No house at all.", HASTY)),


			new ("Re-Logic just nerfed your favorite weapon! How do you adapt?",
				("Keep using it.", HARDY), ("Try a new one!", JOLLY)),

			new ("Do you prefer building or adventuring?",
				("Building.", RELAXED), ("Adventuring.", BOLD), ("Fishing!", QUIRKY)),

			new ("After looting a chest, do you take the chest with you?",
				("Yes.", HARDY), ("No.", QUIRKY)),

			new ("While exploring a cave, you see a treasure chest sitting out in the open.",
				("Yay, treasure!", QUIRKY), ("It's a mimic!", HARDY), ("It's a trapped chest!", QUIET)),

			new ("What's your favorite color?",
				("Red.", BOLD), ("Green.", CALM), ("Blue.", JOLLY)),

			new ("How well organized are your chests?",
				("Not at all.", HASTY), ("A little bit.", QUIRKY), ("Well organized.", QUIET)),

			new ("You've just arrived at the dungeon's entrance, but the night is already half way over! What do you do?",
				("Fight skeletron right away!", HASTY), 
				("Build a makeshift arena first.", HARDY), 
				("Wait until the next night", RELAXED)),

			new ("A Martian probe has caught you off guard and is now flying away! How do you respond?",
				("Quickly save and quit.", QUIET), 
				("Chase after it!", HASTY), 
				("Yay! Martian Invasion!", QUIRKY)),

			new ("Do you prefer sweet foods or savory foods?",
				("Sweet!", JOLLY),
				("Savory.", RELAXED)),

			new ("What's your favorite difficulty to play on?",
				("Normal.", RELAXED),
				("Expert.", HARDY),
				("Master.", BOLD)),

			new ("You're playing multiplayer and just found a one-of-a-kind chest item! How do you handle sharing it?",
				("First come, first serve.", HASTY),
				("Divide the loot evenly.", RELAXED)),

			new ("Do you prefer Squires or whips?",
				("Squires.", CALM),
				("Squires?", QUIET),
				("Squires!", JOLLY)),

			new ("Do you replant trees after chopping them down?",
				("Never!", HASTY),
				("Right away.", CALM),
				("After a while.", RELAXED))
		};

		private static CombatPetsQuizQuestion[] MakeClassSpecificQuestions() => new CombatPetsQuizQuestion[]
		{
			new ("A fiery friend is a great choice! How should their disposition be?",
				("Outgoing and bold!", BOLD), ("Reserved and quiet.", QUIET)),

			new ("An easygoing friend is a great choice! How should their disposition be?",
				("Jolly and carefree!", JOLLY), ("Quirky and eccentric!", QUIRKY)),

			new ("A down to earth friend is a great choice! How should their disposition be?",
				("Hardy and resillient!", HARDY), ("Calm and collected.", CALM)),

			new ("A lofty friend is a great choice! How should their disposition be?",
				("Easygoing and relaxed!", RELAXED), ("Hasty and ambitious.", HASTY)),
		};

		public static CombatPetsQuiz MakeQuizWithDominantTraits(Player player, int questionCount)
		{
			PersonalityType[] personalityTypes = GetPersonalityTypesForPlayerLocation(player);
			var quiz = new CombatPetsQuiz
			{
				Questions = BasicQuestions
					.OrderByDescending(q => personalityTypes.Select(pt => Convert.ToInt32(q.CanGivePointsForType(pt))).Sum())
					.ThenBy(q => Main.rand.Next())
					.Take(questionCount)
					.ToList(),
				ExtraResultItemID = ModContent.ItemType<InertCombatPetFriendshipBow>(),
				IntroLines = new string[]
				{
					"Welcome, to the world of Terraria!",
					"Terraria can be a dangerous place, but having a good friend at your side always makes it better!",
					"To help you and your new friend get to know each other better, here's a few questions!",
					"Be sure to answer sincerely. Let's begin!",
				},

				OutroLines = new string[]
				{
					"Thank you for answering those questions!",
					"Based on your answers, you appear to be... the {0} type!",
					"A {0} person like yourself would be great friends with... the {1}!",
					"May you and your new friend enjoy many adventures!",
				}
			};
			return quiz;
		}

		public static CombatPetsQuiz MakeClassSpecificQuiz(params PersonalityType[] disallowedTypes)
		{
			string[] allAnswerTexts =
			{
				"A friend with a fiery passion!",
				"A friend who can go with the flow!",
				"A friend who's down to earth!",
				"A friend with their head in the clouds!",
			};
			List<int> usedIndices = ClassSpecificQuestions
				.Select((q, idx) => (Question: q, Idx: idx))
				.Where(q => !disallowedTypes.Any(t=>q.Question.CanGivePointsForType(t)))
				.Select(q => q.Idx)
				.ToList();


			CombatPetsQuizQuestion question = new (
				"What sort of friend would you like to have join you on your journey?",
				usedIndices.Select(idx => (allAnswerTexts[idx], NONE)).ToArray())
			{
				AddFollowUpQuestion = idx => ClassSpecificQuestions[usedIndices[idx]]
			};
			var quiz = new CombatPetsQuiz
			{
				Questions = new List<CombatPetsQuizQuestion>() { question },
				ExtraResultItemID = disallowedTypes.Length > 0 ? 
					ModContent.ItemType<CombatPetStylishTeamworkBow>() : ItemID.None,
				IntroLines = new string[]
				{
					"Welcome back, to the world of Terraria!",
					"I hope you and your friends have been enjoying your adventures!",
					"Do you know what the only thing better than having friends is? Having more friends!",
				},
					OutroLines = new string[]
				{
					"Ah, a {0} friend will be great to have!",
					"The {0} {1} is excited to join your team!",
					"May you and your new friend enjoy many adventures!",
				}
			};
			return quiz;
		}

		internal static void DebugPrintAnswerCounts()
		{
			foreach(var key in ResultsMap.Keys)
			{
				int answerCount = BasicQuestions.Where(q => q.CanGivePointsForType(key)).Count();
				Main.NewText(Enum.GetName(key) + ": " + answerCount);
			}
		}

		public static PersonalityType[] GetPersonalityTypesForPlayerLocation(Player player)
		{
			if(player.ZoneJungle)
			{
				// jungle - plant elemental pals
				return new[] { CALM, HARDY };
			} else if (player.ZoneDesert || player.ZoneUndergroundDesert)
			{
				// desert - fire elemental pals
				return new[] { BOLD, QUIET };
			} else if (player.ZoneBeach)
			{
				// beach - water elemental pals
				return new[] { QUIRKY, JOLLY };
			} else if (player.ZoneSkyHeight)
			{
				// beach - water elemental pals
				return new[] { HASTY, RELAXED };
			} else
			{
				// two random types for funsies
				int maxPersonality = Enum.GetValues(typeof(PersonalityType)).Cast<int>().Max();
				return new[]
				{
					(PersonalityType)Main.rand.Next(1, maxPersonality),
					(PersonalityType)Main.rand.Next(1, maxPersonality)
				};
			}
		}
	}
}
