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


		public override void OnEnterWorld(Player player)
		{
			base.OnEnterWorld(player);
			IsTakingQuiz = true;
			CurrentQuiz = CombatPetsQuiz.MakeQuizWithDominantTrait(PersonalityType.CALM, 6);
		}
	}
}
