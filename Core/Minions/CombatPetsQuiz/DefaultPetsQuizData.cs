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
using Terraria.Localization;
using Terraria.ModLoader;
using static AmuletOfManyMinions.Core.Minions.CombatPetsQuiz.PersonalityType;

namespace AmuletOfManyMinions.Core.Minions.CombatPetsQuiz
{
	internal class DefaultPetsQuizData : ModSystem
	{
		public static CombatPetsQuizQuestion[] BasicQuestions { get; private set; }

		public static CombatPetsQuizQuestion[] ClassSpecificQuestions { get; private set; }

		internal static Dictionary<PersonalityType, QuizResult> ResultsMap { get; private set; }

		internal static Dictionary<PersonalityType, LocalizedText> PersonalityTypeNames { get; private set; }

		internal static (LocalizedText[] introLines, LocalizedText[] outroLines) QuizWithDominantTraitsLines { get; private set; }
		internal static (LocalizedText[] introLines, LocalizedText[] outroLines) ClassSpecificQuizLines { get; private set; }

		internal static LocalizedText ClassSpecificQuizFirstQuestion { get; private set; }
		internal static LocalizedText[] ClassSpecificQuizAnswers { get; private set; }

		public override void Load()
		{
			base.Load();
			BasicQuestions = MakeQuestions();
			ClassSpecificQuestions = MakeClassSpecificQuestions();
			PersonalityTypeNames = MakePersonalityTypeNames();

			QuizWithDominantTraitsLines = MakeQuizLines("QuizWithDominantTraits", 4, 4);
			ClassSpecificQuizLines = MakeQuizLines("ClassSpecificQuiz", 3, 3);

			ClassSpecificQuizFirstQuestion = Language.GetOrRegister(Mod.GetLocalizationKey($"CombatPetQuiz.Quizzes.ClassSpecificQuiz.Text"));
			ClassSpecificQuizAnswers = MakeClassSpecificQuizAnswers();
		}

		private LocalizedText[] MakeClassSpecificQuizAnswers()
		{
			var category = $"CombatPetQuiz.Quizzes.ClassSpecificQuiz.Answers.";
			return new LocalizedText[]
			{
				Language.GetOrRegister(Mod.GetLocalizationKey($"{category}0")),
				Language.GetOrRegister(Mod.GetLocalizationKey($"{category}1")),
				Language.GetOrRegister(Mod.GetLocalizationKey($"{category}2")),
				Language.GetOrRegister(Mod.GetLocalizationKey($"{category}3")),
			};
		}

		private (LocalizedText[] introLines, LocalizedText[] outroLines) MakeQuizLines(string quizName, int introLineCount, int outroLineCount)
		{
			var category = $"CombatPetQuiz.Quizzes.{quizName}.";
			var intro = new LocalizedText[introLineCount];
			for (int i = 0; i < introLineCount; i++)
			{
				intro[i] = Language.GetOrRegister(Mod.GetLocalizationKey($"{category}IntroLines.{i}"));
			}

			var outro = new LocalizedText[outroLineCount];
			for (int o = 0; o < outroLineCount; o++)
			{
				outro[o] = Language.GetOrRegister(Mod.GetLocalizationKey($"{category}OutroLines.{o}"));
			}

			return (intro, outro);
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
			PersonalityTypeNames = null;

			QuizWithDominantTraitsLines = (null, null);
			ClassSpecificQuizLines = (null, null);

			ClassSpecificQuizFirstQuestion = null;
			ClassSpecificQuizAnswers = null;
		}

		private Dictionary<PersonalityType, LocalizedText> MakePersonalityTypeNames()
		{
			var dict = new Dictionary<PersonalityType, LocalizedText>();
			var array = Enum.GetValues(typeof(PersonalityType));
			string category = $"CombatPetQuiz.";
			foreach (var item in array)
			{
				var self = (PersonalityType)item;
				dict[self] = Language.GetOrRegister(Mod.GetLocalizationKey($"{category}PersonalityTypes.{self}"));
			}
			return dict;
		}

