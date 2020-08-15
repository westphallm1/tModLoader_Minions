using DemoMod.Projectiles.Minions.MinonBaseClasses;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace DemoMod.Projectiles.Minions.XCXCopter
{
    public class XCXCopterMinionBuff: MinionBuff
    {
        public XCXCopterMinionBuff() : base(ProjectileType<XCXCopterMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Copter-X");
			Description.SetDefault("A flexible helicopter will fight for you!");
        }
    }

    public class XCXCopterMinionItem: EmpoweredMinionItem<XCXCopterMinionBuff, XCXCopterMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copter-X Staff");
			Tooltip.SetDefault("Summons a flexible helicopter to fight for you!");
            
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
            item.damage = 35;
			item.height = 34;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SoulofLight, 6);
            recipe.AddIngredient(ItemID.AdamantiteBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class XCXCopterMinion : WormMinion<XCXCopterMinionBuff>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copter-X");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

        public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.tileCollide = false;
            projectile.type = ProjectileType<XCXCopterMinion>();
            frameSpeed = 2;
        }

        protected override void DrawHead()
        {
            Rectangle head = new Rectangle(0, 0, 26, 36);
            AddSprite(2, head);
        }

        protected override void DrawBody()
        {
            Rectangle body; 
            for(int i = 0; i < GetSegmentCount() + 1; i++)
            {
                if(i%3 == 0)
                {
                    body = GetRotorFrame();
                } else
                {
                    body = new Rectangle(0, 36, 18, 36);
                }
                AddSprite(22 + 16*i, body);
            }
        }

        private Rectangle GetRotorFrame()
        {
            return new Rectangle(0, 108 + 36*projectile.frame, 38, 36);
        }

        protected override void DrawTail()
        {
            Rectangle tail = new Rectangle(0, 72, 32, 36);
            int dist = 26 + 16 * (GetSegmentCount() + 1);
            AddSprite(dist, tail);
        }

        protected override float ComputeSearchDistance()
        {
            return 700 + 30 * GetSegmentCount();
        }

        protected override float ComputeInertia()
        {
            return 22 - GetSegmentCount();
        }

        protected override float ComputeTargetedSpeed()
        {
            return Math.Min(16, 3 + 3f * GetSegmentCount());
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }

        protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
        {
            minFrame = 0;
            maxFrame = 4;
        }
    }
}
