using AmuletOfManyMinions.Projectiles.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;

namespace AmuletOfManyMinions.NPCs
{
    class LootTable : GlobalNPC
    {
        public override void NPCLoot(NPC npc)
        {
            base.NPCLoot(npc);
            if(npc.type == NPCID.Eyezor && Main.rand.NextFloat() < 0.10f)
            {
                Item.NewItem(npc.getRect(), ItemType<SqueyereMinionItem>(), 1);
            }

            if((npc.type == NPCID.AngryBones  || 
               npc.type == NPCID.AngryBonesBig  || 
               npc.type == NPCID.AngryBonesBigHelmet || 
               npc.type == NPCID.AngryBonesBigMuscle) &&
               Main.rand.NextFloat() < 0.02f)
            {
                Item.NewItem(npc.getRect(), ItemType<BoneSquireMinionItem>(), 1);
            }

            if((npc.type == NPCID.BlueArmoredBones || 
               npc.type == NPCID.BlueArmoredBonesMace || 
               npc.type == NPCID.BlueArmoredBonesNoPants || 
               npc.type == NPCID.BlueArmoredBonesSword) &&
               Main.rand.NextFloat() < 0.03f)
            {
                Item.NewItem(npc.getRect(), ItemType<ArmoredBoneSquireMinionItem>(), 1);
            }

            if(npc.type == NPCID.ManEater && Main.rand.NextFloat() < 0.08f)
            {
                Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1);
            }
        }
    }
}