		// generator function to workaround static load/unload constraints
		private static CombatPetsQuizQuestion[] MakeQuestions()
		{
			int index = 0;
			var category = "CombatPetQuiz.Questions.{0}.";
			CombatPetsQuizQuestion initialFollowUpQuestion =
				new(string.Format(category, index++), CALM, QUIRKY, BOLD);

			//This system is not expandable at all :(, should be rewritten to identifiable objects (custom classes or string key)
			var list = new CombatPetsQuizQuestion[]
			{
				new(string.Format(category, index++), HARDY, BOLD, CALM)
				{
					AddFollowUpQuestion = _ => initialFollowUpQuestion
				},

				new(string.Format(category, index++), QUIET, JOLLY, QUIRKY),

				new(string.Format(category, index++), QUIET, BOLD, CALM),

				new(string.Format(category, index++), BOLD, CALM, JOLLY),

				new(string.Format(category, index++), CALM, QUIET, HASTY),

				new(string.Format(category, index++), HARDY, JOLLY),

				new(string.Format(category, index++), RELAXED, BOLD, QUIRKY),

				new(string.Format(category, index++), HARDY, QUIRKY),

				new(string.Format(category, index++), QUIRKY, HARDY, QUIET),

				new(string.Format(category, index++), BOLD, CALM, JOLLY),

				new(string.Format(category, index++), HASTY, QUIRKY, QUIET),

				new(string.Format(category, index++), HASTY, HARDY, RELAXED),

				new(string.Format(category, index++), QUIET, HASTY, QUIRKY),

				new(string.Format(category, index++), JOLLY, RELAXED),

				new(string.Format(category, index++), RELAXED, HARDY, BOLD),

				new(string.Format(category, index++), HASTY, RELAXED),

				new(string.Format(category, index++), CALM, QUIET, JOLLY),

				new(string.Format(category, index++), HASTY, CALM, RELAXED)
			};

			return list;
		}

		private static CombatPetsQuizQuestion[] MakeClassSpecificQuestions()
		{
			int index = 0;
			var category = "CombatPetQuiz.ClassSpecificQuestions.{0}.";
			var list = new CombatPetsQuizQuestion[]
			{
				new(string.Format(category, index++), BOLD, QUIET),

				new(string.Format(category, index++), JOLLY, QUIRKY),

				new(string.Format(category, index++), HARDY, CALM),

				new(string.Format(category, index++), RELAXED, HASTY),
			};

			return list;
		}

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
				IntroLines = QuizWithDominantTraitsLines.introLines,
				OutroLines = QuizWithDominantTraitsLines.outroLines
			};
			return quiz;
		}

		public static CombatPetsQuiz MakeClassSpecificQuiz(params PersonalityType[] disallowedTypes)
		{
			List<int> usedIndices = ClassSpecificQuestions
				.Select((q, idx) => (Question: q, Idx: idx))
				.Where(q => !disallowedTypes.Any(t=>q.Question.CanGivePointsForType(t)))
				.Select(q => q.Idx)
				.ToList();

			CombatPetsQuizQuestion question = new("",
				ClassSpecificQuizFirstQuestion,
				usedIndices.Select(idx => (ClassSpecificQuizAnswers[idx], NONE)).ToArray())
			{
				AddFollowUpQuestion = idx => ClassSpecificQuestions[usedIndices[idx]]
			};
			var quiz = new CombatPetsQuiz
			{
				Questions = new List<CombatPetsQuizQuestion>() { question },
				ExtraResultItemID = disallowedTypes.Length > 0 ? 
					ModContent.ItemType<CombatPetStylishTeamworkBow>() : ItemID.None,
				IntroLines = ClassSpecificQuizLines.introLines,
				OutroLines = ClassSpecificQuizLines.outroLines
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
