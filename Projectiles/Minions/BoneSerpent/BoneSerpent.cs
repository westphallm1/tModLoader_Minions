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

namespace DemoMod.Projectiles.Minions.BoneSerpent
{
    public class BoneSerpentMinionBuff: MinionBuff
    {
        public BoneSerpentMinionBuff() : base(ProjectileType<BoneSerpentMinion>()) { }
        public override void SetDefaults()
        {
            base.SetDefaults();
			DisplayName.SetDefault("Bone Serpent");
			Description.SetDefault("A skeletal dragon will fight for you!");
        }
    }

    public class BoneSerpentMinionItem: EmpoweredMinionItem<BoneSerpentMinionBuff, BoneSerpentMinion>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Serpent Staff");
			Tooltip.SetDefault("Summons a skeletal dragon to fight for you!");
            
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
            item.damage = 18;
			item.height = 32;
			item.value = Item.buyPrice(0, 1, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Bone, 30);
            recipe.AddIngredient(ItemID.HellstoneBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class BoneSerpentMinion : WormMinion<BoneSerpentMinionBuff>
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Serpent");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

        public sealed override void SetDefaults() {
			base.SetDefaults();
			projectile.width = 1;
			projectile.height = 1;
			projectile.tileCollide = false;
            projectile.type = ProjectileType<BoneSerpentMinion>();
        }

        protected override void DrawHead()
        {
            Rectangle head = new Rectangle(0, 0, 34, 28);
            AddSprite(2, head);
        }

        protected override void DrawBody()
        {
            Rectangle body = new Rectangle(0, 28, 16, 28);
            for(int i = 0; i < GetSegmentCount() + 1; i++)
            {
                AddSprite(26 + 15*i, body);
            }

        }
        protected override void DrawTail()
        {
            Rectangle tail = new Rectangle(0, 56, 14, 28);
            int dist = 26 + 15 * (GetSegmentCount() + 1);
            AddSprite(dist, tail);
        }

        protected override float ComputeSearchDistance()
        {
            return 600 + 25 * GetSegmentCount();
        }

        protected override float ComputeInertia()
        {
            return 22 - GetSegmentCount();
        }

        protected override float ComputeTargetedSpeed()
        {
            return Math.Min(14, 4 + 2 * GetSegmentCount());
        }

        protected override float ComputeIdleSpeed()
        {
            return ComputeTargetedSpeed() + 3;
        }
    }
}
