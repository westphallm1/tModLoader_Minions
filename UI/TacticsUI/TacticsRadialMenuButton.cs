using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	class TacticsRadialMenuButton : RadialMenuButton
	{
		public readonly byte tacticId;

		public TacticsRadialMenuButton(Texture2D bgTexture, byte tacticId, Vector2 relativeTopLeft)
			: base(bgTexture, TargetSelectionTacticHandler.GetTexture(tacticId), relativeTopLeft)
		{
			this.tacticId = tacticId;
		}
	}
}
